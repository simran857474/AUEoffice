using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelFileCategory
    {
        public string TableID { get; set; }
        public string FileCat_Code { get; set; }
        public string FileCat_Name { get; set; }
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
        public string FileSubCat_Code { get; set; }
        public string FileSubCat_Name { get; set; }
        public string File_Code { get; set; }
        public string File_Title { get; set; }
        public string File_Desc { get; set; }
        public string File_Lang { get; set; }
        public string File_PrerRef { get; set; }
        public string File_LetterRef { get; set; }
        public string File_Section { get; set; }
        public string File_Remark { get; set; }
        public string FinYear { get; set; }
        public string File_dep { set; get; }
        public string Year { set; get; }
        public string Doc_Type_Code { set; get; }
        public string Status_Flag { set; get; }
        public string Other_Cat { set; get; }

        public string File_Dept { set; get; }
        public string File_ID { set; get; }
        public string Prior_Code { set; get; }
        public string ED_ROW_ID { set; get; }
    }
}