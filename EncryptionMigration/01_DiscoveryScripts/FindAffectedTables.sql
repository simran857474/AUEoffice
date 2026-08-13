-- =============================================
-- Script: Find All Tables with Columns to Encrypt
-- Purpose: Automatically discover tables containing File_Code, Doc_Code, Doc_Name, Doc_Path, Doc_Upload
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'TABLES CONTAINING COLUMNS TO BE ENCRYPTED'
PRINT '=========================================='
PRINT ''

-- Find all tables with the target columns
SELECT 
    t.TABLE_SCHEMA AS [Schema],
    t.TABLE_NAME AS [Table_Name],
    c.COLUMN_NAME AS [Column_Name],
    c.DATA_TYPE AS [Data_Type],
    c.CHARACTER_MAXIMUM_LENGTH AS [Max_Length],
    c.IS_NULLABLE AS [Nullable]
FROM 
    INFORMATION_SCHEMA.TABLES t
INNER JOIN 
    INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
WHERE 
    c.COLUMN_NAME IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
    AND t.TABLE_TYPE = 'BASE TABLE'
ORDER BY 
    t.TABLE_NAME, c.COLUMN_NAME

PRINT ''
PRINT '=========================================='
PRINT 'SUMMARY: COUNT OF AFFECTED TABLES'
PRINT '=========================================='
PRINT ''

-- Summary count
SELECT 
    COUNT(DISTINCT t.TABLE_NAME) AS [Total_Affected_Tables],
    COUNT(*) AS [Total_Affected_Columns]
FROM 
    INFORMATION_SCHEMA.TABLES t
INNER JOIN 
    INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
WHERE 
    c.COLUMN_NAME IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
    AND t.TABLE_TYPE = 'BASE TABLE'

PRINT ''
PRINT 'Script completed successfully.'
