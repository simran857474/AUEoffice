using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelDocPriority
    {
        public string Doc_Code { get; set; }
        public string Doc_Title { get; set; }
        public int TotalPages { get; set; }
        public string Doc_Desc { get; set; }
        public string Doc_Type { get; set; }
        public string Doc_Type_Others { get; set; }
        public string Doc_TypeName { get; set; }
        public string Doc_Section { get; set; }
        public string Doc_Ref { get; set; }
        public string Doc_Auth { get; set; }
        public string File_Code { get; set; }
        public string Doc_Keyword { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string msg { get; set; }
        public string isactive { get; set; }
        public string ParamType { get; set; }
        public string Machine_IP { get; set; }
        public string Doc_Lang { get; set; }
        public string Doc_Upload { get; set; }
        public string Remark { get; set; }
        public string DocPrior_Code { get; set; }
        public string Doc_PriorName { get; set; }
        public string DocStatus_Code { get; set; }
        public string DocStatus_Name { get; set; }
        public string DocStatus_Prior { get; set; }
        public string FinYear { get; set; }
        public string Emp_Code { get; set; }
        public string Forwarded_By { get; set; }
        public string Forwarded_To { get; set; }
        public string Rec_ID { get; set; }
        public string Dep_Code { get; set; }
        public string Status_Flag { get; set; }
        public string Purpose_Type { get; set; }
        public string Purpose_Type_Others { get; set; }

        public string delevery_mode { get; set; }
        public string recieved_dt { get; set; }
        public string letter_dt { get; set; }


        // Dispatch document
        //public int SPType { get; set; }
        //public int dispatch_id { get; set; }
        //public string doc_code { get; set; }
        //public string dis_Date { get; set; }
        //public string dispatch_no { get; set; }
        //public string Dis_add { get; set; }

        //public string upload_doc { get; set; }
        //public string ModifyBy { get; set; }
        //public string ModifyDate { get; set; }
        //public string FromDate { get; set; }
        //public string ToDate { get; set; }

    }
}