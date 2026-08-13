using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace Eoffice.Models
{
    public class ModelClass
    {
        public int UserId { get; set; }

        [Required]
        public string User_Name { get; set; }

        [Required]
        
        public string User_Password { get; set; }
        public string Action { get; set; }
        public string RoleType { get; set; }



       
     
    }
}