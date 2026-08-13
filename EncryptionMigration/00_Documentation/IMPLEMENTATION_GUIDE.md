# Deterministic Encryption Implementation Guide

## Overview

This guide covers the implementation of **deterministic AES-256 encryption** for sensitive columns in the AU_eOffice_Live database. The solution encrypts data at the **application layer (C# Business Layer)** rather than using SQL Server's built-in encryption.

---

## Target Columns

The following columns must be encrypted across **all tables**:

- `File_Code`
- `Doc_Code`
- `Doc_Name`
- `Doc_Path`
- `Doc_Upload`

---

## Encryption Approach

### Why Deterministic Encryption?

**Deterministic encryption** means that the same plaintext always produces the same ciphertext. This is required because:

1. **Cross-table consistency**: `File_Code` and `Doc_Code` appear in multiple tables
2. **JOIN operations**: Tables are joined on these columns
3. **WHERE clause equality**: Exact match searches must continue to work
4. **Referential integrity**: Same value must produce same encrypted output everywhere

### Technical Specifications

- **Algorithm**: AES-256-CBC
- **Key**: 256-bit (32 bytes) - FIXED across all environments
- **IV**: 128-bit (16 bytes) - FIXED for deterministic output
- **Encoding**: UTF-8 input, Base64 output
- **Mode**: CBC (Cipher Block Chaining)
- **Padding**: PKCS7

### Security Considerations

⚠️ **Important Trade-offs**:

1. **Fixed IV**: Required for deterministic encryption but reduces security
2. **Pattern Analysis**: Same values produce same ciphertext (visible patterns)
3. **No LIKE support**: Encrypted values cannot be pattern-matched
4. **Sort order changes**: ORDER BY on encrypted columns produces different results

These trade-offs are **acceptable** because:
- Data is protected at rest
- Application layer controls access
- JOIN and equality operations continue to work
- Alternative solutions for LIKE/ORDER BY are provided

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         MVC Layer                               │
│  (User Interface - Receives/Displays Decrypted Data)            │
└─────────────────┬───────────────────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Business Layer (BAL)                         │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  DeterministicEncryptionHelper                           │   │
│  │  • Encrypt() - Before INSERT/UPDATE                      │   │
│  │  • Decrypt() - After SELECT                              │   │
│  │  • IsEncrypted() - Check if already encrypted            │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────┬───────────────────────────────────────────────┘
                  │
                  ↓ (Encrypted values only)
┌─────────────────────────────────────────────────────────────────┐
│                   Data Layer (DAL)                              │
│  (Passes encrypted values to stored procedures)                 │
└─────────────────┬───────────────────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────────────────┐
│                  Stored Procedures                              │
│  (No changes required - works with encrypted values)            │
└─────────────────┬───────────────────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Database                                   │
│  (Stores encrypted values in existing columns)                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Database Changes

### ✅ What DOES NOT Change

1. **Column names**: File_Code, Doc_Code, etc. remain the same
2. **Column data types**: nvarchar(max), nvarchar(100), etc. remain unchanged
3. **Table structure**: No new columns, no schema modifications
4. **JOIN conditions**: JOIN clauses remain unchanged
5. **WHERE clauses**: Equality comparisons work the same way
6. **Foreign keys**: FK relationships remain intact
7. **Stored procedures**: Parameter names and logic stay the same

### ⚠️ What DOES Change

1. **Column values**: Plaintext → Base64 encrypted strings
2. **Column length**: Encrypted values are longer (factor of ~1.4x)
3. **LIKE operations**: Pattern matching will NOT work on encrypted data
4. **ORDER BY**: Sort order will be different (Base64 alphabetical, not logical)
5. **Visual inspection**: Values are no longer human-readable

---

## Application Changes

### 1. Business Layer (BAL)

**Every BAL method that handles target columns must:**

#### INSERT/UPDATE Operations
```csharp
// BEFORE (Plaintext)
public void AddDocument(ModelAddDocument model)
{
    string docCode = model.Doc_Code;  // "DOC/2024/001"
    string fileCode = model.File_Code; // "FA/2024/001"
    // ... save to DAL
}

// AFTER (Encrypted)
public void AddDocument(ModelAddDocument model)
{
    // Encrypt before passing to DAL
    string docCode = DeterministicEncryptionHelper.Encrypt(model.Doc_Code);
    string fileCode = DeterministicEncryptionHelper.Encrypt(model.File_Code);
    // ... save to DAL with encrypted values
}
```

#### SELECT Operations
```csharp
// BEFORE (Plaintext)
public List<ModelDocument> GetDocuments()
{
    var documents = DAL.GetDocuments();
    return documents; // Return as-is
}

// AFTER (Encrypted)
public List<ModelDocument> GetDocuments()
{
    var documents = DAL.GetDocuments(); // Returns encrypted values
    
    // Decrypt before returning to MVC
    foreach (var doc in documents)
    {
        doc.Doc_Code = DeterministicEncryptionHelper.Decrypt(doc.Doc_Code);
        doc.File_Code = DeterministicEncryptionHelper.Decrypt(doc.File_Code);
        doc.Doc_Name = DeterministicEncryptionHelper.Decrypt(doc.Doc_Name);
        doc.Doc_Path = DeterministicEncryptionHelper.Decrypt(doc.Doc_Path);
        doc.Doc_Upload = DeterministicEncryptionHelper.Decrypt(doc.Doc_Upload);
    }
    
    return documents;
}
```

#### WHERE Clause with User Input
```csharp
// BEFORE (Plaintext)
public ModelDocument GetDocumentByCode(string docCode)
{
    return DAL.GetDocumentByCode(docCode); // Pass plaintext
}

// AFTER (Encrypted)
public ModelDocument GetDocumentByCode(string docCode)
{
    // Encrypt user input before passing to DAL
    string encryptedCode = DeterministicEncryptionHelper.Encrypt(docCode);
    var doc = DAL.GetDocumentByCode(encryptedCode);
    
    // Decrypt result
    if (doc != null)
    {
        doc.Doc_Code = DeterministicEncryptionHelper.Decrypt(doc.Doc_Code);
        // ... decrypt other fields
    }
    
    return doc;
}
```

### 2. Data Layer (DAL)

**DAL requires NO changes** if you follow the pattern above:
- BAL encrypts before calling DAL
- DAL passes encrypted values to stored procedures
- DAL returns encrypted values to BAL
- BAL decrypts before returning to MVC

### 3. Model Classes

**Models require NO changes**:
- Models still use `string` types
- Encryption/decryption happens in BAL, not in models

---

## Migration Process

### Phase 1: Discovery (Read-Only)

Run discovery scripts to understand impact:

```bash
# Run these scripts in SQL Server Management Studio
01_DiscoveryScripts/FindAffectedTables.sql
01_DiscoveryScripts/FindAffectedStoredProcedures.sql
01_DiscoveryScripts/FindAffectedViews.sql
01_DiscoveryScripts/FindAffectedTriggers.sql
01_DiscoveryScripts/FindForeignKeys.sql
```

**Output**: Impact report showing:
- Number of tables affected
- Number of stored procedures affected
- LIKE operations that will break
- ORDER BY operations that will change
- Foreign key relationships

### Phase 2: Code Preparation

1. **Add encryption helper class**:
   - Copy `DeterministicEncryptionHelper.cs` to `App_Data/` or `BAL/`
   - Copy `EncryptionConfig.cs` if using config-based keys

2. **Update Web.config**:
   ```xml
   <appSettings>
     <add key="EnableEncryption" value="true" />
     <add key="EncryptionMigrationMode" value="true" />
   </appSettings>
   ```

3. **Modify BAL methods**:
   - Identify all methods in `UserBAL.cs` and similar files
   - Add encryption/decryption calls as shown above

4. **Test in development**:
   - Test with small dataset
   - Verify INSERT/UPDATE/SELECT operations
   - Check JOIN operations
   - Test WHERE clauses

### Phase 3: Data Migration (Production)

1. **Backup database**:
   ```sql
   BACKUP DATABASE [AU_eOffice_Live] 
   TO DISK = 'D:\Backups\AU_eOffice_Live_PreEncryption.bak'
   WITH COMPRESSION, INIT;
   ```

2. **Enable migration mode**:
   ```xml
   <add key="EncryptionMigrationMode" value="true" />
   ```

3. **Run migration utility**:
   ```bash
   DataEncryptionMigrator.exe
   ```

   This tool:
   - Discovers all tables automatically
   - Encrypts data in batches (1000 rows at a time)
   - Skips already-encrypted values (idempotent)
   - Uses transactions for safety
   - Shows progress in real-time

4. **Verify encryption**:
   ```bash
   DataEncryptionMigrator.exe --verify
   ```

5. **Disable migration mode**:
   ```xml
   <add key="EncryptionMigrationMode" value="false" />
   ```

### Phase 4: Validation

1. **Functional testing**:
   - Login
   - Create file/document
   - Search by File_Code/Doc_Code
   - View file history
   - Test all major workflows

2. **Data validation**:
   ```sql
   -- Check that values are encrypted (Base64 format)
   SELECT TOP 100 File_Code, Doc_Code FROM M_File
   SELECT TOP 100 Doc_Code, Doc_Upload FROM M_Document
   ```

3. **Performance testing**:
   - Compare query execution times
   - Monitor encryption/decryption overhead
   - Check index usage

### Phase 5: Rollback Plan (If Needed)

If issues occur, you can rollback:

```sql
-- Restore from backup
RESTORE DATABASE [AU_eOffice_Live]
FROM DISK = 'D:\Backups\AU_eOffice_Live_PreEncryption.bak'
WITH REPLACE;
```

Then:
1. Revert code changes (remove encryption calls)
2. Set `EnableEncryption = false`
3. Redeploy application

---

## Handling LIKE Operations

### Problem

```sql
-- This will NOT work after encryption:
SELECT * FROM M_File WHERE File_Code LIKE 'FA%'
```

Because encrypted File_Code looks like: `X82JH83JD82H8J2HK3JH==`

### Solution Options

#### Option 1: Remove LIKE (Simplest)

If LIKE searches are rare or non-critical, remove them:

```csharp
// Instead of:
var files = GetFiles("FA%"); // LIKE search

// Use:
var allFiles = GetFiles();
var filtered = allFiles.Where(f => f.File_Code.StartsWith("FA"));
```

#### Option 2: Search Index Table

Create a separate searchable index:

```sql
CREATE TABLE File_SearchIndex (
    File_Code_Encrypted NVARCHAR(MAX),
    File_Code_Prefix NVARCHAR(20),  -- First 10 chars for searching
    File_Code_Year NVARCHAR(4),      -- Extract year
    -- Other searchable fields
)

-- Search using:
SELECT * FROM M_File mf
JOIN File_SearchIndex fsi ON mf.File_Code = fsi.File_Code_Encrypted
WHERE fsi.File_Code_Prefix LIKE 'FA%'
```

#### Option 3: Decrypt in Application

For small result sets:

```csharp
public List<ModelFile> SearchFiles(string pattern)
{
    var allFiles = GetAllFiles(); // Get all, decrypt
    return allFiles.Where(f => f.File_Code.Contains(pattern)).ToList();
}
```

**Recommendation**: Use Option 2 for large datasets, Option 3 for small/infrequent searches.

---

## Handling ORDER BY

### Problem

```sql
-- Sort order will change after encryption:
SELECT * FROM M_File ORDER BY File_Code
```

Original order:
- FA/2024/001
- FA/2024/002
- FA/2024/010

Encrypted order (Base64 alphabetical):
- X82JH83JD...
- A12KL34MN...
- Z98QW76ER...

### Solution Options

#### Option 1: Add Sort Column

```sql
ALTER TABLE M_File ADD File_Code_Sort INT

-- Populate with sequence
UPDATE M_File SET File_Code_Sort = Row_ID

-- Use for sorting
SELECT * FROM M_File ORDER BY File_Code_Sort
```

#### Option 2: Sort in Application

```csharp
public List<ModelFile> GetFilesSorted()
{
    var files = GetFiles(); // Get encrypted, decrypt in BAL
    return files.OrderBy(f => f.File_Code).ToList();
}
```

**Recommendation**: Use Option 1 for large datasets, Option 2 for small result sets.

---

## Performance Considerations

### Encryption Overhead

- **Encryption**: ~0.1ms per value (negligible)
- **Decryption**: ~0.1ms per value (negligible)
- **Batch operations**: 1000 rows in ~1 second

### Recommendations

1. **Batch decryption**: Decrypt in loops, not one-at-a-time
2. **Cache frequently accessed data**: Cache decrypted lookups
3. **Index on encrypted columns**: Create indexes if needed
4. **Monitor query plans**: Ensure indexes are used

---

## Security Best Practices

### Key Management

⚠️ **CRITICAL**: Never hardcode keys in production!

**Development**:
```csharp
// Hardcoded keys (acceptable for dev)
private static readonly byte[] AES_KEY = new byte[32] { ... };
```

**Production** (Choose one):

1. **Azure Key Vault**:
   ```csharp
   var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
   var secret = client.GetSecret("AesEncryptionKey");
   byte[] key = Convert.FromBase64String(secret.Value);
   ```

2. **AWS Secrets Manager**:
   ```csharp
   var client = new AmazonSecretsManagerClient();
   var request = new GetSecretValueRequest { SecretId = "AesEncryptionKey" };
   var response = await client.GetSecretValueAsync(request);
   byte[] key = Convert.FromBase64String(response.SecretString);
   ```

3. **Environment Variables**:
   ```csharp
   string keyBase64 = Environment.GetEnvironmentVariable("AES_KEY");
   byte[] key = Convert.FromBase64String(keyBase64);
   ```

### Access Control

1. **Restrict encryption helper**: Only BAL should call encryption methods
2. **Audit logging**: Log encryption/decryption operations
3. **Rotate keys**: Plan for key rotation (requires re-encryption)

---

## Testing Checklist

### Unit Tests

- [ ] Encrypt/decrypt round-trip
- [ ] Null/empty string handling
- [ ] IsEncrypted() accuracy
- [ ] Deterministic output (same input → same output)

### Integration Tests

- [ ] INSERT with encrypted values
- [ ] UPDATE encrypted values
- [ ] SELECT and decrypt
- [ ] WHERE clause with encrypted comparison
- [ ] JOIN on encrypted columns
- [ ] Foreign key integrity

### User Acceptance Tests

- [ ] Create file/document
- [ ] Search by File_Code/Doc_Code
- [ ] View file details
- [ ] Forward file
- [ ] Generate reports
- [ ] Admin dashboard

---

## Deployment Schedule

### Week 1: Discovery & Planning
- Run discovery scripts
- Review impact report
- Identify LIKE/ORDER BY issues
- Plan fixes

### Week 2: Development
- Add encryption helper
- Modify BAL methods
- Update models if needed
- Write unit tests

### Week 3: Testing
- Test in dev environment
- Migrate dev database
- Run integration tests
- Performance testing

### Week 4: Production Migration
- **Friday evening** (low traffic):
  1. Backup database
  2. Deploy code (migration mode ON)
  3. Run migration utility
  4. Verify encryption
  5. Disable migration mode
  6. Monitor for 2 hours

- **Monday**: Full validation and monitoring

---

## Troubleshooting

### Issue: "Decryption failed"

**Cause**: Data was encrypted with different key/IV

**Solution**:
1. Check that key/IV match across all environments
2. Verify data was actually encrypted (not corrupted)
3. Check for leading/trailing whitespace

### Issue: "JOIN returns no results"

**Cause**: One table has encrypted values, another has plaintext

**Solution**:
1. Verify all tables are migrated
2. Check migration log for errors
3. Re-run migration utility

### Issue: "Performance degradation"

**Cause**: Encryption overhead or missing indexes

**Solution**:
1. Create indexes on encrypted columns
2. Cache frequently accessed data
3. Batch decryption operations

---

## Support

For issues or questions:
1. Check this guide
2. Review impact analysis reports
3. Test in development first
4. Contact: [Your Team Contact Info]

---

## Appendix: SQL Queries

### Check Encryption Status

```sql
-- Sample encrypted values
SELECT TOP 10 File_Code FROM M_File

-- Check if values are Base64
SELECT File_Code,
       CASE 
         WHEN LEN(File_Code) % 4 = 0 AND File_Code LIKE '%==%' THEN 'Encrypted'
         ELSE 'Plaintext'
       END AS Status
FROM M_File
```

### Find Remaining Plaintext

```sql
SELECT COUNT(*) AS PlaintextCount
FROM M_File
WHERE File_Code IS NOT NULL
AND LEN(File_Code) % 4 <> 0  -- Base64 length is multiple of 4
```

---

**Document Version**: 1.0  
**Last Updated**: August 5, 2026  
**Prepared By**: Kiro AI Assistant
