using System;
using System.Data;
using System.Collections.Generic;
using Eoffice.BAL;

namespace Eoffice.Security
{
    /// <summary>
    /// Helper to automatically decrypt sensitive columns in DataSets
    /// </summary>
    public static class DataSetEncryptionHelper
    {
        // Define all columns that need decryption
        private static readonly HashSet<string> EncryptedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "File_Code",
            "Doc_Code",
            "Doc_Name",
            "Doc_Path",
            "Doc_Upload",
            "url",
            "Doc_Upload_rest",
            "DisplayFile"
        };

        /// <summary>
        /// Decrypts all encrypted columns in a DataSet
        /// </summary>
        /// <param name="ds">DataSet to decrypt</param>
        /// <returns>Same DataSet with decrypted values</returns>
        public static DataSet DecryptDataSet(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0)
                return ds;

            foreach (DataTable table in ds.Tables)
            {
                DecryptDataTable(table);
            }

            return ds;
        }

        /// <summary>
        /// Decrypts all encrypted columns in a DataTable
        /// </summary>
        /// <param name="table">DataTable to decrypt</param>
        /// <returns>Same DataTable with decrypted values</returns>
        public static DataTable DecryptDataTable(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
                return table;

            // Find which columns need decryption
            var columnsToDecrypt = new List<DataColumn>();
            foreach (DataColumn column in table.Columns)
            {
                if (EncryptedColumns.Contains(column.ColumnName))
                {
                    column.ReadOnly = false;
                    columnsToDecrypt.Add(column);
                }
            }

            // If no encrypted columns found, return as-is
            if (columnsToDecrypt.Count == 0)
                return table;

            // Decrypt each row
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in columnsToDecrypt)
                {
                    if (row[column] != DBNull.Value && row[column] != null)
                    {
                        string encryptedValue = row[column].ToString();
                        if (!string.IsNullOrEmpty(encryptedValue))
                        {
                            try
                            {
                                // Use SafeDecrypt - handles both encrypted and plaintext values
                                row[column] = DeterministicEncryptionHelper.SafeDecrypt(encryptedValue);
                            }
                            catch (Exception ex)
                            {
                                // If decryption fails, keep original value
                                System.Diagnostics.Debug.WriteLine($"Failed to decrypt {column}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Encrypts all sensitive columns in a DataSet
        /// </summary>
        /// <param name="ds">DataSet to encrypt</param>
        /// <returns>Same DataSet with encrypted values</returns>
        public static DataSet EncryptDataSet(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0)
                return ds;

            foreach (DataTable table in ds.Tables)
            {
                EncryptDataTable(table);
            }

            return ds;
        }

        /// <summary>
        /// Encrypts all sensitive columns in a DataTable
        /// </summary>
        /// <param name="table">DataTable to encrypt</param>
        /// <returns>Same DataTable with encrypted values</returns>
        public static DataTable EncryptDataTable(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
                return table;

            // Find which columns need encryption
            var columnsToEncrypt = new List<DataColumn>();
            foreach (DataColumn column in table.Columns)
            {
                if (EncryptedColumns.Contains(column.ColumnName))
                {
                    columnsToEncrypt.Add(column);
                }
            }

            // If no encrypted columns found, return as-is
            if (columnsToEncrypt.Count == 0)
                return table;

            // Encrypt each row
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in columnsToEncrypt)
                {
                    if (row[column] != DBNull.Value && row[column] != null)
                    {
                        string plainValue = row[column].ToString();
                        if (!string.IsNullOrEmpty(plainValue))
                        {
                            try
                            {
                                row[column] = DeterministicEncryptionHelper.Encrypt(plainValue);
                            }
                            catch
                            {
                                // Handle error as needed
                            }
                        }
                    }
                }
            }

            return table;
        }
    }
}
