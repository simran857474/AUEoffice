using System;
using System.Collections.Generic;
using Eoffice.Security;

namespace Eoffice.BAL.Examples
{
    /// <summary>
    /// Example showing how to modify BAL methods to support encryption
    /// </summary>
    public class DocumentBAL_Examples
    {
        // ============================================================
        // EXAMPLE 1: INSERT Operation
        // ============================================================

        #region BEFORE: Plaintext INSERT
        public string AddDocument_BEFORE(ModelAddDocument model)
        {
            string msg = string.Empty;
            
            try
            {
                // Direct assignment - plaintext values
                model.Doc_Code = GenerateDocCode();
                model.File_Code = model.File_Code;
                
                // Pass to DAL
                msg = DAL.InsertDocument(model);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }
            
            return msg;
        }
        #endregion

        #region AFTER: Encrypted INSERT
        public string AddDocument_AFTER(ModelAddDocument model)
        {
            string msg = string.Empty;
            
            try
            {
                // Generate plaintext code first
                string plainDocCode = GenerateDocCode();
                string plainFileCode = model.File_Code;
                
                // ENCRYPT before passing to DAL
                model.Doc_Code = DeterministicEncryptionHelper.Encrypt(plainDocCode);
                model.File_Code = DeterministicEncryptionHelper.Encrypt(plainFileCode);
                model.Doc_Name = DeterministicEncryptionHelper.Encrypt(model.Doc_Name);
                model.Doc_Path = DeterministicEncryptionHelper.Encrypt(model.Doc_Path);
                model.Doc_Upload = DeterministicEncryptionHelper.Encrypt(model.Doc_Upload);
                
                // Pass encrypted values to DAL
                msg = DAL.InsertDocument(model);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }
            
            return msg;
        }
        #endregion

        // ============================================================
        // EXAMPLE 2: SELECT Operation (List)
        // ============================================================

        #region BEFORE: Plaintext SELECT
        public List<ModelDocument> GetDocuments_BEFORE(string fileCode)
        {
            List<ModelDocument> documents = new List<ModelDocument>();
            
            try
            {
                // Get plaintext data from DAL
                documents = DAL.GetDocumentsByFileCode(fileCode);
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return documents;
        }
        #endregion

        #region AFTER: Encrypted SELECT
        public List<ModelDocument> GetDocuments_AFTER(string fileCode)
        {
            List<ModelDocument> documents = new List<ModelDocument>();
            
            try
            {
                // ENCRYPT the search parameter
                string encryptedFileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
                
                // Get encrypted data from DAL
                documents = DAL.GetDocumentsByFileCode(encryptedFileCode);
                
                // DECRYPT results before returning to MVC
                foreach (var doc in documents)
                {
                    doc.Doc_Code = DeterministicEncryptionHelper.Decrypt(doc.Doc_Code);
                    doc.File_Code = DeterministicEncryptionHelper.Decrypt(doc.File_Code);
                    doc.Doc_Name = DeterministicEncryptionHelper.Decrypt(doc.Doc_Name);
                    doc.Doc_Path = DeterministicEncryptionHelper.Decrypt(doc.Doc_Path);
                    doc.Doc_Upload = DeterministicEncryptionHelper.Decrypt(doc.Doc_Upload);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return documents;
        }
        #endregion

        // ============================================================
        // EXAMPLE 3: SELECT Operation (Single Record)
        // ============================================================

        #region BEFORE: Plaintext SELECT Single
        public ModelDocument GetDocumentByCode_BEFORE(string docCode)
        {
            ModelDocument document = null;
            
            try
            {
                document = DAL.GetDocumentByCode(docCode);
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return document;
        }
        #endregion

        #region AFTER: Encrypted SELECT Single
        public ModelDocument GetDocumentByCode_AFTER(string docCode)
        {
            ModelDocument document = null;
            
            try
            {
                // ENCRYPT search parameter
                string encryptedDocCode = DeterministicEncryptionHelper.Encrypt(docCode);
                
                // Get encrypted data
                document = DAL.GetDocumentByCode(encryptedDocCode);
                
                // DECRYPT result
                if (document != null)
                {
                    document.Doc_Code = DeterministicEncryptionHelper.Decrypt(document.Doc_Code);
                    document.File_Code = DeterministicEncryptionHelper.Decrypt(document.File_Code);
                    document.Doc_Name = DeterministicEncryptionHelper.Decrypt(document.Doc_Name);
                    document.Doc_Path = DeterministicEncryptionHelper.Decrypt(document.Doc_Path);
                    document.Doc_Upload = DeterministicEncryptionHelper.Decrypt(document.Doc_Upload);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return document;
        }
        #endregion

        // ============================================================
        // EXAMPLE 4: UPDATE Operation
        // ============================================================

        #region BEFORE: Plaintext UPDATE
        public string UpdateDocument_BEFORE(ModelAddDocument model)
        {
            string msg = string.Empty;
            
            try
            {
                // Pass plaintext values to DAL
                msg = DAL.UpdateDocument(model);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }
            
            return msg;
        }
        #endregion

        #region AFTER: Encrypted UPDATE
        public string UpdateDocument_AFTER(ModelAddDocument model)
        {
            string msg = string.Empty;
            
            try
            {
                // ENCRYPT values before update
                model.Doc_Code = DeterministicEncryptionHelper.Encrypt(model.Doc_Code);
                model.File_Code = DeterministicEncryptionHelper.Encrypt(model.File_Code);
                model.Doc_Name = DeterministicEncryptionHelper.Encrypt(model.Doc_Name);
                model.Doc_Path = DeterministicEncryptionHelper.Encrypt(model.Doc_Path);
                model.Doc_Upload = DeterministicEncryptionHelper.Encrypt(model.Doc_Upload);
                
                // Pass encrypted values to DAL
                msg = DAL.UpdateDocument(model);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }
            
            return msg;
        }
        #endregion

        // ============================================================
        // EXAMPLE 5: Complex Query with JOINs
        // ============================================================

        #region BEFORE: Plaintext JOIN Query
        public List<ModelFileWithDocuments> GetFilesWithDocuments_BEFORE()
        {
            List<ModelFileWithDocuments> result = new List<ModelFileWithDocuments>();
            
            try
            {
                // DAL returns joined data (plaintext)
                result = DAL.GetFilesWithDocuments();
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return result;
        }
        #endregion

        #region AFTER: Encrypted JOIN Query
        public List<ModelFileWithDocuments> GetFilesWithDocuments_AFTER()
        {
            List<ModelFileWithDocuments> result = new List<ModelFileWithDocuments>();
            
            try
            {
                // DAL returns joined data (encrypted)
                // JOIN still works because deterministic encryption
                result = DAL.GetFilesWithDocuments();
                
                // DECRYPT all fields in result
                foreach (var item in result)
                {
                    // Decrypt file fields
                    item.File_Code = DeterministicEncryptionHelper.Decrypt(item.File_Code);
                    
                    // Decrypt document fields
                    item.Doc_Code = DeterministicEncryptionHelper.Decrypt(item.Doc_Code);
                    item.Doc_Name = DeterministicEncryptionHelper.Decrypt(item.Doc_Name);
                    item.Doc_Path = DeterministicEncryptionHelper.Decrypt(item.Doc_Path);
                    item.Doc_Upload = DeterministicEncryptionHelper.Decrypt(item.Doc_Upload);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return result;
        }
        #endregion

        // ============================================================
        // EXAMPLE 6: Search with Multiple Criteria
        // ============================================================

        #region BEFORE: Plaintext Search
        public List<ModelDocument> SearchDocuments_BEFORE(string fileCode, string docCode)
        {
            List<ModelDocument> documents = new List<ModelDocument>();
            
            try
            {
                documents = DAL.SearchDocuments(fileCode, docCode);
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return documents;
        }
        #endregion

        #region AFTER: Encrypted Search
        public List<ModelDocument> SearchDocuments_AFTER(string fileCode, string docCode)
        {
            List<ModelDocument> documents = new List<ModelDocument>();
            
            try
            {
                // ENCRYPT all search parameters
                string encryptedFileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
                string encryptedDocCode = DeterministicEncryptionHelper.Encrypt(docCode);
                
                // Search with encrypted values
                documents = DAL.SearchDocuments(encryptedFileCode, encryptedDocCode);
                
                // DECRYPT results
                foreach (var doc in documents)
                {
                    doc.Doc_Code = DeterministicEncryptionHelper.Decrypt(doc.Doc_Code);
                    doc.File_Code = DeterministicEncryptionHelper.Decrypt(doc.File_Code);
                    doc.Doc_Name = DeterministicEncryptionHelper.Decrypt(doc.Doc_Name);
                    doc.Doc_Path = DeterministicEncryptionHelper.Decrypt(doc.Doc_Path);
                    doc.Doc_Upload = DeterministicEncryptionHelper.Decrypt(doc.Doc_Upload);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return documents;
        }
        #endregion

        // ============================================================
        // EXAMPLE 7: Dropdown/List Population
        // ============================================================

        #region BEFORE: Plaintext Dropdown
        public List<DropdownModel> GetFileCodeDropdown_BEFORE()
        {
            List<DropdownModel> dropdown = new List<DropdownModel>();
            
            try
            {
                dropdown = DAL.GetFileCodesForDropdown();
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return dropdown;
        }
        #endregion

        #region AFTER: Encrypted Dropdown
        public List<DropdownModel> GetFileCodeDropdown_AFTER()
        {
            List<DropdownModel> dropdown = new List<DropdownModel>();
            
            try
            {
                // Get encrypted values from DAL
                dropdown = DAL.GetFileCodesForDropdown();
                
                // DECRYPT for display
                foreach (var item in dropdown)
                {
                    // Value remains encrypted (for posting back)
                    // Text is decrypted for display
                    item.Text = DeterministicEncryptionHelper.Decrypt(item.Value);
                    // Note: You may want to keep Value encrypted for form submission
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return dropdown;
        }
        #endregion

        // ============================================================
        // EXAMPLE 8: Migration Mode Support (Handles Both)
        // ============================================================

        #region MIGRATION MODE: Handle Both Encrypted and Plaintext
        public List<ModelDocument> GetDocuments_MIGRATION_MODE(string fileCode)
        {
            List<ModelDocument> documents = new List<ModelDocument>();
            
            try
            {
                // Encrypt search parameter (safe even if already encrypted)
                string encryptedFileCode = DeterministicEncryptionHelper.EncryptIfNotEncrypted(fileCode);
                
                // Get data from DAL
                documents = DAL.GetDocumentsByFileCode(encryptedFileCode);
                
                // Safely decrypt (returns original if not encrypted)
                foreach (var doc in documents)
                {
                    doc.Doc_Code = DeterministicEncryptionHelper.SafeDecrypt(doc.Doc_Code);
                    doc.File_Code = DeterministicEncryptionHelper.SafeDecrypt(doc.File_Code);
                    doc.Doc_Name = DeterministicEncryptionHelper.SafeDecrypt(doc.Doc_Name);
                    doc.Doc_Path = DeterministicEncryptionHelper.SafeDecrypt(doc.Doc_Path);
                    doc.Doc_Upload = DeterministicEncryptionHelper.SafeDecrypt(doc.Doc_Upload);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return documents;
        }
        #endregion

        // ============================================================
        // Helper Methods
        // ============================================================

        private string GenerateDocCode()
        {
            // Your existing logic
            return "DOC/2024/001";
        }
        
        // Mock DAL reference
        private static dynamic DAL = null;
    }

    // ============================================================
    // REUSABLE HELPER: Create Extension Methods
    // ============================================================

    /// <summary>
    /// Extension methods to simplify encryption/decryption in BAL
    /// </summary>
    public static class EncryptionExtensions
    {
        /// <summary>
        /// Encrypts all sensitive fields in a document model
        /// </summary>
        public static void EncryptSensitiveFields(this ModelAddDocument model)
        {
            model.Doc_Code = DeterministicEncryptionHelper.Encrypt(model.Doc_Code);
            model.File_Code = DeterministicEncryptionHelper.Encrypt(model.File_Code);
            model.Doc_Name = DeterministicEncryptionHelper.Encrypt(model.Doc_Name);
            model.Doc_Path = DeterministicEncryptionHelper.Encrypt(model.Doc_Path);
            model.Doc_Upload = DeterministicEncryptionHelper.Encrypt(model.Doc_Upload);
        }

        /// <summary>
        /// Decrypts all sensitive fields in a document model
        /// </summary>
        public static void DecryptSensitiveFields(this ModelAddDocument model)
        {
            model.Doc_Code = DeterministicEncryptionHelper.Decrypt(model.Doc_Code);
            model.File_Code = DeterministicEncryptionHelper.Decrypt(model.File_Code);
            model.Doc_Name = DeterministicEncryptionHelper.Decrypt(model.Doc_Name);
            model.Doc_Path = DeterministicEncryptionHelper.Decrypt(model.Doc_Path);
            model.Doc_Upload = DeterministicEncryptionHelper.Decrypt(model.Doc_Upload);
        }

        /// <summary>
        /// Decrypts sensitive fields in a list of documents
        /// </summary>
        public static void DecryptAll(this List<ModelDocument> documents)
        {
            foreach (var doc in documents)
            {
                doc.Doc_Code = DeterministicEncryptionHelper.Decrypt(doc.Doc_Code);
                doc.File_Code = DeterministicEncryptionHelper.Decrypt(doc.File_Code);
                doc.Doc_Name = DeterministicEncryptionHelper.Decrypt(doc.Doc_Name);
                doc.Doc_Path = DeterministicEncryptionHelper.Decrypt(doc.Doc_Path);
                doc.Doc_Upload = DeterministicEncryptionHelper.Decrypt(doc.Doc_Upload);
            }
        }
    }

    /// <summary>
    /// SIMPLIFIED USAGE with Extension Methods
    /// </summary>
    public class DocumentBAL_Simplified
    {
        public string AddDocument(ModelAddDocument model)
        {
            model.EncryptSensitiveFields(); // One line!
            return DAL.InsertDocument(model);
        }

        public List<ModelDocument> GetDocuments(string fileCode)
        {
            string encryptedCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            var documents = DAL.GetDocumentsByFileCode(encryptedCode);
            documents.DecryptAll(); // One line!
            return documents;
        }
        
        private static dynamic DAL = null;
    }

    // ============================================================
    // Mock Model Classes (for example only)
    // ============================================================
    public class ModelAddDocument
    {
        public string Doc_Code { get; set; }
        public string File_Code { get; set; }
        public string Doc_Name { get; set; }
        public string Doc_Path { get; set; }
        public string Doc_Upload { get; set; }
    }

    public class ModelDocument : ModelAddDocument
    {
        public int Row_ID { get; set; }
        public string Doc_Title { get; set; }
    }

    public class ModelFileWithDocuments
    {
        public string File_Code { get; set; }
        public string Doc_Code { get; set; }
        public string Doc_Name { get; set; }
        public string Doc_Path { get; set; }
        public string Doc_Upload { get; set; }
    }

    public class DropdownModel
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}
