using Eoffice.DAL;
using Eoffice.Models;
using Eoffice.Security;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eoffice.BAL
{
    public class UserBAL
    {
        UserDAL dal = new UserDAL();
        public DataSet GetLogin(ModelClass user)
        {
            DataSet ds = dal.GetUserData(user);
            return ds;
        }

        public void CheckLastAccess(string userid)
        {
            dal.CheckLastAccess(userid);
        }

        public DataSet GetPassword(ModelClass user)
        {
            DataSet ds = dal.GetPassword(user);
            return ds;
        }

        #region Department 
        public bool InsertDepartment(ModelDepartment obj)
        {
            bool result = dal.InsertDepartment(obj);
            return result;
        }
        public List<ModelDepartment> GetDepartmentList()
        {
            List<ModelDepartment> deptList = new List<ModelDepartment>();

            deptList = dal.GetDepartmentList();

            return deptList;
        }
        public bool UpdateDepartment(ModelDepartment obj)
        {
            bool result = dal.UpdateDepartment(obj);
            return result;
        }

        public List<SelectListItem> BindEstType()
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = null;

            // Call DAL
            DataTable dt = dal.BindDropdown("TY", "USP_DocType", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Type_id"].ToString(),
                    Text = row["TypeName"].ToString()
                });
            }

            return list;
        }


        public List<SelectListItem> BindAdminEstType()
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = null;

            // Call DAL
            DataTable dt = dal.BindDropdown("admin", "USP_DocType", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Type_id"].ToString(),
                    Text = row["TypeName"].ToString()
                });
            }

            return list;
        }

        //public bool DeleteDepartment(string DepCode)
        //{
        //    bool result = dal.DeleteDepartment(DepCode);
        //    return result;
        //}
        #endregion

        #region Dropdown list
        public List<DropdownModel> GetDropDownList(DropdownModel obj)
        {
            List<DropdownModel> ls = new List<DropdownModel>();

            ls = dal.GetDropDownList(obj);

            return ls;
        }
        #endregion

        #region section 

        public List<SelectListItem> BindDepartment(int DocType_Code)
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@DocType_Code", DocType_Code },
                
            };

            // Call DAL
            DataTable dt = dal.BindDropdown("TN2", "USP_DocType", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Dep_Code"].ToString(),
                    Text = row["Dep_Name"].ToString()
                });
            }

            return list;
        }

        public bool InsertSection(ModelSection obj)
        {
            bool result = dal.InsertSection(obj);
            return result;
        }


        //public List<ModelSection> GetSectionList() 
        //{
        //    List<ModelSection> sectionList = new List<ModelSection>();

        //    string where = "";
        //    string str2 = @"select M_Section.Dep_Code,Sec_Code,Sec_Name,M_Section.Shrt_Name ,Dep_TypeCode ,REPLACE(REPLACE( Dep_Type,'1-',''),'2-','') as DepTypeName,M_Department.Dep_Name,M_Section.Active from M_Section inner join M_Department on M_Department.Dep_Code=M_Section.Dep_Code ";

        //    str2 = str2 + where + " and (isnull(M_Section.Active,'') = 0  ) order by DepTypeName, Dep_Name,Sec_Name";
        //    DataTable dtt = dal.EQ(str2);

        //    if (dtt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dtt.Rows)
        //        {
        //            ModelSection model = new ModelSection
        //            {
        //                Dep_Code = row["Dep_Code"].ToString(),
        //                Sec_Code = row["Sec_Code"].ToString(),
        //                Sec_Name = row["Sec_Name"].ToString(),
        //                sec_short_name = row["Shrt_Name"].ToString(),
        //                DeptypeCode = row["Dep_TypeCode"].ToString(),
        //                EstType = row["DepTypeName"].ToString(),
        //                DepName = row["Dep_Name"].ToString(),
        //                Active = row["Active"].ToString()
        //            };

        //            sectionList.Add(model);
        //        }
        //    }
        //    return sectionList;
        //}


        public List<ModelSection> GetSectionList()
        {
            DataTable dtt = dal.GetSectionList();
            List<ModelSection> sectionList = new List<ModelSection>();

            foreach (DataRow row in dtt.Rows)
            {
                sectionList.Add(new ModelSection
                {
                    Dep_Code = row["Dep_Code"].ToString(),
                    Sec_Code = row["Sec_Code"].ToString(),
                    Sec_Name = row["Sec_Name"].ToString(),
                    sec_short_name = row["Shrt_Name"].ToString(),
                    DeptypeCode = row["Dep_TypeCode"].ToString(),
                    EstType = row["DepTypeName"].ToString(),
                    DepName = row["Dep_Name"].ToString(),
                    Active = row["Active"].ToString()
                });
            }

            return sectionList;
        }



        public bool UpdateSection(ModelSection obj)
        {
            bool result = dal.UpdateSection(obj);
            return result;
        }
        public bool DeleteSection(string Sec_Code)
        {
            bool result = dal.DeleteSection(Sec_Code);
            return result;
        }
#endregion 

        #region Designation 
        public bool InsertDesignation(ModelDesignation obj)
        {
            bool result = dal.InsertDesignation(obj);
            return result;
        }
        public List<ModelDesignation> GetDesignationList()
        {
            List<ModelDesignation> DesList = new List<ModelDesignation>();

            DesList = dal.GetDesignationList();

            return DesList;
        }
        //public bool UpdateDesignation(ModelDesignation obj)
        //{
        //    bool result = dal.UpdateDesignation(obj);
        //    return result;
        //}
        //public bool DeleteDesignation(string Des_Code)
        //{
        //    bool result = dal.DeleteDesignation(Des_Code);
        //    return result;
        //}
        #endregion


        #region Add employee 
        public bool InsertEmployee(ModelAddEmployee obj)
        {
            bool result = dal.InsertEmployee(obj);
            return result;
        }
        public List<ModelAddEmployee> GetEmployeeList()
        {
            List<ModelAddEmployee> EmpList = new List<ModelAddEmployee>();

            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("proc_Employee_Details_Data");

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    ModelAddEmployee emp = new ModelAddEmployee
                    {
                        Emp_Code = row["Emp_Code"].ToString(),
                        Emp_Name = row["Employee_Name"].ToString(),
                        Contact_No = row["Contact_No"].ToString(),
                        Email = row["E_Mail"].ToString(),
                        Type_ = row["Est_TypeName"].ToString(),
                        Dep_Code = row["Est_deptName"].ToString(),
                        Sec_Code = row["Est_secName"].ToString(),
                    };

                    EmpList.Add(emp);
                }
            }

            return EmpList;
        }
        public bool UpdateEmployee(ModelAddEmployee obj)
        {
            bool result = dal.UpdateEmployee(obj);
            return result;
        }
        public bool DeleteEmployee(string Row_Id)
        {
            bool result = dal.DeleteEmployee(Row_Id);
            return result;
        }

        public DataSet GetEmployeeForEdit(string empCode)
        {
            return dal.GetEmployeeForEdit(empCode);
        }

        public DataTable GetEmployeeDepartmentMappings(string empCode)
        {
            return dal.GetEmployeeDepartmentMappings(empCode);
        }
        #endregion

        public List<SelectListItem> BindGender(string TextField, string ValueField, string tablename)
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        { "@TextField", TextField },
        { "@ValueField", ValueField },
        { "@tablename", tablename },

    };

            // Call DAL
            DataTable dt = dal.BindCommonDropdown("USP_FillDLL", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Gender_Code"].ToString(),
                    Text = row["Gender_Name"].ToString()
                });
            }

            return list;
        }


        //public List<SelectListItem> BindDesignation()
        //{
        //    string str = "select Des_Code,(convert(nvarchar,Des_Name)+' | '+Shrt_Name ) as Des_Name from M_Designation where (Active = null or isnull(Active,'')='0') order by Des_Name";
        //    DataTable dt = dal.EQ(str);

        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["Des_Code"].ToString(),
        //            Text = row["Des_Name"].ToString()
        //        });
        //    }

        //    return list;
        //}

        public List<SelectListItem> BindDesignation()
        {
            // Call DAL generic ExecuteDataTableSP
            DataTable dt = dal.ExecuteDataTableSP("USP_GetDesignationList", null);

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Des_Code"].ToString(),
                    Text = row["Des_Name"].ToString()
                });
            }

            return list;
        }



        //public List<SelectListItem> BindSection(string Dep_Code, string EstTypeCode)
        //{
        //    string str = "";
        //    str = "select Dep_Code,Sec_Code,Sec_Name,Dep_TypeCode,Shrt_Name from M_Section where Dep_Code='" + Dep_Code + "' and Dep_TypeCode='" + EstTypeCode + "' ORDER BY Sec_Name";
        //    DataTable dt = dal.EQ(str);

        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["Sec_Code"].ToString(),
        //            Text = row["Sec_Name"].ToString()
        //        });
        //    }

        //    return list;
        //}

        public List<SelectListItem> BindSection(string depCode, string estTypeCode)
        {

            // Prepare parameters
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Dep_Code", depCode),
                new SqlParameter("@EstTypeCode", estTypeCode)
            };

           

            DataTable dt = dal.ExecuteDataTableSP("USP_GetSections", parameters);

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Sec_Code"].ToString(),
                    Text = row["Sec_Name"].ToString()
                });
            }

            return list;
        }


        public List<SelectListItem> BindAllSections(ModelAddDocument obj)
        {
            obj.doc_code = DeterministicEncryptionHelper.Encrypt(obj.doc_code);

            DataTable dt = dal.BindAllSections(obj);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Sec_Code"].ToString(),
                    Text = row["Sec_Name"].ToString()
                });
            } 

            return list;
        }


        public List<SelectListItem> BindEmployee(string Sec_Code, string Dep_Code, string EstTypeCode)
        {
            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("get_emp '" + EstTypeCode + "','" + Dep_Code + "','" + Sec_Code + "' ");


            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["code"].ToString(),
                    Text = row["name"].ToString()
                });
            }

            return list;
        }


        #region Document Priority 
        public bool InsertDocPrior(ModelDocPriority obj)
        {
            bool result = dal.InsertDocPrior(obj);
            return result;
        }
        public List<ModelDocPriority> GetDocPriorList()
        {
            List<ModelDocPriority> DocpriorList = new List<ModelDocPriority>();

            DocpriorList = dal.GetDocPriorList();

            return DocpriorList;
        }
        //public bool UpdateDocumentPriority(ModelDocPriority obj)
        //{
        //    bool result = dal.UpdateDocumentPriority(obj);
        //    return result;
        //}
        //public bool DeleteDocument(string DocPrior_Code)
        //{
        //    bool result = dal.DeleteDocument(DocPrior_Code);
        //    return result;
        //}
#endregion

        #region Document Type
        public bool InsertDocType(ModelDocType obj)
        {
            bool result=dal.InsertDocType(obj);
            return result;
        }
        public List<ModelDocType> GetDocTypeList()
        {
            List<ModelDocType> DoctypeList = new List<ModelDocType>();

            DoctypeList = dal.GetDocTypeList();

            return DoctypeList;
        }
        //public bool UpdateDocType(ModelDocType obj)
        //{
        //    bool result = dal.UpdateDocType(obj);
        //    return result;
        //}
        //public bool DeleteDocumentType(string DocType_Code)
        //{
        //    bool result = dal.DeleteDocumentType(DocType_Code);
        //    return result;
        //}
        #endregion

        #region FileCategory
        public bool InsertFileCategory(ModelFileCategory obj)
        {
            bool result = dal.InsertFileCategory(obj);
            return result;
        }
        public List<ModelFileCategory> GetFileCategoryList()
        {
            List<ModelFileCategory> fileCatList = new List<ModelFileCategory>();

            fileCatList = dal.GetFileCategoryList();

            return fileCatList;
        }
        //public bool UpdateFileCategory(ModelFileCategory obj)
        //{
        //    bool result = dal.UpdateFileCategory(obj);
        //    return result;
        //}
        //public bool DeleteFileCategory(string FileCat_Code)
        //{
        //    bool result = dal.DeleteFileCategory(FileCat_Code);
        //    return result;
        //}
        #endregion

        #region File Sub Category 

        public List<SelectListItem> BindFileCat(string TextField, string ValueField, string tablename)
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@TextField", TextField },
                { "@ValueField", ValueField },
                { "@tablename", tablename },

            };

            // Call DAL
            DataTable dt = dal.BindCommonDropdown("USP_FillDLL", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["FileCat_Code"].ToString(),
                    Text = row["FileCat_Name"].ToString()
                });
            }

            return list;
        }



        public List<SelectListItem> BindDesignation(string TextField, string ValueField, string tablename)
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@TextField", TextField },
                { "@ValueField", ValueField },
                { "@tablename", tablename },

            };

            // Call DAL
            DataTable dt = dal.BindCommonDropdown("USP_FillDLL", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Des_Code"].ToString(),
                    Text = row["Des_Name"].ToString()
                });
            }

            return list;
        }

        public bool InsertFileSubCategory(ModelFileSubCategory obj)
        {
            bool result = dal.InsertFileSubCategory(obj);
                return result;
        }
        public List<ModelFileSubCategory> GetFileSubCategoryList()
        {
            List<ModelFileSubCategory> filesubCatList = new List<ModelFileSubCategory>();

            filesubCatList = dal.GetFileSubCategoryList();

            return filesubCatList;
        }
        //public bool UpdateFileSubCat(ModelFileSubCategory obj)
        //{
        //    bool result = dal.UpdateFileSubCat(obj);
        //    return result;
        //}
        //public bool DeleteFileSubCat(string FileSubCat_Code)
        //{
        //    bool result = dal.DeleteFileSubCat(FileSubCat_Code);
        //    return result;
        //}
        #endregion

        #region ConstituentCollege
        public bool InsertConstituentCollege(ModelConstituentCollege obj)
        {
            bool result = dal.InsertConstituentCollege(obj);
            return result;
        }
        public List<ModelConstituentCollege> GetConstituentCollegeList()
        {
            List<ModelConstituentCollege> ClgList = new List<ModelConstituentCollege>();

            ClgList = dal.GetConstituentCollegeList();

            return ClgList;
        }
        public bool UpdateConstituentCollege(ModelConstituentCollege obj)
        {
            bool result = dal.UpdateConstituentCollege(obj);
            return result;
        }
        public bool DeleteConstituentCollege(string College_ID)
        {
            bool result = dal.DeleteConstituentCollege(College_ID);
            
            return result;
        }
        #endregion


        //public bool ChangePassword(int userId, string currentPassword, string newPassword, out string errorMessage)
        //{
        //    errorMessage = string.Empty;

        //    // Step 1: Verify current password (optional if stored proc checks it)
        //    var user = dal.GetUserById(userId);
        //    if (user == null)
        //    {
        //        errorMessage = "User not found.";
        //        return false;
        //    }

        //    if (user.Password != currentPassword)
        //    {
        //        errorMessage = "Current password is incorrect.";
        //        return false;
        //    }

        //    // Step 2: Call the new unified stored procedure via DAL
        //    bool success = dal.ChangeUserPassword(userId, newPassword, out errorMessage);

        //    return success;
        //}


        #region change password

        public DataTable selectPasword(string login, string pwd)
        {
            return dal.selectPasword(login, pwd);
        }

        public bool updatePssword(string login, string pwd)
        {
            return dal.updatePssword(login, pwd);
        }

        public DataTable ExecuteDataTable(string query)
        {
            DataTable dt = dal.ExecuteDataTable(query);
            
            // Automatically decrypt any encrypted columns in the DataTable
            dt = DataSetEncryptionHelper.DecryptDataTable(dt);
            
            return dt;
        }
        public int ExecuteNonQuery(string query)
        {
            return dal.ExecuteNonQuery(query);
        }
        #endregion

        #region BindEmp_Type
        //public List<SelectListItem> BindEmp_Type()
        //{
        //    DataTable dt = new DataTable();
        //    dt = dal.ExecuteDataTable("select * from M_Emp_Type");


        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["Emp_Type_ID"].ToString(),
        //            Text = row["Emp_Type"].ToString()
        //        });
        //    }

        //    return list;
        //}


        public List<SelectListItem> BindEmp_Type()
        {
            DataTable dt = dal.ExecuteDataTableSP("USP_GetEmpType", null);

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Emp_Type_ID"].ToString(),
                    Text = row["Emp_Type"].ToString()
                });
            }

            return list;
        }


        #endregion

        #region BindUserRole
        //public List<SelectListItem> BindUserRole()
        //{
        //    string str = "select tableid, RoleName from Utility_MRole  where tableid  not in(1)order by RoleName";
        //    DataTable dt = dal.ExecuteDataTable(str);


        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["tableid"].ToString(),
        //            Text = row["RoleName"].ToString()
        //        });
        //    }

        //    return list;
        //}


        public List<SelectListItem> BindUserRole()
        {
            DataTable dt = dal.ExecuteDataTableSP("USP_GetUserRoles", null);

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["tableid"].ToString(),
                    Text = row["RoleName"].ToString()
                });
            }

            return list;
        }


        #endregion

        #region InsertAddUser
        public bool InsertAddUser(ModelUser obj)
        {
            bool result = dal.InsertAddUser(obj);
            return result;
        }
        #endregion

        #region GetUserList
        public List<ModelUser> GetUserList()
        {
            List<ModelUser> ClgList = new List<ModelUser>();


            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("proc_get_created_user");

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    ModelUser emp = new ModelUser
                    {
                        LoginName = row["LoginName"].ToString(),
                        EmpID = row["EmpID"].ToString(),
                        Name = row["EmpName"].ToString(),
                        Login_status = row["LoginStatus"].ToString(),
                        Email = row["EMail"].ToString(),
                        Mobile = row["Mobile"].ToString(),
                        EstType = row["Est_TypeName"].ToString(),
                        EstDep = row["Est_deptName"].ToString(),
                        EstSec = row["Sec_Name"].ToString(),
                        des_code = row["Des_Name"].ToString(),
                        Address = row["Address"].ToString(),
                        
                    };
                    ClgList.Add(emp);
                }
            }
            return ClgList;
            
        }


        public List<ModelUser> GetUserListForEdit()
        {
            List<ModelUser> ClgList = new List<ModelUser>();


            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("proc_get_created_user");

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    ModelUser emp = new ModelUser
                    {
                        LoginName = row["LoginName"]?.ToString(),
                        EmpID = row["EmpID"]?.ToString(),
                        Name = row["EmpName"]?.ToString(),
                        Login_status = row["LoginStatus"]?.ToString(),
                        Email = row["EMail"]?.ToString(),
                        Mobile = row["Mobile"]?.ToString(),
                        EstType = row["Est_TypeCode"]?.ToString(),
                        EstDep = row["Dep_Code"]?.ToString(),
                        EstSec = row["Sec_Code"]?.ToString(),
                        des_code = row["Est_desigCode"]?.ToString(),
                        Address = row["Address"]?.ToString(),
                        EmpType = row["Emp_Type_ID"]?.ToString(),
                        CategoryID =  Convert.ToInt32(row["CategoryCode1"]),
                        Password = row["Password"]?.ToString(),
                        
                    };
                    ClgList.Add(emp);
                }
            }
            return ClgList;

        }

        #endregion
        public bool UpdateUser(ModelUser obj)
        {
            bool result = dal.UpdateUser(obj);
            return result;
        }
        //public bool DeleteUser(string TableID)
        //{
        //    bool result = dal.DeleteUser(TableID);
        //    return result;
        //}


        #region Employee Report

        public List<ModelEmployeeReport> GetEmployeeReport(string estTypeCode)
        {
            return dal.GetEmployeeReport(estTypeCode);
        }

        #endregion


        #region Employee Transfer

        public List<ModelEmployeeTransfer> GetEmployeeTransfers()
        {
            DataSet ds = dal.FN_ExecuteQuerySingle("Proc_getTransferDetails");
            List<ModelEmployeeTransfer> transfers = new List<ModelEmployeeTransfer>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    transfers.Add(new ModelEmployeeTransfer
                    {
                        Row_ID = Convert.ToInt32(row["Row_ID"]),
                        Login_Name = row["Login_Name"].ToString(),
                        Emp_Code = row["Emp_Code"].ToString(),
                        Emp_Name = row["Name"].ToString(),
                        Est_Type = row["Est_TypeName"].ToString(),
                        Department = row["Est_deptName"].ToString(),
                        Section = row["Est_secName"].ToString(),
                        Designation = row["Est_desigName"].ToString(),
                        Old_Est_Type = row["Old_Est_TypeName"].ToString(),
                        Old_Department = row["Old_Est_deptName"].ToString(),
                        Old_Section = row["Old_Est_secName"].ToString(),
                        Old_Designation = row["Old_Est_desigName"].ToString(),
                        Transfer_From_Dt = row["Transfer_From_Dt"].ToString(),
                        Order_File_Name = row["Order_File_Name"].ToString()
                    });
                }
            }

            return transfers;
        }


        public List<ModelEmployeeTransfer> GetEmployeeTransferById(string empCode)
        {
            List<ModelEmployeeTransfer> result = new List<ModelEmployeeTransfer>();
            DataSet ds = dal.FN_ExecuteQuerySingle("Proc_SearchEmployeeforTransfer '" + empCode.Trim() + "' ");

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    result.Add(new ModelEmployeeTransfer
                    {
                        Row_ID = Convert.ToInt32(row["id"]),
                        Login_Name = row["Name"].ToString(),
                        Emp_Code = row["Emp_Code"].ToString(),
                        Emp_Name = row["Employee_Name"].ToString(),
                        Est_Type = row["Est_TypeName"].ToString(),
                        Est_typeCode = row["Est_typeCode"].ToString(),
                        Department = row["Est_deptName"].ToString(),
                        Est_deptCode = row["Est_deptCode"].ToString(),
                        Section = row["Est_secName"].ToString(),
                        Est_secCode = row["Est_secCode"].ToString(),
                        Designation = row["Est_desigName"].ToString(),
                        Est_desigCode = row["Est_desigCode"].ToString(),
                        Status = row["Status"].ToString()
                    });
                }
            }
            
            return result;
        }


        public DataSet InsertEmployeeTransfer(ModelEmployeeTransfer obj)
        {
            DataSet ds = dal.FN_ExecuteQuerySingle("Proc_EmployeeTransfer '" +obj.Row_ID + "','" + obj.Emp_Code + "','" + obj.Est_typeCode + "','" + obj.Est_Type + "','" + obj.Est_deptCode + "','" + obj.Department + "','" + obj.Est_secCode + "','" + obj.Section + "','" + obj.Est_desigCode + "','" + obj.Designation + "','" + obj.UserId + "','" + null + "','" + obj.Transfer_From_Dt + "','" + obj.Order_File_Name + "' ");

            return ds;
        }
        

        #endregion

        public DataTable EQ(string Q)
        {
            DataTable dt = dal.EQ(Q);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }

        public DataSet FN_ExecuteQuerySingle(string Queary)
        {
            DataSet ds = dal.FN_ExecuteQuerySingle(Queary);
            
            // Automatically decrypt any encrypted columns in the DataSet
            ds = DataSetEncryptionHelper.DecryptDataSet(ds);
            
            return ds;
        }

        public DataSet CheckApprovedRevert(string fileCode, string userName)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.CheckApprovedRevert(fileCode, userName);
        }

        public string GetCallbackFlag(string rowId, string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.GetCallbackFlag(rowId, fileCode);
        }


        #region BindDocType change inline query into storeprocedure
        public List<SelectListItem> BindDocType()
        {
            DataTable dt = dal.ExecuteDataTableSP("USP_GetDocType");
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["DocType_Code"].ToString(),
                    Text = row["DocType_Name"].ToString()
                });
            }

            return list;
        }
        #endregion   

        public List<SelectListItem> BindDeliveryMode()
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        { "@TableName", "M_DeleveryMode" },
        { "@TextField", "Delevery_Mode" },
        { "@ValueField", "Delevery_Mode_ID" },
    };
                                      
            // Call DAL
            DataTable dt = dal.BindDDL(parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Delevery_Mode_ID"].ToString(),
                    Text = row["Delevery_Mode"].ToString()
                });
            }

            return list;
        }

        public List<SelectListItem> BindPurposeType()
        {            
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        { "@TableName", "M_PurposeType" },
        { "@TextField", "Purpose_desc" },
        { "@ValueField", "Row_ID" },

    };

            // Call DAL
            DataTable dt = dal.BindDDL(parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Row_ID"].ToString(),
                    Text = row["Purpose_desc"].ToString()
                });
            }

            return list;
        }

        public List<ModelAddDocument> GetDocumentsList(ModelAddDocument obj)
        {
            List<ModelAddDocument> DocList = new List<ModelAddDocument>();

            obj.doc_code = DeterministicEncryptionHelper.Encrypt(obj.doc_code);

            DocList = dal.GetDocumentsList(obj);

            // Safely decrypt (returns original if not encrypted)
            foreach (var doc in DocList)
            {
                doc.doc_code = DeterministicEncryptionHelper.SafeDecrypt(doc.doc_code);
                doc.File_Code = DeterministicEncryptionHelper.SafeDecrypt(doc.File_Code);
                doc.Doc_Upload = DeterministicEncryptionHelper.SafeDecrypt(doc.Doc_Upload);
            }

            return DocList;
        }


        public List<SelectListItem> BindDocPriority()
        {
            // Define parameters if any (in this case, none)
            Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        { "@TableName", "M_DocPrior" },
        { "@TextField", "DocPrior_Name" },
        { "@ValueField", "DocPrior_Code" },

    };

            // Call DAL
            DataTable dt = dal.BindDDL(parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["DocPrior_Code"].ToString(),
                    Text = row["DocPrior_Name"].ToString()
                });
            }

            return list;
        }


        //public List<SelectListItem> BindFileCategory(string depcode)
        //{

        //    string query = "select * from M_FileCat where isnull(dep_code,'') = '" + depcode + "' order by FileCat_Name";
        //    DataTable dt = new DataTable();
        //    dt = dal.ExecuteDataTable(query);

        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["FileCat_Code"].ToString(),
        //            Text = row["FileCat_Name"].ToString()
        //        });
        //    }

        //    return list;
        //}

        #region BindFileCategory change inline query into storeprocedure 
        public List<SelectListItem> BindFileCategory(string depcode)
        {
            // Prepare parameters
            SqlParameter[] param = new SqlParameter[]
            {
        new SqlParameter("@DepCode", SqlDbType.VarChar, 20) { Value = depcode ?? (object)DBNull.Value }
            };

            // Call stored procedure using DAL
            DataTable dt = dal.ExecuteDataTableSP("GetFileCategoryByDept", param);

            // Convert DataTable to List<SelectListItem>
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["FileCat_Code"].ToString(),
                    Text = row["FileCat_Name"].ToString()
                });
            }

            return list;
        }
        #endregion

        #region Add Documents

        public bool InsertAddDocument(ModelAddDocument obj)
        {
            obj.doc_code = DeterministicEncryptionHelper.Encrypt(obj.doc_code);
            obj.File_Code = DeterministicEncryptionHelper.Encrypt(obj.File_Code);
            obj.Doc_Upload = DeterministicEncryptionHelper.Encrypt(obj.Doc_Upload);

            bool result = dal.InsertAddDocument(obj);
            return result;
        }


        public DataTable GenerateDocCode(string docType, string docLang)
        {
            return dal.GenerateDocCode(docType, docLang);
        }

        #endregion

        #region createfile

        public bool InsertCreateFile(ModelCreateFile obj)
        {
            obj.File_Code = DeterministicEncryptionHelper.Encrypt(obj.File_Code);

            bool result=dal.InsertCreateFile(obj);
            return result;
        }
        #endregion

        //public List<SelectListItem> BindFileType()
        //{
        //    DataTable dt = new DataTable();
        //    dt = dal.ExecuteDataTable("select DocType_Code,Row_Id, (convert(nvarchar,DocType_Name)+' | '+Short_Name ) as DocType_Name, Active from M_DocType where isnull(Active,'') = 1 order by DocType_Name");


        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["DocType_Code"].ToString(),
        //            Text = row["DocType_Name"].ToString()
        //        });
        //    }

        //    return list;
        //}
        public List<SelectListItem> BindFileType()
        
        
        {
            // Call stored procedure without parameters
            DataTable dt = dal.ExecuteDataTableSP("usp_GetActiveDocTypes");

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["DocType_Code"].ToString(),
                    Text = row["DocType_Name"].ToString()
                });
            }

            return list;
        }



        //public List<SelectListItem> BindFileSubCategories(string filecategory)
        //{
        //    string query = "select FileSubCat_Code,FileSubCat_Name from M_FileSubCat where FileCat_Code='" + filecategory + "'";
        //    DataTable dt = dal.EQ(query);

        //    if (dt.Rows.Count <= 0)
        //    {
        //        string query1 = "select FileSubCat_Code,FileSubCat_Name from M_FileSubCat where FileCat_Code='N00409'";
        //        dt = dal.EQ(query1);
        //    }

        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        list.Add(new SelectListItem
        //        {
        //            Value = row["FileSubCat_Code"].ToString(),
        //            Text = row["FileSubCat_Name"].ToString()
        //        });
        //    }

        //    return list;
        //}
        public List<SelectListItem> BindFileSubCategories(string filecategory)
        {
            List<SqlParameter> parameters = new List<SqlParameter>
    {
        new SqlParameter("@FileCat_Code", filecategory)
    };

            DataTable dt = dal.ExecuteStoredProcedure("sp_GetFileSubCategories", parameters);

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["FileSubCat_Code"].ToString(),
                    Text = row["FileSubCat_Name"].ToString()
                });
            }

            return list;
        }

        public List<ModelCreateFile> GetFileList(ModelCreateFile obj)
        {
            List<ModelCreateFile> FileList = new List<ModelCreateFile>();

            obj.File_Code = DeterministicEncryptionHelper.Encrypt(obj.File_Code);

            FileList = dal.GetFileList(obj);

            // Safely decrypt (returns original if not encrypted)
            foreach (var file in FileList)
            {
                file.File_Code = DeterministicEncryptionHelper.SafeDecrypt(file.File_Code);
            }

            return FileList;
        }

        public string GetAutoMaxId(string DepCode, string SecCode)
        {
            string autono = dal.GetAutoMaxId(DepCode, SecCode);

            return autono;
        }

        //public string GetDepName(string DepCode)
        //{
        //    string query = "select Dep_Code,(convert(nvarchar,Dep_Name)+' | '+short_name ) as Dep_Name from M_Department where Dep_Code ='" + DepCode + "' order by Dep_Name";
        //    string DepName = string.Empty;
        //    DataTable dt = dal.EQ(query);
        //    if (dt.Rows.Count > 0)
        //    {
        //        DepName = dt.Rows[0]["Dep_Name"].ToString();
        //    }

        //    return DepName;
        //}
            public string GetDepName(string depCode)
            {
                string depName = string.Empty;

                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@DepCode", depCode)
                };

                DataTable dt = dal.ExecuteDataTableSP("usp_GetDepartmentName", parameters);

                if (dt.Rows.Count > 0)
                {
                    depName = dt.Rows[0]["Dep_Name"].ToString();
                }

                return depName;
            }


        //public string GetSecName(string DepCode, string SecCode)
        //{
        //    string query = "select sec_code,(convert(nvarchar,Sec_Name)+' | '+Shrt_Name ) as Sec_Name,Dep_Code from M_Section where Dep_Code='" + DepCode + "' and sec_code ='" + SecCode + "' order by Sec_Name";
        //    string SecName = string.Empty;
        //    DataTable dt = dal.EQ(query);
        //    if (dt.Rows.Count > 0)
        //    {
        //        SecName = dt.Rows[0]["Sec_Name"].ToString();
        //    }

        //    return SecName;
        //}
  
            public string GetSecName(string depCode, string secCode)
            {
                string secName = string.Empty;

                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@DepCode", depCode),
            new SqlParameter("@SecCode", secCode)
                };

                DataTable dt = dal.ExecuteDataTableSP("usp_GetSectionName", parameters);

            if (dt.Rows.Count > 0)
            {
                secName = dt.Rows[0]["Sec_Name"].ToString();
            }

                return secName;
            }

        #region Opened Files 
        public DataTable GetOpenedFilesList(string empid)
        {
            DataTable dt = dal.GetOpenedFilesList(empid);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }
        #endregion

        #region Returned Files 
        public DataTable GetReturnedFilesList(string empid)
        {
            DataTable dt = dal.GetReturnedFilesList(empid);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }
        #endregion

        #region Approved Files 
        public DataTable GetApprovedFilesList(string empid)
        {
            DataTable dt = dal.GetApprovedFilesList(empid);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }
        #endregion

        #region GetInBoxList 
        public DataTable GetInBoxList(string empid)
        {
            DataTable dt = dal.GetInBoxList(empid);
            return dt;
        }
        #endregion

        #region GetOutBoxFileHistory 
        public DataTable GetOutBoxFileHistory(string FileCode)
        {
            FileCode = DeterministicEncryptionHelper.Encrypt(FileCode);
            DataTable dt = dal.GetOutBoxFileHistory(FileCode);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }
        #endregion

        #region BindFinancialYear 
        public List<SelectListItem> BindFinancialYear()
        {
            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("proc_get_financial_Year");

            List<SelectListItem> list = new List<SelectListItem>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new SelectListItem
                    {
                        Value = row["Row_ID"].ToString(),
                        Text = row["Financial_Year"].ToString()
                    });
                }
            }

            return list;
        }

        #endregion

        #region GetCallBackfiles 
        public DataTable GetCallBackFiles(string empid)
        {
            DataTable dt = dal.GetCallBackFiles(empid);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }
        #endregion

        #region GetHandoverfiles
        public DataTable GetHandOverFiles(string empid)
        {
            DataTable dt = dal.GetHandOverFiles(empid);
            return dt;
        }

        #endregion

        #region GetSignedFiles
        public DataTable GetSignedFiles(string empid)
        {
            DataTable dt = dal.GetSignedFiles(empid);
            dt = DataSetEncryptionHelper.DecryptDataTable(dt);
            return dt;
        }
        #endregion
        public DataTable GetFilesHistory(string empid)
        {
            DataTable dt = dal.GetFilesHistory(empid);
            return dt;
        }

        public DataTable BindCreatedFilesList(ModelFileCategory obj)
        {
            obj.File_Code = DeterministicEncryptionHelper.Encrypt(obj.File_Code);
            DataTable dt = dal.BindCreatedFilesList(obj);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }

        public DataTable BindCreatedDocumentList(ModelAddDocument obj)
        {
            DataTable dt = dal.BindCreatedDocumentList(obj);

            dt = DataSetEncryptionHelper.DecryptDataTable(dt);

            return dt;
        }

        public List<SelectListItem> GetForwardToList(string secCode, string deptCode, string currentUser)
        {
            return dal.GetForwardToList(secCode, deptCode, currentUser);
        }

        public List<SelectListItem> BindInternalMovementForwardingOfficers(string username)
        {

            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("pro_get_favorite '" + username + "'");

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["Fav_Emp_Code"].ToString(),
                    Text = row["Fav_Emp_Name"].ToString()
                });
            }
            return list;
        }

        public DataTable GetAdminDashboardReports (ModelAdminDashboard obj)
        {
            DataTable dt = dal.GetAdminDashboardReports(obj);
            dt = DataSetEncryptionHelper.DecryptDataTable(dt);
            return dt;

        }

        public DataTable BindNotingDocuments(string FileCode, string EmpID)
        {
            FileCode = DeterministicEncryptionHelper.Encrypt(FileCode);
            DataTable dt = dal.BindNotingDocuments(FileCode, EmpID);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public DataTable GetFileDetails(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.GetFileDetails(fileCode);
        }


        public DataTable GetAttachedDocs(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataTable dt = dal.GetAttachedDocs(fileCode);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }


        //public List<SelectListItem> GetMergedDocs(string fileCode)
        //{
        //    string str = "select Doc_Code,row_id from M_Document  where File_Code='" + fileCode + "' order by row_id asc";
        //    DataTable dt = dal.EQ(str);


        //    // Convert to SelectListItem
        //    List<SelectListItem> list = new List<SelectListItem>();


        //    list.Add(new SelectListItem { Text = "--All--", Value = "0" });

        //    foreach (DataRow row in dt.Rows)
        //    {

        //        list.Add(new SelectListItem
        //        {
        //            Text = row["Doc_Code"].ToString(),
        //            Value = row["row_id"].ToString()
        //        });
        //    }

        //    return list;
        //}

        public List<SelectListItem> GetMergedDocs(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);

            // Prepare parameters
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@File_Code", SqlDbType.VarChar, 255) { Value = fileCode ?? (object)DBNull.Value }
            };

            DataTable dt = dal.ExecuteDataTableSP("USP_GetMergedDocs", parameters);

            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "--All--", Value = "0" }
            };

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Text = DeterministicEncryptionHelper.SafeDecrypt(row["Doc_Code"].ToString()),
                    Value = row["row_id"].ToString()
                });
            }
            return list;
        }


        public (DataTable EmpDetails, DataTable DesigDetails) GetEmployeeDetails(string empId, string estType, string depCode, string secCode)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@EmpId", empId),
        new SqlParameter("@EstType", estType),
        new SqlParameter("@DeptCode", depCode),
        new SqlParameter("@SecCode", secCode)
            };

            DataSet ds = dal.ExecuteDataSetSP("USP_GetEmployeeDetails", parameters);

            DataTable empDetails = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            DataTable desigDetails = ds.Tables.Count > 1 ? ds.Tables[1] : new DataTable();

            return (empDetails, desigDetails);
        }

        public bool IsLoginNameExists(string loginName)
        {
            DataTable dt = dal.CheckLoginName(loginName);
            return dt.Rows.Count > 0 && dt.Rows[0]["LoginName"].ToString() == loginName;
        }

        public DataTable GetUtilityUser(ModelClass obj, string plainpwd)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@LoginName", obj.User_Name.Trim()),
                new SqlParameter("@Password", cryptography.EncryptText(plainpwd)),
                new SqlParameter("@CategoryCode", obj.RoleType.Trim()),
            };

            

            DataTable dt = dal.ExecuteDataTableSP("USP_GetUtilityUser", parameters);

            return dt;
        }


        #region Filtered Inbox

        public DataTable GetInboxFiles(string forwardedTo, string secCode, string finYear)
        {
            return dal.GetInboxFiles(forwardedTo, secCode, finYear);
        }

        public DataTable GetInboxFilesByFinYear(string userName, string finYear)
        {
            return dal.GetInboxFilesByFinYear(userName, finYear);
        }

        #endregion


        #region Filtered Files history

        public DataTable SearchByDate(string secCode, string fromDate, string toDate, string username)
        {
            DataTable dt = dal.SearchByDate(secCode, fromDate, toDate, username);

            return DataSetEncryptionHelper.DecryptDataTable(dt);

        }

        public DataTable SearchByFileNo(string userName, string fileNo)
        {
            DataTable dt = dal.SearchByFileNo(userName, fileNo);

            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        #endregion

        public List<SelectListItem> BindCreatedByEmployee(string Sec_Code, string Dep_Code, string EstTypeCode, string UserName)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SecCode", Sec_Code),
                new SqlParameter("@DeptCode", Dep_Code),
                new SqlParameter("@LoginName", UserName),
            };

            DataTable dt = dal.ExecuteDataTableSP("USP_GetAdminReportUsers", parameters);

            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["LoginName"].ToString(),
                    Text = row["name"].ToString()
                });
            }

            return list;
        }

        #region Admin Report Filtered

        public DataTable GetFilteredAdminReport(string estType, string deptCode, string secCode, string empCode, string finYear, string ParamType)
        {
            return dal.GetFilteredAdminReport(estType, deptCode, secCode, empCode, finYear, ParamType);
        }


        public List<ModelAddEmployee> GetAdminEmployeeList(string EstType)
        {
            List<ModelAddEmployee> EmpList = new List<ModelAddEmployee>();

            DataSet ds = new DataSet();
            ds = dal.FN_ExecuteQuerySingle("proc_Employee_Details '" + EstType + "'");

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    ModelAddEmployee emp = new ModelAddEmployee
                    {
                        Emp_Code = row["Emp_Code"].ToString(),
                        Emp_Name = row["Employee_Name"].ToString(),
                        Contact_No = row["Contact_No"].ToString(),
                        Email = row["E_Mail"].ToString(),
                        Type_ = row["Est_TypeName"].ToString(),
                        Dep_Code = row["Est_deptName"].ToString(),
                        Sec_Code = row["Est_secName"].ToString(),
                    };

                    EmpList.Add(emp);
                }
            }

            return EmpList;
        }
        #endregion


        #region Merged File flag

        public int UpdateFlag(string[] arr, string docName, string fileNo, string empId)
        {
            docName = DeterministicEncryptionHelper.Encrypt(docName);
            fileNo = DeterministicEncryptionHelper.Encrypt(fileNo);

            // Encrypt every value in arr
            arr = arr.Select(x => DeterministicEncryptionHelper.Encrypt(x)).ToArray();

            return dal.UpdateFlag(arr, docName, fileNo, empId);
        }

        #endregion


        #region Noting Details

        public DataTable GetNotingDetails(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataTable dt = dal.GetNotingDetails(fileCode);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public DataTable GetDepSecNames(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            string str = "select d.Dep_Name,s.Sec_Name from M_File f inner join M_Department d  on d.Dep_Code = f.File_Dept inner join M_Section s on s.Sec_Code = f.File_Section  where file_code='" + DeterministicEncryptionHelper.Encrypt(fileCode) + "'";
            DataTable dt = dal.EQ(str);
            return dt;

        }

        #endregion



        #region Save Notings

        #region Draft Notings

        public DataSet GetDraftNoting(string fileCode, string userName)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataSet ds = dal.GetDraftNoting(fileCode, userName);
            return DataSetEncryptionHelper.DecryptDataSet(ds);
        }

        public DataSet GetDraftNotingAlt(string fileCode, string userName)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataSet ds = dal.GetDraftNotingAlt(fileCode, userName);
            return DataSetEncryptionHelper.DecryptDataSet(ds);
        }


        public DataTable GetLatestNoting(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataTable dt = dal.GetLatestNoting(fileCode);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        #endregion



        #region Save Noting

        public bool SaveDraftNoting(string fileCode, string noteDesc, string empId, string userName, string secCode, 
            string docCode, string docUpload, string ip, string EDRowID, string dftnot)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            docCode = DeterministicEncryptionHelper.Encrypt(docCode);
            docUpload = DeterministicEncryptionHelper.Encrypt(docUpload);

            return dal.SaveDraftNoting(fileCode, noteDesc, empId, userName, secCode, docCode, docUpload, ip, EDRowID, dftnot);
        }

        public bool SaveFinalNoting(string param, string fileCode, string docCode, string docUpload, string noteType,
                            string noteDesc, string empCode, string docSection, string userName,
                            string ip, string commentForFO, string comments, string dftFlag, string edRowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            docCode = DeterministicEncryptionHelper.Encrypt(docCode);
            docUpload = DeterministicEncryptionHelper.Encrypt(docUpload);

            return dal.SaveFinalNoting(param, fileCode, docCode, docUpload, noteType, noteDesc, empCode,
                               docSection, userName, ip, commentForFO, comments, dftFlag, edRowId);
        }
        
        public int SaveNotingPara(string fileCode, string item)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.SaveNotingPara(fileCode, item);
        }

        public void SendFileToSecretary(string fileCode, string userName, string ip, string EDRowID)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            dal.SendFileToSecretary(fileCode, userName, ip, EDRowID);
        }

        #endregion


        #endregion

        #region Forward File

        public void ForwardAsVC(string fileCode, string forwardedFrom, string forwardedTo,
                                 string remark, string remarkType, string selectedRemarkType, string dept, string sec, string flag, 
                                 string appflag, string ipAddress, string EDRowID, string esttype)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            dal.ExecuteForwardSP(fileCode, forwardedFrom, forwardedTo, remark, remarkType, selectedRemarkType, dept, sec, flag, 
                appflag, ipAddress, EDRowID, esttype);
        }

        public void ForwardFile(string fileCode, string forwardedFrom, string forwardedTo,
                                     string remark, string remarkType, string selectedRemarkType, string dept, string sec, bool isApproved, 
                                     string ipAddress, string flag, string appFlag, string EDRowID, string esttype)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            dal.ExecuteForwardSP(fileCode, forwardedFrom, forwardedTo, remark, remarkType, selectedRemarkType, dept, sec,
                ipAddress ,flag, appFlag, EDRowID, esttype);
        }


        #endregion

        #region File Revert

        public List<SelectListItem> GetRevertToList(string fileCode, string userName)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.GetRevertToList(fileCode, userName);
        }


        public bool RevertFile(string fileCode, string forwardedFrom, string revertTo, string remark, string remarktext, string selectedRemarkText,
            string ipAddress, string edRowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.RevertFile(fileCode, forwardedFrom, revertTo, remark, remarktext, selectedRemarkText, ipAddress, edRowId);
           
        }

        #endregion

        #region Approve and revert

        public bool SaveDraftApproveRevertNoting(string fileCode, string docCode, string docUpload, string empId, string section, string userName, string ip, string edRowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            docCode = DeterministicEncryptionHelper.Encrypt(docCode);
            docUpload = DeterministicEncryptionHelper.Encrypt(docUpload);

            return dal.SaveDraftApproveRevertNoting(fileCode, docCode, docUpload, empId, section, userName, ip, edRowId);
        }

        public bool ApproveAndRevert(string fileCode, string forwardedFrom, string forwardedTo,
            string remark, string remarktext, string selectedRemarkText, string ip, string edRowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.ApproveAndRevert(fileCode, forwardedFrom, forwardedTo, remark, remarktext, selectedRemarkText, ip, edRowId);
        }

        #endregion

        #region Handover files

        public DataTable GetFileCounts(string username, string roleCode, string empId)
        {
            return dal.GetFileCounts(username, roleCode, empId);
        }


        public DataTable GetApprovedFiles(string forwardedTo)
        {
            DataTable dt = dal.GetApprovedFiles(forwardedTo);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public DataTable GetOpenedFiles(string forwardedTo)
        {
            DataTable dt = dal.GetOpenedFiles(forwardedTo);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public DataTable GetPendingFiles(string forwardedTo)
        {
            DataTable dt = dal.GetPendingFiles(forwardedTo);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public bool HandOverFile(string fileCode, string forwardedBy, string forwardedTo, string ip, string flag, string fromRowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.HandOverFile(fileCode, forwardedBy, forwardedTo, ip, flag, fromRowId);
        }


        #endregion

        #region Changepassword first time
        public DataTable GetUserDetails(string username)
        {
            return dal.GetUserDetails(username);
        }

        public bool UpdatePassword(string username, string plainPwd)
        {
            string encryptedPwd = cryptography.EncryptText(plainPwd);
            bool success = dal.UpdatePassword(username, encryptedPwd);
            if (success)
            {
                dal.UpdatePlainPassword(username, plainPwd, encryptedPwd);
            }
            return success;
        }
        #endregion


        public DataTable GetMainDocument(string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataTable dt = dal.GetMainDocument(fileCode);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public List<SelectListItem> BindSectionHeads(string Dep_Code, string Sec_Code, string EmpId)
        {
            DataSet ds = dal.FN_ExecuteQuerySingle("proc_get_section_head '" + Dep_Code + "','" + Sec_Code + "','" + EmpId + "' ");
            // Convert to SelectListItem
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["LoginName"].ToString(),
                    Text = row["Emp_Name"].ToString()
                });
            }

            return list;
        }


        #region Call Back

        public DataTable SubmitCallBackRemarkBAL(string fileCode, string remark, string empId, string userName)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.SubmitCallBackRemarkDAL(fileCode, remark, empId, userName);
        }


        public int UpdateDocumentStatusForCallBack(string createdBy, string fileCode)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.UpdateDocumentStatusForCallBack(createdBy, fileCode);
        }

        #endregion

        #region View Handover Files

        public DataTable GetReceivedHandoverFileCounts(string forwardedFrom)
        {
            DataTable dt  = dal.GetReceivedHandoverFileCounts(forwardedFrom);

            return DataSetEncryptionHelper.DecryptDataTable(dt);

        }


        public DataTable GetReceivedHandoverApprovedFiles(string forwardedFrom)
        {
            DataTable dt = dal.GetReceivedHandoverApprovedFiles(forwardedFrom);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public DataTable GetReceivedHandoverOpenedFiles(string forwardedFrom)
        {
            DataTable dt = dal.GetReceivedHandoverOpenedFiles(forwardedFrom);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public DataTable GetReceivedHandoverPendingFiles(string forwardedFrom)
        {
            DataTable dt = dal.GetReceivedHandoverPendingFiles(forwardedFrom);
            return DataSetEncryptionHelper.DecryptDataTable(dt);
        }

        public List<SelectListItem> GetHandoverForwardToList(string secCode, string deptCode, string currentUser)
        {
            return dal.GetHandoverForwardToList(secCode, deptCode, currentUser);
        }

        public bool ForwardReceivedHandOverFiles(string fileCode, string forwardedBy, string forwardedTo, string ip, string fromRowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.ForwardReceivedHandOverFiles(fileCode, forwardedBy, forwardedTo, ip, fromRowId);
        }

        #endregion


        #region SendFilesToVC

        public DataTable GetReceivedFiles(string docCode, string forwardedBy)
        {
            docCode = DeterministicEncryptionHelper.Encrypt(docCode);
            return dal.GetReceivedFiles(docCode, forwardedBy);
        }


        public bool SendSelectedFilesToVC(string fileCode, string forwardedBy, string ip, string fromRowId, string remark)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            return dal.SendSelectedFilesToVC(fileCode, forwardedBy, ip, fromRowId, remark);
        }

        #endregion


        #region Forgot Password

        public DataTable GetUser(string email, string mobile)
        {
            return dal.GetUserByEmailAndMobile(email, mobile);
        }

        #endregion

        public List<ModelUserDetails> GetUserDetailsByLogin(string loginName)
        {
            // Add any business logic here if needed (validation, filtering, etc.)
            if (string.IsNullOrEmpty(loginName))
            {
                return new List<ModelUserDetails>(); // Or throw an error
            }

            // Call DAL
            return dal.GetUserByLoginName(loginName);
        }


        #region DeleteDocumentFromFile

        public bool UpdateAfterDelete(string docCode, string fileCode, string userName)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            docCode = DeterministicEncryptionHelper.Encrypt(docCode);
            return dal.UpdateAfterDelete(docCode, fileCode, userName);
        }

        #endregion



        #region UpdateFlagForCallBack

        public bool UpdateCBFlagIfNeeded(string fileCode, string rowId)
        {
            fileCode = DeterministicEncryptionHelper.Encrypt(fileCode);
            DataTable dt = dal.GetCBFlag(fileCode, rowId);

            if (dt.Rows.Count <= 0)
            {
                
                dal.UpdateCBFlag(fileCode, rowId);
                

                return true;
            }

            return true;
        }

        #endregion
    }
}