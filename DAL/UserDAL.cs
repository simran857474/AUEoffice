using Eoffice.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Net.Configuration;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using static iTextSharp.text.pdf.AcroFields;

namespace Eoffice.DAL
{
    public class UserDAL
    {
        public string CheckDuplicateMaster(string spName, Dictionary<string, object> parameters)
        {
            using (SqlCommand cmd = new SqlCommand(spName, new SqlConnection(connectionString)))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "MA");
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(msgParam);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return msgParam.Value?.ToString();
            }
        }
        // Get connection string
        string connectionString = ConfigurationManager.ConnectionStrings["DBLayer"].ConnectionString;
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbLayer"].ConnectionString);



        public DataSet GetUserData(ModelClass user)
        {


            // Create connection and SqlDataAdapter
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("user_pwd_insert", con);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                // Add parameters
                da.SelectCommand.Parameters.AddWithValue("@emp_id", user.UserId);
                da.SelectCommand.Parameters.AddWithValue("@User_Name", user.User_Name);
                da.SelectCommand.Parameters.AddWithValue("@User_Password", user.User_Password);
                da.SelectCommand.Parameters.AddWithValue("@RoleType", user.RoleType);
                da.SelectCommand.Parameters.AddWithValue("@Action", user.Action);

                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }

        #region CheckLastAcces
        public bool CheckLastAccess(string userid)
        {
            SqlCommand cmd = new SqlCommand("Proc_Check_Last_Access", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@user", userid);

            if (con.State == ConnectionState.Closed)
                con.Open();

            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }
        #endregion

        #region GetPassword
        public DataSet GetPassword(ModelClass user)
        {
            // Create connection and SqlDataAdapter
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("proc_get_password_from_user_new", con);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                // Add parameters
                da.SelectCommand.Parameters.AddWithValue("@user", user.User_Name);
                da.SelectCommand.Parameters.AddWithValue("@UserType", user.RoleType);

                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }
        #endregion

        #region BindDropdown
        public DataTable BindDropdown(string action, string procedurename, Dictionary<string, object> parameters = null)
              {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand(procedurename, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", action);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region BindCommonDropdown
        public DataTable BindCommonDropdown(string procedurename, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand(procedurename, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion 

        #region Department

        public bool InsertDepartment(ModelDepartment obj)
        {
            SqlCommand cmd = new SqlCommand("USP_Department", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@Dep_Code", obj.Dep_Code);
            cmd.Parameters.AddWithValue("@Dep_Type", obj.Dep_Type);
            cmd.Parameters.AddWithValue("@Dep_Name", obj.Dep_Name);
            cmd.Parameters.AddWithValue("@Short_Name", obj.Short_Name);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy); // dummy or real user
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Update_By", (object)obj.UpdatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Est_typeCode", obj.Est_typeCode); // optional or fixed
            cmd.Parameters.AddWithValue("@Active", obj.Active); // No action for insert

            // OUTPUT param
            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }
        public List<ModelDepartment> GetDepartmentList()
        {
            List<ModelDepartment> deptList = new List<ModelDepartment>();
            SqlCommand cmd = new SqlCommand("USP_Department", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ParamType", "L");// Only active departments
            cmd.Parameters.AddWithValue("@Est_typeCode", 0);

            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                //deptList.Add(new ModelDepartment
                //{

                //    Dep_Type = Convert.ToString(dr["Dep_Type"]),
                //    Dep_Name = Convert.ToString(dr["Dep_Name"]),
                //    Dep_Code = Convert.ToString(dr["Dep_Code"]),
                //    Est_typeCode = Convert.ToInt32(dr["Est_typeCode"]),
                //    Short_Name = Convert.ToString(dr["Short_Name"]),
                //   Active = Convert.ToString(dr["Active"]),
                //});


                //adding null checks before converting
                deptList.Add(new ModelDepartment
                {
                    Dep_Type = dr["Dep_Type"] != DBNull.Value ? Convert.ToString(dr["Dep_Type"]) : null,
                    Dep_Name = dr["Dep_Name"] != DBNull.Value ? Convert.ToString(dr["Dep_Name"]) : null,
                    Dep_Code = dr["Dep_Code"] != DBNull.Value ? Convert.ToString(dr["Dep_Code"]) : null,
                    Est_typeCode = dr["Est_typeCode"] != DBNull.Value ? Convert.ToInt32(dr["Est_typeCode"]) : 0, // or use (int?)null if nullable
                    Short_Name = dr["Short_Name"] != DBNull.Value ? Convert.ToString(dr["Short_Name"]) : null,
                    Active = dr["Active"] != DBNull.Value ? Convert.ToString(dr["Active"]) : null,
                });

            }

            return deptList;
        }
        public bool UpdateDepartment(ModelDepartment obj)
        {
            SqlCommand cmd = new SqlCommand("USP_Department", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "U");
            cmd.Parameters.AddWithValue("@Dep_Type", obj.Dep_Type);
            cmd.Parameters.AddWithValue("@Dep_Name", obj.Dep_Name);
            cmd.Parameters.AddWithValue("@Dep_Code", obj.Dep_Code);
            cmd.Parameters.AddWithValue("@Short_Name", obj.Short_Name);
            cmd.Parameters.AddWithValue("@Update_By", "admin");
            cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
            cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value);

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = Convert.ToString(msgParam.Value);
            return !string.IsNullOrEmpty(resultMsg) && resultMsg.ToLower().Contains("success");
        }

        //public bool DeleteDepartment(string DepCode)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteDepartment", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@Dep_Code", DepCode);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();

        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}

        #endregion

        #region Dropdownlist
        public List<DropdownModel> GetDropDownList(DropdownModel model)
        {
            List<DropdownModel> ls = new List<DropdownModel>();

            SqlDataAdapter cmd = new SqlDataAdapter("Dropdown_proc", con);
            cmd.SelectCommand.CommandType = CommandType.StoredProcedure;
            cmd.SelectCommand.Parameters.AddWithValue("@EstType", model.EstType);
            cmd.SelectCommand.Parameters.AddWithValue("@Department", model.Department);
            cmd.SelectCommand.Parameters.AddWithValue("@Section", model.Section);
            cmd.SelectCommand.Parameters.AddWithValue("@FileCat_Name", model.FileCat_Name);
            cmd.SelectCommand.Parameters.AddWithValue("@Emp_Name", model.Emp_Name);
            cmd.SelectCommand.Parameters.AddWithValue("@RoleName", model.User_Role);
            cmd.SelectCommand.Parameters.AddWithValue("@Action", model.Action);

            DataSet ds = new DataSet();

            cmd.Fill(ds);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    DropdownModel obj = new DropdownModel();
                    obj.id = dr["id"].ToString();
                    obj.value = dr["value"].ToString();
                    ls.Add(obj);
                }
            }
            return ls;
        }
        #endregion

        #region section
        public bool InsertSection(ModelSection obj)
        {
            SqlCommand cmd = new SqlCommand("USP_Section", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Sec_Name", obj.Sec_Name == null ? DBNull.Value.ToString() : obj.Sec_Name.ToString());
            cmd.Parameters.AddWithValue("@Sec_Short_Name", obj.sec_short_name == null ? DBNull.Value.ToString() : obj.sec_short_name.ToString());
            cmd.Parameters.AddWithValue("@Sec_Code", obj.Sec_Code == null ? DBNull.Value.ToString() : obj.Sec_Code.ToString());
            cmd.Parameters.Add("@Dep_Code", obj.Dep_Code);
            cmd.Parameters.AddWithValue("@Dep_TypeCode", obj.DeptypeCode == null ? DBNull.Value.ToString() : obj.DeptypeCode.ToString());
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP == null ? DBNull.Value.ToString() : obj.Machine_IP.ToString());
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy == null ? DBNull.Value.ToString() : obj.CreatedBy.ToString());
            cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy == null ? DBNull.Value.ToString() : obj.UpdatedBy.ToString());
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType == null ? DBNull.Value.ToString() : obj.ParamType.ToString());
            cmd.Parameters.AddWithValue("@Active", obj.Active == null ? DBNull.Value.ToString() : obj.Active.ToString());

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }


        public bool UpdateSection(ModelSection obj)
        {
            SqlCommand cmd = new SqlCommand("proc_UpdateSection", con);
            cmd.CommandType = CommandType.StoredProcedure;
            // cmd.Parameters.AddWithValue("@Est_Type", obj.Est_Type);
            // cmd.Parameters.AddWithValue("@Department", obj.Department);
            cmd.Parameters.AddWithValue("@Sec_Name", obj.Sec_Name);
            //cmd.Parameters.AddWithValue("@Sec_Short_Name", obj.SecShortName);
            cmd.Parameters.AddWithValue("@Sec_Code", obj.Sec_Code);
            cmd.Parameters.AddWithValue("@Update_By", "admin"); // dummy or real user
            cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
            cmd.Parameters.AddWithValue("@Status", "Active");

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = Convert.ToString(msgParam.Value);
            return !string.IsNullOrEmpty(resultMsg) && resultMsg.ToLower().Contains("success");
        }
        public bool DeleteSection(string Sec_Code)
        {
            SqlCommand cmd = new SqlCommand("proc_DeleteSection", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Sec_Code", Sec_Code);
            if (con.State == ConnectionState.Closed)
                con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }
        #endregion

        #region Designation 
        public bool InsertDesignation(ModelDesignation obj)
        {
            SqlCommand cmd = new SqlCommand("USP_Designation", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Des_Name", obj.DesName);
            cmd.Parameters.AddWithValue("@Des_Code", obj.DesCode);
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@Des_Short_Name", obj.Shrt_Name);
            cmd.Parameters.AddWithValue("@Active", obj.Active);

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }


        public List<ModelDesignation> GetDesignationList()
        {
            List<ModelDesignation> DesList = new List<ModelDesignation>();
            SqlCommand cmd = new SqlCommand("USP_Designation", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "L");
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                DesList.Add(new ModelDesignation
                {
                    DesCode = Convert.ToString(dr["Des_Code"]),
                    DesName = Convert.ToString(dr["Des_Name"]),
                    Shrt_Name = Convert.ToString(dr["Shrt_Name"]),
                    Active = Convert.ToString(dr["Active"]),
                });
            }

            return DesList;
        }

        //public bool UpdateDesignation(ModelDesignation obj)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_UpdateDesignation", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@Des_Name", obj.Des_Name);
        //    cmd.Parameters.AddWithValue("@Des_Short_Name", obj.Des_short_name);
        //    cmd.Parameters.AddWithValue("@Des_Code", obj.Des_Code);
        //    cmd.Parameters.AddWithValue("@Update_By", "admin"); // dummy or real user
        //    cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
        //    cmd.Parameters.AddWithValue("@Status", "Active");
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        //public bool DeleteDesignation(string Des_Code)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteDesignation", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@Des_Code", Des_Code);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        #endregion

        #region AddEmployee
        public bool InsertEmployee(ModelAddEmployee obj)

        {
            SqlCommand cmd = new SqlCommand("Proc_InsertAddEmployee ", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Emp_Code", obj.Emp_Code);
            cmd.Parameters.AddWithValue("@Est_Type", obj.Type_);
            cmd.Parameters.AddWithValue("@Dep_Code", obj.Dep_Code);
            cmd.Parameters.AddWithValue("@Sec_Code", obj.Sec_Code);
            cmd.Parameters.AddWithValue("@Des_Code", obj.Des_Code);
            cmd.Parameters.AddWithValue("@Emp_Name", obj.Emp_Name);
            cmd.Parameters.AddWithValue("@Father_Name", obj.Father_Name);
            cmd.Parameters.AddWithValue("@Spouse_Name", obj.Spouse_Name);
            cmd.Parameters.AddWithValue("@Gender", obj.Gender);
            cmd.Parameters.AddWithValue("@DOB", obj.DOB);
            cmd.Parameters.AddWithValue("@Contact_No", obj.Contact_No);
            cmd.Parameters.AddWithValue("@Email", obj.Email);
            cmd.Parameters.AddWithValue("@Adhar", obj.Adhar);
            cmd.Parameters.AddWithValue("@Status", obj.isactive);
            cmd.Parameters.AddWithValue("@Adddress", obj.Address);
            cmd.Parameters.AddWithValue("@Remark", obj.Remark);
            cmd.Parameters.AddWithValue("@Created_By", "admin"); // dummy or real user
            cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
            cmd.Parameters.AddWithValue("@profile_name", obj.filename ?? (object)DBNull.Value);

            //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

            if (con.State == ConnectionState.Closed)
                con.Open();

            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }
        //public List<ModelAddEmployee> GetEmployeeList()
        //{
        //    List<ModelAddEmployee> EmpList = new List<ModelAddEmployee>();
        //    SqlCommand cmd = new SqlCommand("proc_SelectEmployee", con);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    cmd.Parameters.AddWithValue("@Status", "Active");

        //    SqlDataAdapter adp = new SqlDataAdapter(cmd);
        //    DataTable dt = new DataTable();
        //    adp.Fill(dt);

        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        EmpList.Add(new ModelAddEmployee
        //        {
        //            Row_Id = Convert.ToString(dr["Row_Id"]),
        //            Emp_Code = Convert.ToString(dr["Emp_Code"]),
        //            Emp_Name = Convert.ToString(dr["Emp_Name"]),
        //            Contact_No = Convert.ToString(dr["Contact_No"]),
        //            Email = Convert.ToString(dr["E_Mail"]),
        //            Est_Type = Convert.ToString(dr["Est_Type"]),
        //            Department = Convert.ToString(dr["Department"]),
        //            Section = Convert.ToString(dr["Section"]),
        //            Status = Convert.ToString(dr["Status"]),
        //            Des_Code = Convert.ToString(dr["Des_Code"]),
        //            Dep_Code = Convert.ToString(dr["Dep_Code"]),
        //            Sec_Code = Convert.ToString(dr["Sec_Code"]),
        //            Father_Name = Convert.ToString(dr["Father_Name"]),
        //            Remark = Convert.ToString(dr["Remark"]),
        //            Spouse_Name = Convert.ToString(dr["Spouse_Name"]),
        //            Gender = Convert.ToString(dr["Gender"]),
        //            DOB = Convert.ToString(dr["DOB"]),
        //            Adhar_No = Convert.ToString(dr["Adhar_No"]),
        //            Address = Convert.ToString(dr["Address"]),
        //        });
        //    }

        //    return EmpList;
        //}

        public bool UpdateEmployee(ModelAddEmployee obj)
        {
            SqlCommand cmd = new SqlCommand("Proc_UpdateAddEmployee", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Row_Id", obj.Emp_Code);
            cmd.Parameters.AddWithValue("@Emp_Code", obj.Emp_Code);
            cmd.Parameters.AddWithValue("@Emp_Name", obj.Emp_Name);
            cmd.Parameters.AddWithValue("@Father_Name", obj.Father_Name);
            cmd.Parameters.AddWithValue("@Spouse_Name", obj.Spouse_Name);
            cmd.Parameters.AddWithValue("@Gender", obj.Gender);
            cmd.Parameters.AddWithValue("@Des_Code", obj.Des_Code);
            cmd.Parameters.AddWithValue("@Dep_Code", obj.Dep_Code);
            cmd.Parameters.AddWithValue("@Sec_Code", obj.Sec_Code);
            cmd.Parameters.AddWithValue("@DOB", obj.DOB);
            cmd.Parameters.AddWithValue("@Contact_No", obj.Contact_No);
            cmd.Parameters.AddWithValue("@Email", obj.Email);
            cmd.Parameters.AddWithValue("@Adhar", obj.Adhar);
            cmd.Parameters.AddWithValue("@Status", obj.isactive);
            cmd.Parameters.AddWithValue("@Adddress", obj.Address);
            cmd.Parameters.AddWithValue("@Remark", obj.Remark);
            cmd.Parameters.AddWithValue("@Created_By", "admin"); // dummy or real user
            cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
            cmd.Parameters.AddWithValue("@profile_name", obj.Emp_Name ?? (object)DBNull.Value);
            //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

            if (con.State == ConnectionState.Closed)
                con.Open();

            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }

        public bool DeleteEmployee(string Row_Id)
        {
            SqlCommand cmd = new SqlCommand("proc_DeleteEmployee", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Row_Id", Row_Id);
            if (con.State == ConnectionState.Closed)
                con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }

        public DataSet GetEmployeeForEdit(string empCode)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand("proc_get_employee", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Emp_Code", empCode);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }
            return ds;
        }

        public DataTable GetEmployeeDepartmentMappings(string empCode)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT 
                                (Est_TypeName + '|' + Est_typeCode) AS 'Type',
                                (Est_deptName + '|' + Est_deptCode) AS 'Department',
                                (Est_secName + '|' + Est_secCode) AS 'Section',
                                (Est_desigName + '|' + Est_desigCode) AS 'Designation'
                            FROM EmployerDepartment 
                            WHERE Emp_Code = @Emp_Code";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Emp_Code", empCode);
                
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region DocumentPriority 

        public bool InsertDocPrior(ModelDocPriority obj)
        {
            SqlCommand cmd = new SqlCommand("USP_DocPriority", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Prio_Code", obj.DocPrior_Code);
            cmd.Parameters.AddWithValue("@Prior_Name", obj.Doc_PriorName);
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@Active", obj.isactive);

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }

        public List<ModelDocPriority> GetDocPriorList()
        {
            List<ModelDocPriority> DocpriorList = new List<ModelDocPriority>();
            SqlCommand cmd = new SqlCommand("USP_DocPriority", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "L");
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                DocpriorList.Add(new ModelDocPriority
                {
                    DocPrior_Code = Convert.ToString(dr["DocPrior_Code"]),
                    Doc_PriorName = Convert.ToString(dr["DocPrior_Name"]),
                    isactive = Convert.ToString(dr["Active"]),
                });
            }

            return DocpriorList;
        }

        //public bool UpdateDocumentPriority(ModelDocPriority obj)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_UpdateDocPriority", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@Prior_Code", obj.DocPrior_Code);

        //    cmd.Parameters.AddWithValue("@Prior_Name", obj.DocPrior_Name);
        //    cmd.Parameters.AddWithValue("@Status", "Active");
        //    cmd.Parameters.AddWithValue("@Updated_By", "admin"); // dummy or real user
        //    cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);

        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;

        //}

        //public bool DeleteDocument(string DocPrior_Code)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteDocumentPrior", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@Prior_Code  ", DocPrior_Code);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        #endregion

        #region DocumentType
        public bool InsertDocType(ModelDocType obj)
        {
            SqlCommand cmd = new SqlCommand("USP_DocType", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DocType_Name", obj.DocType_Name);
            cmd.Parameters.AddWithValue("@DocType_Code", obj.DocType_Code);
            cmd.Parameters.AddWithValue("@Short_Name", obj.Short_Name);
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@Active", obj.Active);

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }
        public List<ModelDocType> GetDocTypeList()
        {
            List<ModelDocType> DoctypeList = new List<ModelDocType>();
            SqlCommand cmd = new SqlCommand("USP_DocType", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "L");
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                DoctypeList.Add(new ModelDocType
                {
                    DocType_Name = Convert.ToString(dr["DocType_Name"]),
                    Short_Name = Convert.ToString(dr["Short_Name"]),
                    Active = Convert.ToString(dr["Active"]),
                    DocType_Code = Convert.ToString(dr["DocType_Code"]),
                });
            }

            return DoctypeList;
        }

        //public bool UpdateDocType(ModelDocType obj)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_UpdateDocType", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@DocType_Code", obj.DocType_Code);
        //    cmd.Parameters.AddWithValue("@DocType_Name", obj.DocType_Name);
        //    cmd.Parameters.AddWithValue("@Short_Name", obj.Short_Name);
        //    cmd.Parameters.AddWithValue("@Status", obj.Active);
        //    cmd.Parameters.AddWithValue("@Created_By", "admin"); // dummy or real user
        //    cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);

        //    //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

        //    if (con.State == ConnectionState.Closed)
        //        con.Open();

        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        //public bool DeleteDocumentType(string DocType_Code)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteDocumentType", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@DocType_Code  ", DocType_Code);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        #endregion

        #region FileCategory
        public bool InsertFileCategory(ModelFileCategory obj)
        {
            SqlCommand cmd = new SqlCommand("USP_FileCat", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FileCat_Name", obj.FileCat_Name);
            cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Code);
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@Active", obj.isactive);
            //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }

        public List<ModelFileCategory> GetFileCategoryList()
        {
            List<ModelFileCategory> fileCatList = new List<ModelFileCategory>();
            SqlCommand cmd = new SqlCommand("USP_FileCat", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ParamType", "L");// Only active departments

            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                fileCatList.Add(new ModelFileCategory
                {
                    FileCat_Code = Convert.ToString(dr["FileCat_Code"]),
                    FileCat_Name = Convert.ToString(dr["FileCat_Name"]),
                    //isactive = Convert.ToString(dr["Status"]),

                });
            }

            return fileCatList;
        }

        //public bool UpdateFileCategory(ModelFileCategory obj)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_UpdateFileCategory  ", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Code);
        //    cmd.Parameters.AddWithValue("@FileCat_Name", obj.FileCat_Name);
        //    cmd.Parameters.AddWithValue("@Update_By", "admin"); // dummy or real user
        //    cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
        //    cmd.Parameters.AddWithValue("@Status", "Active");
        //    //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

        //    if (con.State == ConnectionState.Closed)
        //        con.Open();

        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        //public bool DeleteFileCategory(string FileCat_Code)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteFileCategory", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@FileCat_Code", FileCat_Code);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        #endregion

        #region FileSub Category
        public bool InsertFileSubCategory(ModelFileSubCategory obj)
        {
            SqlCommand cmd = new SqlCommand("USP_FileSubCat", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FileSubCat_Name", obj.FileSubCat_Name);
            cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Name);
            cmd.Parameters.AddWithValue("@FileSubCat_Code", obj.FileSubCat_Code);
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();

            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && resultMsg.ToLower().Contains("success");
        }

        public List<ModelFileSubCategory> GetFileSubCategoryList()
        {
            List<ModelFileSubCategory> filesubCatList = new List<ModelFileSubCategory>();
            SqlCommand cmd = new SqlCommand("USP_FileSubCat", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ParamType", "L");// Only active departments

            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                filesubCatList.Add(new ModelFileSubCategory
                {
                    FileCat_Code = Convert.ToString(dr["FileCat_Code"]),
                    FileSubCat_Code = Convert.ToString(dr["FileSubCat_Code"]),
                    FileSubCat_Name = Convert.ToString(dr["FileSubCat_Name"]),
                    FileCat_Name = Convert.ToString(dr["FileCat_Name"]),
                    //Status = Convert.ToString(dr["Status"]),
                });
            }
            return filesubCatList;
        }

        //public bool UpdateFileSubCat(ModelFileSubCategory obj)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_UpdateFileSubCat", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@FileSubCat_Code", obj.FileSubCat_Code);
        //    cmd.Parameters.AddWithValue("@FileSubCat_Name", obj.FileSubCat_Name);
        //    cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Code);
        //    cmd.Parameters.AddWithValue("@Update_By", "admin"); // dummy or real user
        //    cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
        //    cmd.Parameters.AddWithValue("@Status", "Active");
        //    //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

        //    if (con.State == ConnectionState.Closed)
        //        con.Open();

        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        //public bool DeleteFileSubCat(string FileSubCat_Code)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteFileSubCat", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@FileSubCat_Code", FileSubCat_Code);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        #endregion

        #region ConstituentCollege
        public bool InsertConstituentCollege(ModelConstituentCollege obj)
        {
            SqlCommand cmd = new SqlCommand("USP_CollegeList", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@College_Name", obj.College_Name);
            cmd.Parameters.AddWithValue("@Principal_Name", obj.Principal_Name);
            cmd.Parameters.AddWithValue("@College_Add", obj.College_Add);
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
            cmd.Parameters.AddWithValue("@Status", obj.Status);
            cmd.Parameters.AddWithValue("@College_ID", obj.College_ID);

            if (con.State == ConnectionState.Closed)
                con.Open();

            //int i = cmd.ExecuteNonQuery();
            //con.Close();
            //return i > 0;
            try
            {
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch
            {
                con.Close();
                return false;
            }
        }
        public List<ModelConstituentCollege> GetConstituentCollegeList()
        {
            List<ModelConstituentCollege> ClgList = new List<ModelConstituentCollege>();
            SqlCommand cmd = new SqlCommand("USP_CollegeList", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ParamType", "S");// Only active departments

            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                ClgList.Add(new ModelConstituentCollege
                {
                    College_Name = Convert.ToString(dr["College_Name"]),
                    Principal_Name = Convert.ToString(dr["Principal_Name"]),
                    College_Add = Convert.ToString(dr["College_Add"]),
                    Status = Convert.ToString(dr["Status"]),
                    College_ID = Convert.ToString(dr["College_ID"]),
                });
            }

            return ClgList;
        }
        public bool UpdateConstituentCollege(ModelConstituentCollege obj)
        {
            SqlCommand cmd = new SqlCommand("proc_UpdateConstituentCollege", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@College_ID", obj.College_ID);
            cmd.Parameters.AddWithValue("@College_Name", obj.College_Name);
            cmd.Parameters.AddWithValue("@Principal_Name", obj.Principal_Name);
            cmd.Parameters.AddWithValue("@College_Add", obj.College_Add);
            cmd.Parameters.AddWithValue("@Created_By", "admin"); // dummy or real user
            cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
            cmd.Parameters.AddWithValue("@Status", "Active");

            if (con.State == ConnectionState.Closed)
                con.Open();

            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }
        public bool DeleteConstituentCollege(string College_ID)
        {
            SqlCommand cmd = new SqlCommand("proc_DeleteConstituentCollege", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@College_ID", College_ID);
            if (con.State == ConnectionState.Closed)
                con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }
        #endregion

        #region EmployeeTransfer
        //public bool InsertEmployeeTransfer(ModelEmployeeTransfer obj)
        //{
        //    SqlCommand cmd = new SqlCommand("Proc_EmployeeTransferDetails", con);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    cmd.Parameters.AddWithValue("@Emp_Code", obj.Emp_Code);
        //    cmd.Parameters.AddWithValue("@Est_typeCode", obj.EstType);
        //    cmd.Parameters.AddWithValue("@Est_deptCode", obj.Department);
        //    cmd.Parameters.AddWithValue("@Est_secCode", obj.Section);
        //    cmd.Parameters.AddWithValue("@Est_desigCode", obj.Designation);
        //    cmd.Parameters.AddWithValue("@Created_By", "admin");
        //    cmd.Parameters.AddWithValue("@Machine_IP", HttpContext.Current.Request.UserHostAddress);
        //    cmd.Parameters.AddWithValue("@Order_File_Name", string.IsNullOrEmpty(obj.OrderUrl) ? (object)DBNull.Value : obj.OrderUrl);
        //    cmd.Parameters.AddWithValue("@Transfer_From_Dt", obj.OrderDate.HasValue ? (object)obj.OrderDate : DBNull.Value);

        //    if (con.State == ConnectionState.Closed)
        //        con.Open();

        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();

        //    return i > 0;
        //}




        #endregion
        //Getting the user’s current password with GetUserById()

        #region changepassword

        public DataTable selectPasword(string userName, string pwd)
        {
            SqlCommand cmd = new SqlCommand("sp_selectpwd", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userID", SqlDbType.VarChar, 500).Value = userName;
            cmd.Parameters.Add("@pwd", SqlDbType.VarChar, 500).Value = pwd;
            DataTable dt = new DataTable();
            SqlDataReader sdr;
            try
            {
                con.Open();
                sdr = cmd.ExecuteReader();
                dt.Load(sdr);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            return dt;
        }

        public bool updatePssword(string userName, string pwd)
        {
            bool status = false;
            SqlCommand cmd = new SqlCommand("sp_updatePassword", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userID", SqlDbType.VarChar, 100).Value = userName;
            cmd.Parameters.Add("@pwd", SqlDbType.VarChar, 100).Value = pwd;
            try
            {
                con.Open();
                int flag = cmd.ExecuteNonQuery();
                status = true;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                status = false;
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            return status;
        }
        public DataTable ExecuteDataTable(string query)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
        public int ExecuteNonQuery(string query)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    connection.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            return rowsAffected;
        }
        #endregion

        #region AddUser
        public bool InsertAddUser(ModelUser obj)
        {
            SqlCommand cmd = new SqlCommand("sp_createUser", con);


            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@LoginName", obj.LoginName);
            cmd.Parameters.AddWithValue("@EmpID", obj.EmpID);
            cmd.Parameters.AddWithValue("@Mobile", obj.Mobile);
            cmd.Parameters.AddWithValue("@Email", obj.Email);
            cmd.Parameters.AddWithValue("@Address", obj.Address);
            cmd.Parameters.AddWithValue("@Name", obj.Name);
            cmd.Parameters.AddWithValue("@Password", obj.Password);
            cmd.Parameters.AddWithValue("@status", obj.status);
            cmd.Parameters.AddWithValue("@createdby", obj.createdby);
            cmd.Parameters.AddWithValue("@CategoryID", obj.CategoryID);
            cmd.Parameters.AddWithValue("@des_code", obj.des_code);
            cmd.Parameters.AddWithValue("@ED_RowID", obj.ED_RowID);
            cmd.Parameters.AddWithValue("@Emp_Type", obj.EmpType);
            cmd.Parameters.Add("@qstate", System.Data.SqlDbType.Int);
            cmd.Parameters["@qstate"].Direction = ParameterDirection.Output;

            //cmd.Parameters.AddWithValue("@Est_typeCode", DBNull.Value); // optional or fixed

            if (con.State == ConnectionState.Closed)
                con.Open();

            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i > 0;
        }

        //public List<ModelUser> GetUserList()
        //{
        //    List<ModelUser> UserList = new List<ModelUser>();
        //    SqlCommand cmd = new SqlCommand("Proc_SelectLoginUser", con);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    cmd.Parameters.AddWithValue("@status", "LoginStatus");
        //    SqlDataAdapter adp = new SqlDataAdapter(cmd);
        //    DataTable dt = new DataTable();
        //    adp.Fill(dt);

        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        UserList.Add(new ModelUser
        //        {
        //            LoginName = Convert.ToString(dr["LoginName"]),
        //            EmpID = Convert.ToString(dr["EmpID"]),
        //            Emp_Name = Convert.ToString(dr["Emp_Name"]),
        //            Mobile = Convert.ToString(dr["Mobile"]),
        //            Email = Convert.ToString(dr["Email"]),
        //            Est_Type = Convert.ToString(dr["Est_Type"]),
        //            Department = Convert.ToString(dr["Department"]),
        //            Section = Convert.ToString(dr["Section"]),
        //          //  LoginStatus = Convert.ToString(dr["Active"]),
        //            Designation = Convert.ToString(dr["Designation"]),
        //            Dep_Code = Convert.ToString(dr["Dep_Code"]),
        //            Sec_Code = Convert.ToString(dr["Sec_Code"]),
        //            Address = Convert.ToString(dr["Address"]),
        //            TableID = Convert.ToString(dr["TableID"]),
        //            Password = Convert.ToString(dr["Password"]),
        //            ConfirmPassword = Convert.ToString(dr["Password"]),
        //        });
        //    }

        //    return UserList;
        //}

        public bool UpdateUser(ModelUser obj)
        {
            SqlCommand cmd = new SqlCommand("sp_UpdateUserInfo", con);


            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
            cmd.Parameters.AddWithValue("@Mobile", obj.Mobile);
            cmd.Parameters.AddWithValue("@EmpID", obj.EmpID);
            cmd.Parameters.AddWithValue("@Email", obj.Email);
            cmd.Parameters.AddWithValue("@Address", obj.Address);
            cmd.Parameters.AddWithValue("@Name", obj.Name);
            cmd.Parameters.AddWithValue("@LoginName", obj.LoginName);
            cmd.Parameters.AddWithValue("@updatedBy", obj.updatedby);
            cmd.Parameters.AddWithValue("@CategoryID", obj.CategoryID);
            cmd.Parameters.AddWithValue("@Status", obj.status);
            cmd.Parameters.AddWithValue("@EmpType", obj.EmpType);

            // OUTPUT param
            SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(msgParam);

            if (con.State == ConnectionState.Closed)
                con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            string resultMsg = msgParam.Value?.ToString();

            // Use message to determine success
            return resultMsg != null && (resultMsg.ToLower().Contains("user details updated successfully."));
        }

        //public bool DeleteUser(string TableID)
        //{
        //    SqlCommand cmd = new SqlCommand("proc_DeleteUser", con);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@TableID", TableID);
        //    if (con.State == ConnectionState.Closed)
        //        con.Open();
        //    int i = cmd.ExecuteNonQuery();
        //    con.Close();
        //    return i > 0;
        //}
        #endregion

        #region Employee Report

        public List<ModelEmployeeReport> GetEmployeeReport(string estTypeCode)
        {
            List<ModelEmployeeReport> empList = new List<ModelEmployeeReport>();
            using (SqlCommand cmd = new SqlCommand("proc_Employee_Details", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Est_typeCode", estTypeCode ?? (object)DBNull.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    empList.Add(new ModelEmployeeReport
                    {
                        Emp_Code = row["Emp_Code"].ToString(),
                        Emp_Name = row["Employee_Name"].ToString(),
                        Contact_No = row["Contact_No"].ToString(),
                        Email = row["E_Mail"].ToString(),
                        Type_ = row["Est_TypeName"].ToString(),
                        Dep_Code = row["Est_deptName"].ToString(),
                        Sec_Code = row["Est_secName"].ToString()
                    });
                }
            }

            return empList;
        }

        #endregion

        public DataTable EQ(string Q)
        {

            DataTable dt = new DataTable();
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = Q;
                SqlDataReader sdr;
                sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
                return dt;
            }
            catch (Exception ex)
            {
                //return ex
            }
            return dt;
        }

        public DataSet FN_ExecuteQuerySingle(string Queary)
        {
            SqlCommand sqlCmd = new SqlCommand();
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            string output = Queary.Substring(Queary.IndexOf("'") + 1);
            string checkinput = output;
            string validstring = (checkinput.Replace("'", ""));
            string valid = CheckInvalidQuery(validstring);
            try
            {

                con.Open();
                sqlCmd.Connection = con;
                sqlCmd.CommandText = Queary;
                //sqlCmd.CommandTimeout = 2000000;
                sqlCmd.CommandTimeout = 20000;
                sqlAdapter.SelectCommand = sqlCmd;
                sqlAdapter.Fill(ds);

                con.Close();
                return ds;
            }
            catch (Exception ex)
            {
                con.Close();
                return ds;
            }
        }

        public DataSet CheckApprovedRevert(string fileCode, string userName)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlCommand cmd = new SqlCommand("proc_check_Approved_Revert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@File_Code", fileCode);
                    cmd.Parameters.AddWithValue("@UserName", userName);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                // Log exception if needed
            }
            return ds;
        }


        public string GetCallbackFlag(string rowId, string fileCode)
        {
            string flag = "0";

            using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(cb_flag,'0') FROM T_File WHERE Row_ID=@RowId AND File_Code=@FileCode", con))
            {
                cmd.Parameters.AddWithValue("@RowId", rowId);
                cmd.Parameters.AddWithValue("@FileCode", fileCode);

                con.Open();
                var result = cmd.ExecuteScalar();
                con.Close();

                if (result != null)
                    flag = result.ToString();
            }

            return flag;

        }


        public string CheckInvalidQuery(string Query)
        {

            int count = 0;

            if (Query == null || Query == "")
            {

                return "1";
            }
            else
            {
                string[] stringQuery = { Query };

                string[] stringArray = {"'","--",";--",";","/*","*/","@@",
                                         "char","nchar","varchar","nvarchar",
                                         "alter","begin","cast","create","cursor","declare","delete","drop","exec","execute",
                                         "fetch","insert","kill","open",
                                         "select", "sys","sysobjects","syscolumns",
                                         "table","update"};


                string stringToCheck = Query;
                // string[] stringArray = { "text1", "testtest", "test1test2", "test2text1" };
                foreach (string x in stringArray)
                {
                    if (stringToCheck.Contains(x))
                    {
                        count += 1;
                    }
                }


                if (count > 0)
                {
                    return "0";
                }
                else
                {
                    return "1";
                }
            }
        }

        public DataTable BindDDL(Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_FillDLL", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }

        public List<ModelAddDocument> GetDocumentsList(ModelAddDocument obj)
        {
            List<ModelAddDocument> DocList = new List<ModelAddDocument>();
            SqlCommand cmd = new SqlCommand("USP_Document", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "L");
            cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Doc_Code", obj.doc_code);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                DocList.Add(new ModelAddDocument
                {
                    doc_code = Convert.ToString(dr["Doc_Code"]),
                    Doc_Desc = Convert.ToString(dr["Doc_Desc"]),
                    Doc_TypeName = Convert.ToString(dr["DocType_Name"]),
                    Doc_Type = Convert.ToString(dr["Doc_Type"]),
                    delevery_modename = Convert.ToString(dr["Delevery_Mode"]),
                    delevery_mode = Convert.ToString(dr["Delevery_Mode_ID"]),
                    Doc_Title = Convert.ToString(dr["Doc_Title"]),
                    Doc_Ref = Convert.ToString(dr["Doc_Ref"]),
                    Doc_Auth = Convert.ToString(dr["Doc_Auth"]),
                    File_Code = Convert.ToString(dr["File_Code"]),
                    Doc_Keyword = Convert.ToString(dr["Doc_Keyword"]),
                    Doc_Lang = Convert.ToString(dr["Doc_Lang"]),
                    DocStatus_Name = Convert.ToString(dr["docstatus"]),
                    CreatedBy = Convert.ToString(dr["CreatedBy"]),
                    Purpose_TypeName = Convert.ToString(dr["Purpose_desc"]),
                    Purpose_Type = Convert.ToString(dr["purpose_type"]),
                    Purpose_Type_Others = Convert.ToString(dr["purpose_type_others"]),
                    letter_dt = Convert.ToString(dr["letter_dt"]),
                    recieved_dt = Convert.ToString(dr["recieved_dt"]),
                    Doc_Type_Others = Convert.ToString(dr["Doc_Type_Others"]),
                    Doc_Upload = Convert.ToString(dr["Doc_Upload"]),
                });
            }

            return DocList;
        }

        #region Add Documents

        public bool InsertAddDocument(ModelAddDocument obj)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("USP_Document", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Doc_Code", obj.doc_code);
                cmd.Parameters.AddWithValue("@Doc_Title", obj.Doc_Title);
                cmd.Parameters.AddWithValue("@Doc_TotalPages", obj.TotalPages);
                cmd.Parameters.AddWithValue("@Doc_TypeName", obj.Doc_TypeName);
                cmd.Parameters.AddWithValue("@Doc_Type", obj.Doc_Type);
                cmd.Parameters.AddWithValue("@Doc_Type_Others", obj.Doc_Type_Others);
                cmd.Parameters.AddWithValue("@Doc_Section", obj.Doc_Section);
                cmd.Parameters.AddWithValue("@Doc_Ref", obj.Doc_Ref??"");
                cmd.Parameters.AddWithValue("@Emp_Code", obj.Emp_Code);
                cmd.Parameters.AddWithValue("@Doc_Lang", obj.Doc_Lang);
                cmd.Parameters.AddWithValue("@Doc_Desc", obj.Doc_Desc);
                cmd.Parameters.AddWithValue("@Doc_Auth", obj.Doc_Auth??"");
                cmd.Parameters.AddWithValue("@Doc_Keyword", obj.Doc_Keyword);
                cmd.Parameters.AddWithValue("@Forwarded_By", obj.Forwarded_By);
                cmd.Parameters.AddWithValue("@Doc_Upload", obj.Doc_Upload);
                cmd.Parameters.AddWithValue("@File_Code", obj.File_Code);
                cmd.Parameters.AddWithValue("@Remark", obj.Remark);
                cmd.Parameters.AddWithValue("@Dep_Code", obj.Dep_Code);
                cmd.Parameters.AddWithValue("@Active", obj.isactive);
                cmd.Parameters.AddWithValue("@Status_Flag", obj.Status_Flag);
                cmd.Parameters.AddWithValue("@FinYear", obj.FinYear);
                cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
                cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
                cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
                cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
                cmd.Parameters.AddWithValue("@purpose_type", obj.Purpose_Type);
                cmd.Parameters.AddWithValue("@purpose_type_others", obj.Purpose_Type_Others);
                cmd.Parameters.AddWithValue("@delevery_mode", obj.delevery_mode);
                cmd.Parameters.AddWithValue("@recieved_dt", obj.recieved_dt);
                cmd.Parameters.AddWithValue("@letter_dt", obj.letter_dt);

                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                return resultMsg != null && (resultMsg.ToLower().Contains("document saved successfully") || resultMsg.ToLower().Contains("record updated successfully"));
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public DataTable GenerateDocCode(string docType, string docLang)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("proc_get_doc_Code", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@doc_type", docType ?? "");
                cmd.Parameters.AddWithValue("@len", docLang ?? "");
                cmd.CommandTimeout = 20000;
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }


        #endregion

        #region InsertCreatefile
        public bool InsertCreateFile(ModelCreateFile obj)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("USP_InsertFile", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@File_Code", obj.File_Code);
                cmd.Parameters.AddWithValue("@File_Title", obj.File_Title);
                cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Code);
                cmd.Parameters.AddWithValue("@FileSubCat_Code", obj.FileSubCat_Code);
                cmd.Parameters.AddWithValue("@FileOtherCat", obj.Other_Cat);
                cmd.Parameters.AddWithValue("@File_Desc", obj.File_Desc);
                cmd.Parameters.AddWithValue("@FileS_Lang", obj.File_Lang);
                cmd.Parameters.AddWithValue("@File_PreRef", obj.File_PrerRef);
                cmd.Parameters.AddWithValue("@File_LeterRef", obj.File_LetterRef);
                cmd.Parameters.AddWithValue("@File_Section", obj.File_Section);
                cmd.Parameters.AddWithValue("@Active", obj.isactive);
                cmd.Parameters.AddWithValue("@File_Remark", obj.File_Remark);
                cmd.Parameters.AddWithValue("@FinYear", obj.FinYear);
                cmd.Parameters.AddWithValue("@Machine_IP", obj.Machine_IP);
                cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
                cmd.Parameters.AddWithValue("@Update_By", obj.UpdatedBy);
                cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
                cmd.Parameters.AddWithValue("@Year", obj.Year);
                cmd.Parameters.AddWithValue("@Doc_Type_Code", obj.Doc_Type_Code);
                cmd.Parameters.AddWithValue("@Status_Flag", obj.Status_Flag);
                cmd.Parameters.AddWithValue("@File_Dept", obj.File_Dept);
                cmd.Parameters.AddWithValue("@File_ID", obj.File_ID);
                cmd.Parameters.AddWithValue("@Prior_Code", obj.Prior_Code);
                cmd.Parameters.AddWithValue("@ED_ROW_ID", obj.ED_ROW_ID);


                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                return resultMsg != null && (resultMsg.ToLower().Contains("file created successfully.") || resultMsg.ToLower().Contains("record updated successfully...") || resultMsg.ToLower().Contains("file deleted successfully."));
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public List<ModelCreateFile> GetFileList(ModelCreateFile obj)
        {
            List<ModelCreateFile> FileList = new List<ModelCreateFile>();
            SqlCommand cmd = new SqlCommand("USP_InsertFile", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "C");
            cmd.Parameters.AddWithValue("@CreatedBy", obj.CreatedBy);
            cmd.Parameters.AddWithValue("@Dep_Code", obj.File_dep);
            cmd.Parameters.AddWithValue("@sec_Code", obj.File_Section);
            cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Code);
            cmd.Parameters.AddWithValue("@FileSubCat_Code", obj.FileSubCat_Code);
            cmd.Parameters.AddWithValue("@File_Code", obj.File_Code);
            cmd.Parameters.AddWithValue("@File_Desc", obj.File_Desc);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adp.Fill(ds);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                FileList.Add(new ModelCreateFile
                {
                    TableID = Convert.ToString(dr["ROW_ID"]),
                    Status_Flag = Convert.ToString(dr["status"]),
                    DocType_Code = Convert.ToString(dr["DocType_Code"]),
                    DocType_Name = Convert.ToString(dr["DocType_Name"]),
                    File_Title = Convert.ToString(dr["File_Title"]),
                    File_Desc = Convert.ToString(dr["File_Desc"]),
                    FileCat_Name = Convert.ToString(dr["FileCat_Name"]),
                    FileCat_Code = Convert.ToString(dr["File_Cat"]),
                    FileSubCat_Code = Convert.ToString(dr["File_SubCat"]),
                    FileSubCat_Name = Convert.ToString(dr["FileSubCat_Name"]),
                    Other_Cat = Convert.ToString(dr["Other_Cat"]),
                    Prior_Code = Convert.ToString(dr["DocPrior_Code"]),
                    Prior_Name = Convert.ToString(dr["DocPrior_Name"]),
                    File_Code = Convert.ToString(dr["File_Code"]),
                    File_Lang = Convert.ToString(dr["File_Lang"]),
                    File_PrerRef = Convert.ToString(dr["File_PreRef"]),
                    File_LetterRef = Convert.ToString(dr["File_LaterRef"]),
                    //CreatedBy = Convert.ToString(dr["CreatedBy"]),
                    File_Section = Convert.ToString(dr["File_Section"]),
                    File_SectionName = Convert.ToString(dr["Sec_Name"]),
                    File_Remark = Convert.ToString(dr["File_Remark"]),
                    CreatedDate = Convert.ToString(dr["Created_DT"])
                });
            }

            return FileList;
        }


        public string GetAutoMaxId(string DepCode, string SecCode)
        {
            DataTable dt = new DataTable();
            string autono = string.Empty;

            SqlCommand cmd = new SqlCommand("USP_MaxNo", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ParamType", "MAX");
            cmd.Parameters.AddWithValue("@Dep_Code", DepCode);
            cmd.Parameters.AddWithValue("@Sec_Code", SecCode);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                autono = dt.Rows[0]["max_no"].ToString();
            }

            return autono != string.Empty ? autono : string.Empty;
        }

        #endregion

        #region Bind All Section
        public DataTable BindAllSections(ModelAddDocument obj)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_DocumentRequest", con))

            {

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
                cmd.Parameters.AddWithValue("@Doc_Code", obj.doc_code);
                cmd.Parameters.AddWithValue("@Forwarded_To", obj.Forwarded_To);
                cmd.CommandTimeout = 20000;
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetOutBoxFileHistory
        public DataTable GetOutBoxFileHistory(string FileCode)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_FileMovement", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "M");
                cmd.Parameters.AddWithValue("@File_Code", FileCode);
                cmd.CommandTimeout = 20000;
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetOpenedFileList
        public DataTable GetOpenedFilesList(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_FileMovement", con))

            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "OPEN");
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetReturnFileList
        public DataTable GetReturnedFilesList(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_FileMovement", con))

            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.CommandTimeout = 120;

                cmd.Parameters.AddWithValue("@ParamType", "RET");
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetApprovedFileList
        public DataTable GetApprovedFilesList(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_DocumentRequest", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "AF");
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetInBoxList
        public DataTable GetInBoxList(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_InboxOutboxRequest", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "I");
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetCallBackFile
        public DataTable GetCallBackFiles(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_FileMovement", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "CB");
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetHandoverFiles
        public DataTable GetHandOverFiles(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("Pro_ALL_Recived_file_Admin_User", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetSignedFiles
        public DataTable GetSignedFiles(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_SignedFiles", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "AF");
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                cmd.Parameters.AddWithValue("@Sec_Code", "");
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetFileHistory
        public DataTable GetFilesHistory(string empid)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("proc_file_movement_history", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", empid);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region BindCreatedFileList
        public DataTable BindCreatedFilesList(ModelFileCategory obj)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand("USP_InsertFile", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
                cmd.Parameters.AddWithValue("@CreatedBy", obj.CreatedBy);
                cmd.Parameters.AddWithValue("@Dep_Code", obj.File_dep);
                cmd.Parameters.AddWithValue("@sec_Code", obj.File_Section);
                cmd.Parameters.AddWithValue("@FileCat_Code", obj.FileCat_Code);
                cmd.Parameters.AddWithValue("@FileSubCat_Code", obj.FileSubCat_Code);
                cmd.Parameters.AddWithValue("@File_Code", obj.File_Code);
                cmd.Parameters.AddWithValue("@File_Desc", obj.File_Desc);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                ds.Tables.Add().Load(sdr);
                con.Close();
            }
            return ds.Tables[0];
        }
        #endregion

        #region
        public DataTable BindCreatedDocumentList(ModelAddDocument obj)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_Document", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
                cmd.Parameters.AddWithValue("@Created_By", obj.CreatedBy);
                cmd.Parameters.AddWithValue("@Doc_Code", obj.doc_code);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }
        #endregion

        #region GetForwardList
        public List<SelectListItem> GetForwardToList(string secCode, string deptCode, string currentUser)
        {
            List<SelectListItem> forwardToList = new List<SelectListItem>();

            using (SqlCommand cmd = new SqlCommand("getforwardlist", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SecCode", secCode);
                cmd.Parameters.AddWithValue("@DeptCode", deptCode);
                cmd.Parameters.AddWithValue("@CurrentUser", currentUser);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    forwardToList.Add(new SelectListItem
                    {
                        Value = row["LoginName"].ToString(),
                        Text = row["name"].ToString()
                    });
                }
            }

            return forwardToList;
        }
        #endregion

        #region GetAdminDashboardreports
        public DataTable GetAdminDashboardReports(ModelAdminDashboard obj)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_AdminHome", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", obj.ParamType);
                cmd.Parameters.AddWithValue("@Estb_type", obj.estb_type);
                cmd.Parameters.AddWithValue("@Dep_Code", obj.dept_code);
                cmd.Parameters.AddWithValue("@Sec_Code", obj.sec_code);
                cmd.Parameters.AddWithValue("@Emp_Code", obj.emp_code);
                cmd.Parameters.AddWithValue("@Financial_Year", obj.fin_year);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();

 

            }
            return dt;
        }
        #endregion
        public DataTable BindNotingDocuments(string FileCode, string EmpID)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_DocumentList", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "D");
                cmd.Parameters.AddWithValue("@File_No", FileCode);
                cmd.Parameters.AddWithValue("@forwarded_from", EmpID);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }


        public DataTable GetFileDetails(string fileCode)
        {
            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand("Pro_get_record_file", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@filecode", fileCode);

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }
            return dt;
        }


        public DataTable GetAttachedDocs(string fileCode)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_Noting", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "test");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }
            return dt;
        }
        public DataTable ExecuteDataTableSP(string spName, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(spName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public DataTable ExecuteStoredProcedure(string procedureName, List<SqlParameter> parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString)) // use your actual connection string variable
            {
                using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }


        public DataSet ExecuteDataSetSP(string spName, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(spName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        public DataTable CheckLoginName(string loginName)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@LoginName", loginName)
            };
            return ExecuteDataTableSP("USP_CheckLoginName", parameters);
        }


        public DataTable GetSectionList()
        {
            return ExecuteDataTableSP("USP_GetSectionList", null);
        }


        #region Filtered Inbox

        public DataTable GetInboxFiles(string forwardedTo, string secCode, string finYear)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("USP_Inbox_FinancialYear", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedTo ?? string.Empty);
                cmd.Parameters.AddWithValue("@Sec_Code", secCode ?? null);
                cmd.Parameters.AddWithValue("@Financial_Year", finYear);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable GetInboxFilesByFinYear(string userName, string finYear)
        {
            //SqlParameter[] parameters = new SqlParameter[]
            //{
            //    new SqlParameter("@UserName", userName),
            //    new SqlParameter("@FinYear", finYear)
            //};

            string query = $"proc_inbox_files '{userName}','{finYear}'";
            DataSet ds = FN_ExecuteQuerySingle(query);

            return ds.Tables[0];
        }

        #endregion


        #region FIltered File history

        public DataTable SearchByDate(string secCode, string fromDate, string toDate, string userName)
        {
            SqlParameter[] param = {
                new SqlParameter("@ParamType", "FM"),
                new SqlParameter("@File_Code", ""),
                new SqlParameter("@Forwarded_From", userName),
                new SqlParameter("@from_dt", fromDate),
                new SqlParameter("@to_dt", toDate)
            };

            return ExecuteDataTableSP("USP_FileMovement_Search_12092024", param);
        }

        public DataTable SearchByFileNo(string userName, string fileNo)
        {
            string query = $"proc_search_file '{userName}','{fileNo}'";
            DataSet ds = FN_ExecuteQuerySingle(query);

            return ds.Tables[0];
        }

        #endregion




        #region Admin Reports FIltered

        public DataTable GetFilteredAdminReport(string estType, string deptCode, string secCode, string empCode, string finYear, string ParamType)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_AdminHome", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", ParamType);
                cmd.Parameters.AddWithValue("@Estb_type", estType);
                cmd.Parameters.AddWithValue("@Dep_Code", deptCode);
                cmd.Parameters.AddWithValue("@Sec_Code", secCode);
                cmd.Parameters.AddWithValue("@Emp_Code", empCode);
                cmd.Parameters.AddWithValue("@Financial_Year", finYear);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }

        #endregion


        #region MergedFile

        public int UpdateFlag(string[] arr, string docName, string fileNo, string empId)
        {
            int rowsAffected = 0;

            if (arr != null && arr.Length > 0)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateFileDoc_deep", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //if (i == 0 && string.IsNullOrEmpty(doccode)) // mimic aspx docCode.Text == string.Empty
                        //{
                        //    cmd.Parameters.AddWithValue("@Main_doc", "Y");
                        //    cmd.Parameters.AddWithValue("@DisplayFile", docName);
                        //}


                        if (i == 0)
                        {
                            cmd.Parameters.AddWithValue("@Main_doc", "Y");
                            cmd.Parameters.AddWithValue("@DisplayFile", docName);
                        }



                        if (string.IsNullOrEmpty(arr[i]))
                            continue;

                        cmd.Parameters.AddWithValue("@Action", "Update");
                        cmd.Parameters.AddWithValue("@FILE_NO", fileNo);
                        cmd.Parameters.AddWithValue("@Doc_Code", arr[i]);
                        cmd.Parameters.AddWithValue("@forwarded_To", empId);
                        cmd.Parameters.AddWithValue("@parameter", "ME");

                        con.Open();
                        rowsAffected += cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }

            return rowsAffected;
        }

        #endregion


        #region Noting Details

        public DataTable GetNotingDetails(string fileCode)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_NotingDetails", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "N");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@Doc_Code", "");
                cmd.Parameters.AddWithValue("@Note_Type", "Green");
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                dt.Load(sdr);
                con.Close();
            }
            return dt;
        }

        #endregion


        #region Save Noting

        #region Draft Notings

        public DataSet GetDraftNoting(string fileCode, string userName)
        {
            SqlParameter[] parameters = {
            new SqlParameter("@ParamType", "Dft"),
            new SqlParameter("@File_Code", fileCode),
            new SqlParameter("@Forwarded_From", userName)
        };

            return ExecuteDataSetSP("[USP_FileMovement]", parameters);
        }

        public DataSet GetDraftNotingAlt(string fileCode, string userName)
        {
            SqlParameter[] parameters = {
            new SqlParameter("@ParamType", "Dft1"),
            new SqlParameter("@File_Code", fileCode),
            new SqlParameter("@Forwarded_From", userName),
            new SqlParameter("@Forwarded_To", userName)
        };

            return ExecuteDataSetSP("[USP_FileMovement]", parameters);
        }

        public DataTable GetLatestNoting(string fileCode)
        {
            string sql = @"SELECT TOP 1 * 
                       FROM t_noting 
                       WHERE File_Code = '" + fileCode + "' ORDER BY Row_ID DESC";



            return EQ(sql);
        }

        #endregion


        #region Save Noting

        public bool SaveDraftNoting(string fileCode, string noteDesc, string empId, string userName, string secCode, string docCode, string docUpload, string ip, string EDRowID, string dftnot)
        {
            bool result = false;

            using (SqlCommand cmd = new SqlCommand("USP_Noting", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "dft");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@Doc_Code", docCode);
                cmd.Parameters.AddWithValue("@Doc_Upload", docUpload);
                cmd.Parameters.AddWithValue("@Note_Type", "Green");
                cmd.Parameters.AddWithValue("@pre_comment", "");
                cmd.Parameters.AddWithValue("@Comments", "");
                cmd.Parameters.AddWithValue("@Note_Desc", noteDesc.Trim().Replace("&nbsp;", " "));
                cmd.Parameters.AddWithValue("@Emp_Code", empId);
                cmd.Parameters.AddWithValue("@Doc_Section", secCode);
                cmd.Parameters.AddWithValue("@Created_By", userName); //Session["UserID"].ToString());
                cmd.Parameters.AddWithValue("@Machine_IP", ip.ToString());
                cmd.Parameters.AddWithValue("@dft_flag", "Y");
                cmd.Parameters.AddWithValue("@dft_status", dftnot);
                cmd.Parameters.AddWithValue("@From_ED_RowID", EDRowID);


                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                return resultMsg != null && (resultMsg.ToLower().Contains("record saved successfully.") || resultMsg.ToLower().Contains(""));

            }

            return result;
        }

        public bool SaveFinalNoting(string param, string fileCode, string docCode, string docUpload, string noteType,
                            string noteDesc, string empCode, string docSection, string createdBy,
                            string ip, string commentForFO, string comments, string dftFlag, string edRowId)
        {
            bool i = false;

            using (SqlCommand cmd = new SqlCommand("USP_Noting", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", param);
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@Doc_Code", docCode);
                cmd.Parameters.AddWithValue("@Doc_Upload", docUpload);
                cmd.Parameters.AddWithValue("@Note_Type", noteType);
                cmd.Parameters.AddWithValue("@Note_Desc", noteDesc);
                cmd.Parameters.AddWithValue("@Emp_Code", empCode);
                cmd.Parameters.AddWithValue("@Doc_Section", docSection);
                cmd.Parameters.AddWithValue("@Created_By", createdBy);
                cmd.Parameters.AddWithValue("@Machine_IP", ip.ToString());
                cmd.Parameters.AddWithValue("@pre_comment", "");
                cmd.Parameters.AddWithValue("@Comments", "");
                cmd.Parameters.AddWithValue("@dft_status", "");
                cmd.Parameters.AddWithValue("@dft_flag", dftFlag);
                cmd.Parameters.AddWithValue("@From_ED_RowID", edRowId);

                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                i = resultMsg != null && (resultMsg.ToLower().Contains("record saved successfully.") || resultMsg.ToLower().Contains(""));

            }

            return i;
        }


        public int SaveNotingPara(string fileCode, string item)
        {
            int i = 0;

            using (SqlCommand cmd = new SqlCommand("USP_Noting", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "para");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@Note_Desc", item);
                con.Open();
                i = cmd.ExecuteNonQuery();
                con.Close();
            }

            return i;
        }

        public void SendFileToSecretary(string fileCode, string userName, string ip, string EDRowID)
        {
            using (SqlCommand cmd = new SqlCommand("USP_T_File_vcsec_vco", con))
            {
                cmd.Parameters.AddWithValue("@ParamType", "I");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@forwarded_From", userName);
                cmd.Parameters.AddWithValue("@forwarded_To", "vcsec_vco");
                cmd.Parameters.AddWithValue("@Priority", string.Empty);
                cmd.Parameters.AddWithValue("@final_flag", 2);
                cmd.Parameters.AddWithValue("@Remark", string.Empty);
                cmd.Parameters.AddWithValue("@Machine_IP", ip.ToString());
                cmd.Parameters.AddWithValue("@Status_Flag", 2);
                cmd.Parameters.AddWithValue("@app_flag", 'A');
                cmd.Parameters.AddWithValue("@From_ED_RowID", EDRowID);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        #endregion


        #endregion


        #region FIle FOrward

        public void ExecuteForwardSP(string fileCode, string forwardedFrom, string forwardedTo,
                                      string remark, string remarkType, string selectedRemarkType, string dept, string sec,
                                      string flag, string appflag, string ipAddress, string EDRowID, string esttype)
        {

            using (SqlCommand cmd = new SqlCommand("USP_T_File_21062024", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "I");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@forwarded_From", forwardedFrom);
                cmd.Parameters.AddWithValue("@forwarded_To", forwardedTo);
                cmd.Parameters.AddWithValue("@Priority", null);
                cmd.Parameters.AddWithValue("@Remark", remarkType == "3" ? remark : selectedRemarkType);
                cmd.Parameters.AddWithValue("@Machine_IP", ipAddress);
                cmd.Parameters.AddWithValue("@Status_Flag", flag);
                cmd.Parameters.AddWithValue("@final_flag", "");
                cmd.Parameters.AddWithValue("@App_Flag", appflag);
                cmd.Parameters.AddWithValue("@Due_Date", DateTime.Now.ToString("yyyy/MM/dd"));
                cmd.Parameters.AddWithValue("@From_ED_RowID", EDRowID);
                cmd.Parameters.AddWithValue("@To_EstbType", esttype);
                cmd.Parameters.AddWithValue("@To_Dept", dept);
                cmd.Parameters.AddWithValue("@To_Sec", sec);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

        }

        #endregion


        #region File Revert

        public List<SelectListItem> GetRevertToList(string fileCode, string userName)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            using (SqlCommand cmd = new SqlCommand("USP_RevertTo", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FileCode", fileCode);
                cmd.Parameters.AddWithValue("@UserName", userName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new SelectListItem
                    {
                        Value = row["LoginName"].ToString(),
                        Text = row["Name"].ToString()
                    });
                }
            }

            return list;
        }


        public bool RevertFile(string fileCode, string forwardedFrom, string revertTo, string remark, string remarktext, string selectedRemarkText,
            string ipAddress, string edRowId)
        {
            bool i = false;

            using (SqlCommand cmd = new SqlCommand("USP_T_File", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "RET");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@forwarded_From", forwardedFrom);
                cmd.Parameters.AddWithValue("@forwarded_To", revertTo);
                cmd.Parameters.AddWithValue("@Priority", null);
                cmd.Parameters.AddWithValue("@Remark", remark == "3" ? remarktext : selectedRemarkText);
                cmd.Parameters.AddWithValue("@Machine_IP", ipAddress);
                cmd.Parameters.AddWithValue("@Status_Flag", "-1");
                cmd.Parameters.AddWithValue("@app_flag", null);
                cmd.Parameters.AddWithValue("@From_ED_RowID", edRowId);

                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                i = resultMsg != null && (resultMsg.ToLower().Contains("file returned successfully."));

                return i;
            }
        }

        #endregion


        #region Approve and Revert

        public bool SaveDraftApproveRevertNoting(string fileCode, string docCode, string docUpload, string empId, string section, string userName, string ip, string edRowId)
        {
            bool i = false;

            using (SqlCommand cmd = new SqlCommand("USP_Noting", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "dft");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@Doc_Code", docCode);
                cmd.Parameters.AddWithValue("@Doc_Upload", docUpload);
                cmd.Parameters.AddWithValue("@Note_Type", "Green");
                cmd.Parameters.AddWithValue("@Note_Desc", "Approved");
                cmd.Parameters.AddWithValue("@Emp_Code", empId);
                cmd.Parameters.AddWithValue("@Doc_Section", section);
                cmd.Parameters.AddWithValue("@Created_By", userName);
                cmd.Parameters.AddWithValue("@Machine_IP", ip);
                cmd.Parameters.AddWithValue("@pre_comment", "");
                cmd.Parameters.AddWithValue("@Comments", "");
                cmd.Parameters.AddWithValue("@dft_status", "");
                cmd.Parameters.AddWithValue("@dft_flag", "N");
                cmd.Parameters.AddWithValue("@From_ED_RowID", edRowId);
                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                i = resultMsg != null && (resultMsg.ToLower().Contains("record saved successfully."));

                return i;
            }
        }

        public bool ApproveAndRevert(string fileCode, string forwardedFrom, string forwardedTo, string remark, string remarktext,
            string selectedRemarkText, string ip, string edRowId)
        {
            bool i = false;

            using (SqlCommand cmd = new SqlCommand("USP_T_File", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "I");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@forwarded_From", forwardedFrom);
                cmd.Parameters.AddWithValue("@forwarded_To", forwardedTo);
                cmd.Parameters.AddWithValue("@Priority", null);
                cmd.Parameters.AddWithValue("@final_flag", -5);
                cmd.Parameters.AddWithValue("@Remark", remark == "3" ? remarktext : selectedRemarkText);
                cmd.Parameters.AddWithValue("@Machine_IP", ip);
                cmd.Parameters.AddWithValue("@Status_Flag", -5);
                cmd.Parameters.AddWithValue("@app_flag", 'A');
                cmd.Parameters.AddWithValue("@From_ED_RowID", edRowId);
                // OUTPUT param
                SqlParameter msgParam = new SqlParameter("@msg", SqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();
                con.Close();

                string resultMsg = msgParam.Value?.ToString();

                // Use message to determine success
                i = resultMsg != null && (resultMsg.ToLower().Contains("record saved successfully."));

                return i;
            }
        }

        #endregion


        #region Handover Files

        public DataTable GetFileCounts(string username, string roleCode, string empId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("pro_select_Count_file", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@RoleCode", roleCode);
                    cmd.Parameters.AddWithValue("@emp_id", empId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }
      
        // Approved Files
        public DataTable GetApprovedFiles(string forwardedTo)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("pro_Select_Approved_File", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedTo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
        // Opened Files
        public DataTable GetOpenedFiles(string forwardedTo)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("Pro_Select_forward_file_opened", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedTo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Pending Files
        public DataTable GetPendingFiles(string forwardedTo)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("Pro_Select_forward_file", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedTo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }


        public bool HandOverFile(string fileCode, string forwardedBy, string forwardedTo, string ip, string flag, string fromRowId)
        {

            using (SqlCommand cmd = new SqlCommand("Pro_insert_handover_file", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@filecode", fileCode);
                cmd.Parameters.AddWithValue("@forwarded_from", forwardedBy);
                cmd.Parameters.AddWithValue("@forwarded_to", forwardedTo);
                cmd.Parameters.AddWithValue("@machine_ip", ip);
                cmd.Parameters.AddWithValue("@Flag", flag);
                cmd.Parameters.AddWithValue("@From_ED_RowID", fromRowId);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                con.Close();
                return rows > 0;
            }
        }




        #endregion

        #region changepasswordfirsttime 
        public DataTable GetUserDetails(string username)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetUserDetailsByUsername", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public bool UpdatePassword(string username, string encryptedPassword)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_PwdChange", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "CP");
                cmd.Parameters.AddWithValue("@LoginName", username);
                cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                cmd.Parameters.AddWithValue("@Is_Pwd_Change", "Y");

                con.Open();
                //int rows = cmd.ExecuteNonQuery();
                //return rows > 0;

                var result = cmd.ExecuteScalar();
                return result != null && result.ToString() == "S";

            }
        }

        public void UpdatePlainPassword(string username, string plainPwd, string encryptedPwd)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdatePlainPassword", con); // Create SP for this
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LoginName", username);
                cmd.Parameters.AddWithValue("@PlainPwd", plainPwd);
                cmd.Parameters.AddWithValue("@EncryptedPwd", encryptedPwd);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion


        public DataTable GetMainDocument(string fileCode)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetMainDocumentByFileCode", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FileCode", fileCode);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }


        #region Call Back


        public DataTable SubmitCallBackRemarkDAL(string fileCode, string remark, string empId, string userName)
        {

            using (SqlCommand cmd = new SqlCommand("[USP_CallBackFile]", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "CB");
                cmd.Parameters.AddWithValue("@File_Code", fileCode);
                cmd.Parameters.AddWithValue("@Emp_Code", empId);
                cmd.Parameters.AddWithValue("@Remark", remark);
                cmd.Parameters.AddWithValue("@Created_By", userName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                con.Open();
                da.Fill(dt);
                con.Close();
                return dt;
            }

        }

        public int UpdateDocumentStatusForCallBack(string createdBy, string fileCode)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("USP_UpdateDocumentStatusForCallBack", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Created_By", createdBy);
                cmd.Parameters.AddWithValue("@File_Code", fileCode);

                con.Open();
                return cmd.ExecuteNonQuery(); // returns number of rows updated
            }
        }


        #endregion



        #region View Handover Files

        public DataTable GetReceivedHandoverFileCounts(string forwardedFrom)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Pro_Select_handover_file_count", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@forwarded_form", forwardedFrom);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        // Approved Files
        public DataTable GetReceivedHandoverApprovedFiles(string forwardedFrom)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("pro_select_handover_files", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedFrom);
                cmd.Parameters.AddWithValue("@flag", "A");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
        // Opened Files
        public DataTable GetReceivedHandoverOpenedFiles(string forwardedFrom)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("pro_select_handover_files", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedFrom);
                cmd.Parameters.AddWithValue("@flag", "O");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Pending Files
        public DataTable GetReceivedHandoverPendingFiles(string forwardedFrom)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("pro_select_handover_files", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedFrom);
                cmd.Parameters.AddWithValue("@flag", "I");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }


        public List<SelectListItem> GetHandoverForwardToList(string secCode, string deptCode, string currentUser)
        {
            List<SelectListItem> forwardToList = new List<SelectListItem>();

            using (SqlCommand cmd = new SqlCommand("proc_getHandoverforwardlist", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SecCode", secCode);
                cmd.Parameters.AddWithValue("@DeptCode", deptCode);
                cmd.Parameters.AddWithValue("@CurrentUser", currentUser);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    forwardToList.Add(new SelectListItem
                    {
                        Value = row["LoginName"].ToString(),
                        Text = row["name"].ToString()
                    });
                }
            }

            return forwardToList;
        }


        public bool ForwardReceivedHandOverFiles(string fileCode, string forwardedBy, string forwardedTo, string ip, string fromRowId)
        {

            using (SqlCommand cmd = new SqlCommand("Pro_insert_T_file_Admin_forward", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@filecode", fileCode);
                cmd.Parameters.AddWithValue("@forwarded_from", forwardedBy);
                cmd.Parameters.AddWithValue("@forwarded_to", forwardedTo);
                cmd.Parameters.AddWithValue("@remark", "");
                cmd.Parameters.AddWithValue("@machine_ip", ip);
                //cmd.Parameters.AddWithValue("@From_ED_RowID", fromRowId); 
                con.Open();
                int rows = cmd.ExecuteNonQuery();
                con.Close();
                return rows > 0;
            }
        }




        #endregion


        #region SendFilesToVC

        public DataTable GetReceivedFiles(string docCode, string forwardedBy)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("USP_DocumentRequest", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParamType", "F");
                cmd.Parameters.AddWithValue("@Doc_Code", docCode);
                cmd.Parameters.AddWithValue("@Forwarded_To", forwardedBy);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }


        public bool SendSelectedFilesToVC(string fileCode, string forwardedBy, string ip, string fromRowId, string remark)
        {
            DataSet ds = FN_ExecuteQuerySingle("proc_sendfilesto_vcsec_vco '" + fileCode + "','" + forwardedBy + "','vcsec_vco','" + null + "','" + remark + "','" + ip + "','2','0','" + null + "','" + null + "' ,'" + fromRowId + "'");
            return true;
        }
        #endregion


        #region Forgot Password

        public DataTable GetUserByEmailAndMobile(string email, string mobile)
        {
            using (SqlCommand cmd = new SqlCommand("USP_GetUserByEmailAndMobile", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Mobile", mobile);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                con.Open();
                da.Fill(dt);
                con.Close();
                return dt;
            }
        }

        #endregion

        public List<ModelUserDetails> GetUserByLoginName(string loginName)
        {
            List<ModelUserDetails> userList = new List<ModelUserDetails>();
            using (SqlCommand cmd = new SqlCommand("USP_GetUserByEmployeeCode", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LoginName", loginName);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                       // string encPwd = reader["Password"] == DBNull.Value ? null : reader["Password"].ToString();
                       // string decPwd = string.IsNullOrEmpty(encPwd) ? "" : cryptography.DecryptText(encPwd);
                        userList.Add(new ModelUserDetails
                        {
                            EmpID = reader["EmpID"]?.ToString(),
                            Name = reader["Name"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            Mobile = reader["Mobile"]?.ToString(),
                            Est_TypeName = reader["Est_TypeName"]?.ToString(),
                            Est_deptName = reader["Est_deptName"]?.ToString(),
                            Est_secName = reader["Est_secName"]?.ToString(),
                            Est_desigName = reader["Est_desigName"]?.ToString(),
                            Password = cryptography.DecryptText(reader["Password"]?.ToString())
                        });
                    }
                }

            }

            return userList;
        }


        #region DeleteDocumentFromFile

        public bool UpdateAfterDelete(string docCode, string fileCode, string userName)
        {
            using (SqlCommand cmd = new SqlCommand("sp_UpdateFileDoc_delete", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "UpAfterDelete");
                cmd.Parameters.AddWithValue("@FILE_NO", fileCode);
                cmd.Parameters.AddWithValue("@Doc_Code", docCode);
                cmd.Parameters.AddWithValue("@forwarded_To", userName);
                cmd.Parameters.AddWithValue("@parameter", "ME2");

                if (con.State == ConnectionState.Closed)
                    con.Open();

                int rows = cmd.ExecuteNonQuery();
                con.Close();

                return rows > 0;
            }
        }

        #endregion


        #region UpdateFlagForCallBack

        public DataTable GetCBFlag(string fileCode, string rowId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT ISNULL(CB_Flag, 0) AS CB_Flag 
                         FROM T_File 
                         WHERE File_Code = @FileCode AND Row_ID = @RowID AND CB_Flag =-1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FileCode", fileCode);
                    cmd.Parameters.AddWithValue("@RowID", rowId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }


        public int UpdateCBFlag(string fileCode, string rowId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE T_File 
                         SET CB_Flag = -1, CB_Date = GETDATE() 
                         WHERE File_Code = @FileCode AND Row_ID = @RowID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FileCode", fileCode);
                    cmd.Parameters.AddWithValue("@RowID", rowId);

                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

    }
}





