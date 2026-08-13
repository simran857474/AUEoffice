-- =============================================
-- Script: Verify Data Encryption Status
-- Purpose: Check that all target columns have been encrypted
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'ENCRYPTION VERIFICATION REPORT'
PRINT 'Generated: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '=========================================='
PRINT ''

-- Create temp table to store results
CREATE TABLE #EncryptionStatus (
    TableName NVARCHAR(128),
    ColumnName NVARCHAR(128),
    TotalRows INT,
    EncryptedRows INT,
    PlaintextRows INT,
    NullOrEmptyRows INT,
    EncryptionRate DECIMAL(5,2)
)

-- Dynamically check each table and column
DECLARE @TableName NVARCHAR(128)
DECLARE @ColumnName NVARCHAR(128)
DECLARE @SQL NVARCHAR(MAX)

DECLARE col_cursor CURSOR FOR
SELECT 
    t.TABLE_NAME,
    c.COLUMN_NAME
FROM 
    INFORMATION_SCHEMA.TABLES t
INNER JOIN 
    INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE 
    c.COLUMN_NAME IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
    AND t.TABLE_TYPE = 'BASE TABLE'
ORDER BY 
    t.TABLE_NAME, c.COLUMN_NAME

OPEN col_cursor
FETCH NEXT FROM col_cursor INTO @TableName, @ColumnName

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Build dynamic SQL to check encryption status
    SET @SQL = '
    INSERT INTO #EncryptionStatus (TableName, ColumnName, TotalRows, EncryptedRows, PlaintextRows, NullOrEmptyRows, EncryptionRate)
    SELECT 
        ''' + @TableName + ''',
        ''' + @ColumnName + ''',
        COUNT(*) AS TotalRows,
        SUM(CASE 
            WHEN [' + @ColumnName + '] IS NOT NULL 
            AND [' + @ColumnName + '] <> '''' 
            AND LEN([' + @ColumnName + ']) % 4 = 0
            AND ([' + @ColumnName + '] LIKE ''%=='' OR [' + @ColumnName + '] LIKE ''%='' OR LEN([' + @ColumnName + ']) >= 20)
            THEN 1 
            ELSE 0 
        END) AS EncryptedRows,
        SUM(CASE 
            WHEN [' + @ColumnName + '] IS NOT NULL 
            AND [' + @ColumnName + '] <> '''' 
            AND (LEN([' + @ColumnName + ']) % 4 <> 0 OR LEN([' + @ColumnName + ']) < 20)
            THEN 1 
            ELSE 0 
        END) AS PlaintextRows,
        SUM(CASE 
            WHEN [' + @ColumnName + '] IS NULL OR [' + @ColumnName + '] = '''' 
            THEN 1 
            ELSE 0 
        END) AS NullOrEmptyRows,
        CASE 
            WHEN COUNT(*) > 0 
            THEN CAST(SUM(CASE 
                WHEN [' + @ColumnName + '] IS NOT NULL 
                AND [' + @ColumnName + '] <> '''' 
                AND LEN([' + @ColumnName + ']) % 4 = 0
                THEN 1 
                ELSE 0 
            END) AS DECIMAL(10,2)) / COUNT(*) * 100
            ELSE 0
        END AS EncryptionRate
    FROM [' + @TableName + ']'
    
    EXEC sp_executesql @SQL
    
    FETCH NEXT FROM col_cursor INTO @TableName, @ColumnName
END

CLOSE col_cursor
DEALLOCATE col_cursor

-- Display results
PRINT 'ENCRYPTION STATUS BY TABLE AND COLUMN'
PRINT '=============================================================='
PRINT ''

SELECT 
    TableName,
    ColumnName,
    TotalRows,
    EncryptedRows,
    PlaintextRows,
    NullOrEmptyRows,
    EncryptionRate AS [Encryption_%],
    CASE 
        WHEN PlaintextRows = 0 AND EncryptedRows > 0 THEN 'PASS - Fully Encrypted'
        WHEN PlaintextRows > 0 THEN 'FAIL - Contains Plaintext'
        WHEN TotalRows = NullOrEmptyRows THEN 'WARNING - All NULL/Empty'
        ELSE 'UNKNOWN'
    END AS Status
FROM 
    #EncryptionStatus
ORDER BY 
    TableName, ColumnName

PRINT ''
PRINT '=============================================================='
PRINT 'SUMMARY'
PRINT '=============================================================='
PRINT ''

-- Overall summary
DECLARE @TotalColumns INT
DECLARE @PassedColumns INT
DECLARE @FailedColumns INT
DECLARE @WarningColumns INT

SELECT 
    @TotalColumns = COUNT(*),
    @PassedColumns = SUM(CASE WHEN PlaintextRows = 0 AND EncryptedRows > 0 THEN 1 ELSE 0 END),
    @FailedColumns = SUM(CASE WHEN PlaintextRows > 0 THEN 1 ELSE 0 END),
    @WarningColumns = SUM(CASE WHEN TotalRows = NullOrEmptyRows THEN 1 ELSE 0 END)
FROM 
    #EncryptionStatus

PRINT 'Total Columns Checked: ' + CAST(@TotalColumns AS VARCHAR)
PRINT 'Passed (Fully Encrypted): ' + CAST(@PassedColumns AS VARCHAR)
PRINT 'Failed (Contains Plaintext): ' + CAST(@FailedColumns AS VARCHAR)
PRINT 'Warnings (All NULL/Empty): ' + CAST(@WarningColumns AS VARCHAR)
PRINT ''

IF @FailedColumns = 0 AND @PassedColumns > 0
BEGIN
    PRINT '✓ VERIFICATION PASSED: All data is encrypted!'
    PRINT ''
    PRINT 'Next Steps:'
    PRINT '1. Disable migration mode in Web.config'
    PRINT '2. Test application functionality'
    PRINT '3. Monitor for any issues'
END
ELSE IF @FailedColumns > 0
BEGIN
    PRINT '✗ VERIFICATION FAILED: Plaintext data detected!'
    PRINT ''
    PRINT 'Action Required:'
    PRINT '1. Review tables with plaintext data'
    PRINT '2. Re-run migration utility'
    PRINT '3. Verify encryption again'
    PRINT ''
    PRINT 'Tables with plaintext:'
    SELECT DISTINCT TableName 
    FROM #EncryptionStatus 
    WHERE PlaintextRows > 0
    ORDER BY TableName
END
ELSE
BEGIN
    PRINT '? VERIFICATION INCONCLUSIVE: No encrypted data found'
    PRINT ''
    PRINT 'Possible causes:'
    PRINT '1. Migration has not been run yet'
    PRINT '2. All columns are NULL/empty'
    PRINT '3. Database connection issue'
END

PRINT ''
PRINT '=============================================================='
PRINT 'SAMPLE DATA (First 10 rows)'
PRINT '=============================================================='
PRINT ''

-- Show sample encrypted data from M_File
PRINT 'Sample from M_File:'
SELECT TOP 10 Row_ID, File_Code, LEFT(File_Code, 50) AS File_Code_Preview
FROM M_File
WHERE File_Code IS NOT NULL AND File_Code <> ''

PRINT ''

-- Show sample encrypted data from M_Document
PRINT 'Sample from M_Document:'
SELECT TOP 10 Row_ID, LEFT(Doc_Code, 50) AS Doc_Code_Preview, LEFT(Doc_Upload, 50) AS Doc_Upload_Preview
FROM M_Document
WHERE Doc_Code IS NOT NULL AND Doc_Code <> ''

-- Cleanup
DROP TABLE #EncryptionStatus

PRINT ''
PRINT 'Verification complete.'
PRINT 'Generated: ' + CONVERT(VARCHAR, GETDATE(), 120)
