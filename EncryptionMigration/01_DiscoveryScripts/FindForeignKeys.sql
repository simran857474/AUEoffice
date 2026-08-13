-- =============================================
-- Script: Find All Foreign Keys on Target Columns
-- Purpose: Discover FK relationships for File_Code, Doc_Code to determine migration order
-- =============================================

USE [AU_eOffice_Live]
GO

SET NOCOUNT ON;

PRINT '=========================================='
PRINT 'FOREIGN KEY RELATIONSHIPS'
PRINT '=========================================='
PRINT ''

-- Find all foreign keys involving target columns
SELECT 
    fk.name AS [FK_Constraint_Name],
    SCHEMA_NAME(parent.schema_id) AS [Parent_Schema],
    parent.name AS [Parent_Table],
    pcol.name AS [Parent_Column],
    SCHEMA_NAME(referenced.schema_id) AS [Referenced_Schema],
    referenced.name AS [Referenced_Table],
    rcol.name AS [Referenced_Column]
FROM 
    sys.foreign_keys fk
INNER JOIN 
    sys.tables parent ON fk.parent_object_id = parent.object_id
INNER JOIN 
    sys.tables referenced ON fk.referenced_object_id = referenced.object_id
INNER JOIN 
    sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN 
    sys.columns pcol ON fkc.parent_object_id = pcol.object_id AND fkc.parent_column_id = pcol.column_id
INNER JOIN 
    sys.columns rcol ON fkc.referenced_object_id = rcol.object_id AND fkc.referenced_column_id = rcol.column_id
WHERE 
    pcol.name IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
    OR rcol.name IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
ORDER BY 
    referenced.name, parent.name

PRINT ''
PRINT '=========================================='
PRINT 'PRIMARY KEYS ON TARGET COLUMNS'
PRINT '=========================================='
PRINT ''

-- Find primary keys on target columns
SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS [Table_Name],
    i.name AS [PK_Constraint_Name],
    c.name AS [Column_Name]
FROM 
    sys.tables t
INNER JOIN 
    sys.indexes i ON t.object_id = i.object_id
INNER JOIN 
    sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN 
    sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE 
    i.is_primary_key = 1
    AND c.name IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
ORDER BY 
    t.name, c.name

PRINT ''
PRINT '=========================================='
PRINT 'UNIQUE CONSTRAINTS ON TARGET COLUMNS'
PRINT '=========================================='
PRINT ''

-- Find unique constraints on target columns
SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS [Table_Name],
    i.name AS [Unique_Constraint_Name],
    c.name AS [Column_Name]
FROM 
    sys.tables t
INNER JOIN 
    sys.indexes i ON t.object_id = i.object_id
INNER JOIN 
    sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN 
    sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE 
    i.is_unique_constraint = 1
    AND c.name IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
ORDER BY 
    t.name, c.name

PRINT ''
PRINT 'Script completed successfully.'
