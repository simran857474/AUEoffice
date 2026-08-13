using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelSection
    {

        public string TableID { get; set; }
        [Required]
        
        public string DeptypeCode { get; set; }
        public string sub_DeptypeCode { get; set; }
        public string Facultycode { get; set; }
        [Required]
        public string Sec_Name { get; set; }
        public string Sec_Code { get; set; }
        [Required]
        public string Dep_Code { get; set; }
        public string DocType_Code { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string msg { get; set; }
        public string isactive { get; set; }
        public string ParamType { get; set; }
        public string Machine_IP { get; set; }
        public string sec_short_name { get; set; }
        public string Active { get; set; }
        public string EstType { get; set; }
        public string DepName { get; set; }
    }
}