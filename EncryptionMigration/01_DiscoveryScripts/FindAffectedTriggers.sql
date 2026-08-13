-- =============================================
-- Script: Find All Triggers Using Target Columns
-- Purpose: Discover all triggers that reference File_Code, Doc_Code, Doc_Name, Doc_Path, Doc_Upload
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'TRIGGERS USING TARGET COLUMNS'
PRINT '=========================================='
PRINT ''

-- Find all triggers containing target columns
SELECT DISTINCT
    SCHEMA_NAME(o.schema_id) AS [Schema],
    o.name AS [Trigger_Name],
    OBJECT_NAME(parent_object_id) AS [Parent_Table],
    o.create_date AS [Created_Date],
    o.modify_date AS [Modified_Date],
    STUFF((
        SELECT DISTINCT ', ' + col_name
        FROM (
            SELECT 'File_Code' AS col_name WHERE m.definition LIKE '%File_Code%'
            UNION ALL
            SELECT 'Doc_Code' WHERE m.definition LIKE '%Doc_Code%'
            UNION ALL
            SELECT 'Doc_Name' WHERE m.definition LIKE '%Doc_Name%'
            UNION ALL
            SELECT 'Doc_Path' WHERE m.definition LIKE '%Doc_Path%'
            UNION ALL
            SELECT 'Doc_Upload' WHERE m.definition LIKE '%Doc_Upload%'
        ) cols
        FOR XML PATH('')
    ), 1, 2, '') AS [Referenced_Columns]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'TR'
    AND (
        m.definition LIKE '%File_Code%' 
        OR m.definition LIKE '%Doc_Code%'
        OR m.definition LIKE '%Doc_Name%'
        OR m.definition LIKE '%Doc_Path%'
        OR m.definition LIKE '%Doc_Upload%'
    )
ORDER BY 
    OBJECT_NAME(parent_object_id), o.name

PRINT ''
PRINT '=========================================='
PRINT 'SUMMARY'
PRINT '=========================================='
PRINT ''

-- Summary count
SELECT 
    COUNT(DISTINCT o.name) AS [Total_Affected_Triggers]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'TR'
    AND (
        m.definition LIKE '%File_Code%' 
        OR m.definition LIKE '%Doc_Code%'
        OR m.definition LIKE '%Doc_Name%'
        OR m.definition LIKE '%Doc_Path%'
        OR m.definition LIKE '%Doc_Upload%'
    )

PRINT ''
PRINT 'Script completed successfully.'
