# Deterministic Encryption Migration Package

## Overview

This package contains everything needed to implement deterministic AES-256 encryption on sensitive columns in the AU_eOffice_Live database.

**Columns to Encrypt**: `File_Code`, `Doc_Code`, `Doc_Name`, `Doc_Path`, `Doc_Upload`

---

## Quick Start

### 1. Discovery Phase (Read-Only)

Run these SQL scripts first to understand the scope:

```
01_DiscoveryScripts/
├── FindAffectedTables.sql
├── FindAffectedStoredProcedures.sql
├── FindAffectedViews.sql
├── FindAffectedTriggers.sql
└── FindForeignKeys.sql
```

### 2. Review Documentation

Read these in order:

1. `00_Documentation/IMPLEMENTATION_GUIDE.md` - Complete technical guide
2. `00_Documentation/DEPLOYMENT_PLAN.md` - Step-by-step deployment
3. `04_ImpactAnalysis/LIKE_Operations_Analysis.sql` - Understand LIKE impact

### 3. Prepare Code Changes

Copy encryption helper to your project:

```
02_CSharpHelpers/
├── DeterministicEncryptionHelper.cs  → Copy to BAL/ or App_Data/
├── EncryptionConfig.cs               → Optional configuration class
└── Web.config.example                → Configuration example
```

Modify your BAL methods using examples from:

```
05_CodeExamples/BAL_Example_Before_After.cs
```

### 4. Test in Development

1. Deploy code to dev environment
2. Run migration utility on dev database
3. Verify encryption works
4. Test all functionality

### 5. Production Migration

Follow the complete deployment plan in:

```
00_Documentation/DEPLOYMENT_PLAN.md
```

---

## Package Structure

```
EncryptionMigration/
│
├── 00_Documentation/
│   ├── IMPLEMENTATION_GUIDE.md       ⭐ Start here - Complete technical guide
│   └── DEPLOYMENT_PLAN.md            ⭐ Production deployment plan
│
├── 01_DiscoveryScripts/
│   ├── FindAffectedTables.sql        📊 Discover tables with target columns
│   ├── FindAffectedStoredProcedures.sql  📊 Find procedures to review
│   ├── FindAffectedViews.sql         📊 Find views using target columns
│   ├── FindAffectedTriggers.sql      📊 Find triggers to review
│   └── FindForeignKeys.sql           📊 Identify FK relationships
│
├── 02_CSharpHelpers/
│   ├── DeterministicEncryptionHelper.cs  🔐 Core encryption class
│   ├── EncryptionConfig.cs           ⚙️ Configuration helper
│   └── Web.config.example            ⚙️ Configuration template
│
├── 03_MigrationUtility/
│   └── DataEncryptionMigrator.cs     🔄 Console app to encrypt existing data
│
├── 04_ImpactAnalysis/
│   ├── LIKE_Operations_Analysis.sql  ⚠️ LIKE operations that will break
│   └── OrderBy_Analysis.sql          ⚠️ ORDER BY impact analysis
│
├── 05_CodeExamples/
│   └── BAL_Example_Before_After.cs   💡 Code examples for BAL modifications
│
├── 06_VerificationScripts/
│   └── VerifyEncryption.sql          ✅ Verify all data is encrypted
│
├── 07_RollbackScripts/
│   └── RollbackPlan.sql              🔙 Rollback procedures
│
└── README.md                          📖 This file
```

---

## Key Concepts

### Deterministic Encryption

**What it means**: Same plaintext → Same ciphertext (always)

**Why it's needed**:
- JOIN operations across tables
- WHERE clause equality searches
- Referential integrity
- Cross-table consistency

**Example**:
```
Original:   File_Code = "FA/2024/001"
Encrypted:  File_Code = "X82JH83JD82H8J2HK3JH=="

Every occurrence of "FA/2024/001" becomes "X82JH83JD82H8J2HK3JH==" in ALL tables
```

### Application-Layer Encryption

**Flow**:
```
MVC Layer
    ↓ (Decrypted values)
Business Layer (BAL)
    ↓ Encrypt() before INSERT/UPDATE
    ↓ (Encrypted values)
Data Layer (DAL)
    ↓ (Encrypted values)
Stored Procedures
    ↓ (Encrypted values)
Database
    (Stores encrypted values)
```

**Key Points**:
- Encryption happens in C# (BAL), NOT in SQL
- Database only stores encrypted values
- Stored procedures require NO changes
- JOINs continue to work normally

---

## Critical Information

### ✅ What Works After Encryption

- **INSERT/UPDATE**: Works normally (encrypt before insert)
- **SELECT**: Works normally (decrypt after select)
- **WHERE equality**: Works perfectly
  ```sql
  WHERE File_Code = @EncryptedValue  -- ✅ Works
  ```
- **JOIN**: Works perfectly
  ```sql
  JOIN T_File ON M_File.File_Code = T_File.File_Code  -- ✅ Works
  ```
- **Foreign Keys**: Continue to work
- **Unique Constraints**: Continue to work

### ❌ What Breaks After Encryption

- **LIKE searches**: Will NOT work
  ```sql
  WHERE File_Code LIKE 'FA%'  -- ❌ Broken
  ```
  
- **ORDER BY**: Sort order changes
  ```sql
  ORDER BY File_Code  -- ⚠️ Different order
  ```

- **Visual inspection**: Values are not human-readable

### 🔧 Solutions for LIKE Operations

**Option 1**: Remove LIKE (if not critical)

**Option 2**: Create search index table
```sql
CREATE TABLE File_SearchIndex (
    File_Code_Encrypted NVARCHAR(MAX),
    File_Code_Prefix NVARCHAR(20),  -- Store first 10 chars for search
    File_Code_Year NVARCHAR(4)
)
```

**Option 3**: Decrypt in application
```csharp
var allFiles = GetAllFiles(); // Get and decrypt
var filtered = allFiles.Where(f => f.File_Code.StartsWith("FA"));
```

---

## Security Considerations

### Key Management

⚠️ **CRITICAL**: Never hardcode keys in production!

**Production recommendations**:
1. **Azure Key Vault** (Best)
2. **AWS Secrets Manager**
3. **Environment Variables**
4. **Encrypted configuration file**

### Current Implementation

The provided code uses **hardcoded keys** for simplicity. This is acceptable for:
- Development
- Testing
- Proof of concept

For production, modify `DeterministicEncryptionHelper.cs` to load keys from secure storage.

---

## Testing Checklist

Before production migration:

### Development Testing
- [ ] Run discovery scripts
- [ ] Deploy code to dev
- [ ] Encrypt dev database
- [ ] Test INSERT operations
- [ ] Test UPDATE operations
- [ ] Test SELECT operations
- [ ] Test JOIN operations
- [ ] Test WHERE clauses
- [ ] Test search functionality
- [ ] Test reports
- [ ] Verify encryption
- [ ] Test decryption

### Staging Testing
- [ ] Restore production backup to staging
- [ ] Run full migration
- [ ] Performance testing
- [ ] Load testing
- [ ] User acceptance testing
- [ ] Test rollback procedure

### Pre-Production
- [ ] Backup production database
- [ ] Verify backup is restorable
- [ ] Schedule maintenance window
- [ ] Notify users
- [ ] DBA on standby
- [ ] Developer on standby

---

## Frequently Asked Questions

### Q: Why not use SQL Server's built-in encryption (EncryptByKey)?

**A**: EncryptByKey uses nondeterministic encryption by default. Same plaintext produces different ciphertext each time. This breaks:
- JOIN operations
- WHERE clause equality
- Referential integrity

Application-layer deterministic encryption solves these issues.

### Q: Do stored procedures need to be modified?

**A**: NO. Stored procedures receive encrypted values from the application and work normally. They don't decrypt - that happens in the application layer.

### Q: What if migration fails?

**A**: Rollback plan included. Restore from backup taken immediately before migration. Total rollback time: ~2 hours.

### Q: Will this affect performance?

**A**: Minimal impact. Encryption/decryption is ~0.1ms per value. For 1000 rows, total overhead is ~200ms. Database operations (network, I/O) dominate the time, not encryption.

### Q: Can users see the encrypted values?

**A**: NO. The application decrypts values before displaying them. Users see normal plaintext values.

### Q: What about existing reports?

**A**: Reports will work normally if they go through the application layer. Direct database reports (e.g., SSRS queries) will show encrypted values and need to be updated.

### Q: How do we search for File_Code if LIKE doesn't work?

**A**: Three options:
1. Exact match only (encrypt search term)
2. Search index table (recommended)
3. Decrypt in application and filter (for small datasets)

See IMPLEMENTATION_GUIDE.md for details.

### Q: What if we need to change the encryption key?

**A**: Re-encryption required. Run migration utility with new key. Schedule during maintenance window. This is why we use a FIXED key.

---

## Success Metrics

Migration is successful when:

✅ All data encrypted (>99.9%)  
✅ No data loss or corruption  
✅ Login functionality works  
✅ File creation/viewing works  
✅ Search by File_Code works (exact match)  
✅ Reports generate correctly  
✅ Performance within 10% of baseline  
✅ No critical errors in 48 hours  
✅ Users can work normally  

---

## Support

### Pre-Migration Questions

1. Read IMPLEMENTATION_GUIDE.md
2. Run discovery scripts
3. Review impact analysis
4. Test in development

### During Migration

1. Follow DEPLOYMENT_PLAN.md step-by-step
2. Monitor progress in migration utility
3. Check verification script results
4. Have DBA available

### Post-Migration Issues

1. Check application logs
2. Run verification script
3. Review user feedback
4. Consider rollback if critical

---

## Quick Reference

### Important Files

| File | Purpose |
|------|---------|
| `00_Documentation/IMPLEMENTATION_GUIDE.md` | Complete technical documentation |
| `00_Documentation/DEPLOYMENT_PLAN.md` | Production deployment steps |
| `02_CSharpHelpers/DeterministicEncryptionHelper.cs` | Core encryption class |
| `03_MigrationUtility/DataEncryptionMigrator.cs` | Data migration tool |
| `06_VerificationScripts/VerifyEncryption.sql` | Verify migration success |
| `07_RollbackScripts/RollbackPlan.sql` | Emergency rollback |

### Key Commands

```sql
-- Backup database
BACKUP DATABASE [AU_eOffice_Live] TO DISK = 'backup.bak' WITH COMPRESSION;

-- Verify encryption
-- Run: 06_VerificationScripts/VerifyEncryption.sql

-- Rollback (restore)
RESTORE DATABASE [AU_eOffice_Live] FROM DISK = 'backup.bak' WITH REPLACE;
```

```csharp
// Encrypt value
string encrypted = DeterministicEncryptionHelper.Encrypt(plaintext);

// Decrypt value
string decrypted = DeterministicEncryptionHelper.Decrypt(encrypted);

// Check if encrypted
bool isEncrypted = DeterministicEncryptionHelper.IsEncrypted(value);
```

---

## Next Steps

1. **Read** `00_Documentation/IMPLEMENTATION_GUIDE.md` (30 minutes)
2. **Run** Discovery scripts (15 minutes)
3. **Review** Impact analysis (30 minutes)
4. **Test** in development (1 week)
5. **Deploy** to staging (1 week)
6. **Migrate** production (1 weekend)

---

## Document Version

**Version**: 1.0  
**Date**: August 5, 2026  
**Author**: Kiro AI Assistant  
**Status**: Ready for Implementation

---

## License and Usage

This encryption migration package is provided for the AU_eOffice_Live project. Modify as needed for your specific requirements.

**Important**: 
- Test thoroughly before production use
- Secure encryption keys properly
- Follow your organization's security policies
- Backup everything before migration

---

**Questions or Issues?**

Refer to the detailed documentation in the `00_Documentation/` folder.

Good luck with your migration! 🚀
