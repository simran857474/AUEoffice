using Eoffice.BAL;
using Eoffice.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.WebPages;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace Eoffice.Controllers
{
    [SessionExpireFilter]
    public class UserController : Controller
    {
        private readonly string _cs = ConfigurationManager.ConnectionStrings["DBLayer"].ConnectionString;
        protected string empid;

        // Override OnActionExecuting to initialize empid before any action runs
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Initialize empid from session
            if (Session["UserName"] != null)
            {
                empid = Session["UserName"].ToString();
            }
        }
        UserBAL bal = new UserBAL();
        // GET: User
        public ActionResult Index()
        {
            DataSet dsCountData = new DataSet();
            dsCountData = bal.FN_ExecuteQuerySingle("proc_file_count_06092024 '" + Session["UserName"].ToString() + "' ");
            return View(dsCountData);
        }

        #region Create Document
        public ActionResult CreateDocument(string doc_code = null)
        {
            ViewBag.DocType = bal.BindDocType();
            ViewBag.DeliveryMode = bal.BindDeliveryMode();
            ViewBag.PurposeType = bal.BindPurposeType();

            ModelAddDocument obj = new ModelAddDocument();
            obj.CreatedBy = Session["UserName"].ToString();

            var documents = bal.GetDocumentsList(obj);

            ModelAddDocument document = null;
            if (!string.IsNullOrEmpty(doc_code))
            {

                document = documents.FirstOrDefault(d => d.doc_code == doc_code);
            }

            ViewBag.EditDocument = document;

            return View(documents);
        }

        [HttpPost]
        public ActionResult CreateDocument(ModelAddDocument obj)
        {
            string FileName = string.Empty, FileExtension = string.Empty;
            TempData["doc_path"] = string.Empty;
            TempData["file_name"] = string.Empty;

            HttpPostedFileBase file = Request.Files["Doc_File"];
            if (file != null && file.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads/Temp_Doc/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                FileName = System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".pdf";

                string fullPath = Path.Combine(uploadsFolder, FileName);
                file.SaveAs(fullPath);

                TempData["file_name"] = FileName.ToString();
                TempData["doc_path"] = Server.MapPath("~/Uploads/Temp_Doc/") + FileName;
            }

            string path = TempData["doc_path"].ToString();
            if (path == string.Empty)
            {
                TempData["msg"] = "Please upload file.";
                return RedirectToAction("CreateDocument");
            }

            obj.TotalPages = GetNumberOfPdfPages(path);

            if (TempData["file_name"].ToString() != null)
            {
                if (TempData["doc_path"].ToString() != null)
                {
                    string uploadsFolder = Server.MapPath("~/Uploads/CreatedFile/");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string destinationPath = Path.Combine(Server.MapPath("~/Uploads/CreatedFile/"), TempData["file_name"].ToString());
                    System.IO.File.Copy(TempData["doc_path"].ToString(), destinationPath, true);
                }
                obj.Doc_Lang = "English";
                obj.Doc_Upload = TempData["file_name"].ToString();
                obj.isactive = "1";
                obj.Status_Flag = "0";
                obj.Machine_IP = Session["Ip_Address"].ToString();
                obj.Forwarded_By = Session["EmpID"].ToString();
                obj.ParamType = "I";
                obj.CreatedBy = Session["UserName"].ToString();
                obj.FinYear = Convert.ToString(Session["FINYEAR"]);
                obj.Doc_Title = string.Empty;
                obj.Doc_Section = string.Empty;
                obj.Doc_Keyword = string.Empty;
                obj.Emp_Code = string.Empty;
                obj.Dep_Code = string.Empty;

                if (!string.IsNullOrEmpty(obj.recieved_dt))
                {
                    obj.recieved_dt = DateTime.ParseExact(obj.recieved_dt, "yyyy-MM-dd", null).ToString("yyyy/MM/dd");
                }

                if (!string.IsNullOrEmpty(obj.letter_dt))
                {
                    obj.letter_dt = DateTime.ParseExact(obj.letter_dt, "yyyy-MM-dd", null).ToString("yyyy/MM/dd");
                }
            }
            else
            {
                TempData["msg"] = "Please upload file.";
                return RedirectToAction("CreateDocument");
            }

            DataTable dt = bal.GenerateDocCode(obj.Doc_TypeName, obj.Doc_Lang);

            if (dt == null || dt.Rows.Count <= 0)
            {
                TempData["msg"] = "Error while generating Doc Code.";
                return RedirectToAction("CreateDocument");
            }

            obj.doc_code = dt.Rows[0][0]?.ToString();

            bool result = bal.InsertAddDocument(obj);

            if (result)
                TempData["msg"] = "Document inserted successfully.";
            else
                TempData["msg"] = "Document not inserted.";

            return RedirectToAction("CreateDocument");
        }


        [HttpPost]
        public ActionResult EditDocument(ModelAddDocument up)
        {
            string FileName = string.Empty, FileExtension = string.Empty;
            TempData["doc_path"] = string.Empty;
            TempData["file_name"] = string.Empty;
            HttpPostedFileBase file = Request.Files["Doc_File"];
            if (file != null && file.ContentLength > 0)
            {
                //string[] filesnames = Directory.GetFiles(Server.MapPath("~/Uploads/Temp_Doc/"));

                //foreach(string files in filesnames)
                //{
                //    if (files.Contains(TempData["file_name"].ToString()))
                //    {
                //        TempData["msg"] = "File already exists.";
                //        return RedirectToAction("CreateDocument");
                //    }
                //}

                string uploadsFolder = Server.MapPath("~/Uploads/Temp_Doc/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                FileName = System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".pdf";

                string fullPath = Path.Combine(uploadsFolder, FileName);
                file.SaveAs(fullPath);

                TempData["file_name"] = FileName.ToString();
                TempData["doc_path"] = Server.MapPath("~/Uploads/Temp_Doc/") + FileName;
            }

            if (TempData["doc_path"].ToString() != "")
            {
                string uploadsFolder = Server.MapPath("~/Uploads/CreatedFile/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string destinationPath = Path.Combine(Server.MapPath("~/Uploads/CreatedFile/"), TempData["file_name"].ToString());
                System.IO.File.Copy(TempData["doc_path"].ToString(), destinationPath, true);

                up.Doc_Upload = TempData["file_name"].ToString();
                up.TotalPages = GetNumberOfPdfPages(TempData["doc_path"].ToString());
            }

            up.Doc_Lang = "English";
            up.Machine_IP = Session["Ip_Address"].ToString();
            up.Forwarded_By = Session["EmpID"].ToString();
            up.ParamType = "UP";
            up.Status_Flag = "0";
            up.isactive = "1";
            up.FinYear = Convert.ToString(Session["FINYEAR"]);
            up.Doc_Title = string.Empty;
            up.Doc_Section = string.Empty;
            up.Doc_Keyword = string.Empty;
            up.Emp_Code = string.Empty;
            up.Dep_Code = string.Empty;

            if (!string.IsNullOrEmpty(up.recieved_dt?.Trim()))
            {
                up.recieved_dt = DateTime.ParseExact(up.recieved_dt, "yyyy-MM-dd", null).ToString("yyyy/MM/dd");
            }

            if (!string.IsNullOrEmpty(up.letter_dt?.Trim()))
            {
                up.letter_dt = DateTime.ParseExact(up.letter_dt, "yyyy-MM-dd", null).ToString("yyyy/MM/dd");
            }

            bool result = bal.InsertAddDocument(up);
            if (result)
            {
                TempData["msg"] = "Document updated successfully.";
                return RedirectToAction("CreateDocument");
            }

            TempData["msg"] = "Document not inserted.";
            return RedirectToAction("CreateDocument");
        }

        #endregion EditDocument

        #region CreateFile
        public ActionResult CreateFile(string File_Code = null)
        {
            string depcode = Session["Dep_Code"].ToString() == "001" ? "001" : "";

            ModelCreateFile obj = new ModelCreateFile();
            obj.CreatedBy = Session["UserName"].ToString();
            obj.TableID = "0";

            ViewBag.DocPriority = bal.BindDocPriority();
            ViewBag.FileCategory = bal.BindFileCategory(depcode);
            ViewBag.FileType = bal.BindFileType();
            ViewBag.AutoNumber = bal.GetAutoMaxId(Session["Dep_Code"].ToString(), Session["Scode"].ToString());
            ViewBag.DepName = bal.GetDepName(Session["Dep_Code"].ToString());
            ViewBag.SecName = bal.GetSecName(Session["Dep_Code"].ToString(), Session["Scode"].ToString());

            var Files = bal.GetFileList(obj);

            ModelCreateFile file = null;
            if (!string.IsNullOrEmpty(File_Code))
            {

                file = Files.FirstOrDefault(d => d.File_Code == File_Code);
            }

            ViewBag.EditFile = file;

            return View(Files);
        }

        [HttpPost]
        public ActionResult CreateFile(ModelCreateFile obj)
        {
            string doctypename = obj.OtherDoc_Type == null ? obj.DocType_Name.Split('|')[1].Trim() : obj.OtherDoc_Type.Trim();

            obj.File_Code = obj.File_DeptName.Trim().Split('|')[1].Trim() + "/" + obj.File_SectionName.Trim().Split('|')[1].Trim() + "/" +
              doctypename + "/" + DateTime.Now.Month.ToString()
                + "/" + obj.AutoNumber + "/" + DateTime.Now.Year.ToString();

            obj.File_Title = string.Empty;
            obj.File_Lang = "English";
            obj.File_Section = Session["Scode"].ToString();
            obj.isactive = "1";
            obj.Status_Flag = "0";
            obj.TableID = "0";
            obj.Machine_IP = Session["Ip_Address"].ToString();
            obj.ParamType = "I";
            obj.FinYear = Convert.ToString(Session["FINYEAR"]);
            obj.Year = DateTime.Now.Year.ToString();
            obj.File_Dept = Session["Dep_Code"].ToString();
            obj.File_ID = obj.AutoNumber;
            obj.ED_ROW_ID = Session["ED_Row_ID"].ToString();
            obj.CreatedBy = Session["UserName"].ToString();
            bool result = bal.InsertCreateFile(obj);

            if (result)
                TempData["msg"] = "File created successfully.";
            else
                TempData["msg"] = "File not created.";

            return RedirectToAction("CreateFile");
        }

        [HttpPost]
        public ActionResult EditFile(ModelCreateFile obj)
        {
            obj.File_Title = "";
            obj.File_Lang = "English";
            obj.File_Remark = null;
            obj.isactive = "1";
            obj.UpdatedBy = Session["UserName"] == null ? string.Empty : Session["UserName"].ToString();
            obj.FinYear = Convert.ToString(Session["FINYEAR"]);
            obj.Year = DateTime.Now.Year.ToString();
            obj.Status_Flag = "0";
            obj.TableID = "0";
            obj.Machine_IP = Session["Ip_Address"].ToString();
            obj.ParamType = "U";

            bool result = bal.InsertCreateFile(obj);

            if (result)
                TempData["msg"] = "File updated successfully.";
            else
                TempData["msg"] = "File not updated.";

            return RedirectToAction("CreateFile");
        }

        public ActionResult DeleteFile(string File_Code = null)
        {
            ModelCreateFile obj = new ModelCreateFile
            {
                File_Code = File_Code,
                ParamType = "D",
                CreatedBy = Session["UserName"] == null ? string.Empty : Session["UserName"].ToString(),
            };

            bool result = bal.InsertCreateFile(obj);
            if (result)
            {
                TempData["msg"] = "File deleted successfully.";
                return RedirectToAction("CreateFile");

            }
            TempData["msg"] = "File not deleted.";
            return RedirectToAction("CreateFile");
        }
        #endregion

        [HttpPost]
        public JsonResult GetFileSubCategoryByFileCategory(string filecategory)
        {

            var subcategories = bal.BindFileSubCategories(filecategory);

            var result = subcategories.Select(x => new
            {
                Value = x.Value,
                Text = x.Text
            });

            return Json(result);
        }
       

        public ActionResult HandOverFile()
        {
            string username = Session["UserName"]?.ToString();
            string roleCode = Session["RoleCode"]?.ToString();
            string empId = Session["EmpID"]?.ToString();
            string forwardedTo = Session["UserName"]?.ToString();
            string Dep_Code = Session["Dep_Code"]?.ToString();
            string Sec_Code = Session["Scode"]?.ToString();

            ViewBag.SectionHeads = bal.BindSectionHeads(Dep_Code, Sec_Code, empId);

            DataTable dt = bal.GetFileCounts(username, roleCode, empId);

            if (dt.Rows.Count > 0)
            {
                ViewBag.InboxCount = dt.Rows[0]["inboxcount"].ToString();
                ViewBag.OpenFileCount = dt.Rows[0]["openfilecount"].ToString();
                ViewBag.ApproveFileCount = dt.Rows[0]["appfilecount"].ToString();
            }
            else
            {
                ViewBag.InboxCount = "0";
                ViewBag.OpenFileCount = "0";
                ViewBag.ApproveFileCount = "0";
            }


            ViewBag.ApprovedFiles = bal.GetApprovedFiles(forwardedTo);
            ViewBag.OpenedFiles = bal.GetOpenedFiles(forwardedTo);
            ViewBag.PendingFiles = bal.GetPendingFiles(forwardedTo);



            return View();
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult HandOverFiles(string handoverTo, List<Dictionary<string, string>> files)
        {
            try
            {
                if (string.IsNullOrEmpty(handoverTo))
                    return Json(new { success = false, message = "Please select handover person" });

                string forwardedBy = Session["UserName"]?.ToString() ?? "";
                string forwardedTo = handoverTo;
                string ip = Request.UserHostAddress;
                string fromRowId = Session["ED_Row_ID"]?.ToString();

                int inserted = 0;

                foreach (var f in files)
                {
                    string fileCode = f.ContainsKey("fileCode") ? f["fileCode"] : null;
                    string flag = f.ContainsKey("flag") ? f["flag"] : null;

                    if (!string.IsNullOrEmpty(fileCode))
                    {
                        bool ok = bal.HandOverFile(fileCode, forwardedBy, forwardedTo, ip, flag, fromRowId);
                        if (ok) inserted++;
                    }
                }


                if (inserted > 0)
                    return Json(new { success = true, message = "Files handed over successfully." });
                else
                    return Json(new { success = false, message = "No files handed over." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public int GetNumberOfPdfPages(string filePath)
        {
            try
            {
                string content = System.IO.File.ReadAllText(filePath);

                // Match "/Type /Page" or "/Type/Page"
                var regex = new System.Text.RegularExpressions.Regex(@"/Type\s*/Page\b");

                return regex.Matches(content).Count;
            }
            catch (Exception ex)
            {
                // log or handle error
                return -1;
            }
        }


        public ActionResult Frm_InBoxList()
        {
            ModelAddDocument obj = new ModelAddDocument();
            obj.ParamType = "SEC";
            obj.doc_code = "";
            obj.Forwarded_To = Session["UserName"] == null ? string.Empty : Session["UserName"].ToString();
            ViewBag.FinancialYear = bal.BindFinancialYear();
            ViewBag.Sections = bal.BindAllSections(obj);
            var AllFinYears = bal.BindFinancialYear();
            DataSet ds = new DataSet();
            ds = bal.FN_ExecuteQuerySingle("proc_inbox_files '" + Session["UserName"].ToString() + "','" + AllFinYears.FirstOrDefault()?.Text + "' ");
            return View(ds);
        }


        public ActionResult OpenFile(string fileCode, string rowId)
        {
            try
            {
                // Update flag if required
                bool result = bal.UpdateCBFlagIfNeeded(fileCode, rowId);

                if (!result)
                {
                    TempData["Error"] = "File not found.";
                    return RedirectToAction("Frm_InBoxList");
                }

                // Store back URL 
                //Session["BackPageUrl"] = Url.Action("Frm_InBoxList", "User");

                // Redirect to Noting Page
                return RedirectToAction("Noting_Page", "User", new { fileCode = fileCode });
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong.";
                return RedirectToAction("Frm_InBoxList");
            }
        }


        public ActionResult Frm_OutBoxList()
        {
            DataSet ds = new DataSet();
            ds = bal.FN_ExecuteQuerySingle("proc_outbox_files '" + Session["UserName"].ToString() + "' ");

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (!ds.Tables[0].Columns.Contains("CB_Flag"))
                {
                    ds.Tables[0].Columns.Add("CB_Flag");
                }

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string rowId = row["Row_ID"].ToString();
                    string fileCode = row["File_Code"].ToString();

                    // Call DB to get cb_flag
                    string cbFlag = bal.GetCallbackFlag(rowId, fileCode);

                    row["CB_Flag"] = cbFlag; // add dynamically (ensure column exists OR use new column)
                }
            }



            return View(ds);
        }


        // cannot returning a DataTable directly, convert it to a list of objects or dictionaries and return JsonResult
        [HttpGet]
        public JsonResult GetOutBoxFileHistory(string fileCode)
        {
            var dt = new DataTable();

            if (!string.IsNullOrWhiteSpace(fileCode))
            {
                dt = bal.GetOutBoxFileHistory(fileCode);
            }

            // Convert DataTable to List<object>
            var data = dt.AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                ForwardedFrom = row["from_f"].ToString(),
                ForwardedTo = row["Emp_Name"].ToString(),
                Date = row["forwarded_DT"].ToString(),
                Action = row["Doc_status"].ToString(),
                FileName = row["File_Code"].ToString(),
                SectionName = row["Sec_Name"].ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Frm_OpenedFiles()
        {
            //string emp_name = Session["EmpID"].ToString();
            DataTable dt = bal.GetOpenedFilesList(empid);
            return View(dt);
        }
        public ActionResult Frm_ReturnFiles()
        {
            DataTable dt = bal.GetReturnedFilesList(empid);
            return View(dt);
        }
        public ActionResult Frm_ApprovedFiles()
        {

            DataTable dt = bal.GetApprovedFilesList(empid);
            return View(dt);
        }
        public ActionResult Frm_CallBackFiles()
        {

            DataTable dt = bal.GetCallBackFiles(empid);
            return View(dt);
        }

        [HttpGet]
        public JsonResult DeleteCallBackFile(string FileCode = null)
        {
            if (!string.IsNullOrWhiteSpace(FileCode))
            {
                var obj = new ModelCreateFile
                {
                    ParamType = "D",
                    File_Code = FileCode,
                    CreatedBy = empid
                };

                bool result = bal.InsertCreateFile(obj);

                return Json(new { success = result }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HandOver_Files()
        {

            DataTable dt = bal.GetHandOverFiles(empid);
            return View(dt);
        }
      
        public ActionResult SignedFiles()
        {

            DataTable dt = bal.GetSignedFiles(empid);
            return View(dt);
        }

        public ActionResult Created_Files()
        {
            ModelFileCategory obj = new ModelFileCategory();
            obj.CreatedBy = Session["UserName"].ToString();
            obj.ParamType = "W";
            obj.TableID = "0";

            DataTable dt = bal.BindCreatedFilesList(obj);
            return View(dt);
        }
        //public ActionResult Files_History()
        //{
        //    DataTable dt = bal.GetFilesHistory(empid);
        //    return View(dt);
        //}


        public ActionResult Files_History()
        {
            try
            {
                // Bind financial year dropdown
                ViewBag.FinancialYear = bal.BindFinancialYear();

                // Get financial year values
                var AllFinYears = bal.BindFinancialYear();

                // Prepare DataSet
                DataSet ds = new DataSet();

                // Fix: Properly format the SQL query string with correct quotes and comma
                string userName = Session["UserName"]?.ToString();
                string finYear = AllFinYears?.FirstOrDefault()?.Text;

                // Debug: Store values in ViewBag for troubleshooting
                ViewBag.DebugUserName = userName;
                ViewBag.DebugFinYear = finYear;

                // Validate inputs
                if (string.IsNullOrEmpty(userName))
                {
                    ViewBag.ErrorMessage = "User session expired. Please login again.";
                    return View(new DataTable());
                }

                if (string.IsNullOrEmpty(finYear))
                {
                    ViewBag.ErrorMessage = "No financial year available.";
                    return View(new DataTable());
                }

                string query = $"proc_file_movement_history '{userName}', '{finYear}'";

                // Execute query
                ds = bal.FN_ExecuteQuerySingle(query);

                // Return the first table as the model if available
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return View(ds.Tables[0]);
                }
                else
                {
                    ViewBag.ErrorMessage = "No file history records found for the selected criteria.";
                    return View(new DataTable());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading file history: {ex.Message}";
                return View(new DataTable());
            }
        }

        [HttpGet]
        public JsonResult GetAttachedDocument(string fileCode)
        {
            if (string.IsNullOrWhiteSpace(fileCode))
                return Json(new { url = "", fileName = "" }, JsonRequestBehavior.AllowGet);

            try
            {
                DataSet ds = bal.FN_ExecuteQuerySingle($"USP_AttachedDocument '{DeterministicEncryptionHelper.Encrypt(fileCode)}'");

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (row["url"] != DBNull.Value)
                        {
                            string currentUrl = row["url"].ToString();

                            if (!string.IsNullOrEmpty(currentUrl))
                            {
                                row["url"] = "CreatedFile/" + currentUrl;
                            }
                        }
                    }

                    string url = ds.Tables[0].Rows[0]["url"]?.ToString().Trim();
                    string fileName = ds.Tables[0].Rows[0]["Doc_Upload_rest"]?.ToString().Trim();

                    return Json(new { url = url, fileName = fileName }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { url = "", fileName = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { url = "", fileName = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Created_Document()
        {
            ModelAddDocument obj = new ModelAddDocument();
            obj.ParamType = "X";
            obj.CreatedBy = Session["UserName"].ToString();

            DataTable dt = bal.BindCreatedDocumentList(obj);

            return View(dt);
        }


        public ActionResult Internal_Movement()
        {

            DataSet ds = new DataSet();
            ds = bal.FN_ExecuteQuerySingle("proc_get_InternalMovement '" + Session["UserName"].ToString() + "' ");

            if (ds.Tables.Count > 0)
            {

                return View(ds.Tables[0]);
            }
            return View();
        }


        public ActionResult Noting_Page(string fileCode = null)
        {
            ViewBag.EstTypeList = bal.BindEstType();
            ViewBag.FileCode = fileCode;
            ViewBag.InternalMovementOfficers = bal.BindInternalMovementForwardingOfficers(empid);
            Session["Paralist"] = null;
            Session["final"] = null;
            Session["F_by"] = "";
            Session["forwarded"] = "";
            Session["DraftCount"] = "";
            Session["DisableNotingButtons"] = "";

            DisplayDraftNoting(fileCode);

            ViewBag.DisableButtons = Session["DisableNotingButtons"]?.ToString();

            DataTable dt1 = bal.GetDepSecNames(fileCode);

            // dep sec name
            ViewBag.DepSecName = dt1.Rows[0][0].ToString() + " / " + dt1.Rows[0][1].ToString();
            //ViewBag.DepSecName = "test";

            DataTable attachedDocs = bal.GetAttachedDocs(fileCode);

            // append folder name before file name.
            if (attachedDocs.Columns.Contains("url"))
            {
                foreach (DataRow row in attachedDocs.Rows)
                {
                    if (row["url"] != DBNull.Value)
                    {
                        string currentUrl = row["url"].ToString();

                        if (!string.IsNullOrEmpty(currentUrl))
                        {
                            row["url"] = "../Master/CreatedFile/" + currentUrl;
                        }
                    }
                }
            }

            if (attachedDocs.Rows.Count > 0)
            {
                ViewBag.AttachedDocs = attachedDocs;
                var mergedDocs = bal.GetMergedDocs(fileCode);
                ViewBag.MergedDocs = mergedDocs;
                ViewBag.MergedDocURL = attachedDocs.Rows[0]["url"].ToString();
                //ViewBag.DefaultDocId = mergedDocs.Skip(1).FirstOrDefault()?.Value; // First after "--All--"
            }
            else
            {
                ViewBag.AttachedDocs = null;
                ViewBag.MergedDocs = new List<SelectListItem>();
                //ViewBag.DefaultDocId = 0;
            }

            // Check if user has already approved/reverted this file
            string userName = Session["UserName"]?.ToString();
            string desCode = Session["Des_Code"]?.ToString();
            
            // Check for specific designation codes (VC, Registrar, etc.)
            bool isApprovalAuthority = (desCode == "001" || desCode == "033" || userName == "vcsec_vco");
            ViewBag.IsApprovalAuthority = isApprovalAuthority;
            
            if (isApprovalAuthority && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(fileCode))
            {
                DataSet dsCheck = bal.CheckApprovedRevert(fileCode, userName);
                bool hasAlreadyActioned = (dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0);
                ViewBag.HasAlreadyActioned = hasAlreadyActioned;
            }
            else
            {
                ViewBag.HasAlreadyActioned = false;
            }

            // get notings for this file from db
            //List<ModelNoting> notes = new List<ModelNoting>();

            DataTable dt = bal.GetNotingDetails(fileCode);
            ViewBag.Filedt = bal.GetFileDetails(fileCode);

            ViewBag.ActiveTab = TempData["ActiveTab"]?.ToString() ?? "noting"; // default noting tab

            //if (dt.Rows.Count > 0)
            //{
            //    notes.Add(new ModelNoting
            //    {
            //        File_Code = dt.Rows[0]["File_Code"].ToString(),
            //        Doc_Code = dt.Rows[0]["Doc_Code"].ToString(),
            //        Comments = dt.Rows[0]["Comments"].ToString(),
            //        Doc_Upload = dt.Rows[0]["Doc_Upload"].ToString(),
            //        Note_Desc = dt.Rows[0]["CreatedDate"].ToString(),
            //        Note_Type = dt.Rows[0]["CreatedDate"].ToString()
            //    });
            //}

            return View(dt);
        }

        [HttpPost]
        public JsonResult CheckApprovedRevertStatus(string fileCode)
        {
            try
            {
                string userName = Session["UserName"]?.ToString();
                
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(fileCode))
                {
                    return Json(new { success = false, hasActioned = false });
                }

                DataSet ds = bal.CheckApprovedRevert(fileCode, userName);
                bool hasActioned = (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0);

                return Json(new { success = true, hasActioned = hasActioned });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetFileDetails(string fileCode)
        {
            var dt = bal.GetFileDetails(fileCode);

            // Convert DataTable → List<object>
            var result = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(new
                {
                    Dep_Name = row["Dep_Name"].ToString(),
                    Section = row["Section"].ToString(),
                    FileCat_Name = row["FileCat_Name"].ToString(),
                    FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                    File_Desc = row["File_Desc"].ToString(),
                    DocPrior_Name = row["DocPrior_Name"].ToString(),
                    Name = row["Name"].ToString(),
                    date = row["date"].ToString(),
                    time = row["time"].ToString()
                });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetNotingDetails(string fileCode)
        {
            // BAL → DAL → Stored Procedure
            var dt = bal.GetNotingDetails(fileCode);

            var result = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(new
                {
                    Note_Desc = row["Note_Desc"].ToString()
                });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetDepartmentsByEstType(int estTypeCode)
        {
            var departments = bal.BindDepartment(estTypeCode);

            var result = departments.Select(x => new
            {
                Value = x.Value,
                Text = x.Text
            });

            return Json(result);
        }

        [HttpPost]
        public JsonResult GetSectionsByDepartment(string Dep_Code, string EstTypeCode)
        {
            var sections = bal.BindSection(Dep_Code, EstTypeCode);

            var result = sections.Select(x => new
            {
                Value = x.Value,
                Text = x.Text
            });

            return Json(result);
        }

        [HttpPost]
        public JsonResult GetForwardToList(string secCode, string deptCode)
        {
            // Get current logged-in username from session
            string currentUser = Session["UserName"]?.ToString() ?? "";

            var forwardToList = bal.GetForwardToList(secCode, deptCode, currentUser);

            return Json(forwardToList);
        }

        [HttpGet]
        public JsonResult GetFileHistory(string fileCode)
        {
            var dt = new DataTable();

            if (!string.IsNullOrWhiteSpace(fileCode))
            {
                dt = bal.GetOutBoxFileHistory(fileCode);
            }

            // Convert DataTable to List<object>
            var data = dt.AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                FileDescription = row["File_Desc"].ToString(),
                SentFrom = row["from_f"].ToString(),
                SentTo = row["Emp_Name"].ToString(),
                ForwardedDT = row["forwarded_DT"].ToString(),
                ActionDate = row["Action_Date"].ToString(),
                Status = row["Doc_status"].ToString(),
                Remark = row["Remark"].ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BindNotingDocuments(string fileCode)
        {
            var dt = new DataTable();

            if (!string.IsNullOrWhiteSpace(fileCode))
            {
                dt = bal.BindNotingDocuments(fileCode, Session["EmpID"].ToString());
            }

            // Convert DataTable to List<object>
            var data = dt.AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                DocNo = row["Doc_Code"].ToString(),
                DocType = row["DocType_Name"].ToString(),
                Subject = row["Doc_Desc"].ToString(),
                ReferenceNo = row["Doc_Ref"].ToString(),
                ReceivedFrom = row["Doc_Auth"].ToString(),
                Purpose = row["Purpose_desc"].ToString(),
                CreatedDateTime = row["Created_DT"].ToString(),
                DocName = row["Doc_Upload"].ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public JsonResult GetDocumentDetails(string docCode, string docText, string fileCode, int selectedIndex)
        //{
        //    try
        //    {

        //        int get_page = 0, getoriginal = 0, tothepage = 0;

        //        // total page for selected doc
        //        string str = $"select TotalPageNo from M_Document where row_id='{docCode.Trim()}'";
        //        DataTable dt = bal.EQ(str);
        //        if (dt.Rows.Count > 0)
        //            get_page = Convert.ToInt32(dt.Rows[0][0]);

        //        // total page for first doc
        //        string str1 = $"select TotalPageNo from M_Document where row_id=(select top 1 row_id from M_Document where File_Code='{fileCode}' order by row_id asc)";
        //        DataTable dt1 = bal.EQ(str1);
        //        if (dt1.Rows.Count > 0)
        //            getoriginal = Convert.ToInt32(dt1.Rows[0][0]);

        //        // loop through docs
        //        for (int i = 1; i <= selectedIndex; i++)
        //        {
        //            string s = $"select TotalPageNo from M_Document where row_id='{docCode}'";
        //            DataTable d = bal.EQ(s);
        //            if (d.Rows.Count > 0)
        //                tothepage += Convert.ToInt32(d.Rows[0][0]);
        //        }

        //        int no = get_page - 1;
        //        tothepage -= no;

        //        string totalPages = $"Total page from {tothepage} to {tothepage + (get_page - 1)}";

        //        // fetch document details
        //        string st = $"select Row_ID,Name,Doc_Upload,m.Created_By,m.forwarded_BY,isnull(CB_Date,'') 'CB_Date' " +
        //                    $"from M_Document m join Utility_MUser ut on ut.LoginName = m.Created_By " +
        //                    $"where Doc_Code='{docText}'";
        //        DataTable dt2 = bal.EQ(st);

        //        string docName = "", createdBy = "", fBy = "", cbDate = "";
        //        if (dt2.Rows.Count > 0)
        //        {
        //            docName = dt2.Rows[0]["doc_upload"].ToString();
        //            createdBy = dt2.Rows[0]["Name"].ToString();
        //            Session["C_by"] = dt2.Rows[0]["Created_By"].ToString();
        //            Session["F_by"] = dt2.Rows[0]["forwarded_BY"].ToString();
        //            Session["CB_Date"] = dt2.Rows[0]["CB_Date"].ToString();
        //            fBy = dt2.Rows[0]["forwarded_BY"].ToString();
        //            cbDate = dt2.Rows[0]["CB_Date"].ToString();
        //        }

        //        int startPage = tothepage;
        //        int endPage = tothepage + (get_page - 1);

        //        return Json(new
        //        {
        //            success = true,
        //            totalPages = $"Total page from {startPage} to {endPage}",
        //            startPage,
        //            endPage,
        //            docName,
        //            createdBy,
        //            forwardedBy = fBy,
        //            cbDate
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        [HttpPost]
        public JsonResult GetDocumentDetails(string fileCode, int selectedIndex, List<Dictionary<string, object>> docs)
        {
            try
            {

                int get_page = 0, getoriginal = 0, tothepage = 0;

                // get the selected doc from docs list
                var selectedDoc = docs[selectedIndex];
                string docCode = selectedDoc["docCode"].ToString();
                string docText = selectedDoc["docText"].ToString();

                // total page for selected doc
                string str = $"select TotalPageNo from M_Document where row_id='{docCode.Trim()}'";
                DataTable dt = bal.EQ(str);
                if (dt.Rows.Count > 0)
                    get_page = Convert.ToInt32(dt.Rows[0][0]);

                // total page for first doc
                string strr = "select TotalPageNo from M_Document  where row_id='" + docs[1]["docCode"].ToString().Trim() + "'";
                DataTable dtt11 = bal.EQ(strr);
                if (dtt11.Rows.Count > 0)
                {
                    getoriginal = int.Parse(dtt11.Rows[0][0].ToString());
                }


                // loop through docs till selected index
                for (int i = 1; i <= docs.Count; i++)
                {
                    string s = $"SELECT TotalPageNo FROM M_Document WHERE row_id='{docs[i]["docCode"].ToString().Trim()}'";
                    DataTable d = bal.EQ(s);
                    if (d.Rows.Count > 0)
                        tothepage += Convert.ToInt32(d.Rows[0][0]);

                    if (selectedIndex == i)
                    {
                        tothepage -= (get_page - 1);
                        break;
                    }
                }


                //string str1 = $"select TotalPageNo from M_Document where row_id=(select top 1 row_id from M_Document where File_Code='{fileCode}' order by row_id asc)";
                //DataTable dt1 = bal.EQ(str1);
                //if (dt1.Rows.Count > 0)
                //    getoriginal = Convert.ToInt32(dt1.Rows[0][0]);

                //// loop through docs
                //for (int i = 1; i <= selectedIndex; i++)
                //{
                //    string s = $"select TotalPageNo from M_Document where row_id='{docCode}'";
                //    DataTable d = bal.EQ(s);
                //    if (d.Rows.Count > 0)
                //        tothepage += Convert.ToInt32(d.Rows[0][0]);
                //}

                //int no = get_page - 1;
                //tothepage -= no;

                string totalPages = $"Total page from {tothepage} to {tothepage + (get_page - 1)}";

                // fetch document details
                string st = $"select Row_ID,Name,Doc_Upload,m.Created_By,m.forwarded_BY,isnull(CB_Date,'') 'CB_Date' " +
                            $"from M_Document m join Utility_MUser ut on ut.LoginName = m.Created_By " +
                            $"where Doc_Code='{DeterministicEncryptionHelper.Encrypt(docText)}'";
                DataTable dt2 = bal.EQ(st);

                string docName = "", createdBy = "", fBy = "", cbDate = "";
                if (dt2.Rows.Count > 0)
                {
                    docName = dt2.Rows[0]["doc_upload"].ToString();
                    createdBy = dt2.Rows[0]["Name"].ToString();
                    Session["C_by"] = dt2.Rows[0]["Created_By"].ToString();
                    Session["F_by"] = dt2.Rows[0]["forwarded_BY"].ToString();
                    Session["CB_Date"] = dt2.Rows[0]["CB_Date"].ToString();
                    fBy = dt2.Rows[0]["forwarded_BY"].ToString();
                    cbDate = dt2.Rows[0]["CB_Date"].ToString();

                }

                int startPage = tothepage;
                int endPage = tothepage + (get_page - 1);

                bool canDelete = false;

                if (Session["C_by"]?.ToString() == Session["UserName"]?.ToString() &&
                    string.IsNullOrEmpty(Session["F_by"]?.ToString()) &&
                    !(Session["final"] != null && Session["final"].ToString() == "ok"))
                {
                    canDelete = true;
                }

                return Json(new
                {
                    success = true,
                    totalPages = $"Total page from {startPage} to {endPage}",
                    startPage,
                    endPage,
                    docName,
                    createdBy,
                    forwardedBy = fBy,
                    cbDate,
                    canDelete = canDelete
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Merge, delete files

        [HttpPost]
        public ActionResult MergeDocuments(string fileCode, List<string> selectedDocs)
        {
            try
            {
                string destpath = "";
                string sourceLocation = "";
                string attacheddoc = ""; string attachdocode = "";

                string final = Session["final"] != null ? Session["final"].ToString() : "";

                if (selectedDocs == null || !selectedDocs.Any())
                {
                    return Json(new { success = false, message = "Please select at least one document." });
                }

                DataTable dt = bal.GetNotingDetails(fileCode);

                //// check whether any noting was saved or not. if saved not allow file merging
                //if (dt.Rows.Count > 0)
                //    return Json(new { success = false, message = "Document cannot be merged after final noting is saved." });

                // check if final save is done or not. if saved not allow file merging
                if (final == "ok")
                    return Json(new { success = false, message = "Document cannot be merged after final submission of noting." });

                // Each selectedDocs item looks like "123|Agreement.pdf"
                var docList = selectedDocs
                    .Select(d => new
                    {
                        DocNo = d.Split('|')[0],
                        DocName = d.Split('|')[1]
                    })
                    .ToList();

                // check directory - if not then create
                string destFolder = Server.MapPath($"~/Uploads/MergerFile/Dest{Session["UserID"].ToString()}/");
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                else
                {
                    // delete existing files

                    Directory.Delete(destFolder, true);
                    Directory.CreateDirectory(destFolder);
                }

                //  call db to check maindoc for this filecode

                string docnameNew = "";
                string str2 = "select Doc_Upload,Doc_Code,DisplayFile from M_Document where Main_doc='Y'  and  File_Code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
                DataTable dt3 = bal.EQ(str2);

                string mainFilePath = "";

                if (dt3.Rows.Count > 0)
                {
                    //Adding main document in to the source folder
                    //for (int i = 0; i < dt3.Rows.Count; i++)
                    //{
                    destpath = Server.MapPath("~/Uploads/MergerFile/Dest" + Session["UserID"].ToString() + "/" + dt3.Rows[0]["DisplayFile"].ToString());
                    sourceLocation = Server.MapPath("~/Uploads/CreatedFile/" + dt3.Rows[0]["DisplayFile"].ToString());
                    System.IO.File.Copy(sourceLocation, destpath, true);
                    //}
                    attacheddoc = dt3.Rows[0][0].ToString();
                    attachdocode = dt3.Rows[0][1].ToString();
                    docnameNew = dt3.Rows[0]["DisplayFile"].ToString();
                    //Adding main document in to the source folder


                    // get main document
                    mainFilePath = Path.Combine(destFolder, dt3.Rows[0]["DisplayFile"].ToString());
                }
                else
                {
                    // if not found, get first docname in attachedoc
                    attacheddoc = docList.FirstOrDefault()?.DocName.ToString();

                }

                // create array of selected docs size and storing docno in it
                string[] docNos = docList.Select(d => d.DocNo).ToArray();

                // Copy selected docs into temp folder
                List<string> pdfFiles = new List<string>();
                foreach (var doc in selectedDocs)
                {
                    string docFileName = doc.Split('|')[1];

                    string source = Server.MapPath("~/Uploads/CreatedFile/" + docFileName);
                    string dest = Path.Combine(destFolder, docFileName);
                    System.IO.File.Copy(source, dest, true);

                    // storing full path of files in mergedfile in this list
                    pdfFiles.Add(dest);
                }

                // Ensure main document is included FIRST
                if (!string.IsNullOrEmpty(mainFilePath) && !pdfFiles.Contains(mainFilePath))
                {
                    pdfFiles.Insert(0, mainFilePath);
                }

                // Maintain FILE ORDER
                pdfFiles = pdfFiles
                .OrderBy(f => new FileInfo(f).CreationTime)
                .ToList();

                // Merge PDFs; create ouput filename with username and storing in mergedfile first
                string outputFileName = docnameNew == "" ? $"{Session["userName"].ToString()}-{attacheddoc}" : docnameNew;
                string outputPath = Server.MapPath("~/Uploads/MergerFile/MergedFiles/" + outputFileName);
                string outputDirectory = Path.GetDirectoryName(outputPath);

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                using (FileStream stream = new FileStream(outputPath, FileMode.Create))
                using (Document doc = new Document())
                using (PdfCopy writer = new PdfCopy(doc, stream))
                {
                    //  open document 
                    doc.Open();

                    // search for username in pdfFiles list if found set flag=true else false
                    bool flag = false;
                    string userName = Session["userName"].ToString();

                    foreach (string filePath in pdfFiles)
                    {
                        if (filePath.Contains(userName))
                        {
                            flag = true;
                            break;
                        }
                    }

                    // reading each file in pdfFiles list and write it
                    foreach (var file in pdfFiles)
                    {
                        PdfReader reader = new PdfReader(file);
                        reader.ConsolidateNamedDestinations();
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            writer.AddPage(writer.GetImportedPage(reader, i));
                        }
                        reader.Close();
                    }
                }

                // Save merged copy to CreatedFile folder
                string createdPath = Server.MapPath("~/Uploads/CreatedFile/" + outputFileName);
                System.IO.File.Copy(outputPath, createdPath, true);

                // Delete the temp folder
                System.IO.File.Delete(outputPath);

                // Call DB to update flag
                int result = bal.UpdateFlag(docNos, outputFileName, fileCode, Session["EmpID"].ToString());

                if (Directory.Exists(destFolder))
                {
                    Directory.Delete(destFolder, true);
                }
                                                                                                                 
                if (result > 0)
                    return Json(new { success = true, message = "Documents merged successfully", fileName = outputFileName });

                return Json(new { success = false, message = "Something went wrong while updating." });

                //return Json(new { success = true, message = "Document merged successfully.",  });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult DeleteDocument(string docCode, string fileCode, List<SelectListItem> docLists, string mergedFile, string htmlText)
        {
            try
            {
                if (string.IsNullOrEmpty(docCode))
                {
                    return Json(new { success = false, message = "Please select document." });
                }

                string sourceLocation = Server.MapPath("~/Uploads/CreatedFile/" + mergedFile);
                string destpath = Server.MapPath("~/Uploads/MergerFile/MergedFiles/" + mergedFile);

                //var docLists = bal.GetMergedDocs(fileCode);

                // CASE 1: Only 1 document left
                if (docLists != null && docLists.Count == 2)
                {
                    if ((System.IO.File.Exists(sourceLocation)))
                    {
                        System.IO.File.Delete(sourceLocation);
                    }
                }
                else
                {   // Extract page numbers from HTML

                    //string docCode = ddlMergedDoc.SelectedItem.Text.Trim();
                    string[] numbers = Regex.Split(htmlText, @"\D+");

                    if (numbers.Length < 3)
                    {
                        return Json(new { success = false, message = "Invalid page range." });
                    }

                    string fecthedN = numbers[1];
                    string fecthedN1 = numbers[2];
                    string newRange = fecthedN + "-" + fecthedN1;

                    // Delete pages from PDF
                    DeletePagesNew(newRange, sourceLocation, destpath, "");

                    // Replace original file
                    if ((System.IO.File.Exists(sourceLocation)))
                    {
                        System.IO.File.Delete(sourceLocation);
                    }

                    System.IO.File.Copy(destpath, sourceLocation, true);

                    if ((System.IO.File.Exists(destpath)))
                    {
                        System.IO.File.Delete(destpath);
                    }
                }



                // Call BAL to delete the document from DB and optionally delete file from server
                var result = bal.UpdateAfterDelete(docCode, fileCode, Session["UserName"]?.ToString());

                if (result)
                    return Json(new { success = true, message = "Document deleted successfully." });

                return Json(new { success = false, message = "Document could not be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        private void DeletePagesNew(string pageRange, string SourcePdfPath, string OutputPdfPath, string Password = "")
        {
            try
            {
                var pagesToDelete = new List<int>();

                if (pageRange.Contains(","))
                {
                    foreach (var part in pageRange.Split(','))
                    {
                        if (part.Contains("-"))
                        {
                            var range = part.Split('-');
                            for (int i = Convert.ToInt32(range[0]); i <= Convert.ToInt32(range[1]); i++)
                                pagesToDelete.Add(i);
                        }
                        else
                        {
                            pagesToDelete.Add(Convert.ToInt32(part));
                        }
                    }
                }
                else if (pageRange.Contains("-"))
                {
                    var range = pageRange.Split('-');
                    for (int i = Convert.ToInt32(range[0]); i <= Convert.ToInt32(range[1]); i++)
                        pagesToDelete.Add(i);
                }
                else
                {
                    pagesToDelete.Add(Convert.ToInt32(pageRange));
                }

                PdfReader reader = new PdfReader(SourcePdfPath);

                using (FileStream fs = new FileStream(OutputPdfPath, FileMode.Create))
                using (Document doc = new Document())
                using (PdfWriter writer = PdfWriter.GetInstance(doc, fs))
                {
                    doc.Open();

                    for (int p = 1; p <= reader.NumberOfPages; p++)
                    {
                        if (pagesToDelete.Contains(p))
                            continue;

                        doc.SetPageSize(reader.GetPageSize(p));
                        doc.NewPage();

                        PdfContentByte cb = writer.DirectContent;
                        PdfImportedPage page = writer.GetImportedPage(reader, p);

                        int rot = reader.GetPageRotation(p);

                        if (rot == 90 || rot == 270)
                        {
                            cb.AddTemplate(page, 0, -1, 1, 0, 0, reader.GetPageSizeWithRotation(p).Height);
                        }
                        else
                        {
                            cb.AddTemplate(page, 1, 0, 0, 1, 0, 0);
                        }
                    }

                    doc.Close();
                }

                reader.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }



        #endregion

        #region Filtered Inbox

        [HttpPost]
        public JsonResult GetInboxFiles(string secCode, string finYear)
        {
            string forwardedTo = Session["UserName"]?.ToString();

            DataTable dt = bal.GetInboxFiles(forwardedTo, secCode, finYear);

            // Convert DataTable rows into JSON serializable anonymous objects
            var data = dt.AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                File_Code = DeterministicEncryptionHelper.SafeDecrypt(row["File_Code"].ToString()),
                File_Desc = row["File_Desc"].ToString(),
                FileCat_Name = row["FileCat_Name"].ToString(),
                FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                Sec_Name = row["Sec_Name"].ToString(),
                F_From = row["F_From"].ToString(),
                F_To = row["F_To"].ToString(),
                Sent_Dt = row["sent_dt"].ToString(),
                DocPrior_Name = row["DocPrior_Name"].ToString(),
                File_status = row["File_status"].ToString(),
                Remark = row["Remark"].ToString(),
                Cb_Flag = row["Cb_Flag"].ToString() // for coloring rows later in JS
            });

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SearchByFinYear(string finYear)
        {
            try
            {
                string userName = Session["UserName"]?.ToString() ?? "";

                DataTable dt = bal.GetInboxFilesByFinYear(userName, finYear);

                var data = dt.AsEnumerable().Select((row, index) => new
                {
                    SNo = index + 1,
                    File_Code = DeterministicEncryptionHelper.SafeDecrypt(row["File_Code"].ToString()),
                    File_Desc = row["File_Desc"].ToString(),
                    FileCat_Name = row["FileCat_Name"].ToString(),
                    FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                    Sec_Name = row["Sec_Name"].ToString(),
                    F_From = row["F_From"].ToString(),
                    F_To = row["F_To"].ToString(),
                    Sent_Dt = row["Sent_Dt"].ToString(),
                    DocPrior_Name = row["DocPrior_Name"].ToString(),
                    File_status = row["File_status"].ToString(),
                    Remark = row["Remark"].ToString(),
                    Cb_Flag = row["Cb_Flag"].ToString() // for coloring rows later in JS
                }).ToList();

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region Filtered file history

        [HttpPost]
        public JsonResult FileHistorySearchByFinYear(string finYear)
        {
            string username = Session["UserName"].ToString();

            string query = $"proc_file_movement_history '{username}', '{finYear}'";

            // Execute query
            DataSet ds = bal.FN_ExecuteQuerySingle(query);

            var data = ds.Tables[0].AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                File_Code = row["File_Code"].ToString(),
                File_Desc = row["File_Desc"].ToString(),
                FileCat_Name = row["FileCat_Name"].ToString(),
                FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                Sec_Name = row["Sec_Name"].ToString(),
                DocPrior_Name = row["DocPrior_Name"].ToString(),
                Created_DT = row["Created_DT"].ToString()
            }).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SearchByDate(string secCode, string fromDate, string toDate)
        {
            string username = Session["UserName"].ToString();
            DataTable dt = bal.SearchByDate(secCode, fromDate, toDate, username);

            var data = dt.AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                File_Code = row["File_Code"].ToString(),
                File_Desc = row["File_Desc"].ToString(),
                FileCat_Name = row["FileCat_Name"].ToString(),
                FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                Sec_Name = row["Sec_Name"].ToString(),
                DocPrior_Name = row["DocPrior_Name"].ToString(),
                Created_DT = row["Created_DT"].ToString()
            }).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SearchByFileNo(string fileNo)
        {
            string userName = Session["UserName"].ToString();
            DataTable dt = bal.SearchByFileNo(userName, fileNo);

            var data = dt.AsEnumerable().Select((row, index) => new
            {
                SNo = index + 1,
                File_Code = row["File_Code"].ToString(),
                File_Desc = row["File_Desc"].ToString(),
                FileCat_Name = row["FileCat_Name"].ToString(),
                FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                Sec_Name = row["Sec_Name"].ToString(),
                DocPrior_Name = row["DocPrior_Name"].ToString(),
                Created_DT = row["Created_DT"].ToString()
            }).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }




        #endregion

        public ActionResult HandOver_Inbox()
        {
            return View();
        }
        public ActionResult HandOver_Outbox()
        {
            return View();
        }
        public ActionResult HandOver_OpenedFiles()
        {
            return View();
        }
        
        #region Internal Movement 

        [HttpPost]
        public JsonResult SendInternalMovement(string fileCode, string forwardTo, bool approved, string docCode)
        {
            try
            {
                string empId = Session["EmpID"].ToString();
                string ip = Session["Ip_Address"].ToString();
                string edRowId = Session["ED_Row_ID"].ToString();
                string uname = Session["UserName"].ToString();


                //var result = bal.SendInternalMovementBAL(fileCode, forwardTo, empId, ip, edRowId, approved, docCode);

                string flag = "";
                string appFlag = null;

                // flag logic like in aspx
                if (approved)
                    flag = "-5";
                else
                    flag = "2";


                if (empId != "ADM28" || empId == "ADM28")
                {
                    string str2 = "select * from m_document where file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
                    int count = 0;
                    DataTable dt2 = bal.EQ(str2);

                    if (dt2.Rows.Count > 0)
                    {
                        count = count + 1;
                    }

                    if (!string.IsNullOrEmpty(docCode))
                    {
                        string str22 = "SELECT * FROM T_Noting WHERE File_Code ='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' AND Doc_Code='" + DeterministicEncryptionHelper.Encrypt(docCode) + "' AND Emp_Code='" + empId + "'";
                        DataTable dt33 = bal.EQ(str22);
                        if (dt33.Rows.Count > 0)
                        {
                            count = count + 1;
                        }
                    }
                    else
                    {
                        string str22 = "SELECT * FROM T_Noting WHERE File_Code ='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' AND Emp_Code='" + empId + "'";
                        DataTable dt33 = bal.EQ(str22);
                        if (dt33.Rows.Count > 0)
                        {
                            count = count + 1;
                        }
                    }
                    if (count == 0)
                    {
                        if (empId != "ADM28")
                        {
                            return Json(new { success = false, message = "File can not be forward without noting. Please enter Noting." });
                        }

                    }
                }


                string str3 = "select * from T_File where forwarded_To = '" + uname + "' and Action_Date is null  and  file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
                DataTable dt3 = bal.EQ(str3);
                if (dt3.Rows.Count > 0)
                {
                    if (dt3.Rows[0]["status_flag"].ToString() == "-5")
                    {

                        flag = "-5";
                        appFlag = "A";
                    }
                }
                else
                {
                    //if its called back 
                    string strr = "select * from T_File where forwarded_From = '" + uname + "' and Action_Date is null  and  file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";   //and app_flag ='A'
                    DataTable dtt = bal.EQ(strr);
                    {
                        if (dtt.Rows.Count > 0)
                        {
                            if (dtt.Rows[0]["status_flag"].ToString() == "-5")
                            {
                                flag = "-5";
                                appFlag = "A";
                            }
                        }
                    }
                }
                DataSet dsInternal = new DataSet();
                dsInternal = bal.FN_ExecuteQuerySingle("Proc_InternalMovement '" + DeterministicEncryptionHelper.Encrypt(fileCode) + "','" + uname + "','" + forwardTo + "','" + null + "','" + null + "','" + Session["Ip_Address"].ToString() + "','" + flag + "','" + null + "','" + appFlag + "','" + Session["ED_Row_ID"].ToString() + "' ");
                if (dsInternal.Tables[0].Rows.Count > 0)
                {
                    Session["forwarded"] = "ok";
                    //ResetForward();
                    //disableButtons();
                    Session["F"] = null;
                    //btnForwardNew.Enabled = false;
                    //ddlFavorite.SelectedIndex = 0;
                    //chkapproved.Checked = false;
                    return Json(new { success = true, message = "File Forwarded Successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Something went wrong. File not forwarded." });
                }


            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Something went wrong." });
            }
        }

        #endregion

        #region Save Noting

        [HttpPost]
        [ValidateInput(false)]  // allow HTML
        public JsonResult SaveNoting(string noting, string fileCode, string action)
        {
            try
            {
                bool result = false;
                string Note_Desc = "";
                string dftnot = "";

                // 1. Check empty note content
                if (string.IsNullOrWhiteSpace(noting))
                {
                    Session["cAdded"] = null;
                    return Json(new { success = false, status = "warning", message = "Enter noting details!" });
                }

                // 2. Reset final flag
                if (Session["final"] != null)
                {
                    Session["final"] = null;
                }

                // 3. Validate dropdown selection
                if (string.IsNullOrEmpty(action))
                {
                    return Json(new { success = false, status = "warning", message = "Please, choose option!" });
                }

                // 4. Check for existing draft
                DataTable draftData = DisplayDraftNoting(fileCode);
                if (draftData != null && draftData.Rows.Count > 0)
                {
                    if (Session["cAdded"] != null && Session["cAdded"].ToString() == "ok")
                    {
                        dftnot = "ND";
                        Session["cAdded"] = null; // allow overwrite
                    }
                    else
                    {
                        Session["cAdded"] = null;
                        return Json(new { success = false, status = "warning", message = "You have already saved a draft noting." });
                    }
                }

                // 5. Handle Final Save
                if (action == "final")
                {
                    string param = "I";
                    string dftstatus = "Y";

                    // Secretariat or first draft case
                    if (Session["DraftCount"].ToString() == ""
                        && !string.IsNullOrEmpty(noting)
                        || Session["UserName"].ToString().Contains("Secretariat"))
                    {
                        dftnot = "ND";
                        param = "dft";
                        Session["final"] = "ok";
                        dftstatus = "N";
                    }

                    // Save paras from Session["Paralist"]
                    if (Session["Paralist"] != null)
                    {
                        foreach (var item in (ArrayList)Session["Paralist"])
                        {
                            if (noting.Contains(item.ToString()))
                            {
                                int i = bal.SaveNotingPara(fileCode, item.ToString());
                            }
                        }
                    }


                    // Get IP
                    string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipAddress))
                        ipAddress = Request.ServerVariables["REMOTE_ADDR"];

                    // Check attached docs
                    DataTable docs = bal.GetAttachedDocs(fileCode);

                    if (docs != null && docs.Rows.Count > 0)
                    {
                        //foreach (DataRow row in docs.Rows)
                        //{
                        //    result = bal.SaveFinalNoting(
                        //        param,
                        //        fileCode,
                        //        row["Doc_Code"].ToString(),
                        //        row["Doc_Upload"].ToString(),
                        //        "Green",
                        //        noting,
                        //        Session["EmpID"].ToString(),
                        //        Session["Section"].ToString(),
                        //        Session["UserName"].ToString(),
                        //        ipAddress,
                        //        "",
                        //        "",
                        //        dftstatus,
                        //        Session["ED_Row_ID"].ToString()
                        //    );

                        //}

                        // get ONLY MAIN DOCUMENT for final noting save
                        //DataRow mainDoc = docs.AsEnumerable().FirstOrDefault(r => r["Main_doc"]?.ToString() == "Y");

                        //if (mainDoc != null)
                        //{
                        //    result = bal.SaveFinalNoting(
                        //        param,
                        //        fileCode,
                        //        mainDoc["Doc_Code"].ToString(),
                        //        mainDoc["Doc_Upload"].ToString(),
                        //        "Green",
                        //        noting,
                        //        Session["EmpID"].ToString(),
                        //        Session["Section"].ToString(),
                        //        Session["UserName"].ToString(),
                        //        ipAddress,
                        //        "",
                        //        "",
                        //        dftstatus,
                        //        Session["ED_Row_ID"].ToString()
                        //    );
                        //}


                        DataRow firstDoc = docs.Rows[0];

                        result = bal.SaveFinalNoting(
                            param,
                            fileCode,
                            firstDoc["Doc_Code"].ToString(),
                            firstDoc["Doc_Upload"].ToString(),
                            "Green",
                            noting,
                            Session["EmpID"].ToString(),
                            Session["Section"].ToString(),
                            Session["UserName"].ToString(),
                            ipAddress,
                            "",
                            "",
                            dftstatus,
                            Session["ED_Row_ID"].ToString()
                        );
                    }
                    else
                    {
                        result = bal.SaveFinalNoting(
                            param,
                            fileCode,
                            "",
                            "",
                            "Green",
                            noting,
                            Session["EmpID"].ToString(),
                            Session["Section"].ToString(),
                            Session["UserName"].ToString(),
                            ipAddress,
                            "",
                            "",
                            dftstatus,
                            Session["ED_Row_ID"].ToString()
                        );
                    }

                    Session["final"] = "ok";
                    Session["cAdded"] = null;


                    // VC case → auto forward
                    if (Session["UserName"].ToString() == "vc_au")
                    {
                        bal.SendFileToSecretary(fileCode, Session["UserName"].ToString(), ipAddress, Session["ED_Row_ID"].ToString());

                        Session["forwarded"] = "ok";
                    }

                    if (result)
                        return Json(new { success = true, status = "success", message = "Noting has been finally submitted!" });
                    else
                        return Json(new { success = false, status = "error", message = "Something went wrong!" });
                }

                // 6. Handle Draft Save
                DataTable attachedDocs = bal.GetAttachedDocs(fileCode);
                string clientIP = Request.UserHostAddress;

                if (attachedDocs != null && attachedDocs.Rows.Count > 0)
                {
                    // Save Draft for each attached doc
                    foreach (DataRow row in attachedDocs.Rows)
                    {
                        result = bal.SaveDraftNoting(
                            fileCode,
                            noting,
                            Session["EmpID"].ToString(),
                            Session["UserName"].ToString(),
                            Session["Section"].ToString(),
                            row["Doc_Code"].ToString(),
                            row["Doc_Upload"].ToString(),
                            clientIP,
                            Session["ED_Row_ID"].ToString(),
                            dftnot
                        );
                    }
                }
                else
                {
                    // Save Draft without documents
                    result = bal.SaveDraftNoting(
                        fileCode,
                        noting,
                        Session["EmpID"].ToString(),
                        Session["UserName"].ToString(),
                        Session["Section"].ToString(),
                        "",
                        "",
                        clientIP,
                        Session["ED_Row_ID"].ToString(),
                        dftnot
                    );
                }

                if (result)
                    return Json(new { success = true, status = "success", message = "Noting Saved as Draft, Successfully!" });
                else
                    return Json(new { success = false, status = "error", message = "Something went wrong!" });

            }
            catch (Exception)
            {
                return Json(new { success = false, status = "error", message = "Something went wrong!" });
            }
        }



        public DataTable DisplayDraftNoting(string fileCode)
        {
            string userId = Session["UserName"].ToString();

            var result = new Dictionary<string, object>();

            // 1. First SP call (Dft)
            DataSet ds = bal.GetDraftNoting(fileCode, userId);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                result["DraftNoting"] = ds.Tables[0];
                result["IsFinalSubmitted"] = false;
                Session["F"] = 1;
                Session["DraftCount"] = "Draft";
                //return result;
            }
            else
            {
                DataSet dsAlt = bal.GetDraftNotingAlt(fileCode, userId);

                if (dsAlt != null && dsAlt.Tables[0].Rows.Count > 0)
                {
                    if (dsAlt.Tables[1].Rows.Count > 0)
                    {
                        if (dsAlt.Tables[1].Rows[0]["Status_Flag"].ToString() == "-1" || dsAlt.Tables[1].Rows[0]["Status_Flag"].ToString() == "-5")
                        {
                            if (Session["final"] != null)
                            {
                                if (Session["final"].ToString() == "ok")
                                {
                                    //abc.Visible = false;
                                    //LinkButton2.Visible = false;
                                    //lnkReset.Visible = false;
                                    //lnkAddNotes.Visible = false;
                                    //msgdraft.Visible = true;
                                    //msgdraft.Text = "Your noting has finally submitted, now you can forward the file.";
                                    //ddldelete.Enabled = false;
                                    //msgdraft.ForeColor = System.Drawing.Color.Red;
                                    //lbldraftCount.Text = "";
                                    //lnkAttachDoc.Enabled = false;

                                    Session["DraftCount"] = "";
                                    Session["DisableNotingButtons"] = "ok";
                                }
                                //else
                                //{
                                //    msgdraft.Visible = false;
                                //}
                            }
                            //else
                            //{
                            //    msgdraft.Visible = false;
                            //}
                        }
                        else
                        {
                            //abc.Visible = false;
                            //LinkButton2.Visible = false;
                            //lnkReset.Visible = false;
                            //lnkAddNotes.Visible = false;
                            //msgdraft.Visible = true;
                            //msgdraft.Text = "Your noting has finally submitted, now you can forward the file.";
                            //ddldelete.Enabled = false;
                            //msgdraft.ForeColor = System.Drawing.Color.Red;
                            //lbldraftCount.Text = "";

                            Session["DraftCount"] = "";
                            Session["DisableNotingButtons"] = "ok";
                        }
                    }
                    else
                    {
                        if (Session["final"] != null)
                        {
                            if (Session["final"].ToString() == "ok")
                            {
                                //abc.Visible = false;
                                //LinkButton2.Visible = false;
                                //lnkReset.Visible = false;
                                //lnkAddNotes.Visible = false;
                                //msgdraft.Text = "Your noting has finally submitted, now you can forward the file.";
                                //ddldelete.Enabled = false;
                                //msgdraft.ForeColor = System.Drawing.Color.Red;
                                //lbldraftCount.Text = "";
                                Session["DraftCount"] = "";
                                Session["DisableNotingButtons"] = "ok";
                            }
                            else
                            {
                                //abc.Visible = false;
                                //LinkButton2.Visible = false;
                                //lnkReset.Visible = false;
                                //lnkAddNotes.Visible = false;
                                //msgdraft.Text = "";
                                //lbldraftCount.Text = "";
                                Session["DraftCount"] = "";
                                Session["final"] = null;
                                Session["DisableNotingButtons"] = "ok";
                            }
                        }
                        else
                        {
                            //abc.Visible = true;
                            //LinkButton2.Visible = true;
                            //lnkReset.Visible = true;
                            //lbldraftCount.Text = "";
                            Session["DraftCount"] = "";
                            Session["final"] = null;
                            Session["DisableNotingButtons"] = "";
                        }
                    }
                }
                else
                {
                    Session["DisableNotingButtons"] = "";
                }

                    string getNote = "select top 1 *  from t_noting where File_Code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'  order by Row_ID desc ";
                DataTable Notedata = bal.EQ(getNote);
                if (Notedata.Rows.Count > 0)
                {
                    if (Notedata.Rows[0]["Created_By"].ToString() == Session["UserName"].ToString())
                    {

                        bool R_true = false;
                        //if(Request.QueryString["IsReturn"] != null)
                        //{
                        //    if(Request.QueryString["IsReturn"].ToString() == "R")
                        //    {
                        //            R_true = true;
                        //    }

                        //}

                        if (R_true == false)
                        {
                            //abc.Visible = false;
                            //LinkButton2.Visible = false;
                            //lnkReset.Visible = false;
                            //lnkAddNotes.Visible = false;
                            //if (Session["UserName"].ToString() == "vc_au")
                            //{
                            //    msgdraft.Text = "Your noting has been finally submitted and file forwarded to vc secretary.";
                            //}
                            //else
                            //{
                            //    msgdraft.Text = "Your noting has been finally submitted, now you can forward the file.";
                            //}
                            //msgdraft.ForeColor = System.Drawing.Color.Red;
                            //lbldraftCount.Text = "";
                            //ddldelete.Enabled = false;
                            Session["DraftCount"] = "";
                            Session["final"] = "ok";
                            Session["DisableNotingButtons"] = "ok";
                        }
                    }
                }
                TempData["Draftdata"] = null;
                Session["F"] = null;

            }

            //// 2. Alternate SP call (Dft1)
            //DataSet dsAlt = bal.GetDraftNotingAlt(fileCode, userId);

            //if (ds.Tables[0].Rows.Count > 0)
            //{
            //    if (ds.Tables[1].Rows.Count > 0)
            //    {
            //        if (ds.Tables[1].Rows[0]["Status_Flag"].ToString() == "-1" || ds.Tables[1].Rows[0]["Status_Flag"].ToString() == "-5")
            //        {
            //            if (Session["final"] != null)
            //            {
            //                if (Session["final"].ToString() == "ok")
            //                {
            //                    abc.Visible = false;
            //                    LinkButton2.Visible = false;
            //                    lnkReset.Visible = false;
            //                    lnkAddNotes.Visible = false;
            //                    msgdraft.Visible = true;
            //                    msgdraft.Text = "Your noting has finally submitted, now you can forward the file.";
            //                    ddldelete.Enabled = false;
            //                    msgdraft.ForeColor = System.Drawing.Color.Red;
            //                    lbldraftCount.Text = "";
            //                    lnkAttachDoc.Enabled = false;
            //                }
            //                else
            //                {
            //                    msgdraft.Visible = false;
            //                }
            //            }
            //            else
            //            {
            //                msgdraft.Visible = false;
            //            }
            //        }
            //    }
            //}


            //if (dsAlt.Tables.Count > 1 && dsAlt.Tables[0].Rows.Count > 0)
            //{
            //    string status = dsAlt.Tables[1].Rows[0]["Status_Flag"].ToString();
            //    result["StatusFlag"] = status;

            //    if (status == "-1" || status == "-5")
            //        result["IsFinalSubmitted"] = false;
            //    else
            //        result["IsFinalSubmitted"] = true;
            //}

            // 3. Latest noting check
            //DataTable dtLatest = bal.GetLatestNoting(fileCode);
            //if (dtLatest.Rows.Count > 0)
            //{
            //    if (dtLatest.Rows[0]["Created_By"].ToString() == userId)
            //    {
            //        bool 
            //    }
            //        result["IsFinalSubmitted"] = true;
            //}






            //var data = bal.GetDraftNotingStatus(fileCode, userId);

            //ViewBag.DraftNoting = data.ContainsKey("DraftNoting") ? data["DraftNoting"] : null;
            //ViewBag.StatusFlag = data.ContainsKey("StatusFlag") ? data["StatusFlag"] : null;
            //ViewBag.IsFinalSubmitted = data.ContainsKey("IsFinalSubmitted") ? data["IsFinalSubmitted"] : false;

            return ds.Tables[0];
        }


        [HttpGet]
        public JsonResult JsonDraftNoting(string fileCode, string performedEvent = "load")
        {
            DataTable dt = DisplayDraftNoting(fileCode);

            if (dt == null || dt.Rows.Count == 0)
            {
                return Json(new { success = false, message = "No draft noting found." }, JsonRequestBehavior.AllowGet);
            }

            // Convert DataTable to JSON-friendly format
            var result = dt.AsEnumerable().Select(row => dt.Columns.Cast<DataColumn>()
                                .ToDictionary(col => col.ColumnName, col => row[col]));

            if (performedEvent?.ToString() == "clicked")
                Session["cAdded"] = "ok";
            else
                Session["cAdded"] = null;

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Forward file

        [HttpPost]
        public JsonResult ForwardFile(string fileCode, string forwardedTo, string remark, string remarkType, string selectedRemarkType,
                                  string dept, string sec, bool isApproved, string esttype)
        {
            try
            {
                string uname = Session["UserName"].ToString();
                string empId = Session["EmpID"].ToString();
                string EDRowID = Session["ED_Row_ID"].ToString();
                string flag = "";
                string appFlag = null;

                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                // Step 1: Validation
                if (string.IsNullOrEmpty(fileCode) || string.IsNullOrEmpty(forwardedTo))
                    return Json(new { success = false, message = "Invalid data" });

                // set flag on the basis of checkbox
                if (isApproved)
                {
                    flag = "-5";
                    appFlag = "A";
                }
                else
                    flag = "2";



                // Step 2: VC special case
                if (uname == "vc_au" && dept == "095" && sec == "A095")
                {
                    flag = "2";
                    appFlag = null;


                    bal.ForwardAsVC(fileCode, uname, forwardedTo, remark, remarkType, selectedRemarkType, dept, sec, flag, appFlag, ipAddress, EDRowID, esttype);
                    Session["forwarded"] = "ok";
                    Session["F"] = null;
                    TempData["ActiveTab"] = "history";
                    return Json(new { success = true, message = "File forwarded successfully." });
                }

                // Step 3: Admin bypass
                if (empId == "ADM28")
                {
                    if (string.IsNullOrEmpty(forwardedTo))
                        return Json(new { success = false, message = "File Can Not Be Forwarded Because Selected Emloyee Has No User Account Created." });

                    

                    string str2 = "select * from m_document where file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
                    int count = 0;
                    DataTable dt2 = bal.EQ(str2);
                    if (dt2.Rows.Count > 0)
                    {
                        count = count + 1;
                    }

                    // Check attached docs
                    DataTable docs = bal.GetAttachedDocs(fileCode);

                    if (docs != null && docs.Rows.Count > 0)
                    {
                        string str22 = "SELECT * FROM T_Noting WHERE File_Code ='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' AND Doc_Code='" + DeterministicEncryptionHelper.Encrypt(docs.Rows[0]["Doc_Code"].ToString()) + "' AND Emp_Code='" + empId + "'";
                        DataTable dt333 = bal.EQ(str22);
                        if (dt333.Rows.Count > 0)
                        {
                            count = count + 1;
                        }
                    }
                    else
                    {
                        string str22 = "SELECT * FROM T_Noting WHERE File_Code ='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' AND Emp_Code='" + empId + "'";
                        DataTable dt3333 = bal.EQ(str22);
                        if (dt3333.Rows.Count > 0)
                        {
                            count = count + 1;
                        }
                    }

                    string str3 = "select * from T_File where forwarded_To = '" + uname + "' and Action_Date is null  and  file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";   //and app_flag ='A'
                    DataTable dt3 = bal.EQ(str3);
                    if (dt3.Rows.Count > 0)
                    {
                        if (dt3.Rows[0]["status_flag"].ToString() == "-5")
                        {
                            string strCat = "select CategoryCode from Utility_MUser where LoginName='" + forwardedTo + "' ";
                            DataTable dtcat = bal.EQ(strCat);
                            if (dtcat.Rows.Count > 0)
                            {
                                if (dtcat.Rows[0][0].ToString() != "5")
                                {
                                    flag = "-5";
                                    appFlag = "A";
                                }

                            }
                        }
                    }
                    else
                    {
                        //if its called back 
                        string strr = "select * from T_File where forwarded_From = '" + uname + "' and Action_Date is null  and  file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";   //and app_flag ='A'
                        DataTable dtt = bal.EQ(strr);
                        {
                            if (dtt.Rows.Count > 0)
                            {
                                if (dtt.Rows[0]["status_flag"].ToString() == "-5")
                                {
                                    flag = "-5";
                                    appFlag = "A";
                                }
                            }
                        }
                    }

                    // get file category
                    string str32 = "select File_Cat from M_File where File_Code = '" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' and Created_By = '" + uname + "'";
                    DataTable dt33 = bal.EQ(str32);
                    if (dt33.Rows.Count > 0)
                    {
                        TempData["File_Cat"] = dt33.Rows[0]["File_Cat"].ToString();
                    }
                    else
                    {
                        TempData["File_Cat"] = "";
                    }

                    if (dept == "095" && sec == "A095" && TempData["File_Cat"].ToString() == "C00856")
                        forwardedTo = forwardedTo;
                    else if (dept == "095" && sec == "A095" && empid == "ADM28")
                        forwardedTo = "vcsec_vco";
                    else
                        forwardedTo = forwardedTo;

                    bal.ForwardFile(fileCode, uname, forwardedTo, remark, remarkType, selectedRemarkType, dept, sec, isApproved, ipAddress, flag, appFlag, EDRowID, esttype);

                    Session["F"] = null;
                    TempData["ActiveTab"] = "history";
                    return Json(new { success = true, message = "File forwarded successfully." });
                }

                // Step 4: Check if final submission is done
                if (Session["F"] == null)
                {
                    if (Session["final"] != null)
                    {
                        if (string.IsNullOrEmpty(forwardedTo))
                            return Json(new { success = false, message = "File Can Not Be Forwarded Because Selected Emloyee Has No User Account Created." });

                        if (isApproved)
                        {
                            flag = "-5";
                            appFlag = "A";
                        }
                        else
                            flag = "2";


                        // Check attached docs
                        DataTable docs = bal.GetAttachedDocs(fileCode);


                        // if file is attached then only check
                        if (docs.Rows.Count > 0)
                        {
                            string strCheckNote = "SELECT * FROM T_NOTING where Doc_Code='" + DeterministicEncryptionHelper.Encrypt(docs.Rows[0]["Doc_Code"].ToString()) + "' and file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' and Created_By='" + uname + "' ";
                            DataTable dtcheck = bal.EQ(strCheckNote);
                            if (dtcheck.Rows.Count <= 0)
                            {
                                //if (msgdraft.Text != string.Empty)
                                //{ }
                                //else
                                //{
                                // ScriptManager.RegisterStartupScript(this, this.GetType(), "swal", "alert ('Please, Enter noting if you are attaching a document then proceed to forward');", true);
                                return Json(new { success = false, message = "Please, Enter noting if you are attaching a document then proceed to forward." });
                                //}
                            }

                        }



                        string str2 = "select * from m_document where file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
                        int count = 0;
                        DataTable dt2 = bal.EQ(str2);
                        if (dt2.Rows.Count > 0)
                        {
                            count = count + 1;
                        }


                        if (docs != null && docs.Rows.Count > 0)
                        {
                            string str22 = "SELECT * FROM T_Noting WHERE File_Code ='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' AND Doc_Code='" + DeterministicEncryptionHelper.Encrypt(docs.Rows[0]["Doc_Code"].ToString()) + "' AND Emp_Code='" + empId + "'";
                            DataTable dt333 = bal.EQ(str22);
                            if (dt333.Rows.Count > 0)
                            {
                                count = count + 1;
                            }
                        }
                        else
                        {
                            string str22 = "SELECT * FROM T_Noting WHERE File_Code ='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' AND Emp_Code='" + empId + "'";
                            DataTable dt3333 = bal.EQ(str22);
                            if (dt3333.Rows.Count > 0)
                            {
                                count = count + 1;
                            }
                        }

                        if (count == 0)
                        {
                            return Json(new { success = false, message = "File can not be forward without noting. Please, Enter Noting." });
                        }

                        string str3 = "select * from T_File where forwarded_To = '" + uname + "' and Action_Date is null  and  file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";   //and app_flag ='A'
                        DataTable dt3 = bal.EQ(str3);
                        if (dt3.Rows.Count > 0)
                        {
                            if (dt3.Rows[0]["status_flag"].ToString() == "-5")
                            {
                                string strCat = "select CategoryCode from Utility_MUser where LoginName='" + forwardedTo + "' ";
                                DataTable dtcat = bal.EQ(strCat);
                                if (dtcat.Rows.Count > 0)
                                {
                                    if (dtcat.Rows[0][0].ToString() != "5")
                                    {
                                        flag = "-5";
                                        appFlag = "A";
                                    }

                                }
                            }
                        }
                        else
                        {
                            //if its called back 
                            string strr = "select * from T_File where forwarded_From = '" + uname + "' and Action_Date is null  and  file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";   //and app_flag ='A'
                            DataTable dtt = bal.EQ(strr);
                            {
                                if (dtt.Rows.Count > 0)
                                {
                                    if (dtt.Rows[0]["status_flag"].ToString() == "-5")
                                    {
                                        flag = "-5";
                                        appFlag = "A";
                                    }
                                }
                            }
                        }

                        // get file category
                        string str32 = "select File_Cat from M_File where File_Code = '" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' and Created_By = '" + uname + "'";
                        DataTable dt33 = bal.EQ(str32);
                        if (dt33.Rows.Count > 0)
                        {
                            TempData["File_Cat"] = dt33.Rows[0]["File_Cat"].ToString();
                        }
                        else
                        {
                            TempData["File_Cat"] = "";
                        }

                        if (dept == "095" && sec == "A095" && TempData["File_Cat"].ToString() == "C00856")
                            forwardedTo = forwardedTo;
                        else if (dept == "095" && sec == "A095" && empId == "ADM28")
                            forwardedTo = "vcsec_vco";
                        else
                            forwardedTo = forwardedTo;

                        bal.ForwardFile(fileCode, uname, forwardedTo, remark, remarkType, selectedRemarkType, dept, sec, isApproved, ipAddress, flag, appFlag, EDRowID, esttype);
                        Session["F"] = null;
                        TempData["ActiveTab"] = "history";
                        return Json(new { success = true, message = "File forwarded successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "File can not be forward without noting and final submission." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Please, Submit Draft As Final" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion

        #region File Revert

        [HttpPost]
        public JsonResult GetRevertToList(string fileCode)
        {
            string userName = Session["UserName"].ToString();
            var items = bal.GetRevertToList(fileCode, userName);
            return Json(items);
        }


        [HttpPost]
        public JsonResult RevertFile(string fileCode, string revertTo, string remark, string remarktext, string selectedRemarkText)
        {
            try
            {
                //DataTable dt = DisplayDraftNoting(fileCode);

                if (Session["DraftCount"].ToString() == "Draft")
                {
                    return Json(new { success = false, message = "Please do final submit of noting first." }, JsonRequestBehavior.AllowGet);
                }

                if (revertTo == null || revertTo == "")
                {
                    return Json(new { success = false, message = "Employee not selected or there is no employee for revert back." }, JsonRequestBehavior.AllowGet);
                }

                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                string str2 = "select File_Code,forwarded_From,forwarded_To from T_File where file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
                DataTable dt3 = bal.EQ(str2);

                if (dt3.Rows.Count > 0)
                {
                    // Call BAL
                    bool result = bal.RevertFile(
                        fileCode,
                        Session["UserName"].ToString(),
                        revertTo,
                        remark,
                        remarktext,
                        selectedRemarkText,
                        ipAddress,
                        Session["ED_Row_ID"].ToString()
                    );

                    if (result)
                    {
                        Session["forwarded"] = "ok";
                        TempData["ActiveTab"] = "history";
                        return Json(new { success = true, message = "File reverted successfully!" });
                    }
                    else
                        return Json(new { success = false, message = "Something went wrong while reverting file." });


                }

                return Json(new { success = false, message = "Something went wrong while reverting file." });
            }
            catch (Exception ex)
            {
                // Log error here if logging system exists
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        #endregion

        #region Approve and revert

        [HttpPost]
        public JsonResult ApproveRevert(string fileCode, string remark, string remarktext, string selectedRemarkText)
        {
            try
            {
                string empId = Session["EmpID"].ToString();
                string section = Session["Section"].ToString();
                string userName = Session["UserName"].ToString();
                string edRowId = Session["ED_Row_ID"].ToString();

                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                DataTable dt = DisplayDraftNoting(fileCode);

                if (Session["DraftCount"].ToString() == "Draft")
                {
                    return Json(new { success = false, message = "Please do final submit of noting first." }, JsonRequestBehavior.AllowGet);
                }


                // Check attached docs
                DataTable docs = bal.GetAttachedDocs(fileCode);

                if (Session["Des_Code"].ToString().Trim() == "001" || Session["Des_Code"].ToString().Trim() == "033")
                {

                    if (Session["final"] == null)
                    {

                        if (docs != null && docs.Rows.Count > 0)
                        {
                            foreach (DataRow row in docs.Rows)
                            {
                                bal.SaveDraftApproveRevertNoting(fileCode, row["Doc_Code"].ToString(), row["Doc_Upload"].ToString(), empId, section, userName, ipAddress, edRowId);
                                DisplayDraftNoting(fileCode);
                            }
                        }
                    }

                    bool success = false;
                    string str2 = "select File_Code,forwarded_From,forwarded_To from T_File where file_code='" + fileCode + "' and Action_Date is null";
                    DataTable dt3 = bal.EQ(str2);

                    if (dt3.Rows.Count > 0)
                        success = bal.ApproveAndRevert(fileCode, userName, dt3.Rows[0]["forwarded_From"].ToString(), remark, remarktext, selectedRemarkText,
                            ipAddress, edRowId);

                    if (success)
                    {
                        Session["forwarded"] = "ok";
                        TempData["ActiveTab"] = "history";
                        return Json(new { success = true, message = "File Approved and Returned Successfully!" });
                    }
                    else
                        return Json(new { success = false, message = "File could not be reverted." });
                }

                if (Session["final"] == null)
                {
                    return Json(new { success = false, message = "File can not be approved and revert without noting and final submission." });
                }
                else
                {
                    bool success = false;
                    string str2 = "select File_Code,forwarded_From,forwarded_To from T_File where file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "' and Action_Date is null";
                    DataTable dt3 = bal.EQ(str2);

                    if (dt3.Rows.Count > 0)
                        success = bal.ApproveAndRevert(fileCode, userName, dt3.Rows[0]["forwarded_From"].ToString(), remark, remarktext, 
                            selectedRemarkText, ipAddress, edRowId);

                    if (success)
                    {
                        Session["forwarded"] = "ok";
                        TempData["ActiveTab"] = "history";
                        return Json(new { success = true, message = "File Approved and Returned Successfully!" });
                    }
                    else
                        return Json(new { success = false, message = "File could not be reverted." });
                }


                    
            }
            catch
            {
                return Json(new { success = false, message = "Something went wrong." });
            }
        }

        #endregion  

        [HttpPost]
        public JsonResult AddPageReference(string docId, int pageNo, string fileCode, string totalPages, string docName, string docText)
        {
            try
            {
                // Using your BAL method to get the main document
                DataTable dt = bal.GetMainDocument(fileCode);

                if (dt.Rows.Count == 0)
                    return Json(new { success = false, message = "Document not found." });

                string displayFile = dt.Rows[0]["DisplayFile"].ToString();
                //string path = Server.MapPath("~/Master/CreatedFile/" + displayFile);
                string path = Server.MapPath("~/Uploads/CreatedFile/" + displayFile);
                
                if (!System.IO.File.Exists(path))
                    return Json(new { success = false, message = "File not found." });

                using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(path))
                {
                    if (pageNo < 1 || pageNo > reader.NumberOfPages)
                        return Json(new { success = false, message = "Invalid page number." });
                }

                // Create the clickable reference link
                //string link = $"<a href='#' onclick='showPDF(\"{docId}\", \"{pageNo}\")' style='color:blue; text-decoration:underline;'>Page {pageNo}</a>";

                // Build a normal URL to your viewer
                //var url = Url.Action("ViewPDF", "User", new { docId = docId, page = pageNo });
                var url = Url.Content("~/Uploads/CreatedFile/" + docName) + "#page=" + pageNo;

                // IMPORTANT: No onclick here. Keep it simple.
                string linkHtml = $"<a href=\"{url}\" target=\"_blank\" class=\"pdf-ref-link\" data-doc-id=\"{docId}\" data-page=\"{pageNo}\">Page {pageNo} - {docText}</a>";


                return Json(new { success = true, link = linkHtml, url = url });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        #region Call Back File

        [HttpGet]
        public JsonResult CheckCallBackFile(string fileCode, string rowid, string forwardedFrom, string forwardedTo)
        {
            string cb_flag = "0";

            try
            {
                // Check flag in DB
                string query = "SELECT ISNULL(cb_flag, '0') FROM T_File WHERE Row_ID = '" + rowid.ToString() + "' and File_Code='" + DeterministicEncryptionHelper.Encrypt(fileCode.ToString())  + "'";

                DataTable dt = bal.EQ(query);


                if (dt.Rows.Count > 0)
                    cb_flag = dt.Rows[0][0].ToString();

                // Handle conditions
                if (cb_flag == "0")
                {
                    Session["data_t"] = rowid.ToString() + "|" + forwardedFrom.ToString() + "|" + fileCode.ToString() + "|" + forwardedTo.ToString();

                    return Json(new { success = true, message = "Allowed" }, JsonRequestBehavior.AllowGet);
                }
                else if (cb_flag == "1")
                {
                    return Json(new { success = false, message = "Sorry! Document has been taken and status is (Forwarded)" }, JsonRequestBehavior.AllowGet);
                }
                else if (cb_flag == "-1")
                {
                    return Json(new { success = false, message = "Sorry! Action has been taken from other side and status is (In-Progress)" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Unknown status." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Something went wrong." }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult SubmitCallBackRemark(string remark)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remark))
                {
                    return Json(new { success = false, message = "Please enter remark." });
                }

                // TempData from CheckCallBackFile
                if (Session["data_t"] == null)
                {
                    return Json(new { success = false, message = "Session expired. Please try again." });
                }

                string[] abc = Session["data_t"].ToString().Split('|');
                string rowid = abc[0];
                string forwardedFrom = abc[1];
                string fileCode = abc[2];
                string forwardedTo = abc[3];

                // Call BAL
                DataTable dt = bal.SubmitCallBackRemarkBAL(fileCode, remark, Session["EmpID"].ToString(), Session["UserName"].ToString());

                if (dt.Rows.Count > 0)
                {
                    //// Update M_Document (reset forwarded_BY)
                    //string stUpdated = "UPDATE M_Document SET forwarded_BY = NULL " +
                    //                   "WHERE Created_By = '" + Session["UserName"].ToString() + "' " +
                    //                   "AND File_Code = '" + fileCode + "' " +
                    //                   "AND T_FIleRowID IS NULL";

                    //bal.ExecuteNonQuery(stUpdated);

                    int i = bal.UpdateDocumentStatusForCallBack(Session["UserName"].ToString(), fileCode);

                    return Json(new { success = true, message = "File has been restored successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Something went wrong." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        #endregion

        #region View Handover files

        public ActionResult Frm_ViewHandoverFiles(string forwardedFrom = null)
        {
            string username = Session["UserName"]?.ToString();
            string roleCode = Session["RoleCode"]?.ToString();
            string empId = Session["EmpID"]?.ToString();
            string forwardedTo = Session["UserName"]?.ToString();
            string Dep_Code = Session["Dep_Code"]?.ToString();
            string Sec_Code = Session["Scode"]?.ToString();

            forwardedFrom = forwardedFrom.Replace(" ", "+");

            ViewBag.EstType = bal.BindEstType();

            DataTable dt = bal.GetReceivedHandoverFileCounts(forwardedFrom);

            if (dt.Rows.Count > 0)
            {
                ViewBag.InboxCount = dt.Rows[0]["inboxcount"].ToString();
                ViewBag.OpenFileCount = dt.Rows[0]["openfilecount"].ToString();
                ViewBag.ApproveFileCount = dt.Rows[0]["appfilecount"].ToString();
            }
            else
            {
                ViewBag.InboxCount = "0";
                ViewBag.OpenFileCount = "0";
                ViewBag.ApproveFileCount = "0";
            }


            ViewBag.ApprovedFiles = bal.GetReceivedHandoverApprovedFiles(forwardedFrom);
            ViewBag.OpenedFiles = bal.GetReceivedHandoverOpenedFiles(forwardedFrom);
            ViewBag.PendingFiles = bal.GetReceivedHandoverPendingFiles(forwardedFrom);

            return View();
        }


        [HttpPost]
        public JsonResult GetHandoverForwardToList(string secCode, string deptCode)
        {
            // Get current logged-in username from session
            string currentUser = Session["UserName"]?.ToString() ?? "";

            var forwardToList = bal.GetHandoverForwardToList(secCode, deptCode, currentUser);

            return Json(forwardToList);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult ForwardReceivedHandOverFiles(string forwardedTo, List<Dictionary<string, string>> files)
        {
            try
            {
                if (string.IsNullOrEmpty(forwardedTo))
                    return Json(new { success = false, message = "Please select a person" });

                string forwardedBy = Session["UserName"]?.ToString() ?? "";
                string ip = Request.UserHostAddress;
                string fromRowId = Session["ED_Row_ID"]?.ToString();

                int inserted = 0;

                foreach (var f in files)
                {
                    string fileCode = f.ContainsKey("fileCode") ? f["fileCode"] : null;
                    string flag = f.ContainsKey("flag") ? f["flag"] : null;

                    if (!string.IsNullOrEmpty(fileCode))
                    {
                        bool ok = bal.ForwardReceivedHandOverFiles(fileCode, forwardedBy, forwardedTo, ip, fromRowId);
                        if (ok) inserted++;
                    }
                }


                if (inserted > 0)
                    return Json(new { success = true, message = "Files forwarded successfully." });
                else
                    return Json(new { success = false, message = "No files forwarded." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region SendFilesToVC

        public ActionResult SendFileTo_Vcsec()
        {
            string docCode = "";
            string forwardedBy = Session["UserName"]?.ToString();

            ViewBag.RecievedFiles = bal.GetReceivedFiles(docCode, forwardedBy);

            return View();
        }
     
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult SendSelectedFilesToVC(string remark, List<Dictionary<string, string>> files)
        {
            try
            {
                if (string.IsNullOrEmpty(remark))
                    return Json(new { success = false, message = "Please enter a remark" });

                string forwardedBy = Session["UserName"]?.ToString() ?? "";
                string ip = Request.UserHostAddress;
                string fromRowId = Session["ED_Row_ID"]?.ToString();

                int inserted = 0;

                foreach (var f in files)
                {
                    string fileCode = f.ContainsKey("fileCode") ? f["fileCode"] : null;
                    string flag = f.ContainsKey("flag") ? f["flag"] : null;

                    if (!string.IsNullOrEmpty(fileCode))
                    {
                        bool ok = bal.SendSelectedFilesToVC(fileCode, forwardedBy, ip, fromRowId, remark);
                        if (ok) inserted++;
                    }
                }


                if (inserted > 0)
                    return Json(new { success = true, message = "Files forwarded successfully." });
                else
                    return Json(new { success = false, message = "No files forwarded." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion


    }
}
