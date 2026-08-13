using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AwasBandhuEcourt.Modules.Legal
{
    public partial class Flipbook : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string pdfName = Request.QueryString["pdf"]; // e.g., Merged_10.pdf
                string pdfPath = Server.MapPath("~/Modules/Legal/MergeUploadedFiles/" + pdfName);

                if (File.Exists(pdfPath))
                {
                    // Set hidden field
                    hdnPdfName.Value = pdfName;
                }
                else
                {
                    Response.Write("<h2>PDF file not found!</h2>");
                    Response.End();
                }
            }
        }
    }
}