-- =============================================
-- Script: Find All Stored Procedures Using Target Columns
-- Purpose: Discover all stored procedures that reference File_Code, Doc_Code, Doc_Name, Doc_Path, Doc_Upload
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'STORED PROCEDURES USING TARGET COLUMNS'
PRINT '=========================================='
PRINT ''

-- Find all stored procedures containing target columns
SELECT DISTINCT
    SCHEMA_NAME(o.schema_id) AS [Schema],
    o.name AS [Stored_Procedure_Name],
    o.create_date AS [Created_Date],
    o.modify_date AS [Modified_Date],
    CASE 
        WHEN m.definition LIKE '%File_Code%' THEN 'File_Code'
        WHEN m.definition LIKE '%Doc_Code%' THEN 'Doc_Code'
        WHEN m.definition LIKE '%Doc_Name%' THEN 'Doc_Name'
        WHEN m.definition LIKE '%Doc_Path%' THEN 'Doc_Path'
        WHEN m.definition LIKE '%Doc_Upload%' THEN 'Doc_Upload'
    END AS [Referenced_Column]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%File_Code%' 
        OR m.definition LIKE '%Doc_Code%'
        OR m.definition LIKE '%Doc_Name%'
        OR m.definition LIKE '%Doc_Path%'
        OR m.definition LIKE '%Doc_Upload%'
    )
ORDER BY 
    o.name

PRINT ''
PRINT '=========================================='
PRINT 'PROCEDURES WITH LIKE OPERATIONS'
PRINT '=========================================='
PRINT ''

-- Find stored procedures with LIKE operations on target columns
SELECT DISTINCT
    SCHEMA_NAME(o.schema_id) AS [Schema],
    o.name AS [Stored_Procedure_Name],
    'Contains LIKE operation' AS [Warning]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%File_Code%LIKE%' 
        OR m.definition LIKE '%Doc_Code%LIKE%'
        OR m.definition LIKE '%LIKE%File_Code%'
        OR m.definition LIKE '%LIKE%Doc_Code%'
    )
ORDER BY 
    o.name

PRINT ''
PRINT '=========================================='
PRINT 'SUMMARY'
PRINT '=========================================='
PRINT ''

-- Summary count
SELECT 
    COUNT(DISTINCT o.name) AS [Total_Affected_Stored_Procedures]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%File_Code%' 
        OR m.definition LIKE '%Doc_Code%'
        OR m.definition LIKE '%Doc_Name%'
        OR m.definition LIKE '%Doc_Path%'
        OR m.definition LIKE '%Doc_Upload%'
    )

PRINT ''
PRINT 'Script completed successfully.'
