# Encryption Migration Deployment Plan

## Executive Summary

This document outlines the complete deployment plan for implementing deterministic AES-256 encryption on sensitive columns in the AU_eOffice_Live database.

**Target Columns**: File_Code, Doc_Code, Doc_Name, Doc_Path, Doc_Upload  
**Encryption Method**: Application-layer (C#) deterministic AES-256  
**Estimated Duration**: 4-6 hours for production migration  
**Risk Level**: Medium (Full backup and rollback plan in place)

---

## Timeline Overview

| Phase | Duration | Description |
|-------|----------|-------------|
| **Week 1** | 5 days | Discovery and Impact Analysis |
| **Week 2** | 5 days | Development and Code Changes |
| **Week 3** | 5 days | Testing in Development/Staging |
| **Week 4** | Weekend | Production Migration |
| **Week 5** | 5 days | Monitoring and Optimization |

---

## Week 1: Discovery and Impact Analysis

### Day 1-2: Database Analysis

**Objective**: Understand complete scope of changes

**Tasks**:
1. Run all discovery scripts
   ```
   01_DiscoveryScripts/FindAffectedTables.sql
   01_DiscoveryScripts/FindAffectedStoredProcedures.sql
   01_DiscoveryScripts/FindAffectedViews.sql
   01_DiscoveryScripts/FindAffectedTriggers.sql
   01_DiscoveryScripts/FindForeignKeys.sql
   ```

2. Generate impact report
   - Count of affected tables
   - Count of affected stored procedures
   - List of LIKE operations
   - List of ORDER BY operations
   - Foreign key dependencies

3. Review results with team

**Deliverables**:
- [ ] Impact Analysis Report (PDF/Word)
- [ ] List of affected tables (Excel)
- [ ] List of stored procedures requiring review
- [ ] Risk assessment document

### Day 3-4: LIKE and ORDER BY Analysis

**Objective**: Identify operations that will break

**Tasks**:
1. Run impact analysis scripts
   ```
   04_ImpactAnalysis/LIKE_Operations_Analysis.sql
   04_ImpactAnalysis/OrderBy_Analysis.sql
   ```

2. Review each stored procedure with LIKE
3. Document alternatives for LIKE operations
4. Document ORDER BY impact
5. Plan workarounds

**Deliverables**:
- [ ] LIKE Operations Report
- [ ] ORDER BY Impact Report
- [ ] Workaround Implementation Plan

### Day 5: Planning and Approval

**Objective**: Get stakeholder approval

**Tasks**:
1. Prepare presentation for stakeholders
2. Present findings and recommendations
3. Discuss risks and mitigation
4. Get approval to proceed
5. Schedule production migration window

**Deliverables**:
- [ ] Stakeholder Presentation
- [ ] Approved Migration Plan
- [ ] Scheduled Migration Window

---

## Week 2: Development and Code Changes

### Day 1: Setup and Configuration

**Objective**: Add encryption infrastructure

**Tasks**:
1. Add encryption helper classes to project
   - Copy `DeterministicEncryptionHelper.cs` to `App_Data/` or `BAL/`
   - Copy `EncryptionConfig.cs`

2. Generate encryption keys
   ```csharp
   // Run this once to generate keys
   byte[] key = new byte[32];
   byte[] iv = new byte[16];
   using (var rng = new RNGCryptoServiceProvider())
   {
       rng.GetBytes(key);
       rng.GetBytes(iv);
   }
   string keyBase64 = Convert.ToBase64String(key);
   string ivBase64 = Convert.ToBase64String(iv);
   ```

3. Update `Web.config`
   ```xml
   <appSettings>
     <add key="EnableEncryption" value="true" />
     <add key="EncryptionMigrationMode" value="true" />
     <add key="AesEncryptionKey" value="[generated key]" />
     <add key="AesEncryptionIV" value="[generated iv]" />
   </appSettings>
   ```

4. Commit to source control (dev branch)

**Deliverables**:
- [ ] Encryption helper classes added
- [ ] Configuration updated
- [ ] Keys securely stored
- [ ] Code committed to dev branch

### Day 2-3: Modify BAL Methods

**Objective**: Add encryption/decryption to all BAL methods

**Tasks**:
1. Identify all BAL methods handling target columns
   - UserBAL.cs
   - DocumentBAL.cs (if exists)
   - FileBAL.cs (if exists)
   - Others as needed

2. Modify INSERT methods (encrypt before DAL)
3. Modify UPDATE methods (encrypt before DAL)
4. Modify SELECT methods (decrypt after DAL)
5. Modify WHERE clause methods (encrypt parameters)

**Reference**: See `05_CodeExamples/BAL_Example_Before_After.cs`

**Deliverables**:
- [ ] All BAL methods modified
- [ ] Code review completed
- [ ] Unit tests written

### Day 4: Create Migration Utility

**Objective**: Build data migration tool

**Tasks**:
1. Create console application project
2. Add `DataEncryptionMigrator.cs`
3. Add configuration for connection string
4. Test with sample database
5. Build release version

**Deliverables**:
- [ ] Migration utility built
- [ ] Tested in dev environment
- [ ] Executable ready for production

### Day 5: Code Review and Documentation

**Objective**: Ensure quality and document changes

**Tasks**:
1. Peer code review
2. Security review of encryption implementation
3. Document all code changes
4. Update developer documentation
5. Create training materials for team

**Deliverables**:
- [ ] Code review completed
- [ ] Security review passed
- [ ] Documentation updated
- [ ] Training materials prepared

---

## Week 3: Testing

### Day 1-2: Development Environment Testing

**Objective**: Test with sample data

**Tasks**:
1. Deploy code to dev environment
2. Run migration utility on dev database
3. Test all CRUD operations
4. Test search functionality
5. Test JOIN operations
6. Test reports

**Test Cases**:
- [ ] Login functionality
- [ ] Create new file
- [ ] Create new document
- [ ] Search by File_Code
- [ ] Search by Doc_Code
- [ ] View file history
- [ ] Forward file
- [ ] Approve file
- [ ] Generate reports
- [ ] Admin dashboard

### Day 3-4: Staging Environment Testing

**Objective**: Test with production-like data

**Tasks**:
1. Restore production backup to staging
2. Deploy code to staging
3. Run migration utility
4. Verify encryption
5. Full regression testing
6. Performance testing

**Performance Benchmarks**:
- [ ] Login time < 2 seconds
- [ ] File list load time < 3 seconds
- [ ] Search response time < 2 seconds
- [ ] Report generation < 5 seconds

### Day 5: User Acceptance Testing

**Objective**: Get user approval

**Tasks**:
1. Setup UAT environment
2. Provide access to test users
3. User testing sessions
4. Collect feedback
5. Fix any issues found

**Deliverables**:
- [ ] UAT Test Results
- [ ] User Sign-off
- [ ] Bug Fixes (if any)

---

## Week 4: Production Migration

### Pre-Migration Checklist (Friday 5:00 PM)

**Objective**: Prepare for migration

**Tasks**:
- [ ] Verify backup strategy
- [ ] Notify users of maintenance window
- [ ] Prepare rollback plan
- [ ] Review migration steps
- [ ] DBA on standby
- [ ] Developer on standby
- [ ] Test environment available
- [ ] Communication plan ready

### Migration Window (Friday 6:00 PM - Saturday 2:00 AM)

#### Phase 1: Preparation (6:00 PM - 6:30 PM)

**Tasks**:
1. [ ] Stop IIS / Application Pool
2. [ ] Verify all users logged out
3. [ ] Check database connections (should be zero)
4. [ ] Verify backup exists and is valid

#### Phase 2: Backup (6:30 PM - 7:00 PM)

**Tasks**:
1. [ ] Take full database backup
   ```sql
   BACKUP DATABASE [AU_eOffice_Live] 
   TO DISK = 'D:\Backups\AU_eOffice_Live_PreEncryption_20260805.bak'
   WITH COMPRESSION, INIT, STATS = 10;
   ```

2. [ ] Verify backup completed successfully
   ```sql
   RESTORE VERIFYONLY 
   FROM DISK = 'D:\Backups\AU_eOffice_Live_PreEncryption_20260805.bak';
   ```

3. [ ] Copy backup to secondary location
4. [ ] Document backup file details (size, location, timestamp)

#### Phase 3: Code Deployment (7:00 PM - 7:30 PM)

**Tasks**:
1. [ ] Deploy new application code
2. [ ] Update Web.config
   ```xml
   <add key="EnableEncryption" value="true" />
   <add key="EncryptionMigrationMode" value="true" />
   ```
3. [ ] Verify deployment (files, permissions)
4. [ ] DO NOT start IIS yet

#### Phase 4: Data Migration (7:30 PM - 11:00 PM)

**Tasks**:
1. [ ] Run migration utility
   ```
   DataEncryptionMigrator.exe
   ```

2. [ ] Monitor progress
   - Check console output
   - Monitor database CPU/memory
   - Watch for errors

3. [ ] Document migration statistics
   - Tables processed
   - Rows encrypted
   - Time taken
   - Any errors

**Expected Duration**: 3-4 hours for lakhs of records

#### Phase 5: Verification (11:00 PM - 11:30 PM)

**Tasks**:
1. [ ] Run verification script
   ```sql
   -- Run: 06_VerificationScripts/VerifyEncryption.sql
   ```

2. [ ] Check sample data
   ```sql
   SELECT TOP 100 File_Code FROM M_File;
   SELECT TOP 100 Doc_Code FROM M_Document;
   ```

3. [ ] Verify all values are Base64 format
4. [ ] Check for any remaining plaintext

#### Phase 6: Application Testing (11:30 PM - 12:30 AM)

**Tasks**:
1. [ ] Update Web.config
   ```xml
   <add key="EncryptionMigrationMode" value="false" />
   ```

2. [ ] Start IIS / Application Pool

3. [ ] Smoke testing
   - [ ] Login as admin
   - [ ] View dashboard
   - [ ] Create test file
   - [ ] Search by File_Code
   - [ ] View file details
   - [ ] Create test document
   - [ ] Forward file (test workflow)

4. [ ] Check application logs for errors

#### Phase 7: Final Verification (12:30 AM - 1:00 AM)

**Tasks**:
1. [ ] Run comprehensive tests
2. [ ] Verify JOIN operations work
3. [ ] Verify search functionality
4. [ ] Check report generation
5. [ ] Monitor database performance

#### Phase 8: Go Live (1:00 AM)

**Tasks**:
1. [ ] Enable application for users
2. [ ] Send notification to users
3. [ ] Monitor for 1 hour
4. [ ] Document any issues

### Post-Migration (Saturday 2:00 AM)

**Tasks**:
1. [ ] Send completion notification
2. [ ] Document migration results
3. [ ] Keep monitoring enabled
4. [ ] Schedule follow-up for Monday

---

## Week 5: Monitoring and Optimization

### Day 1-2: Intensive Monitoring

**Objective**: Catch issues early

**Tasks**:
1. Monitor application logs hourly
2. Check database performance metrics
3. Review user feedback
4. Track any errors
5. Measure response times

**Metrics to Track**:
- Login success rate
- Page load times
- Database query performance
- Error rates
- User complaints

### Day 3-4: Optimization

**Objective**: Fine-tune performance

**Tasks**:
1. Identify slow queries
2. Add indexes if needed
3. Optimize decryption logic
4. Cache frequently accessed data
5. Update statistics

### Day 5: Review and Documentation

**Objective**: Close out migration

**Tasks**:
1. Conduct post-implementation review
2. Document lessons learned
3. Update runbooks
4. Archive migration artifacts
5. Close migration project

---

## Rollback Procedures

### When to Rollback

Rollback if:
- Migration fails partway through
- Data corruption detected
- Critical functionality broken
- Performance degradation > 50%
- Business-critical errors

### Rollback Steps

1. **Stop Application** (5 minutes)
   ```
   iisreset /stop
   ```

2. **Restore Database** (30-60 minutes)
   ```sql
   ALTER DATABASE [AU_eOffice_Live] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   
   RESTORE DATABASE [AU_eOffice_Live]
   FROM DISK = 'D:\Backups\AU_eOffice_Live_PreEncryption_20260805.bak'
   WITH REPLACE, RECOVERY;
   
   ALTER DATABASE [AU_eOffice_Live] SET MULTI_USER;
   ```

3. **Revert Code** (10 minutes)
   - Restore previous application version
   - Update Web.config: `EnableEncryption = false`

4. **Test and Verify** (30 minutes)
   - Login test
   - Create file test
   - Search test

5. **Go Live** (5 minutes)
   ```
   iisreset /start
   ```

6. **Notify Users** (10 minutes)

**Total Rollback Time**: ~2 hours

---

## Communication Plan

### Pre-Migration Communications

**1 Week Before**:
- [ ] Email to all users announcing maintenance
- [ ] Post notice on application dashboard
- [ ] Send reminder to department heads

**1 Day Before**:
- [ ] Email reminder with exact timing
- [ ] Post on internal communication channels
- [ ] Reminder to save work before maintenance

**Migration Day**:
- [ ] Email 2 hours before shutdown
- [ ] Application banner 1 hour before
- [ ] Final warning 15 minutes before

### During Migration

- [ ] Status updates every hour (email/SMS to stakeholders)
- [ ] Immediate notification if issues occur

### Post-Migration

**Success**:
- [ ] Email notification that system is available
- [ ] Thank users for patience
- [ ] Provide feedback channel

**Issues/Rollback**:
- [ ] Immediate notification with explanation
- [ ] Revised timeline
- [ ] Apology and next steps

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Data corruption | Low | Critical | Full backup, tested restore |
| Migration fails | Medium | High | Rollback plan, staging testing |
| Performance issues | Medium | Medium | Performance testing, optimization |
| LIKE queries break | High | Low | Alternative search methods planned |
| User confusion | Low | Low | Documentation, training |
| Rollback needed | Low | High | Tested rollback procedure |

---

## Success Criteria

Migration is successful if:
- [ ] All data encrypted (>99.9%)
- [ ] No data loss
- [ ] All critical workflows functional
- [ ] Performance within acceptable limits
- [ ] No critical errors in 48 hours
- [ ] User acceptance

---

## Contacts

| Role | Name | Phone | Email |
|------|------|-------|-------|
| Project Lead | [Name] | [Phone] | [Email] |
| Database Administrator | [Name] | [Phone] | [Email] |
| Application Developer | [Name] | [Phone] | [Email] |
| Infrastructure Lead | [Name] | [Phone] | [Email] |
| Security Officer | [Name] | [Phone] | [Email] |
| Business Sponsor | [Name] | [Phone] | [Email] |

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-08-05 | Kiro AI Assistant | Initial deployment plan |

---

## Appendix A: Command Reference

### Useful SQL Commands

```sql
-- Check database size
EXEC sp_spaceused;

-- Check table row counts
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.OBJECT_ID
WHERE t.NAME IN ('M_File', 'M_Document', 'T_File', 'T_Document')
AND p.index_id < 2
ORDER BY TableName;

-- Check for blocking sessions
SELECT * FROM sys.dm_exec_requests WHERE blocking_session_id > 0;

-- Kill all connections (for restore)
ALTER DATABASE [AU_eOffice_Live] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
```

### Useful PowerShell Commands

```powershell
# Stop IIS
iisreset /stop

# Start IIS
iisreset /start

# Check application pool status
Get-WebAppPoolState -Name "EofficeAppPool"

# Restart application pool
Restart-WebAppPool -Name "EofficeAppPool"
```

---

**END OF DEPLOYMENT PLAN**
