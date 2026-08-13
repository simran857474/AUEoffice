-- =============================================
-- Script: Rollback Plan for Encryption Migration
-- Purpose: Steps to rollback if encryption causes issues
-- =============================================

USE [master]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'ENCRYPTION ROLLBACK PLAN'
PRINT '=========================================='
PRINT ''
PRINT 'WARNING: This script provides guidance for rollback.'
PRINT 'Review each step carefully before executing.'
PRINT ''
PRINT '=========================================='

-- ============================================================
-- OPTION 1: RESTORE FROM BACKUP (Recommended)
-- ============================================================

PRINT ''
PRINT 'OPTION 1: RESTORE FROM BACKUP (Recommended)'
PRINT '=============================================================='
PRINT ''
PRINT 'This is the SAFEST and FASTEST rollback method.'
PRINT ''
PRINT 'Prerequisites:'
PRINT '  - You must have a backup taken BEFORE encryption migration'
PRINT '  - Backup file location: D:\Backups\AU_eOffice_Live_PreEncryption.bak'
PRINT ''
PRINT 'Steps:'
PRINT '  1. Stop application (IIS)'
PRINT '  2. Close all database connections'
PRINT '  3. Restore database from backup'
PRINT '  4. Revert application code changes'
PRINT '  5. Update Web.config: EnableEncryption = false'
PRINT '  6. Start application'
PRINT ''
PRINT 'SQL Commands:'
PRINT '-----------------------------------------------------------'
PRINT ''

-- Show restore command (DO NOT EXECUTE automatically)
PRINT '-- Step 1: Set database to single user mode'
PRINT 'ALTER DATABASE [AU_eOffice_Live] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;'
PRINT 'GO'
PRINT ''
PRINT '-- Step 2: Restore from backup'
PRINT 'RESTORE DATABASE [AU_eOffice_Live]'
PRINT 'FROM DISK = ''D:\Backups\AU_eOffice_Live_PreEncryption.bak'''
PRINT 'WITH REPLACE, RECOVERY;'
PRINT 'GO'
PRINT ''
PRINT '-- Step 3: Set database back to multi-user'
PRINT 'ALTER DATABASE [AU_eOffice_Live] SET MULTI_USER;'
PRINT 'GO'
PRINT ''

-- ============================================================
-- OPTION 2: DECRYPT IN PLACE (Not Recommended - Complex)
-- ============================================================

PRINT ''
PRINT 'OPTION 2: DECRYPT IN PLACE (Not Recommended)'
PRINT '=============================================================='
PRINT ''
PRINT 'This option attempts to decrypt data in the database.'
PRINT 'WARNING: This is COMPLEX and ERROR-PRONE!'
PRINT ''
PRINT 'Why NOT recommended:'
PRINT '  - Cannot decrypt from SQL (encryption is in application)'
PRINT '  - Would require custom decryption utility'
PRINT '  - High risk of data corruption'
PRINT '  - Time-consuming for large datasets'
PRINT ''
PRINT 'If you MUST use this option:'
PRINT '  1. Create a C# console app with decryption logic'
PRINT '  2. Read encrypted values from database'
PRINT '  3. Decrypt using DeterministicEncryptionHelper.Decrypt()'
PRINT '  4. Update database with plaintext values'
PRINT '  5. Verify decryption'
PRINT ''

-- ============================================================
-- OPTION 3: PARTIAL ROLLBACK (Specific Tables Only)
-- ============================================================

PRINT ''
PRINT 'OPTION 3: PARTIAL ROLLBACK (Specific Tables)'
PRINT '=============================================================='
PRINT ''
PRINT 'Use this if only specific tables have issues.'
PRINT ''
PRINT 'Steps:'
PRINT '  1. Identify problematic tables'
PRINT '  2. Restore those tables from backup'
PRINT '  3. Keep other encrypted tables'
PRINT ''
PRINT 'Example: Restore M_File only'
PRINT '-----------------------------------------------------------'
PRINT ''

-- Example: Restore single table from backup
PRINT '-- This requires a separate table-level backup or'
PRINT '-- Extracting from full backup using third-party tools'
PRINT ''

-- ============================================================
-- PRE-ROLLBACK CHECKLIST
-- ============================================================

PRINT ''
PRINT 'PRE-ROLLBACK CHECKLIST'
PRINT '=============================================================='
PRINT ''
PRINT '□ Identify root cause of issue'
PRINT '□ Document what went wrong'
PRINT '□ Verify backup exists and is valid'
PRINT '□ Notify users of downtime'
PRINT '□ Stop application (IIS/services)'
PRINT '□ Export any new data created after encryption (if needed)'
PRINT '□ Test restore in dev/staging first (if possible)'
PRINT '□ Have application code rollback ready'
PRINT '□ Backup current state (encrypted) before rollback'
PRINT '□ Plan communication with users'
PRINT ''

-- ============================================================
-- POST-ROLLBACK CHECKLIST
-- ============================================================

PRINT ''
PRINT 'POST-ROLLBACK CHECKLIST'
PRINT '=============================================================='
PRINT ''
PRINT '□ Verify database is restored successfully'
PRINT '□ Check table row counts match backup'
PRINT '□ Verify data is in plaintext format'
PRINT '□ Update Web.config: EnableEncryption = false'
PRINT '□ Deploy rolled-back application code'
PRINT '□ Test critical workflows'
PRINT '□ Test user login'
PRINT '□ Test file/document creation'
PRINT '□ Test search functionality'
PRINT '□ Monitor application logs for errors'
PRINT '□ Notify users that system is restored'
PRINT '□ Schedule post-mortem meeting'
PRINT ''

-- ============================================================
-- VERIFICATION QUERIES AFTER ROLLBACK
-- ============================================================

PRINT ''
PRINT 'VERIFICATION QUERIES (Run After Rollback)'
PRINT '=============================================================='
PRINT ''

-- Check if data is plaintext
PRINT '-- Verify data is plaintext (not Base64)'
PRINT 'SELECT TOP 10 File_Code FROM M_File WHERE File_Code IS NOT NULL;'
PRINT 'SELECT TOP 10 Doc_Code FROM M_Document WHERE Doc_Code IS NOT NULL;'
PRINT ''

-- Check row counts
PRINT '-- Verify row counts match backup'
PRINT 'SELECT ''M_File'' AS TableName, COUNT(*) AS RowCount FROM M_File'
PRINT 'UNION ALL'
PRINT 'SELECT ''M_Document'', COUNT(*) FROM M_Document'
PRINT 'UNION ALL'
PRINT 'SELECT ''T_File'', COUNT(*) FROM T_File'
PRINT 'UNION ALL'
PRINT 'SELECT ''T_Document'', COUNT(*) FROM T_Document;'
PRINT ''

-- ============================================================
-- CONTACT INFORMATION
-- ============================================================

PRINT ''
PRINT 'EMERGENCY CONTACTS'
PRINT '=============================================================='
PRINT ''
PRINT 'Database Administrator: [DBA Contact]'
PRINT 'Application Team Lead: [Dev Lead Contact]'
PRINT 'Infrastructure Team: [Ops Contact]'
PRINT 'Vendor Support: [Vendor Contact if applicable]'
PRINT ''

-- ============================================================
-- LESSONS LEARNED / PREVENTION
-- ============================================================

PRINT ''
PRINT 'PREVENTION FOR FUTURE MIGRATIONS'
PRINT '=============================================================='
PRINT ''
PRINT '1. Test thoroughly in dev/staging with production-like data'
PRINT '2. Perform dry-run migrations multiple times'
PRINT '3. Verify backups are restorable BEFORE migration'
PRINT '4. Have automated rollback procedure'
PRINT '5. Schedule migration during low-traffic period'
PRINT '6. Have DBA and developer on standby during migration'
PRINT '7. Plan rollback time in migration window'
PRINT '8. Document all steps taken during migration'
PRINT '9. Create checkpoint backups during migration'
PRINT '10. Test rollback procedure in staging'
PRINT ''

-- ============================================================
-- EXAMPLE RESTORE SCRIPT (TEMPLATE)
-- ============================================================

PRINT ''
PRINT 'EXAMPLE RESTORE SCRIPT (Copy and customize)'
PRINT '=============================================================='
PRINT ''

PRINT '
/*
-- RESTORE SCRIPT TEMPLATE
-- Customize paths and database names as needed

USE master;
GO

-- Kill all connections
ALTER DATABASE [AU_eOffice_Live] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Restore database
RESTORE DATABASE [AU_eOffice_Live]
FROM DISK = ''D:\Backups\AU_eOffice_Live_PreEncryption.bak''
WITH 
    MOVE ''eOffice_Live'' TO ''D:\DB\AU_eOffice_Live.mdf'',
    MOVE ''eOffice_Live_log'' TO ''D:\DB\AU_eOffice_Live_log.ldf'',
    REPLACE,
    RECOVERY,
    STATS = 10;
GO

-- Set back to multi-user
ALTER DATABASE [AU_eOffice_Live] SET MULTI_USER;
GO

-- Verify restore
USE [AU_eOffice_Live];
GO

SELECT ''M_File'' AS TableName, COUNT(*) AS RowCount FROM M_File
UNION ALL
SELECT ''M_Document'', COUNT(*) FROM M_Document;
GO

PRINT ''Restore completed successfully!'';
*/
'

PRINT ''
PRINT '=============================================================='
PRINT 'END OF ROLLBACK PLAN'
PRINT '=============================================================='
PRINT ''
PRINT 'Remember: ALWAYS test rollback in staging before production!'
PRINT ''
