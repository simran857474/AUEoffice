using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class DropdownModel
    {
        public string id { get; set; }
        public string value { get; set; }
        public string EstType { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Designation { get; set; } 
        public string FileCat_Name {  get; set; }
        public string Emp_Name { get; set; }

        public string User_Role { get; set; }
        public string Action { get; set; }
    }
}