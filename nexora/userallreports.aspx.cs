using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace nexora
{
    public partial class userallreports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if logged in
            if (Session["IS_LOGGED_IN"] == null || !(bool)Session["IS_LOGGED_IN"])
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                 // Load accessible reports
                LoadUserReports();
            }
        }

        private void LoadUserReports()
        {
            // Get user's accessible reports from session
            DataTable dtAccess = Session["USER_ACCESS"] as DataTable;

            if (dtAccess != null && dtAccess.Rows.Count > 0)
            {
                rptReports.DataSource = dtAccess;
                rptReports.DataBind();
                pnlNoReports.Visible = false;
            }
            else
            {
                pnlNoReports.Visible = true;
            }
        }

        protected void lnkReadMore_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;

            // Store SL_NO
            Session["SL_NO"] = btn.CommandArgument;

            // Redirect
            Response.Redirect("~/users/userreportingdata.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}