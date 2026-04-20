using System;
using System.Configuration;
using System.Data;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace nexora
{
    public partial class admindashboard1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Session check
            if (Session["USERNAME"] == null || Session["PRIVILEGE"]?.ToString() != "ADMIN")
            {
                Response.Redirect("~/login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDashboardStats();
            }
        }

        private void LoadDashboardStats()
        {
            string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();

                // 1. Get Summary Stats
                string summarySql = @"
                    SELECT 
                        (SELECT COUNT(DISTINCT DOCUMENT_DTL) FROM NEXORA_LEGAL_PAPERS_DATA_APP WHERE STATUS = 'PENDING') as TOTAL_PENDING,
                        (SELECT COUNT(DISTINCT DOCUMENT_DTL) FROM NEXORA_LEGAL_PAPERS_DATA) as TOTAL_APPROVED
                    FROM DUAL";

                using (OracleCommand cmd = new OracleCommand(summarySql, con))
                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        lblTotalPending.Text = rdr["TOTAL_PENDING"].ToString();
                        lblTotalApproved.Text = rdr["TOTAL_APPROVED"].ToString();
                    }
                }

                // 2. Get Per-Report Stats
                string reportSql = @"
                    SELECT 
                        M.SL_NO,
                        M.NAME,
                        (SELECT COUNT(DISTINCT DOCUMENT_DTL) 
                         FROM NEXORA_LEGAL_PAPERS_DATA_APP 
                         WHERE FL_TYPE = M.SL_NO AND STATUS = 'PENDING') as PENDING_COUNT,
                        (SELECT COUNT(DISTINCT DOCUMENT_DTL) 
                         FROM NEXORA_LEGAL_PAPERS_DATA 
                         WHERE FL_TYPE = M.SL_NO) as APPROVED_COUNT
                    FROM nexora_legal_master_file M
                    WHERE M.ACTIVE = 'Y'
                    ORDER BY M.SL_NO";

                using (OracleDataAdapter da = new OracleDataAdapter(reportSql, con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    dt.Columns.Add("DAYS_LEFT", typeof(int));

                    // Safely calculate DAYS_LEFT in C# to avoid ORA-01858 due to bad date formats
                    string dateSql = @"
                        SELECT D1.FL_TYPE, D1.DATAS
                        FROM NEXORA_LEGAL_PAPERS_DATA D1
                        WHERE D1.COL_NAME = 'END_DATE'
                          AND D1.DATAS IS NOT NULL
                          AND D1.DTL_ID = (SELECT MAX(DTL_ID) 
                                           FROM NEXORA_LEGAL_PAPERS_DATA D2 
                                           WHERE D2.DOCUMENT_DTL = D1.DOCUMENT_DTL 
                                             AND D2.COL_NAME = 'END_DATE')";

                    using (OracleCommand cmdDates = new OracleCommand(dateSql, con))
                    using (OracleDataReader rdrDates = cmdDates.ExecuteReader())
                    {
                        var minDaysLeft = new System.Collections.Generic.Dictionary<string, int>();

                        while (rdrDates.Read())
                        {
                            string flType = rdrDates["FL_TYPE"].ToString();
                            string datas = rdrDates["DATAS"].ToString();

                            if (DateTime.TryParse(datas, out DateTime endDate))
                            {
                                int days = (int)(endDate.Date - DateTime.Now.Date).TotalDays;

                                if (!minDaysLeft.ContainsKey(flType) || days < minDaysLeft[flType])
                                {
                                    minDaysLeft[flType] = days;
                                }
                            }
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            string flType = row["SL_NO"].ToString();
                            if (minDaysLeft.ContainsKey(flType))
                            {
                                row["DAYS_LEFT"] = minDaysLeft[flType];
                            }
                            else
                            {
                                row["DAYS_LEFT"] = DBNull.Value;
                            }
                        }
                    }

                    rptReports.DataSource = dt;
                    rptReports.DataBind();
                }
            }
        }

        protected void lnkOpenReport_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            Session["SL_NO"] = btn.CommandArgument;
            Response.Redirect("~/reports/adminreport.aspx");
        }

        protected string GetExpiryClass(object daysLeft)
        {
            if (daysLeft == null || daysLeft == DBNull.Value) return "n-a";
            int days = Convert.ToInt32(daysLeft);
            if (days < 0) return "expired";
            if (days <= 7) return "urgent";
            return "safe";
        }

        protected string GetExpiryText(object daysLeft)
        {
            if (daysLeft == null || daysLeft == DBNull.Value) return "N/A";
            int days = Convert.ToInt32(daysLeft);
            if (days < 0) return "Expired";
            return days.ToString() + " Days Left";
        }
    }
}