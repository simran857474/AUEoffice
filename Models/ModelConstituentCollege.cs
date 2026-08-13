using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelConstituentCollege
    {
        public string TableID { get; set; }
        public string College_ID { get; set; }
        public string College_Name  {  get; set; }

        public string Principal_Name { get; set; }

        public string College_Add {  get; set; }

        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string msg { get; set; }
        public string ParamType { get; set; }
        public string Machine_IP { get; set; }


    }
}