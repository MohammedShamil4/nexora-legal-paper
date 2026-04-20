using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace nexora
{
    public partial class admindashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Session check
            if (Session["USERNAME"] == null || Session["PRIVILEGE"]?.ToString() != "ADMIN")
            {
                Response.Redirect("~/login.aspx");
            }

            if (!IsPostBack)
            {
                LoadReports();
            }
        }

        private void LoadReports()
        {
            string connStr = ConfigurationManager
                                .ConnectionStrings["OracleDB"]
                                .ConnectionString;

            string sql = @"
                SELECT 
                    SL_NO,
                    NAME
                FROM nexora_legal_master_file
                WHERE ACTIVE = 'Y'
                ORDER BY SL_NO";

            using (OracleConnection con = new OracleConnection(connStr))
            using (OracleCommand cmd = new OracleCommand(sql, con))
            {
                con.Open();

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptReports.DataSource = dt;
                rptReports.DataBind();
            }
        }


        protected void lnkReadMore_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;

            // Store SL_NO
            Session["SL_NO"] = btn.CommandArgument;

            // Redirect
            Response.Redirect("~/reports/adminreport.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}