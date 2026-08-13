# LIKE Operations: Analysis and Recommendations

## Executive Summary

This document analyzes the impact of encryption on LIKE pattern matching operations and provides specific recommendations for the AU_eOffice_Live system.

**Key Finding**: Deterministic AES encryption is **incompatible** with SQL LIKE operations. Alternative approaches must be implemented.

---

## Technical Explanation

### Why LIKE Doesn't Work with Encryption

#### Original (Plaintext)
```sql
-- Works perfectly
SELECT * FROM M_File WHERE File_Code LIKE 'FA%'

Results:
- FA/2024/001
- FA/2024/002
- FA/2024/100
```

#### After Encryption (Base64)
```sql
-- Returns NO results or wrong results
SELECT * FROM M_File WHERE File_Code LIKE 'FA%'

Actual encrypted values:
- X82JH83JD82H8J2HK3JH==  (was FA/2024/001)
- A12KL34MN56O7P8QR9ST==  (was FA/2024/002)
- Z98QW76ER54TY32UI10VB==  (was FA/2024/100)

Problem: No relationship between plaintext pattern and encrypted pattern
```

### Why This Happens

1. **AES encryption** produces pseudorandom output
2. **Base64 encoding** creates ASCII strings like "X82JH83..."
3. **No correlation** between plaintext "FA" and encrypted output
4. **Pattern matching impossible** without decryption

---

## Impact Assessment

### Low Risk Scenarios (Likely in Your System)

If LIKE is used for:
- **Admin search features** → Low usage frequency
- **Optional filters** → Users can search without LIKE
- **Prefix-only searches** (e.g., `LIKE 'FA%'`) → Can be replaced with dropdown

### High Risk Scenarios (Check Your System)

If LIKE is used for:
- **Primary search functionality** → Major user impact
- **Core business processes** → Critical to operations
- **Automated processes** → System integrations may break
- **Reports** → Need alternative implementation

---

## Recommended Solutions

### Option 1: Eliminate LIKE (Simplest) ⭐ RECOMMENDED

**When to use**: LIKE searches are infrequent or non-critical

**Implementation**:

#### Before (Plaintext with LIKE)
```sql
-- Stored Procedure
CREATE PROCEDURE SearchFilesByPrefix
    @Prefix NVARCHAR(50)
AS
BEGIN
    SELECT * FROM M_File
    WHERE File_Code LIKE @Prefix + '%'
END
```

```csharp
// BAL - Before
public List<ModelFile> SearchFiles(string prefix)
{
    return DAL.SearchFilesByPrefix(prefix);
}
```

#### After (Encrypted, LIKE removed)
```sql
-- Stored Procedure (unchanged)
CREATE PROCEDURE GetFileByCode
    @FileCode NVARCHAR(MAX)
AS
BEGIN
    SELECT * FROM M_File
    WHERE File_Code = @FileCode
END
```

```csharp
// BAL - After
public List<ModelFile> SearchFiles(string prefix)
{
    // Get all files and filter in application
    var allFiles = GetAllFiles(); // Returns decrypted
    return allFiles
        .Where(f => f.File_Code.StartsWith(prefix))
        .ToList();
}
```

**Pros**:
- ✅ No database changes
- ✅ Simple to implement
- ✅ Works with encrypted data

**Cons**:
- ❌ Poor performance for large datasets
- ❌ Loads all data to filter client-side

**Best for**: 
- Small datasets (< 10,000 rows)
- Infrequent searches
- User-initiated searches

---

### Option 2: Search Index Table ⭐ RECOMMENDED FOR LARGE DATASETS

**When to use**: Frequent LIKE searches on large datasets

**Implementation**:

#### Step 1: Create Index Table
```sql
CREATE TABLE File_SearchIndex (
    Row_ID BIGINT PRIMARY KEY IDENTITY(1,1),
    File_Code_Encrypted NVARCHAR(MAX) NOT NULL,  -- Links to M_File
    File_Code_Prefix NVARCHAR(20),                -- First 10-15 chars
    File_Code_Year NVARCHAR(4),                   -- Extract year
    File_Code_Dept NVARCHAR(20),                  -- Extract department code
    File_Code_Category NVARCHAR(20),              -- Extract category
    Created_DT DATETIME DEFAULT GETDATE(),
    INDEX IX_Prefix (File_Code_Prefix),
    INDEX IX_Year (File_Code_Year),
    INDEX IX_Dept (File_Code_Dept)
)
```

#### Step 2: Populate Index (During Migration)
```csharp
public void BuildSearchIndex()
{
    var files = GetAllFiles(); // Returns decrypted
    
    foreach (var file in files)
    {
        // Parse File_Code components
        string[] parts = file.File_Code.Split('/');
        string dept = parts.Length > 0 ? parts[0] : "";
        string category = parts.Length > 1 ? parts[1] : "";
        string year = parts.Length > 2 ? parts[2] : "";
        
        // Insert into index
        DAL.InsertSearchIndex(
            file.File_Code_Encrypted,
            file.File_Code.Substring(0, Math.Min(15, file.File_Code.Length)),
            year,
            dept,
            category
        );
    }
}
```

#### Step 3: Update Stored Procedure
```sql
CREATE PROCEDURE SearchFilesByPrefix
    @Prefix NVARCHAR(20)
AS
BEGIN
    SELECT 
        mf.*
    FROM 
        M_File mf
    INNER JOIN 
        File_SearchIndex fsi ON mf.File_Code = fsi.File_Code_Encrypted
    WHERE 
        fsi.File_Code_Prefix LIKE @Prefix + '%'
    ORDER BY 
        mf.Row_ID DESC
END
```

#### Step 4: Maintain Index
```csharp
// In BAL - After INSERT
public string AddFile(ModelFile model)
{
    // Encrypt
    string encryptedCode = DeterministicEncryptionHelper.Encrypt(model.File_Code);
    
    // Insert file
    string msg = DAL.InsertFile(model);
    
    // Update search index
    if (msg.Contains("Success"))
    {
        string[] parts = model.File_Code.Split('/');
        DAL.InsertSearchIndex(
            encryptedCode,
            model.File_Code.Substring(0, Math.Min(15, model.File_Code.Length)),
            parts.Length > 2 ? parts[2] : "",
            parts.Length > 0 ? parts[0] : "",
            parts.Length > 1 ? parts[1] : ""
        );
    }
    
    return msg;
}
```

**Pros**:
- ✅ Fast search performance
- ✅ Works with large datasets
- ✅ Supports complex queries
- ✅ Indexed for speed

**Cons**:
- ❌ Additional table to maintain
- ❌ Must sync on INSERT/UPDATE
- ❌ More complex implementation

**Best for**: 
- Large datasets (> 10,000 rows)
- Frequent searches
- Production systems

---

### Option 3: Hybrid - Store Prefix Separately

**When to use**: Only prefix searches needed, security allows

**Implementation**:

#### Step 1: Add Column
```sql
ALTER TABLE M_File ADD File_Code_Prefix NVARCHAR(20);

CREATE INDEX IX_File_Code_Prefix ON M_File(File_Code_Prefix);
```

#### Step 2: Populate During Migration
```csharp
// In migration utility
foreach (var file in files)
{
    string plaintext = file.File_Code;
    string encrypted = DeterministicEncryptionHelper.Encrypt(plaintext);
    string prefix = plaintext.Substring(0, Math.Min(10, plaintext.Length));
    
    // Update both columns
    DAL.UpdateFile(file.Row_ID, encrypted, prefix);
}
```

#### Step 3: Search Using Prefix
```sql
CREATE PROCEDURE SearchFilesByPrefix
    @Prefix NVARCHAR(20)
AS
BEGIN
    SELECT * FROM M_File
    WHERE File_Code_Prefix LIKE @Prefix + '%'
END
```

**Pros**:
- ✅ Simple implementation
- ✅ Fast searches
- ✅ No separate table

**Cons**:
- ❌ Prefix stored in plaintext (security risk)
- ❌ Only supports prefix matching
- ❌ Reveals partial information

**Best for**: 
- Low-sensitivity prefixes
- Prefix-only searches
- Quick implementation

---

### Option 4: Full-Text Search (NOT RECOMMENDED)

**Implementation**: Use SQL Server Full-Text Search on decrypted values

**Pros**:
- ✅ Powerful search capabilities

**Cons**:
- ❌ Cannot index encrypted data
- ❌ Would require decryption in database (violates architecture)
- ❌ Complex to implement
- ❌ Performance overhead

**Verdict**: Not suitable for this use case

---

## Specific Recommendations for AU_eOffice_Live

### Analysis Required

Run this script to determine LIKE usage in your system:

```sql
-- Find all procedures using LIKE on File_Code or Doc_Code
SELECT 
    o.name AS Procedure_Name,
    CASE 
        WHEN m.definition LIKE '%File_Code%LIKE%' THEN 'File_Code'
        WHEN m.definition LIKE '%Doc_Code%LIKE%' THEN 'Doc_Code'
    END AS Column_Used,
    LEN(m.definition) AS Definition_Length
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%File_Code%LIKE%'
        OR m.definition LIKE '%Doc_Code%LIKE%'
    )
ORDER BY 
    o.name
```

### Recommended Approach

Based on typical e-office systems:

1. **For File_Code searches**: Use **Option 2 (Search Index Table)**
   - Reason: File codes are frequently searched
   - File codes have predictable structure (DEPT/CATEGORY/YEAR/NUM)
   - Can extract searchable components

2. **For Doc_Code searches**: Use **Option 1 (Eliminate LIKE)**
   - Reason: Usually search by exact document code
   - If LIKE is used, likely infrequent

3. **For dropdown filters**: Replace with exact match dropdowns
   - Example: Department dropdown → Encrypt selected value → Exact match

### File Code Structure Analysis

If your File_Code follows pattern: `FA/FELLOW/Payment/7/499/2024`

**Components**:
- FA = Department
- FELLOW = Category
- Payment = Subcategory
- 7, 499 = Sequential numbers
- 2024 = Year

**Search Index Implementation**:
```sql
CREATE TABLE File_SearchIndex (
    File_Code_Encrypted NVARCHAR(MAX),
    Dept_Code NVARCHAR(20),          -- FA
    Category NVARCHAR(50),           -- FELLOW
    SubCategory NVARCHAR(50),        -- Payment
    Year NVARCHAR(4),                -- 2024
    Sequential_Num INT,              -- 499
    Full_Prefix NVARCHAR(100),       -- FA/FELLOW/Payment
    INDEX IX_Dept (Dept_Code),
    INDEX IX_Category (Category),
    INDEX IX_Year (Year),
    INDEX IX_Prefix (Full_Prefix)
)
```

**Searching**:
```sql
-- Search by department
SELECT * FROM M_File mf
JOIN File_SearchIndex fsi ON mf.File_Code = fsi.File_Code_Encrypted
WHERE fsi.Dept_Code = 'FA'

-- Search by year
WHERE fsi.Year = '2024'

-- Search by prefix
WHERE fsi.Full_Prefix = 'FA/FELLOW/Payment'
```

---

## Implementation Priority

### Phase 1: Migration (Encrypt Data)
1. Run encryption migration
2. Disable LIKE-dependent features temporarily
3. Use exact match only

### Phase 2: Search Enhancement (Post-Migration)
1. Build search index table
2. Populate index from encrypted data
3. Update stored procedures
4. Re-enable search features

### Phase 3: Optimization
1. Add more searchable fields to index
2. Implement advanced filters
3. Add full-text search if needed

---

## Decision Matrix

| Scenario | Dataset Size | Search Frequency | Recommendation |
|----------|-------------|------------------|----------------|
| Admin searches | Any | Low | Option 1: Eliminate LIKE |
| User searches | < 10K rows | Medium | Option 1: Eliminate LIKE |
| User searches | > 10K rows | High | Option 2: Search Index |
| Report filters | Any | Low | Option 1: Dropdown with exact match |
| Prefix-only | Any | High | Option 3: Hybrid (if security allows) |

---

## Testing Checklist

After implementing LIKE alternative:

- [ ] Test exact match searches
- [ ] Test prefix searches (if applicable)
- [ ] Test year-based searches
- [ ] Test department-based searches
- [ ] Test performance with large datasets
- [ ] Test concurrent searches
- [ ] Verify search index maintenance (INSERT/UPDATE/DELETE)
- [ ] Test search result accuracy

---

## Conclusion

**Bottom Line**: 
- LIKE operations **cannot work** with encrypted data
- **Option 1** (Eliminate LIKE) is fastest to implement
- **Option 2** (Search Index) is best for production long-term
- Choose based on your specific usage patterns

**Action Items**:
1. Run the LIKE detection script on your database
2. Review each stored procedure with LIKE
3. Decide: Option 1, 2, or 3 for each use case
4. Implement chosen solution
5. Test thoroughly

---

**Document Version**: 1.0  
**Last Updated**: August 5, 2026  
**Status**: Ready for Review
