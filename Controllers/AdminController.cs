using Eoffice.BAL;
using Eoffice.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Xml.Linq;

namespace Eoffice.Controllers
{
    [SessionExpireFilter]
    public class AdminController : Controller
    {
        //dbconnection db = new dbconnection();
        UserBAL bal = new UserBAL();
        // GET: Admin
        public ActionResult Index()
        {
            DataSet dsCountData = new DataSet();
            dsCountData = bal.FN_ExecuteQuerySingle("proc_Dashboard_Count_Admin");
            //    dsCountData = bal.FN_ExecuteQuerySingle("proc_Eoffice_Dashboard");
            return View(dsCountData);
        }

        #region FillDropdown
        //public JsonResult fillDropdown(string Action, string DeptypeCode = null, string Department = null, string Section = null, string Designation = null, string FileCat_Name = null)
        //{
        //    var obj = new DropdownModel
        //    {
        //        Action = Action,
        //        EstType = EstType,
        //        Department = Department,
        //        Section = Section,
        //        Designation = Designation,
        //        FileCat_Name = FileCat_Name
        //    };

        //    var data = bal.GetDropDownList(obj);
        //    return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
        //}
        public JsonResult fillDropdown(string Action, string DeptypeCode = null)
        {
            if (Action == "bindDepartments")
            {
                int deptypeCode = Convert.ToInt32(DeptypeCode);
                var list = bal.BindDepartment(deptypeCode);

                var result = list.Select(x => new {
                    id = x.Value,
                    value = x.Text
                });

                return Json(result);
            }
            return Json(new { success = false });
        }


        #endregion

        #region Department
        public ActionResult Department(string DepCode = null)
        {
            var data = bal.GetDepartmentList();  // Fetching data from database

            // Bind dropdown list
            ViewBag.EstTypeList = bal.BindEstType();

            //ModelDepartment editDepartment = null;
            var departments = bal.GetDepartmentList();
            ModelDepartment department = null;
            if (!string.IsNullOrEmpty(DepCode))
            {

                department = departments.FirstOrDefault(d => d.Dep_Code == DepCode);
            }

            ViewBag.EditDepartment = department; // Use ViewBag to pass selected department

            return View(data); // Passing data to view
        }
        [HttpPost]
        public ActionResult Department(ModelDepartment i)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_Department", new Dictionary<string, object> { { "@Dep_Name", i.Dep_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("Department");
            }
            i.ParamType = "I";
            i.Active = "0";
            i.Machine_IP = Session["Ip_Address"].ToString();
            i.CreatedBy = Session["UserName"].ToString();
            bool result = bal.InsertDepartment(i);

            if (result)
            {

                TempData["insertmsg"] = "1";
            }
            else
            {

                TempData["insertmsg"] = "2";
            }
            return RedirectToAction("Department");

        }

        [HttpPost]
        public ActionResult EditDepartment(ModelDepartment up)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_Department", new Dictionary<string, object> { { "@Dep_Name", up.Dep_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("Department");
            }
            up.ParamType = "U";
            up.UpdatedBy = Session["UserName"].ToString();
            up.Machine_IP = Session["Ip_Address"].ToString();
            bool result = bal.InsertDepartment(up);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("Department");
            }

            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("Department");
        }

        public ActionResult DeleteDepartment(string DepCode = null)
        {
            ModelDepartment obj = new ModelDepartment
            {
                Dep_Code = DepCode,
                ParamType = "D",
                Active = "1",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };

            bool result = bal.InsertDepartment(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("Department");
            }

            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("Department");

        }
        #endregion

        #region Section

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

        public ActionResult Section(string Sec_Code = null)
        {
            // Bind dropdown list
            ViewBag.EstTypeList = bal.BindEstType();

            var sections = bal.GetSectionList();
            ModelSection section = null;
            if (!string.IsNullOrEmpty(Sec_Code))
            {

                section = sections.FirstOrDefault(d => d.Sec_Code == Sec_Code);
            }

            ViewBag.EditSection = section;

            return View(sections);        // Passing data to view 
        }
        [HttpPost]
        public ActionResult Section(ModelSection i)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_Section", new Dictionary<string, object> { { "@Sec_Name", i.Sec_Name }, { "@Dep_Code", i.Dep_Code } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("Section");
            }
            i.CreatedBy = Session["UserName"] == null ? string.Empty : Session["UserName"].ToString();
            i.Active = "0";
            i.Machine_IP = Session["Ip_Address"].ToString();
            i.ParamType = "I";
            bool result = bal.InsertSection(i);

            if (result)
            {
                TempData["insertmsg"] = "1";

            }
            else
            {
                TempData["insertmsg"] = "2";

            }
            return RedirectToAction("Section");

        }

        [HttpPost]
        public ActionResult EditSection(ModelSection up)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_Section", new Dictionary<string, object> { { "@Sec_Name", up.Sec_Name }, { "@Dep_Code", up.Dep_Code } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("Section");
            }
            up.UpdatedBy = Session["UserName"] == null ? string.Empty : Session["UserName"].ToString();
            up.Active = "0";
            up.Machine_IP = Session["Ip_Address"].ToString();
            up.ParamType = "U";

            bool result = bal.InsertSection(up);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("Section");
            }

            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("Section");
        }


        public ActionResult DeleteSection(string Sec_Code = null)
        {
            ModelSection obj = new ModelSection
            {
                Sec_Code = Sec_Code,
                ParamType = "D",
                Active = "1",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };

            bool result = bal.InsertSection(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("Section");

            }
            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("Section");
        }
        #endregion

        #region Designation
        public ActionResult Designation(string Des_Code = null)
        {
            var designations = bal.GetDesignationList();
            ModelDesignation designation = null;
            if (!string.IsNullOrEmpty(Des_Code))
            {

                designation = designations.FirstOrDefault(d => d.DesCode == Des_Code);
            }

            ViewBag.EditDesignation = designation;

            return View(designations);        // Passing data to view 
        }

        [HttpPost]
        public ActionResult Designation(ModelDesignation des)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_Designation", new Dictionary<string, object> { { "@Des_Name", des.DesName } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("Designation");
            }
            des.ParamType = "I";
            des.Active = "0";
            des.CreatedBy = Session["UserName"].ToString();
            des.Machine_IP = Session["Ip_Address"].ToString();
            bool result = bal.InsertDesignation(des);

            if (result)
            {
                TempData["insertmsg"] = "1";
            }
            else
            {
                TempData["insertmsg"] = "2";
            }

            return RedirectToAction("Designation");
        }
        public ActionResult EditDesignation(ModelDesignation up)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_Designation", new Dictionary<string, object> { { "@Des_Name", up.DesName } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("Designation");
            }
            up.ParamType = "U";
            up.Machine_IP = Session["Ip_Address"].ToString();
            up.UpdatedBy = Session["UserName"].ToString();
            bool result = bal.InsertDesignation(up);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("Designation");
            }

            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("Designation");

        }
        public ActionResult DeleteDesignation(string Des_Code = null)
        {
            ModelDesignation obj = new ModelDesignation
            {
                DesCode = Des_Code,
                ParamType = "D",
                Active = "1",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };
            bool result = bal.InsertDesignation(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("Designation");

            }
            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("Designation");
        }

        #endregion

        #region AddEmployee


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

        public ActionResult AddEmployee(string Emp_Code = null)
        {
            ViewBag.GenderList = bal.BindGender("Gender_Name", "Gender_Code", "M_Gender");
            ViewBag.DesignationList = bal.BindDesignation();
            ViewBag.EstTypeList = bal.BindEstType();

            var employees = bal.GetEmployeeList();
            ModelAddEmployee employee = null;
            if (!string.IsNullOrEmpty(Emp_Code))
            {

                employee = employees.FirstOrDefault(d => d.Emp_Code == Emp_Code);
            }

            ViewBag.EditEmployee = employee;

            return View(employees);

        }

        [HttpPost]
        public ActionResult AddEmployee(ModelAddEmployee emp)
        {
            if (string.IsNullOrWhiteSpace(emp.Emp_Code))
            {
                emp.Emp_Code = emp.Emp_CodeDisplay;
            }

            HttpPostedFileBase file = Request.Files["ProfileImage"];

            if (file != null && file.ContentLength > 0)
            {
                string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName);
                string datePart = DateTime.Now.ToString("yyyyMMdd"); // e.g., 20250702

                string uniqueFileName = $"{originalFileName}_{datePart}{extension}";
                string filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), uniqueFileName);

                file.SaveAs(filePath);       // Save image to server
                emp.filename = uniqueFileName;       // Save unique name to DB
            }
            //if (emp.ProfileImage != null && emp.ProfileImage.ContentLength > 0)
            //{
            //    string fileName = Path.GetFileName(emp.ProfileImage.FileName);
            //    string filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
            //    emp.ProfileImage.SaveAs(filePath);  // Save image to server
            //    emp.profile_name = fileName;       // Set for DB
            //}
            else
            {
                emp.filename = ""; // Default image fallback
            }


            DataSet ds = new DataSet();
            DataSet dsEd = new DataSet();
            ds = bal.FN_ExecuteQuerySingle("proc_insert_employee_master '" + emp.Emp_Code + "','" + emp.Emp_Name + "','" + emp.Father_Name + "','" + emp.Spouse_Name + "','" + emp.Gender_Code + "','" + emp.DOB + "','" + emp.Contact_No + "','" + emp.Email + "','" + emp.Adhar + "','1','" + emp.Address + "','" + emp.Remark + "','" + Session["UserName"].ToString() + "','" + Session["Ip_Address"].ToString() + "','" + emp.filename + "' ");



            // 2. Get EstDeptSections JSON and Deserialize
            string jsonData = Request.Form["EstDeptSections"];
            if (!string.IsNullOrEmpty(jsonData))
            {
                emp.EstDeptSections = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModelEstDeptSection>>(jsonData);
            }
            else
            {
                emp.EstDeptSections = new List<ModelEstDeptSection>();
            }

            foreach (var item in emp.EstDeptSections)
            {
                string mapQuery = $"proc_insert_employerdepartment '{emp.Emp_Code}','{emp.Emp_Name}','{item.Type_}','{item.Type_Text}','{item.Dep_Code}','{item.Dep_Text}','{item.Sec_Code}','{item.Sec_Text}','{item.Des_Code}','{item.Des_Text}'";



                dsEd = bal.FN_ExecuteQuerySingle(mapQuery);
            }

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && dsEd.Tables.Count > 0 && dsEd.Tables[0].Rows.Count > 0)
            {
                TempData["insertmsg"] = ds.Tables[0].Rows[0]["msg"].ToString() == "s" && dsEd.Tables[0].Rows[0]["msg"].ToString() == "s" ? "1" : "2";
            }


            return RedirectToAction("AddEmployee");

        }

        [HttpPost]
        public ActionResult EditEmployee(ModelAddEmployee emp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emp.Emp_Code))
                {
                    emp.Emp_Code = emp.Emp_CodeDisplay;
                }

                // Handle file upload
                HttpPostedFileBase file = Request.Files["ProfileImage"];

                if (file != null && file.ContentLength > 0)
                {
                    string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string datePart = DateTime.Now.ToString("yyyyMMdd");

                    string uniqueFileName = $"{originalFileName}_{datePart}{extension}";
                    string filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), uniqueFileName);

                    file.SaveAs(filePath);
                    emp.filename = uniqueFileName;
                }
                else
                {
                    // Keep existing filename if no new file uploaded
                    emp.filename = emp.filename ?? "";
                }

                DataSet ds = new DataSet();
                DataSet dsEd = new DataSet();

                // Update employee master record
                ds = bal.FN_ExecuteQuerySingle("proc_update_employee_master '" + emp.Emp_Code + "','" + emp.Emp_Name + "','" + emp.Father_Name + "','" + emp.Spouse_Name + "','" + emp.Gender_Code + "','" + emp.DOB + "','" + emp.Contact_No + "','" + emp.Email + "','" + emp.Adhar + "','1','" + emp.Address + "','" + emp.Remark + "','" + Session["UserName"].ToString() + "','" + Session["Ip_Address"].ToString() + "','" + emp.filename + "' ");

                // Get EstDeptSections JSON and Deserialize
                string jsonData = Request.Form["EstDeptSections"];
                if (!string.IsNullOrEmpty(jsonData))
                {
                    emp.EstDeptSections = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModelEstDeptSection>>(jsonData);
                }
                else
                {
                    emp.EstDeptSections = new List<ModelEstDeptSection>();
                }

                // Delete existing department mappings and insert new ones
                if (emp.EstDeptSections.Count > 0)
                {
                    // First delete existing mappings
                    bal.FN_ExecuteQuerySingle("DELETE FROM EmployerDepartment WHERE Emp_Code = '" + emp.Emp_Code + "'");

                    // Insert new mappings
                    foreach (var item in emp.EstDeptSections)
                    {
                        string mapQuery = $"proc_insert_employerdepartment '{emp.Emp_Code}','{emp.Emp_Name}','{item.Type_}','{item.Type_Text}','{item.Dep_Code}','{item.Dep_Text}','{item.Sec_Code}','{item.Sec_Text}','{item.Des_Code}','{item.Des_Text}'";
                        dsEd = bal.FN_ExecuteQuerySingle(mapQuery);
                    }
                }

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string msg = ds.Tables[0].Rows[0]["msg"].ToString();
                    if (msg == "s" || msg.ToLower().Contains("success") || msg.ToLower().Contains("updated"))
                    {
                        TempData["insertmsg"] = "updated";
                        return RedirectToAction("AddEmployee");
                    }
                }

                TempData["insertmsg"] = "updatefail";
                return RedirectToAction("AddEmployee");
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                TempData["insertmsg"] = "error";
                return RedirectToAction("AddEmployee");
            }
        }
        public ActionResult DeleteEmployee(string Row_Id = null)
        {
            bool result = bal.DeleteEmployee(Row_Id);
            if (result)
            {
                TempData["insertmsg"] = "Deleted";
                return RedirectToAction("AddEmployee");

            }
            TempData["insertmsg"] = "Deletefail";
            return View();
        }
        #endregion

        #region DocumentPriority
        public ActionResult DocumentPriority(string DocPrior_Code = null)
        {
            var documents = bal.GetDocPriorList();
            ModelDocPriority document = null;
            if (!string.IsNullOrEmpty(DocPrior_Code))
            {

                document = documents.FirstOrDefault(d => d.DocPrior_Code == DocPrior_Code);
            }

            ViewBag.EditDocument = document;

            return View(documents);
        }
        [HttpPost]
        public ActionResult DocumentPriority(ModelDocPriority doc)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_DocPriority", new Dictionary<string, object> { { "@Prior_Name", doc.Doc_PriorName } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("DocumentPriority");
            }
            doc.CreatedBy = Session["UserName"].ToString();
            doc.Machine_IP = Session["Ip_Address"].ToString();
            doc.ParamType = "I";
            doc.isactive = "0";

            bool result = bal.InsertDocPrior(doc);

            if (result)
            {
                TempData["insertmsg"] = "1";
            }
            else
            {
                TempData["insertmsg"] = "2";
            }

            return RedirectToAction("DocumentPriority");

        }
        [HttpPost]
        public ActionResult EditDocument(ModelDocPriority doc)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_DocPriority", new Dictionary<string, object> { { "@Prior_Name", doc.Doc_PriorName } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("DocumentPriority");
            }
            doc.UpdatedBy = Session["UserName"].ToString();
            doc.Machine_IP = Session["Ip_Address"].ToString();
            doc.ParamType = "U";

            bool result = bal.InsertDocPrior(doc);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("DocumentPriority");
            }
            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("DocumentPriority");

        }
        public ActionResult DeleteDocument(string DocPrior_Code = null)
        {
            ModelDocPriority obj = new ModelDocPriority
            {
                DocPrior_Code = DocPrior_Code,
                ParamType = "D",
                isactive = "1",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };

            bool result = bal.InsertDocPrior(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("DocumentPriority");
            }
            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("DocumentPriority");
        }
        #endregion

        #region  DocumentType
        public ActionResult DocumentType(string DocType_Code = null)
        {
            var docTypes = bal.GetDocTypeList();
            ModelDocType docType = null;
            if (!string.IsNullOrEmpty(DocType_Code))
            {

                docType = docTypes.FirstOrDefault(d => d.DocType_Code == DocType_Code);
            }

            ViewBag.EditDocumentType = docType;

            return View(docTypes);
        }
        [HttpPost]
        public ActionResult DocumentType(ModelDocType doctype)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_DocType", new Dictionary<string, object> { { "@DocType_Name", doctype.DocType_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("DocumentType");
            }
            doctype.CreatedBy = Session["UserName"].ToString();
            doctype.TableID = "0";
            doctype.Machine_IP = Session["Ip_Address"].ToString();
            doctype.ParamType = "I";
            doctype.Active = "1";

            bool result = bal.InsertDocType(doctype);

            if (result)
            {
                TempData["insertmsg"] = "1";
            }
            else
            {
                TempData["insertmsg"] = "2";
            }

            return RedirectToAction("DocumentType");

        }
        [HttpPost]
        public ActionResult EditDocumentType(ModelDocType Doctype)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_DocType", new Dictionary<string, object> { { "@DocType_Name", Doctype.DocType_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("DocumentType");
            }
            Doctype.UpdatedBy = Session["UserName"].ToString();
            Doctype.TableID = "0";
            Doctype.Machine_IP = Session["Ip_Address"].ToString();
            Doctype.ParamType = "U";

            bool result = bal.InsertDocType(Doctype);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("DocumentType");
            }

            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("DocumentType");
        }
        public ActionResult DeleteDocumentType(string DocType_Code = null)
        {
            ModelDocType obj = new ModelDocType
            {
                DocType_Code = DocType_Code,
                ParamType = "D",
                Active = "0",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };

            bool result = bal.InsertDocType(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("DocumentType");

            }
            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("DocumentType");
        }
        #endregion

        #region FileCategory
        public ActionResult FileCategory(string FileCat_Code = null)
        {
            var categories = bal.GetFileCategoryList();
            ModelFileCategory category = null;
            if (!string.IsNullOrEmpty(FileCat_Code))
            {

                category = categories.FirstOrDefault(d => d.FileCat_Code == FileCat_Code);
            }

            ViewBag.EditFileCategory = category;

            return View(categories);
        }
        [HttpPost]
        public ActionResult FileCategory(ModelFileCategory i)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_FileCat", new Dictionary<string, object> { { "@FileCat_Name", i.FileCat_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("FileCategory");
            }
            i.CreatedBy = Session["UserName"].ToString();
            i.TableID = "0";
            i.Machine_IP = Session["Ip_Address"].ToString();
            i.ParamType = "I";
            i.isactive = "0";

            bool result = bal.InsertFileCategory(i);

            if (result)
            {
                TempData["insertmsg"] = "1";
            }
            else
            {
                TempData["insertmsg"] = "2";
            }


            return RedirectToAction("FileCategory");
        }
        [HttpPost]
        public ActionResult EditFileCategory(ModelFileCategory up)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_FileCat", new Dictionary<string, object> { { "@FileCat_Name", up.FileCat_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("FileCategory");
            }
            up.UpdatedBy = Session["UserName"].ToString();
            up.TableID = "0";
            up.Machine_IP = Session["Ip_Address"].ToString();
            up.ParamType = "U";

            bool result = bal.InsertFileCategory(up);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("FileCategory");
            }

            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("FileCategory");
        }
        public ActionResult DeleteFileCategory(string FileCat_Code = null)
        {
            ModelFileCategory obj = new ModelFileCategory
            {
                FileCat_Code = FileCat_Code,
                ParamType = "D",
                isactive = "1",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };

            bool result = bal.InsertFileCategory(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("FileCategory");

            }
            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("FileCategory");
        }
        #endregion

        #region FileSubCategory 
        public ActionResult FileSubCategory(string FileSubCat_Code = null)
        {
            // Bind dropdown list
            ViewBag.FileCategoryList = bal.BindFileCat("FileCat_Name", "FileCat_Code", "M_FileCat");

            var subcategories = bal.GetFileSubCategoryList();
            ModelFileSubCategory subcategory = null;
            if (!string.IsNullOrEmpty(FileSubCat_Code))
            {

                subcategory = subcategories.FirstOrDefault(d => d.FileSubCat_Code == FileSubCat_Code);
            }

            ViewBag.EditFileSubCat = subcategory;

            return View(subcategories);
        }
        [HttpPost]
        public ActionResult FileSubCategory(ModelFileSubCategory i)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_FileSubCat", new Dictionary<string, object> { { "@FileSubCat_Name", i.FileSubCat_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("FileSubCategory");
            }
            i.CreatedBy = Session["UserName"].ToString();
            i.TableID = "0";
            i.Machine_IP = Session["Ip_Address"].ToString();
            i.ParamType = "I";

            bool result = bal.InsertFileSubCategory(i);

            if (result)
            {
                TempData["insertmsg"] = "1";
            }
            else
            {
                TempData["insertmsg"] = "2";
            }

            return RedirectToAction("FileSubCategory");
        }
        [HttpPost]
        public ActionResult EditFileSubCat(ModelFileSubCategory up)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_FileSubCat", new Dictionary<string, object> { { "@FileSubCat_Name", up.FileSubCat_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("FileSubCategory");
            }
            up.UpdatedBy = Session["UserName"].ToString();
            up.TableID = "0";
            up.Machine_IP = Session["Ip_Address"].ToString();
            up.ParamType = "U";

            bool result = bal.InsertFileSubCategory(up);
            if (result)
            {
                TempData["insertmsg"] = "updated";
                return RedirectToAction("FileSubCategory");
            }

            TempData["insertmsg"] = "updatefail";
            return RedirectToAction("FileSubCategory");
        }
        public ActionResult DeleteFileSubCat(string FileSubCat_Code = null)
        {
            ModelFileSubCategory obj = new ModelFileSubCategory
            {
                FileSubCat_Code = FileSubCat_Code,
                ParamType = "D",
                UpdatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString()
            };

            bool result = bal.InsertFileSubCategory(obj);
            if (result)
            {
                TempData["insertmsg"] = "deleted";
                return RedirectToAction("FileSubCategory");

            }
            TempData["insertmsg"] = "deletefail";
            return RedirectToAction("FileSubCategory");
        }
        #endregion

        #region Constituent College 
        public ActionResult ConstituentCollege(string College_ID = null)
        {
            var ConstituentColleges = bal.GetConstituentCollegeList();
            ModelConstituentCollege ConstituentCollege = null;
            if (!string.IsNullOrEmpty(College_ID))
            {

                ConstituentCollege = ConstituentColleges.FirstOrDefault(d => d.College_ID == College_ID);
            }

            ViewBag.EditConstituentCollege = ConstituentCollege;

            return View(ConstituentColleges);
        }

        [HttpPost]
        public ActionResult ConstituentCollege(ModelConstituentCollege i)
        {

            string dupMsg = bal.CheckDuplicateMaster("USP_CollegeList", new Dictionary<string, object> { { "@College_Name", i.College_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("ConstituentCollege");
            }

            i.CreatedBy = Session["UserName"].ToString();
            i.Machine_IP = Session["Ip_Address"].ToString();
            i.ParamType = "I";
            i.Status = "0";

            bool result = bal.InsertConstituentCollege(i);

            if (result)
            {
                TempData["Message"] = "Constituent College added successfully!";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Failed to add Constituent College.";
                TempData["MessageType"] = "error";
            }


            return RedirectToAction("ConstituentCollege");
        }
        [HttpPost]
        public ActionResult EditConstituentCollege(ModelConstituentCollege up)
        {
            string dupMsg = bal.CheckDuplicateMaster("USP_CollegeList", new Dictionary<string, object> { { "@College_Name", up.College_Name } });
            if (!string.IsNullOrEmpty(dupMsg))
            {
                TempData["insertmsg"] = dupMsg;
                return RedirectToAction("ConstituentCollege");
            }

            up.CreatedBy = Session["UserName"].ToString();
            up.TableID = "0";
            up.Machine_IP = Session["Ip_Address"].ToString();
            up.ParamType = "U";

            bool result = bal.InsertConstituentCollege(up);
            if (result)
            {
                TempData["Message"] = "Constituent College updated successfully!";
                TempData["MessageType"] = "success";
                return RedirectToAction("ConstituentCollege");
            }

            else
            {
                TempData["Message"] = "Failed to update Constituent College.";
                TempData["MessageType"] = "error";
            }
            return RedirectToAction("ConstituentCollege");
        }



        public ActionResult DeleteConstituentCollege(string College_ID = null)
        {
            ModelConstituentCollege obj = new ModelConstituentCollege
            {
                College_ID = College_ID,
                ParamType = "D",
                CreatedBy = Session["UserName"].ToString(),
                Machine_IP = Session["Ip_Address"].ToString(),
                Status = "1"
            };

            bool result = bal.InsertConstituentCollege(obj);
            TempData["insertmsg"] = result ? "deleted" : "deletefail";
            return RedirectToAction("ConstituentCollege");
        }

        #endregion


        #region Employee Transfer


        [HttpGet]
        public JsonResult GetEmployeeTransferList()
        {
            var data = bal.GetEmployeeTransfers();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmployeeTransfer()
        {
            ViewBag.EstTypeList = bal.BindEstType();
            ViewBag.DesignationList = bal.BindDesignation();
            return View();
        }

        [HttpPost]
        public ActionResult EmployeeTransfer(ModelEmployeeTransfer obj)
        {
            string empCode = obj.Emp_Code;
            string savedFilePath = null;

            HttpPostedFileBase file = Request.Files["OrderFile"];
            string fileName = string.Empty;

            // Handle file upload 
            if (file != null && file.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads/OrderFiles/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                fileName = Path.GetFileName(file.FileName);
                string fullPath = Path.Combine(uploadsFolder, fileName);
                file.SaveAs(fullPath);

                savedFilePath = "/Uploads/OrderFiles/" + fileName;
            }


            obj.Order_File_Name = fileName;
            obj.UserId = Session["UserID"].ToString();

            DataSet ds = bal.InsertEmployeeTransfer(obj);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                TempData["insertmsg"] = "1";
            }
            else
            {
                TempData["insertmsg"] = "2";
            }

            return RedirectToAction("EmployeeTransfer");
        }

        [HttpPost]
        public JsonResult GetEmployeeTransferById(string empCode)
        {
            var result = bal.GetEmployeeTransferById(empCode);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Employee Report 
        // Loads the view (initially with empty model)
        public ActionResult EmployeeReort()
        {
            // Bind dropdown list
            ViewBag.EstTypeList = bal.BindEstType();
            // Fetch all employees initially (no filter)
            var empList = bal.GetEmployeeReport(null);

            return View(empList);
        }

        // AJAX call to get data by EstType
        [HttpGet]
        public JsonResult GetEmployeeReportByEstType(string EstType)
        {
            var data = bal.GetEmployeeReport(EstType);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Add user

        [HttpPost]
        public JsonResult GetEmployeeBySection(string Sec_Code, string Dep_Code, string EstTypeCode)
        {
            var employees = bal.BindEmployee(Sec_Code, Dep_Code, EstTypeCode);

            var result = employees.Select(x => new
            {
                Value = x.Value,
                Text = x.Text
            });

            return Json(result);
        }

        [HttpPost]
        public JsonResult GetEmployeeForEdit(string empCode)
        {
            try
            {
                DataSet ds = bal.GetEmployeeForEdit(empCode);
                
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    
                    // Get department mappings
                    DataTable dtMappings = bal.GetEmployeeDepartmentMappings(empCode);
                    
                    var mappings = new List<object>();
                    foreach (DataRow mapRow in dtMappings.Rows)
                    {
                        mappings.Add(new
                        {
                            Type = mapRow["Type"].ToString(),
                            Department = mapRow["Department"].ToString(),
                            Section = mapRow["Section"].ToString(),
                            Designation = mapRow["Designation"].ToString()
                        });
                    }

                    string fileName = row["fileName"].ToString();
                    string imageUrl = string.IsNullOrEmpty(fileName) 
                        ? Url.Content("~/Content/Images/default.png") 
                        : Url.Content("~/UploadedFiles/" + fileName);

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            Emp_Code = row["Emp_Code"].ToString(),
                            Emp_Name = row["Emp_Name"].ToString(),
                            Father_Name = row["Father_Name"].ToString(),
                            Spouse_Name = row["Spouse_Name"].ToString(),
                            Gender = row["Gender"].ToString(),
                            DOB = row["DOB"].ToString(),
                            Contact_No = row["Contact_No"].ToString(),
                            E_Mail = row["E_Mail"].ToString(),
                            Remark = row["Remark"].ToString(),
                            Adhar = row["Adhar"].ToString(),
                            Adddress = row["Adddress"].ToString(),
                            Active = row["Active"].ToString(),
                            ImageUrl = imageUrl,
                            FileName = fileName,
                            DepartmentMappings = mappings
                        }
                    });
                }

                return Json(new { success = false, message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult CreateUser(string TableID = null) /*string TableID = null*/
        {
            ViewBag.EstTypeList = bal.BindAdminEstType();  // another binding for admin
            ViewBag.EmpTypeList = bal.BindEmp_Type();
            ViewBag.UserRoleList = bal.BindUserRole();

            var users = bal.GetUserList();
            ModelUser user = null;

            ModelUser editUser = null;


            if (!string.IsNullOrEmpty(TableID))
            {
                var editUserList = bal.GetUserListForEdit();
                editUser = editUserList.FirstOrDefault(d => d.EmpID == TableID);
                editUser.Password = cryptography.DecryptText(editUser.Password);
                //user = users.FirstOrDefault(d => d.EmpID == TableID);
            }

            ViewBag.EditCreateUser = editUser;

            return View(users); /*users*/

        }

        [HttpPost]
        public ActionResult CreateUser(ModelUser user)
        {
            DataSet ds = new DataSet();
            ds = bal.FN_ExecuteQuerySingle("proc_check_userid '" + user.EstType + "','" + user.EstDep + "','" + user.EstSec + "','" + user.EmpID + "'");
            if (ds.Tables[0].Rows.Count > 0)
            {
                TempData["msg"] = "3";
                return RedirectToAction("CreateUser");
            }
            else
            {
                string normalpwd = user.Password;
                user.Password = cryptography.EncryptText(user.Password);

                user.status = "1";
                user.createdby = "001";
                //if (!string.IsNullOrEmpty(user.LoginName))
                //{
                //    string usr = "select LoginName from Utility_MUser where LoginName='" + user.LoginName.Trim() + "'";
                //    DataTable dt = bal.EQ(usr);
                //    if (dt.Rows.Count > 0)
                //    {
                //        if (dt.Rows[0][0].ToString() == user.LoginName.Trim())
                //        {
                //            TempData["msg"] = "2";
                //            return RedirectToAction("CreateUser");
                //        }
                //    }
                //}

                if (!string.IsNullOrEmpty(user.LoginName))
                {
                    if (bal.IsLoginNameExists(user.LoginName.Trim()))
                    {
                        TempData["msg"] = "2";
                        return RedirectToAction("CreateUser");
                    }
                }


                bal.InsertAddUser(user);

                DataSet dsuserpwd = new DataSet();
                dsuserpwd = bal.FN_ExecuteQuerySingle("proc_create_user_pwd '" + user.EmpID + "','" + user.Name + "','" + user.LoginName + "','" + normalpwd + "','" + user.Password + "' ");
                if (dsuserpwd.Tables[0].Rows.Count > 0)
                {
                    TempData["msg"] = "1";
                    return RedirectToAction("CreateUser");
                }
            }


            TempData["msg"] = "4";
            return RedirectToAction("CreateUser");
        }


        //[HttpPost]
        //public JsonResult GetEmployeeDetails(string empId, string estType, string depCode, string secCode)
        //{
        //    DataTable dt2 = new DataTable();

        //    string str1 = "SELECT  Emp_Name, Adddress, E_mail,Contact_No,Sec_Name FROM [M_Employee] LEFT JOIN [M_Section] on[M_Section].Sec_Code =[M_Employee].Sec_Code where Emp_Code='" + empId + "'";
        //    DataTable dt = bal.EQ(str1);

        //    if (dt.Rows.Count > 0)
        //    {
        //        if (estType != "3")
        //        {
        //            string str2 = "select Est_desigCode,id from EmployerDepartment where  Emp_Code ='" + empId + "' and Est_secCode='" + secCode + "' and Est_deptCode = '" + depCode + "'";
        //            dt2 = bal.EQ(str2);

        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            empName = dt.Rows[0]["Emp_Name"].ToString(),
        //            address = dt.Rows[0]["Adddress"].ToString(),
        //            email = dt.Rows[0]["E_mail"].ToString(),
        //            mobile = dt.Rows[0]["Contact_No"].ToString(),
        //            desigCode = dt2.Rows.Count > 0 ? dt2.Rows[0]["Est_desigCode"].ToString() : "",
        //            edRowId = dt2.Rows.Count > 0 ? dt2.Rows[0]["id"].ToString() : ""
        //        });


        //    }



        //    return Json(new { success = false });
        //}


        [HttpPost]
        public JsonResult GetEmployeeDetails(string empId, string estType, string depCode, string secCode)
        {
            var (empDetails, desigDetails) = bal.GetEmployeeDetails(empId, estType, depCode, secCode);

            if (empDetails.Rows.Count > 0)
            {
                return Json(new
                {
                    success = true,
                    empName = empDetails.Rows[0]["Emp_Name"].ToString(),
                    address = empDetails.Rows[0]["Adddress"].ToString(),
                    email = empDetails.Rows[0]["E_mail"].ToString(),
                    mobile = empDetails.Rows[0]["Contact_No"].ToString(),
                    desigCode = desigDetails.Rows.Count > 0 ? desigDetails.Rows[0]["Est_desigCode"].ToString() : "",
                    edRowId = desigDetails.Rows.Count > 0 ? desigDetails.Rows[0]["id"].ToString() : ""
                });
            }

            return Json(new { success = false });
        }


        #endregion

        [HttpPost]
        public ActionResult EditUser(ModelUser user)
        {
            user.ParamType = "U";

            bool result = bal.UpdateUser(user);
            if (result)
            {
                TempData["msg"] = "5";
                return RedirectToAction("CreateUser");
            }

            TempData["msg"] = "4";
            return View(user);

        }
        //public ActionResult DeleteUser(string TableID = null)
        //{
        //    bool result = bal.DeleteUser(TableID);
        //    if (result)
        //    {
        //        TempData["insertmsg"] = "deleted";
        //        return RedirectToAction("CreateUser");

        //    }
        //    TempData["insertmsg"] = "deletefail";
        //    return View();
        //}

        #region ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            // Assume the logged-in user is UserId = 1 (for testing)
            var model = new ModelChangePassword
            {
                UserId = 1
            };
            return View(model);
        }

        //public ActionResult EmployeeTransferForm()
        //{
        //    return View();
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ModelChangePassword model)

        {
            if (!ModelState.IsValid)
                return View(model);

            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            string loginname = Session["UserName"].ToString();

            string strpwd = cryptography.EncryptText(model.CurrentPassword);
            string strnewpwd = cryptography.EncryptText(model.NewPassword);
            string str1 = "";
            str1 = @"select a.EmpID,b.Emp_Name,a.LoginName,a.Password,b.E_Mail,b.Contact_No,a.Is_Pwd_Change from Utility_MUser a inner join M_Employee b on  a.EmpID=b.Emp_Code where a.LoginName='" + loginname + "' and a.LoginStatus='1' ";
            DataTable dt111 = bal.EQ(str1);

            if (dt111.Rows.Count > 0)
            {
                string EmpID = "", Emp_Name = "";
                EmpID = dt111.Rows[0]["EmpID"].ToString();
                Emp_Name = dt111.Rows[0]["Emp_Name"].ToString();
                if (Session["UserName"].ToString() != "admin")
                {
                    DataTable dt = new DataTable();
                    dt = bal.selectPasword(loginname, strpwd);
                    if (dt.Rows.Count == 1)
                    {
                        bool updateFlag = false;
                        updateFlag = bal.updatePssword(loginname, strnewpwd);
                        if (updateFlag == true)
                        {
                            string st = "Select login_name from user_pwd where login_name ='" + loginname + "' ";
                            DataTable dt1 = bal.ExecuteDataTable(st);
                            if (dt1.Rows.Count > 0)
                            {
                                string str_up = "update user_pwd  set  plain_pwd ='" + model.NewPassword + "' , encripted_pwd='" + strnewpwd + "' where login_name ='" + loginname + "'";
                                bal.ExecuteNonQuery(str_up);
                            }
                            else
                            {
                                string str11 = "insert into user_pwd  (emp_id,emp_name,login_name,plain_pwd,encripted_pwd) values('" + EmpID + "','" + Emp_Name + "','" + loginname + "','" + model.NewPassword + "','" + strnewpwd + "' ) ";
                                DataTable dt2 = bal.EQ(str11);
                            }
                            TempData["msg"] = "Password changed. Please login again";
                            return RedirectToAction("Login", "Home");
                        }
                        else
                        {
                            TempData["msg"] = "Password not updated";
                            return RedirectToAction("ChangePassword");
                        }
                    }
                    else
                    {
                        TempData["msg"] = "Incorrect old password";
                        return RedirectToAction("ChangePassword");
                    }
                }
                else
                {
                    bool updateFlag = false;
                    updateFlag = bal.updatePssword(loginname, strnewpwd);
                    if (updateFlag == true)
                    {
                        //string str_up = "update user_pwd  set  plain_pwd ='" + txtPassword.Text.Trim() + "' , encripted_pwd='" + strnewpwd + "' where login_name ='" + ViewState["LoginName"].ToString() + "'";
                        //cc.SUD(str_up);
                        //Messagebox.Show("Password Updated.");
                        string st = "Select login_name from user_pwd where login_name ='" + loginname + "' ";
                        DataTable dt1 = bal.ExecuteDataTable(st);
                        if (dt1.Rows.Count > 0)
                        {
                            string str_up = "update user_pwd  set  plain_pwd ='" + model.NewPassword + "' , encripted_pwd='" + strnewpwd + "' where login_name ='" + loginname + "'";
                            bal.ExecuteNonQuery(str_up);
                        }
                        else
                        {
                            string str11 = "insert into user_pwd  (emp_id,emp_name,login_name,plain_pwd,encripted_pwd) values('" + EmpID + "','" + Emp_Name + "','" + loginname + "','" + model.NewPassword + "','" + strnewpwd + "' ) ";
                            DataTable dt2 = bal.EQ(str11);
                        }
                        TempData["msg"] = "Password changed";
                        return RedirectToAction("Login", "Home");
                    }
                    else
                    {
                        TempData["msg"] = "Password not updated";
                        return RedirectToAction("ChangePassword");
                    }
                }
            }

            return RedirectToAction("ChangePassword");
        }

        #endregion


        public ActionResult OpenedFiles()
        {
            ViewBag.EstTypeList = bal.BindEstType();

            ModelAdminDashboard obj = new ModelAdminDashboard();
            obj.ParamType = "OPEN";
            DataTable dt = bal.GetAdminDashboardReports(obj);
            return View(dt);

        }
        public ActionResult TotalFiles()
        {
            ViewBag.EstTypeList = bal.BindEstType();
            // Bind financial year dropdown
            ViewBag.FinancialYear = bal.BindFinancialYear();

            ModelAdminDashboard obj = new ModelAdminDashboard();
            var AllFinYears = bal.BindFinancialYear();
            obj.ParamType = "FH";
            obj.fin_year = AllFinYears.FirstOrDefault()?.Text;
            DataTable dt = bal.GetAdminDashboardReports(obj);
            return View(dt);

           

        }
        public ActionResult ApprovedFiles()
        {
            ViewBag.EstTypeList = bal.BindEstType();

            ModelAdminDashboard obj = new ModelAdminDashboard();
            obj.ParamType = "AF";
            DataTable dt = bal.GetAdminDashboardReports(obj);
            return View(dt);

        }
        public ActionResult TotalEmployees()
        {
            DataSet ds = new DataSet();

            var estTypeList = bal.BindEstType(); // Get est type
            estTypeList.Insert(0, new SelectListItem { Value = "0", Text = "All" }); // Insert default value for All
            ViewBag.EstTypeList = estTypeList; // Pass to view
           
            ds = bal.FN_ExecuteQuerySingle("proc_Employee_Details '" + estTypeList.First().Value + "'");
            return View(ds);

        }



        [HttpPost]
        public JsonResult GetCreatedByEmployeesBySection(string Sec_Code, string Dep_Code, string EstTypeCode)
        {
            var employees = bal.BindCreatedByEmployee(Sec_Code, Dep_Code, EstTypeCode, Session["UserName"].ToString());

            var result = employees.Select(x => new
            {
                Value = x.Value,
                Text = x.Text
            });

            return Json(result);
        }


        #region Admin Filtered Reports

        [HttpPost]
        public JsonResult SearchOpenedFiles(string estType, string deptCode, string secCode, string empCode)
        {
            try
            {
                string ParamType = "OPEN";

                DataTable dt = bal.GetFilteredAdminReport(estType, deptCode, secCode, empCode, "", ParamType);

                var data = dt.AsEnumerable().Select((row, index) => new
                {
                    SNo = index + 1,
                    File_Code = row["File_Code"].ToString(),
                    File_Desc = row["File_Desc"].ToString(),
                    FileCat_Name = row["FileCat_Name"].ToString(),
                    FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                    DocPrior_Name = row["DocPrior_Name"].ToString(),
                    Sec_Name = row["Sec_Name"].ToString(),
                    Created_DT = row["Created_DT"].ToString(),
                    File_status = row["File_status"].ToString(),
                    file_Remark = row["file_Remark"].ToString()
                });

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #region Filtered Total Files

        [HttpPost]
        public JsonResult SearchTotalFiles(string estType, string deptCode, string secCode, string empCode)
        {
            try
            {
                string ParamType = "FH";

                DataTable dt = bal.GetFilteredAdminReport(estType, deptCode, secCode, empCode, "", ParamType);

                var data = dt.AsEnumerable().Select((row, index) => new
                {
                    SNo = index + 1,
                    File_Code = row["File_Code"].ToString(),
                    File_Desc = row["File_Desc"].ToString(),
                    FileCat_Name = row["FileCat_Name"].ToString(),
                    FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                    DocPrior_Name = row["DocPrior_Name"].ToString(),
                    Sec_Name = row["Sec_Name"].ToString(),
                    Created_DT = row["Created_DT"].ToString(),
                    //File_status = row["File_status"].ToString(),
                    //file_Remark = row["file_Remark"].ToString()
                });

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult TotalFilesSearchByFinYear(string finYear)
        {
            try
            {
                string ParamType = "FH";

                DataTable dt = bal.GetFilteredAdminReport("", "", "", "", finYear, ParamType);

                var data = dt.AsEnumerable().Select((row, index) => new
                {
                    SNo = index + 1,
                    File_Code = row["File_Code"].ToString(),
                    File_Desc = row["File_Desc"].ToString(),
                    FileCat_Name = row["FileCat_Name"].ToString(),
                    FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                    DocPrior_Name = row["DocPrior_Name"].ToString(),
                    Sec_Name = row["Sec_Name"].ToString(),
                    Created_DT = row["Created_DT"].ToString(),
                    //File_status = row["File_status"].ToString(),
                    //file_Remark = row["file_Remark"].ToString()
                });

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion



        [HttpPost]
        public JsonResult SearchApprovedFiles(string estType, string deptCode, string secCode, string empCode)
        {
            try
            {
                string ParamType = "AF";

                DataTable dt = bal.GetFilteredAdminReport(estType, deptCode, secCode, empCode, "", ParamType);

                var data = dt.AsEnumerable().Select((row, index) => new
                {
                    SNo = index + 1,
                    File_Code = row["File_Code"].ToString(),
                    File_Desc = row["File_Desc"].ToString(),
                    FileCat_Name = row["FileCat_Name"].ToString(),
                    FileSubCat_Name = row["FileSubCat_Name"].ToString(),
                    DocPrior_Name = row["DocPrior_Name"].ToString(),
                    Sec_Name = row["Sec_Name"].ToString(),
                    F_From = row["F_From"].ToString(),
                    F_To = row["F_To"].ToString(),
                    sent_dt = row["sent_dt"].ToString(),
                    Created_DT = row["Created_DT"].ToString(),
                    File_status = row["File_status"].ToString(),
                    file_Remark = row["Remark"].ToString()
                });

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult SearchEmployees(string estType)
        {
            try
            {

                var data = bal.GetAdminEmployeeList(estType);



                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        public ActionResult UserDetails()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetUserDetails(string loginName)
        {
            UserBAL bal = new UserBAL();
            var data = bal.GetUserDetailsByLogin(loginName);
            return Json(data);
        }

    }
}