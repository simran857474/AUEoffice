using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Eoffice.Security;

namespace Eoffice.DataMigration
{
    /// <summary>
    /// Console utility to migrate existing plaintext data to encrypted format.
    /// This tool is idempotent - it can be run multiple times safely.
    /// </summary>
    public class DataEncryptionMigrator
    {
        private readonly string _connectionString;
        private readonly List<string> _targetColumns = new List<string> 
        { 
            "File_Code", 
            "Doc_Code", 
            "Doc_Name", 
            "Doc_Path", 
            "Doc_Upload" 
        };

        public DataEncryptionMigrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Main migration entry point
        /// </summary>
        public void MigrateAllData()
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("Data Encryption Migration Utility");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            try
            {
                // Step 1: Discover all affected tables
                Console.WriteLine("Step 1: Discovering affected tables...");
                var affectedTables = DiscoverAffectedTables();
                Console.WriteLine($"Found {affectedTables.Count} tables to process.");
                Console.WriteLine();

                // Step 2: Process each table
                foreach (var table in affectedTables)
                {
                    Console.WriteLine($"Processing table: {table.TableName}");
                    Console.WriteLine(new string('-', 60));

                    foreach (var column in table.Columns)
                    {
                        Console.WriteLine($"  Encrypting column: {column}");
                        int rowsProcessed = EncryptColumnData(table.TableName, column, table.PrimaryKey);
                        Console.WriteLine($"  Rows processed: {rowsProcessed}");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine("==============================================");
                Console.WriteLine("Migration completed successfully!");
                Console.WriteLine("==============================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: Migration failed!");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Discovers all tables containing target columns
        /// </summary>
        private List<TableInfo> DiscoverAffectedTables()
        {
            var tables = new List<TableInfo>();

            string query = @"
                SELECT 
                    t.TABLE_NAME,
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.CHARACTER_MAXIMUM_LENGTH
                FROM 
                    INFORMATION_SCHEMA.TABLES t
                INNER JOIN 
                    INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
                WHERE 
                    c.COLUMN_NAME IN ('File_Code', 'Doc_Code', 'Doc_Name', 'Doc_Path', 'Doc_Upload')
                    AND t.TABLE_TYPE = 'BASE TABLE'
                ORDER BY 
                    t.TABLE_NAME, c.COLUMN_NAME";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string currentTable = null;
                        TableInfo currentTableInfo = null;

                        while (reader.Read())
                        {
                            string tableName = reader["TABLE_NAME"].ToString();
                            string columnName = reader["COLUMN_NAME"].ToString();

                            if (currentTable != tableName)
                            {
                                if (currentTableInfo != null)
                                    tables.Add(currentTableInfo);

                                currentTableInfo = new TableInfo
                                {
                                    TableName = tableName,
                                    PrimaryKey = GetPrimaryKeyColumn(tableName, conn),
                                    Columns = new List<string>()
                                };
                                currentTable = tableName;
                            }

                            currentTableInfo.Columns.Add(columnName);
                        }

                        if (currentTableInfo != null)
                            tables.Add(currentTableInfo);
                    }
                }
            }

            return tables;
        }

        /// <summary>
        /// Gets the primary key column for a table
        /// </summary>
        private string GetPrimaryKeyColumn(string tableName, SqlConnection conn)
        {
            string query = @"
                SELECT c.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.COLUMNS c ON ccu.COLUMN_NAME = c.COLUMN_NAME AND ccu.TABLE_NAME = c.TABLE_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND tc.TABLE_NAME = @TableName";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                object result = cmd.ExecuteScalar();
                return result?.ToString() ?? "Row_ID"; // Default fallback
            }
        }

        /// <summary>
        /// Encrypts data in a specific column
        /// </summary>
        private int EncryptColumnData(string tableName, string columnName, string primaryKey)
        {
            int rowsProcessed = 0;
            int batchSize = 1000;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Get total count
                string countQuery = $"SELECT COUNT(*) FROM [{tableName}] WHERE [{columnName}] IS NOT NULL AND [{columnName}] <> ''";
                int totalRows = 0;
                using (SqlCommand cmd = new SqlCommand(countQuery, conn))
                {
                    totalRows = (int)cmd.ExecuteScalar();
                }

                Console.WriteLine($"    Total rows to process: {totalRows}");

                // Process in batches
                string selectQuery = $@"
                    SELECT TOP {batchSize} [{primaryKey}], [{columnName}]
                    FROM [{tableName}]
                    WHERE [{columnName}] IS NOT NULL 
                    AND [{columnName}] <> ''
                    AND [{primaryKey}] NOT IN (
                        SELECT [{primaryKey}] FROM [{tableName}] 
                        WHERE [{columnName}] IS NOT NULL 
                        ORDER BY [{primaryKey}]
                        OFFSET 0 ROWS FETCH NEXT @RowsProcessed ROWS ONLY
                    )";

                bool hasMoreData = true;
                while (hasMoreData)
                {
                    var updates = new List<KeyValuePair<object, string>>();

                    // Read batch
                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@RowsProcessed", rowsProcessed);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                object pkValue = reader[primaryKey];
                                string plainValue = reader[columnName]?.ToString();

                                if (!string.IsNullOrEmpty(plainValue))
                                {
                                    // Only encrypt if not already encrypted
                                    if (!DeterministicEncryptionHelper.IsEncrypted(plainValue))
                                    {
                                        string encryptedValue = DeterministicEncryptionHelper.Encrypt(plainValue);
                                        updates.Add(new KeyValuePair<object, string>(pkValue, encryptedValue));
                                    }
                                }
                            }

                            hasMoreData = reader.HasRows;
                        }
                    }

                    // Update batch
                    if (updates.Count > 0)
                    {
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                string updateQuery = $"UPDATE [{tableName}] SET [{columnName}] = @EncryptedValue WHERE [{primaryKey}] = @PkValue";

                                foreach (var update in updates)
                                {
                                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@EncryptedValue", update.Value);
                                        cmd.Parameters.AddWithValue("@PkValue", update.Key);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                rowsProcessed += updates.Count;
                                Console.WriteLine($"    Progress: {rowsProcessed}/{totalRows}");
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        hasMoreData = false;
                    }
                }
            }

            return rowsProcessed;
        }

        /// <summary>
        /// Verifies that all data has been encrypted
        /// </summary>
        public void VerifyEncryption()
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("Verification: Checking for remaining plaintext");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            var affectedTables = DiscoverAffectedTables();

            foreach (var table in affectedTables)
            {
                foreach (var column in table.Columns)
                {
                    int plaintextCount = CountPlaintextRows(table.TableName, column);
                    
                    if (plaintextCount > 0)
                    {
                        Console.WriteLine($"WARNING: {table.TableName}.{column} has {plaintextCount} plaintext rows");
                    }
                    else
                    {
                        Console.WriteLine($"OK: {table.TableName}.{column} - All data encrypted");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Verification complete.");
        }

        /// <summary>
        /// Counts rows that appear to contain plaintext (not Base64)
        /// </summary>
        private int CountPlaintextRows(string tableName, string columnName)
        {
            // This is approximate - checks for common plaintext patterns
            string query = $@"
                SELECT COUNT(*)
                FROM [{tableName}]
                WHERE [{columnName}] IS NOT NULL 
                AND [{columnName}] <> ''
                AND (
                    [{columnName}] NOT LIKE '%==%'  -- Base64 typically ends with = or ==
                    OR LEN([{columnName}]) % 4 <> 0  -- Base64 length is multiple of 4
                )";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private class TableInfo
        {
            public string TableName { get; set; }
            public string PrimaryKey { get; set; }
            public List<string> Columns { get; set; }
        }

        static void Main(string[] args)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var migrator = new DataEncryptionMigrator(connectionString);

                Console.WriteLine("Select operation:");
                Console.WriteLine("1. Migrate Data (Encrypt)");
                Console.WriteLine("2. Verify Encryption");
                Console.Write("Enter choice (1 or 2): ");

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    Console.WriteLine();
                    Console.WriteLine("WARNING: This will encrypt all plaintext data in the database.");
                    Console.Write("Continue? (yes/no): ");
                    string confirm = Console.ReadLine();

                    if (confirm?.ToLower() == "yes")
                    {
                        migrator.MigrateAllData();
                    }
                    else
                    {
                        Console.WriteLine("Operation cancelled.");
                    }
                }
                else if (choice == "2")
                {
                    migrator.VerifyEncryption();
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL ERROR:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
