using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelEmployeeTransfer
    {
        public int Row_ID { get; set; }
        public string Login_Name { get; set; }
        public string UserId { get; set; }
        public string Emp_Code { get; set; }
        public string Emp_Name { get; set; }
        public string Est_Type { get; set; }
        public string Est_typeCode { get; set; }
        public string Est_deptCode { get; set; }
        public string Est_secCode { get; set; }
        public string Est_desigCode { get; set; }
        public string Status { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Designation { get; set; }
        public string Old_Est_Type { get; set; }
        public string Old_Department { get; set; }
        public string Old_Section { get; set; }
        public string Old_Designation { get; set; }
        public string Transfer_From_Dt { get; set; }
        public string Order_File_Name { get; set; }
        public HttpPostedFileBase OrderFile { get; set; }

    }
}