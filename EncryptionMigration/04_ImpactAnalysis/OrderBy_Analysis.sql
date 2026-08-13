-- =============================================
-- Script: Analyze ORDER BY Impact on Encrypted Columns
-- Purpose: Identify all ORDER BY operations that will be affected by encryption
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'ORDER BY IMPACT ANALYSIS'
PRINT '=========================================='
PRINT ''
PRINT 'CRITICAL: ORDER BY on encrypted columns will produce different sort order.'
PRINT ''
PRINT 'Original Order (Plaintext):'
PRINT '  FA/2024/001'
PRINT '  FA/2024/002'
PRINT '  FA/2024/010'
PRINT '  FA/2024/100'
PRINT ''
PRINT 'Encrypted Order (Base64):'
PRINT '  X82JH83JD82H...'
PRINT '  A12KL34MN56O...'
PRINT '  Z98QW76ER54T...'
PRINT '  (No logical relationship to original order)'
PRINT ''
PRINT '=========================================='
PRINT ''

-- Find all procedures with ORDER BY on File_Code
PRINT 'Procedures using ORDER BY on File_Code:'
PRINT '--------------------------------------------'
SELECT DISTINCT
    o.name AS [Procedure_Name],
    'File_Code' AS [Column],
    'ORDER BY detected' AS [Impact]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%ORDER BY%File_Code%'
        OR m.definition LIKE '%ORDER BY%[[]File_Code%'
    )
ORDER BY 
    o.name

PRINT ''

-- Find all procedures with ORDER BY on Doc_Code
PRINT 'Procedures using ORDER BY on Doc_Code:'
PRINT '--------------------------------------------'
SELECT DISTINCT
    o.name AS [Procedure_Name],
    'Doc_Code' AS [Column],
    'ORDER BY detected' AS [Impact]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%ORDER BY%Doc_Code%'
        OR m.definition LIKE '%ORDER BY%[[]Doc_Code%'
    )
ORDER BY 
    o.name

PRINT ''

-- Find all views with ORDER BY
PRINT 'Views using ORDER BY on target columns:'
PRINT '--------------------------------------------'
SELECT DISTINCT
    o.name AS [View_Name],
    CASE 
        WHEN m.definition LIKE '%ORDER BY%File_Code%' THEN 'File_Code'
        WHEN m.definition LIKE '%ORDER BY%Doc_Code%' THEN 'Doc_Code'
        WHEN m.definition LIKE '%ORDER BY%Doc_Name%' THEN 'Doc_Name'
        ELSE 'Multiple'
    END AS [Column],
    'ORDER BY detected' AS [Impact]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'V'
    AND (
        m.definition LIKE '%ORDER BY%File_Code%'
        OR m.definition LIKE '%ORDER BY%Doc_Code%'
        OR m.definition LIKE '%ORDER BY%Doc_Name%'
    )
ORDER BY 
    o.name

PRINT ''
PRINT '=========================================='
PRINT 'RECOMMENDATIONS FOR ORDER BY'
PRINT '=========================================='
PRINT ''
PRINT 'Option 1: Remove ORDER BY (if not critical)'
PRINT '  - If sort order is not important, remove ORDER BY'
PRINT '  - Application can sort decrypted values if needed'
PRINT ''
PRINT 'Option 2: Create separate sort column'
PRINT '  - Add a new column like File_Code_Sort'
PRINT '  - Store plaintext or sortable hash'
PRINT '  - ORDER BY File_Code_Sort instead'
PRINT ''
PRINT 'Option 3: Add surrogate sort key'
PRINT '  - Add sequential integer column (File_Seq, Doc_Seq)'
PRINT '  - Maintain sequence during INSERT'
PRINT '  - ORDER BY File_Seq instead'
PRINT ''
PRINT 'Option 4: Application-layer sorting'
PRINT '  - Retrieve data without ORDER BY'
PRINT '  - Decrypt in application'
PRINT '  - Sort decrypted values'
PRINT '  - Best for small result sets'
PRINT ''
PRINT 'Recommended: Option 2 or 3 for best performance'
PRINT ''
PRINT '=========================================='
PRINT ''

PRINT 'Script completed successfully.'
