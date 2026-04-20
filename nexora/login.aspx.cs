using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;

namespace nexora
{
    public partial class login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Clear any existing session
                Session.Clear();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string connStr = ConfigurationManager
                                   .ConnectionStrings["OracleDB"]
                                   .ConnectionString;

                using (OracleConnection con = new OracleConnection(connStr))
                {
                    con.Open();

                    // Step 1: Validate User and Get All Details
                    string sql = @"
                        SELECT 
                            EMP_CODE,
                            EMP_NAME,
                            SHORT_NAME,
                            PRIVILEGE
                        FROM NEXORA_USERS
                        WHERE USERNAME = :p_username
                          AND PASSWORD = :p_password
                          AND USR_ACTIVE = 'Y'";

                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        cmd.Parameters.Add(":p_username", txtEmail.Text.Trim().ToUpper());
                        cmd.Parameters.Add(":p_password", txtPassword.Text.Trim());

                        using (OracleDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // Store User Details in Session
                                string empCode = dr["EMP_CODE"].ToString();
                                string empName = dr["EMP_NAME"].ToString();
                                string shortName = dr["SHORT_NAME"].ToString();
                                string privilege = dr["PRIVILEGE"].ToString().ToUpper();

                                Session["EMP_CODE"] = empCode;
                                Session["EMP_NAME"] = empName;
                                Session["SHORT_NAME"] = shortName;
                                Session["USERNAME"] = txtEmail.Text.Trim().ToUpper();
                                Session["PRIVILEGE"] = privilege;
                                Session["IS_LOGGED_IN"] = true;
                                Session["LOGIN_TIME"] = DateTime.Now;

                                dr.Close();

                                // Step 2: Load User Access (Reports & Locations)
                                LoadUserAccess(con, empCode);

                                // Step 3: Redirect Based on Privilege
                                if (privilege == "ADMIN")
                                {
                                    Response.Redirect("admindashboard.aspx", false);
                                }
                                else if (privilege == "USER")
                                {
                                    Response.Redirect("userdashboard.aspx", false);
                                }
                                else
                                {
                                    Response.Redirect("dashboard.aspx", false);
                                }
                            }
                            else
                            {
                                lblMessage.Text = "Invalid username or password";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
            }
        }

        // ===============================
        //  LOAD USER ACCESS DETAILS
        // ===============================
        private void LoadUserAccess(OracleConnection con, string empCode)
        {
            try
            {
                // Get all reports user has access to
                string sqlAccess = @"
                    SELECT 
                        A.REPORT_ID,
                        A.LOCATION_ID,
                        A.TABLE_NAME AS ACCESS_PAGE,
                        M.NAME AS REPORT_NAME,
                        M.TABLE_NAME AS REPORT_PAGE,
                        M.ICON
                    FROM NEXORA_USER_ACCESS A
                    JOIN NEXORA_LEGAL_MASTER_FILE M
                        ON A.REPORT_ID = M.SL_NO
                    WHERE A.EMP_CODE = :EMP_CODE
                      AND M.ACTIVE = 'Y'
                    ORDER BY M.SL_NO";

                using (OracleCommand cmdAccess = new OracleCommand(sqlAccess, con))
                {
                    cmdAccess.Parameters.Add(":EMP_CODE", empCode);

                    DataTable dtAccess = new DataTable();
                    using (OracleDataAdapter da = new OracleDataAdapter(cmdAccess))
                    {
                        da.Fill(dtAccess);
                    }

                    //  Store Access DataTable in Session
                    Session["USER_ACCESS"] = dtAccess;

                    //  Build comma-separated Report IDs and Location IDs
                    List<string> reportIdList = new List<string>();
                    List<string> locationIdList = new List<string>();
                    List<string> reportNameList = new List<string>();

                    foreach (DataRow row in dtAccess.Rows)
                    {
                        string reportId = row["REPORT_ID"].ToString();
                        string locationId = row["LOCATION_ID"].ToString();
                        string reportName = row["REPORT_NAME"].ToString();

                        if (!reportIdList.Contains(reportId))
                            reportIdList.Add(reportId);

                        if (!locationIdList.Contains(locationId))
                            locationIdList.Add(locationId);

                        if (!reportNameList.Contains(reportName))
                            reportNameList.Add(reportName);
                    }

                    Session["REPORT_IDS"] = string.Join(",", reportIdList);
                    Session["LOCATION_IDS"] = string.Join(",", locationIdList);
                    Session["REPORT_NAMES"] = string.Join(",", reportNameList);

                    //  Store default values (first record)
                    if (dtAccess.Rows.Count > 0)
                    {
                        Session["DEFAULT_LOCATION"] = dtAccess.Rows[0]["LOCATION_ID"].ToString();
                        Session["DEFAULT_REPORT_ID"] = dtAccess.Rows[0]["REPORT_ID"].ToString();
                        Session["DEFAULT_REPORT_NAME"] = dtAccess.Rows[0]["REPORT_NAME"].ToString();
                    }

                    //  Store count of accessible reports
                    Session["REPORT_COUNT"] = dtAccess.Rows.Count;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop login
                System.Diagnostics.Debug.WriteLine("Error loading user access: " + ex.Message);
            }
        }
    }
}