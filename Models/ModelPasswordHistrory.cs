using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelPasswordHistrory
    {

        
            public int Id { get; set; }
            public int UserId { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
            public DateTime ChangeDate { get; set; }
        
    }
}