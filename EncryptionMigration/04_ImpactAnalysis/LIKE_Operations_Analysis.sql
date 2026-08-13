-- =============================================
-- Script: Analyze LIKE Operations on Target Columns
-- Purpose: Identify all LIKE searches that will be affected by encryption
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'LIKE OPERATIONS IMPACT ANALYSIS'
PRINT '=========================================='
PRINT ''
PRINT 'CRITICAL: Deterministic AES encryption does NOT support LIKE searches.'
PRINT 'Encrypted data is Base64 encoded - pattern matching will not work.'
PRINT ''
PRINT 'Example: File_Code = "FA/2024/001"'
PRINT 'Encrypted: "X82JH83JD82H..." (no relationship to original pattern)'
PRINT 'LIKE ''FA%'' will NOT match encrypted values.'
PRINT ''
PRINT '=========================================='
PRINT ''

-- Find all procedures with LIKE on File_Code
PRINT 'Procedures using LIKE on File_Code:'
PRINT '--------------------------------------------'
SELECT DISTINCT
    o.name AS [Procedure_Name],
    'File_Code' AS [Column],
    'LIKE operation detected' AS [Impact]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%File_Code%LIKE%'
        OR m.definition LIKE '%LIKE%File_Code%'
    )
ORDER BY 
    o.name

PRINT ''

-- Find all procedures with LIKE on Doc_Code
PRINT 'Procedures using LIKE on Doc_Code:'
PRINT '--------------------------------------------'
SELECT DISTINCT
    o.name AS [Procedure_Name],
    'Doc_Code' AS [Column],
    'LIKE operation detected' AS [Impact]
FROM 
    sys.objects o
INNER JOIN 
    sys.sql_modules m ON o.object_id = m.object_id
WHERE 
    o.type = 'P'
    AND (
        m.definition LIKE '%Doc_Code%LIKE%'
        OR m.definition LIKE '%LIKE%Doc_Code%'
    )
ORDER BY 
    o.name

PRINT ''
PRINT '=========================================='
PRINT 'RECOMMENDATIONS FOR LIKE OPERATIONS'
PRINT '=========================================='
PRINT ''
PRINT 'Option 1: Remove LIKE functionality'
PRINT '  - If LIKE searches are rare/unused, consider removing them'
PRINT '  - Use exact match only (WHERE File_Code = @EncryptedValue)'
PRINT ''
PRINT 'Option 2: Create search index table'
PRINT '  - Maintain a separate searchable index table'
PRINT '  - Store searchable tokens (first few chars, patterns)'
PRINT '  - Map tokens to encrypted primary keys'
PRINT ''
PRINT 'Option 3: Full-text search'
PRINT '  - Decrypt values in application layer for search'
PRINT '  - Use full-text indexing on decrypted values in memory'
PRINT '  - Not recommended for large datasets'
PRINT ''
PRINT 'Option 4: Hybrid approach'
PRINT '  - Keep original value in separate "search helper" column'
PRINT '  - Store only prefix (first 5-10 chars) for LIKE searches'
PRINT '  - Store full encrypted value in main column'
PRINT ''
PRINT '=========================================='
PRINT 'SPECIFIC PROCEDURES TO REVIEW'
PRINT '=========================================='
PRINT ''

-- Get full procedure definitions that use LIKE
DECLARE @ProcName NVARCHAR(255)
DECLARE proc_cursor CURSOR FOR
SELECT DISTINCT o.name
FROM sys.objects o
INNER JOIN sys.sql_modules m ON o.object_id = m.object_id
WHERE o.type = 'P'
AND (
    (m.definition LIKE '%File_Code%LIKE%' OR m.definition LIKE '%LIKE%File_Code%')
    OR (m.definition LIKE '%Doc_Code%LIKE%' OR m.definition LIKE '%LIKE%Doc_Code%')
)
ORDER BY o.name

OPEN proc_cursor
FETCH NEXT FROM proc_cursor INTO @ProcName

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT ''
    PRINT 'Procedure: ' + @ProcName
    PRINT REPLICATE('-', 60)
    
    DECLARE @Definition NVARCHAR(MAX)
    SELECT @Definition = m.definition
    FROM sys.objects o
    INNER JOIN sys.sql_modules m ON o.object_id = m.object_id
    WHERE o.name = @ProcName
    
    -- Show relevant lines only
    PRINT 'Action Required: Review and modify LIKE operations'
    PRINT ''
    
    FETCH NEXT FROM proc_cursor INTO @ProcName
END

CLOSE proc_cursor
DEALLOCATE proc_cursor

PRINT ''
PRINT 'Script completed successfully.'
