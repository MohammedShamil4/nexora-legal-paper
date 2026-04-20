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
    public partial class usermanagement : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //  Session & Security Check
            if (Session["USERNAME"] == null || Session["PRIVILEGE"]?.ToString() != "ADMIN")
            {
                Response.Redirect("~/login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUsers();
                LoadReportsList();
                LoadReportsListInline();
            }
        }

        private void LoadUsers()
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT EMP_CODE, EMP_NAME, USERNAME, PRIVILEGE, USR_ACTIVE FROM NEXORA_USERS ORDER BY EMP_CODE";
                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        private void LoadReportsList()
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT SL_NO, NAME FROM NEXORA_LEGAL_MASTER_FILE WHERE ACTIVE = 'Y' ORDER BY NAME";
                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlReports.DataSource = dt;
                ddlReports.DataTextField = "NAME";
                ddlReports.DataValueField = "SL_NO";
                ddlReports.DataBind();
                ddlReports.Items.Insert(0, new ListItem("-- Select Report --", ""));

                ddlReportsInline.DataSource = dt;
                ddlReportsInline.DataTextField = "NAME";
                ddlReportsInline.DataValueField = "SL_NO";
                ddlReportsInline.DataBind();
                ddlReportsInline.Items.Insert(0, new ListItem("-- Select Report --", ""));
            }
        }

        private void LoadReportsListInline()
        {
            // Already handled in LoadReportsList for efficiency
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string empCode = e.CommandArgument.ToString();

            if (e.CommandName == "EditUser")
            {
                LoadUserForEdit(empCode);
                upUserModal.Update();
                ScriptManager.RegisterStartupScript(this, GetType(), "openModal", "openUserModal();", true);
            }
            else if (e.CommandName == "ViewAccess")
            {
                hfSelectedEmpCode.Value = empCode;
                litActiveEmpCode.Text = empCode;
                LoadUserAccessInline(empCode);
                pnlAccessDetails.Style["display"] = "block";
                upAccessDetails.Update();
                upHiddenFields.Update();
            }
            else if (e.CommandName == "ManageAccess")
            {
                hfSelectedEmpCode.Value = empCode;
                litActiveEmpCode.Text = empCode;
                LoadUserAccess(empCode);
                upHiddenFields.Update();
                upModalAccess.Update();
                ScriptManager.RegisterStartupScript(this, GetType(), "openAccess", "openAccessModal();", true);
            }
        }

        protected void btnNewUser_Click(object sender, EventArgs e)
        {
            txtEmpCode.Text = "";
            txtEmpCode.ReadOnly = false;
            txtFullName.Text = "";
            txtLoginName.Text = "";
            txtPassword.Text = "";
            txtPassword.Attributes["value"] = "";
            ddlPrivilege.SelectedIndex = 0;
            chkIsActive.Checked = true;
            hfMode.Value = "ADD";
            
            upUserModal.Update();
            ScriptManager.RegisterStartupScript(this, GetType(), "openModal", "openUserModal();", true);
        }

        private void LoadUserForEdit(string empCode)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT * FROM NEXORA_USERS WHERE EMP_CODE = :EMP_CODE";
                OracleCommand cmd = new OracleCommand(sql, con);
                cmd.BindByName = true;
                cmd.Parameters.Add("EMP_CODE", empCode);
                con.Open();
                using (OracleDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        txtEmpCode.Text = dr["EMP_CODE"].ToString();
                        txtEmpCode.ReadOnly = true; // Cannot change ID on edit
                        txtFullName.Text = dr["EMP_NAME"].ToString();
                        txtLoginName.Text = dr["USERNAME"].ToString();
                        
                        // Force password to show in the UI for consistent editing
                        string pwd = dr["PASSWORD"].ToString();
                        txtPassword.Attributes["value"] = pwd;
                        txtPassword.Text = pwd;

                        ddlPrivilege.SelectedValue = dr["PRIVILEGE"].ToString();
                        chkIsActive.Checked = dr["USR_ACTIVE"].ToString() == "Y";
                        hfMode.Value = "EDIT";
                    }
                }
            }
        }

        protected void btnSaveUser_Click(object sender, EventArgs e)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "";
                if (hfMode.Value == "ADD")
                {
                    sql = "INSERT INTO NEXORA_USERS (EMP_CODE, EMP_NAME, USERNAME, PASSWORD, PRIVILEGE, USR_ACTIVE, SHORT_NAME) VALUES (:EMP_CODE, :EMP_NAME, :USERNAME, :PASSWORD, :PRIVILEGE, :USR_ACTIVE, :SHORT_NAME)";
                }
                else
                {
                    sql = "UPDATE NEXORA_USERS SET EMP_NAME = :EMP_NAME, USERNAME = :USERNAME, PASSWORD = :PASSWORD, PRIVILEGE = :PRIVILEGE, USR_ACTIVE = :USR_ACTIVE, SHORT_NAME = :SHORT_NAME WHERE EMP_CODE = :EMP_CODE";
                }

                OracleCommand cmd = new OracleCommand(sql, con);
                cmd.BindByName = true;
                cmd.Parameters.Add("EMP_NAME", txtFullName.Text.Trim().ToUpper());
                cmd.Parameters.Add("USERNAME", txtLoginName.Text.Trim().ToUpper());
                cmd.Parameters.Add("PASSWORD", txtPassword.Text.Trim().ToUpper());
                cmd.Parameters.Add("PRIVILEGE", ddlPrivilege.SelectedValue.ToUpper());
                cmd.Parameters.Add("USR_ACTIVE", chkIsActive.Checked ? "Y" : "N");
                cmd.Parameters.Add("SHORT_NAME", txtFullName.Text.Split(' ')[0].ToUpper());
                cmd.Parameters.Add("EMP_CODE", txtEmpCode.Text.Trim().ToUpper());

                con.Open();
                cmd.ExecuteNonQuery();
            }
            LoadUsers();
            ScriptManager.RegisterStartupScript(this, GetType(), "closeModal", "closeUserModal();", true);
        }

        private void LoadUserAccess(string empCode)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sqlName = "SELECT EMP_NAME FROM NEXORA_USERS WHERE EMP_CODE = :EMP_CODE";
                OracleCommand cmdName = new OracleCommand(sqlName, con);
                cmdName.BindByName = true;
                cmdName.Parameters.Add("EMP_CODE", empCode);
                con.Open();
                lblAccessUser.Text = cmdName.ExecuteScalar()?.ToString();

                string sql = @"
                    SELECT A.REPORT_ID, A.LOCATION_ID, A.TABLE_NAME, M.NAME AS REPORT_NAME 
                    FROM NEXORA_USER_ACCESS A 
                    JOIN NEXORA_LEGAL_MASTER_FILE M ON A.REPORT_ID = M.SL_NO 
                    WHERE A.EMP_CODE = :EMP_CODE";

                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                da.SelectCommand.Parameters.Add("EMP_CODE", empCode);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUserAccess.DataSource = dt;
                gvUserAccess.DataBind();
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "openAccess", "openAccessModal();", true);
        }

        protected void btnAddAccess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlReports.SelectedValue)) return;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "INSERT INTO NEXORA_USER_ACCESS (EMP_CODE, REPORT_ID, LOCATION_ID, TABLE_NAME) VALUES (:EMP_CODE, :REPORT_ID, :LOCATION_ID, :TABLE_NAME)";
                OracleCommand cmd = new OracleCommand(sql, con);
                cmd.BindByName = true;
                cmd.Parameters.Add("EMP_CODE", hfSelectedEmpCode.Value.ToUpper());
                cmd.Parameters.Add("REPORT_ID", ddlReports.SelectedValue);
                cmd.Parameters.Add("LOCATION_ID", txtLocation.Text.Trim().ToUpper());
                cmd.Parameters.Add("TABLE_NAME", txtTablePath.Text.Trim().ToUpper());

                con.Open();
                try 
                { 
                    cmd.ExecuteNonQuery(); 
                    ScriptManager.RegisterStartupScript(this, GetType(), "grantSuccess", "alert('Access granted successfully!');", true);
                } 
                catch { /* Handle duplicates if any */ }
            }
            LoadUserAccess(hfSelectedEmpCode.Value);
            upModalAccess.Update();
            ScriptManager.RegisterStartupScript(this, GetType(), "openAccess", "openAccessModal();", true);
        }

        protected void gvUserAccess_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAccess")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                string reportId = args[0];
                string locationId = args[1];

                using (OracleConnection con = new OracleConnection(connStr))
                {
                    string sql = "DELETE FROM NEXORA_USER_ACCESS WHERE EMP_CODE = :EMP_CODE AND REPORT_ID = :REPORT_ID AND LOCATION_ID = :LOCATION_ID";
                    OracleCommand cmd = new OracleCommand(sql, con);
                    cmd.BindByName = true;
                    cmd.Parameters.Add("EMP_CODE", hfSelectedEmpCode.Value);
                    cmd.Parameters.Add("REPORT_ID", reportId);
                    cmd.Parameters.Add("LOCATION_ID", locationId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadUserAccess(hfSelectedEmpCode.Value);
                upModalAccess.Update();
                ScriptManager.RegisterStartupScript(this, GetType(), "openAccess", "openAccessModal();", true);
            }
        }

        private void LoadUserAccessInline(string empCode)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sqlName = "SELECT EMP_NAME FROM NEXORA_USERS WHERE EMP_CODE = :EMP_CODE";
                OracleCommand cmdName = new OracleCommand(sqlName, con);
                cmdName.BindByName = true;
                cmdName.Parameters.Add("EMP_CODE", empCode);
                con.Open();
                lblInlineUser.Text = cmdName.ExecuteScalar()?.ToString();

                string sql = @"
                    SELECT A.REPORT_ID, A.LOCATION_ID, A.TABLE_NAME, M.NAME AS REPORT_NAME 
                    FROM NEXORA_USER_ACCESS A 
                    JOIN NEXORA_LEGAL_MASTER_FILE M ON A.REPORT_ID = M.SL_NO 
                    WHERE A.EMP_CODE = :EMP_CODE";

                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                da.SelectCommand.Parameters.Add("EMP_CODE", empCode);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUserAccessInline.DataSource = dt;
                gvUserAccessInline.DataBind();
            }
        }

        protected void btnGrantInline_Click(object sender, EventArgs e)
        {
            lblInlineError.Text = "";
            
            if (string.IsNullOrEmpty(hfSelectedEmpCode.Value))
            {
                lblInlineError.Text = "Critical Error: No user selected. Please select a user from the grid above.";
                return;
            }

            if (string.IsNullOrEmpty(ddlReportsInline.SelectedValue))
            {
                lblInlineError.Text = "Action required: Please select a report from the catalog.";
                return;
            }

            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "INSERT INTO NEXORA_USER_ACCESS (EMP_CODE, REPORT_ID, LOCATION_ID, TABLE_NAME) VALUES (:EMP_CODE, :REPORT_ID, :LOCATION_ID, :TABLE_NAME)";
                OracleCommand cmd = new OracleCommand(sql, con);
                cmd.BindByName = true;
                cmd.Parameters.Add("EMP_CODE", hfSelectedEmpCode.Value.Trim().ToUpper());
                cmd.Parameters.Add("REPORT_ID", ddlReportsInline.SelectedValue);
                cmd.Parameters.Add("LOCATION_ID", txtLocationInline.Text.Trim().ToUpper());
                cmd.Parameters.Add("TABLE_NAME", txtTablePathInline.Text.Trim().ToUpper());

                con.Open();
                try 
                { 
                    cmd.ExecuteNonQuery(); 
                    ScriptManager.RegisterStartupScript(this, GetType(), "grantInlineSuccess", "alert('Operational access initialized successfully!');", true);
                } 
                catch (Exception ex)
                {
                    lblInlineError.Text = "Database Error: " + ex.Message;
                }
            }
            LoadUserAccessInline(hfSelectedEmpCode.Value);
            upAccessDetails.Update();
        }

        protected void gvUserAccessInline_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAccess")
            {
                if (string.IsNullOrEmpty(hfSelectedEmpCode.Value))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "err", "alert('Session expired or user lost. Please re-select the user.');", true);
                    return;
                }

                string[] args = e.CommandArgument.ToString().Split('|');
                string reportId = args[0].Trim();
                string locationId = args[1].Trim();

                using (OracleConnection con = new OracleConnection(connStr))
                {
                    string sql = "DELETE FROM NEXORA_USER_ACCESS WHERE EMP_CODE = :EMP_CODE AND REPORT_ID = :REPORT_ID AND LOCATION_ID = :LOCATION_ID";
                    OracleCommand cmd = new OracleCommand(sql, con);
                    cmd.BindByName = true;
                    cmd.Parameters.Add("EMP_CODE", hfSelectedEmpCode.Value.Trim());
                    cmd.Parameters.Add("REPORT_ID", reportId);
                    cmd.Parameters.Add("LOCATION_ID", locationId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadUserAccessInline(hfSelectedEmpCode.Value);
                upAccessDetails.Update();
            }
        }

        protected void btnCloseInline_Click(object sender, EventArgs e)
        {
            pnlAccessDetails.Style["display"] = "none";
            upAccessDetails.Update();
        }
    }
}