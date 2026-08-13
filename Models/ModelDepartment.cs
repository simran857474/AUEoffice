using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Eoffice.Models
{
    public class ModelDepartment
    {
        public string TableID { get; set; }
        public string Dep_Name { get; set; }
        public string Short_Name { get; set; }
        public string Dep_Code { get; set; }
        public string Dep_Type { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string msg { get; set; }
        public string isactive { get; set; }
        public string ParamType { get; set; }
        public string Machine_IP { get; set; }
        public string DocType_Code { get; set; }
        public string DocType_Name { get; set; }
        public int Est_typeCode { get; set; }
        public string DocType_ShortName { get; set; }
        public string Active { get; set; }


    }

    
}