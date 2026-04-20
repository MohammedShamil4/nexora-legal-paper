using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace nexora
{
    public partial class adminsettings : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //  Security Check: Admin Session Required
            if (Session["USERNAME"] == null || Session["PRIVILEGE"]?.ToString() != "ADMIN")
            {
                Response.Redirect("~/login.aspx");
            }

            if (!IsPostBack)
            {
                LoadAllData();
            }
        }

        private void LoadAllData()
        {
            LoadMasterFiles();
            LoadFieldDictionary();
            LoadStructureBindings();
        }

        // ==========================================
        // 🔹 TAB NAVIGATION
        // ==========================================
        protected void TabChange_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string tab = btn.CommandArgument;
            hfActiveTab.Value = tab;

            // Update Tab CSS
            btnTabReports.CssClass = "tab-btn" + (tab == "reports" ? " active" : "");
            btnTabFields.CssClass = "tab-btn" + (tab == "fields" ? " active" : "");
            btnTabBindings.CssClass = "tab-btn" + (tab == "bindings" ? " active" : "");

            // Update Panel Visibility
            pnlReports.Visible = (tab == "reports");
            pnlFields.Visible = (tab == "fields");
            pnlBindings.Visible = (tab == "bindings");

            upReports.Update();
            upFields.Update();
            upBindings.Update();
        }

        // ==========================================
        // 🔹 TABLE 1: NEXORA_LEGAL_MASTER_FILE
        // ==========================================
        private void LoadMasterFiles()
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT * FROM NEXORA_LEGAL_MASTER_FILE ORDER BY SL_NO DESC";
                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvReports.DataSource = dt;
                gvReports.DataBind();
            }
        }

        protected void btnNewReport_Click(object sender, EventArgs e)
        {
            litModalTitle.Text = "Register New Report Path";
            mvForms.SetActiveView(vwReport);
            hfSelectedID.Value = "";
            txtReportName.Text = "";
            txtReportPath.Text = "~/reports/";
            txtReportIcon.Text = "";
            chkReportActive.Checked = true;

            modalOverlay.Visible = true;
            upModals.Update();
        }

        protected void gvReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditReport")
            {
                string id = e.CommandArgument.ToString();
                hfSelectedID.Value = id;
                LoadReportForEdit(id);
            }
            else if (e.CommandName == "DeleteReport")
            {
                DeleteRecord("NEXORA_LEGAL_MASTER_FILE", "SL_NO", e.CommandArgument.ToString());
                LoadMasterFiles();
            }
        }

        private void LoadReportForEdit(string id)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT * FROM NEXORA_LEGAL_MASTER_FILE WHERE SL_NO = :ID";
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.Parameters.Add("ID", id);
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            txtReportName.Text = dr["NAME"].ToString();
                            txtReportPath.Text = dr["TABLE_NAME"].ToString();
                            txtReportIcon.Text = dr["ICON"].ToString();
                            chkReportActive.Checked = dr["ACTIVE"].ToString() == "Y";

                            litModalTitle.Text = "Modify Report Master";
                            mvForms.SetActiveView(vwReport);
                            modalOverlay.Visible = true;
                            upModals.Update();
                        }
                    }
                }
            }
        }

        protected void btnSaveReport_Click(object sender, EventArgs e)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql;
                bool isEdit = !string.IsNullOrEmpty(hfSelectedID.Value);
                int nextId = 0;

                con.Open();

                if (isEdit)
                {
                    sql = "UPDATE NEXORA_LEGAL_MASTER_FILE SET NAME=:NAME, TABLE_NAME=:PATH, ICON=:ICON, ACTIVE=:ACT WHERE SL_NO=:ID";
                }
                else
                {
                    // Generate Max(ID) + 1
                    using (OracleCommand cmdId = new OracleCommand("SELECT NVL(MAX(SL_NO), 0) + 1 FROM NEXORA_LEGAL_MASTER_FILE", con))
                    {
                        nextId = Convert.ToInt32(cmdId.ExecuteScalar());
                    }
                    sql = "INSERT INTO NEXORA_LEGAL_MASTER_FILE (SL_NO, NAME, TABLE_NAME, ICON, ACTIVE) VALUES (:ID, :NAME, :PATH, :ICON, :ACT)";
                }

                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("NAME", txtReportName.Text.Trim().ToUpper());
                    cmd.Parameters.Add("PATH", txtReportPath.Text.Trim().ToUpper());
                    cmd.Parameters.Add("ICON", txtReportIcon.Text.Trim().ToUpper());
                    cmd.Parameters.Add("ACT", chkReportActive.Checked ? "Y" : "N");
                    
                    if (isEdit)
                    {
                        cmd.Parameters.Add("ID", hfSelectedID.Value);
                    }
                    else
                    {
                        cmd.Parameters.Add("ID", nextId);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            CloseModal();
            LoadMasterFiles();
            upReports.Update();
        }

        // ==========================================
        // 🔹 TABLE 2: NEXORA_REPORT_PAPER_DATATYPE
        // ==========================================
        private void LoadFieldDictionary()
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT * FROM NEXORA_REPORT_PAPER_DATATYPE ORDER BY SL_NO DESC";
                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvFields.DataSource = dt;
                gvFields.DataBind();
            }
        }

        protected void btnNewField_Click(object sender, EventArgs e)
        {
            litModalTitle.Text = "Register Data Definition";
            mvForms.SetActiveView(vwField);
            hfSelectedID.Value = "";
            txtFieldName.Text = "";
            ddlFieldType.SelectedIndex = 0;
            txtFieldSource.Text = "";

            modalOverlay.Visible = true;
            upModals.Update();
        }

        protected void gvFields_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditField")
            {
                string id = e.CommandArgument.ToString();
                hfSelectedID.Value = id;
                LoadFieldForEdit(id);
            }
            else if (e.CommandName == "DeleteField")
            {
                DeleteRecord("NEXORA_REPORT_PAPER_DATATYPE", "SL_NO", e.CommandArgument.ToString());
                LoadFieldDictionary();
                upFields.Update();
            }
        }

        private void LoadFieldForEdit(string id)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT * FROM NEXORA_REPORT_PAPER_DATATYPE WHERE SL_NO = :ID";
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.Parameters.Add("ID", id);
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            txtFieldName.Text = dr["COLUMN_NAME"].ToString();
                            
                            string dtype = dr["DATA_TYPE"].ToString().Trim();
                            if (ddlFieldType.Items.FindByValue(dtype) != null)
                            {
                                ddlFieldType.SelectedValue = dtype;
                            }
                            else
                            {
                                ddlFieldType.SelectedIndex = 0; // Fallback
                            }

                            txtFieldSource.Text = dr["TABLE_NAME"].ToString();

                            litModalTitle.Text = "Modify Data Field";
                            mvForms.SetActiveView(vwField);
                            modalOverlay.Visible = true;
                            upModals.Update();
                        }
                    }
                }
            }
        }

        protected void btnSaveField_Click(object sender, EventArgs e)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql;
                bool isEdit = !string.IsNullOrEmpty(hfSelectedID.Value);
                int nextId = 0;

                con.Open();

                if (isEdit)
                {
                    sql = "UPDATE NEXORA_REPORT_PAPER_DATATYPE SET COLUMN_NAME=:CNAME, DATA_TYPE=:DTYPE, TABLE_NAME=:TNAME WHERE SL_NO=:ID";
                }
                else
                {
                    // Generate Max(ID) + 1
                    using (OracleCommand cmdId = new OracleCommand("SELECT NVL(MAX(SL_NO), 0) + 1 FROM NEXORA_REPORT_PAPER_DATATYPE", con))
                    {
                        nextId = Convert.ToInt32(cmdId.ExecuteScalar());
                    }
                    sql = "INSERT INTO NEXORA_REPORT_PAPER_DATATYPE (SL_NO, COLUMN_NAME, DATA_TYPE, TABLE_NAME) VALUES (:ID, :CNAME, :DTYPE, :TNAME)";
                }

                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("CNAME", txtFieldName.Text.Trim().ToUpper());
                    cmd.Parameters.Add("DTYPE", ddlFieldType.SelectedValue.ToUpper());
                    cmd.Parameters.Add("TNAME", txtFieldSource.Text.Trim().ToUpper());

                    if (isEdit)
                    {
                        cmd.Parameters.Add("ID", hfSelectedID.Value);
                    }
                    else
                    {
                        cmd.Parameters.Add("ID", nextId);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            CloseModal();
            LoadFieldDictionary();
            upFields.Update();
        }

        // ==========================================
        // 🔹 TABLE 3: NEXORA_LEGAL_PAPERS_DATATYPE
        // ==========================================
        private void LoadStructureBindings()
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = @"
                    SELECT B.FL_NO, B.DATATYPE_NO, 
                           M.NAME as REPORT_NAME, 
                           D.COLUMN_NAME as FIELD_IDENTIFIER, 
                           B.ORDER_NO, B.MANDATORY
                    FROM NEXORA_LEGAL_PAPERS_DATATYPE B
                    JOIN NEXORA_LEGAL_MASTER_FILE M ON B.FL_NO = M.SL_NO
                    JOIN NEXORA_REPORT_PAPER_DATATYPE D ON B.DATATYPE_NO = D.SL_NO
                    ORDER BY M.NAME, B.ORDER_NO";

                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvBindings.DataSource = dt;
                gvBindings.DataBind();
            }
        }

        protected void btnNewBinding_Click(object sender, EventArgs e)
        {
            PopulateBindingLists();
            litModalTitle.Text = "Establish Structural Link";
            mvForms.SetActiveView(vwBinding);
            hfSelectedID.Value = "";
            txtBindOrder.Text = "1";
            chkBindMandatory.Checked = true;

            // Enable controls for new entry
            ddlBindReport.Enabled = true;
            ddlBindField.Enabled = true;
            chkBindMandatory.Enabled = true;

            modalOverlay.Visible = true;
            upModals.Update();
        }

        private void PopulateBindingLists()
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();

                // Reports
                OracleCommand cmd1 = new OracleCommand("SELECT SL_NO, NAME FROM NEXORA_LEGAL_MASTER_FILE ORDER BY NAME", con);
                ddlBindReport.DataSource = cmd1.ExecuteReader();
                ddlBindReport.DataTextField = "NAME";
                ddlBindReport.DataValueField = "SL_NO";
                ddlBindReport.DataBind();

                // Fields
                OracleCommand cmd2 = new OracleCommand("SELECT SL_NO, COLUMN_NAME FROM NEXORA_REPORT_PAPER_DATATYPE ORDER BY COLUMN_NAME", con);
                ddlBindField.DataSource = cmd2.ExecuteReader();
                ddlBindField.DataTextField = "COLUMN_NAME";
                ddlBindField.DataValueField = "SL_NO";
                ddlBindField.DataBind();
            }
        }

        protected void gvBindings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditBinding")
            {
                string id = e.CommandArgument.ToString();
                hfSelectedID.Value = id;
                LoadBindingForEdit(id);
            }
            else if (e.CommandName == "DeleteBinding")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                string flNo = parts[0];
                string dtNo = parts[1];

                using (OracleConnection con = new OracleConnection(connStr))
                {
                    string sql = "DELETE FROM NEXORA_LEGAL_PAPERS_DATATYPE WHERE FL_NO=:FL AND DATATYPE_NO=:DT";
                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        cmd.Parameters.Add("FL", flNo);
                        cmd.Parameters.Add("DT", dtNo);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadStructureBindings();
            }
        }

        private void LoadBindingForEdit(string compositeId)
        {
            string[] parts = compositeId.Split('|');
            string flNo = parts[0];
            string dtNo = parts[1];

            PopulateBindingLists();

            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = "SELECT * FROM NEXORA_LEGAL_PAPERS_DATATYPE WHERE FL_NO=:FL AND DATATYPE_NO=:DT";
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.Parameters.Add("FL", flNo);
                    cmd.Parameters.Add("DT", dtNo);
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            ddlBindReport.SelectedValue = dr["FL_NO"].ToString();
                            ddlBindField.SelectedValue = dr["DATATYPE_NO"].ToString();
                            txtBindOrder.Text = dr["ORDER_NO"].ToString();
                            chkBindMandatory.Checked = dr["MANDATORY"].ToString() == "Y";

                            // Disable key fields and requirement status during edit
                            ddlBindReport.Enabled = false;
                            ddlBindField.Enabled = false;
                            chkBindMandatory.Enabled = false;

                            litModalTitle.Text = "Modify Structural Link";
                            mvForms.SetActiveView(vwBinding);
                            modalOverlay.Visible = true;
                            upModals.Update();
                        }
                    }
                }
            }
        }

        protected void btnSaveBinding_Click(object sender, EventArgs e)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql;
                string valFL, valDT;
                bool isEdit = !string.IsNullOrEmpty(hfSelectedID.Value);

                if (isEdit)
                {
                    string[] parts = hfSelectedID.Value.Split('|');
                    valFL = parts[0];
                    valDT = parts[1];
                    sql = "UPDATE NEXORA_LEGAL_PAPERS_DATATYPE SET ORDER_NO=:ORD, MANDATORY=:MAND WHERE FL_NO=:FL AND DATATYPE_NO=:DT";
                }
                else
                {
                    valFL = ddlBindReport.SelectedValue;
                    valDT = ddlBindField.SelectedValue;
                    sql = "INSERT INTO NEXORA_LEGAL_PAPERS_DATATYPE (FL_NO, DATATYPE_NO, ORDER_NO, MANDATORY) VALUES (:FL, :DT, :ORD, :MAND)";
                }

                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("FL", valFL);
                    cmd.Parameters.Add("DT", valDT);
                    cmd.Parameters.Add("ORD", txtBindOrder.Text);
                    cmd.Parameters.Add("MAND", chkBindMandatory.Checked ? "Y" : "N");

                    con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Handle duplicates or other DB errors
                    }
                }
            }
            CloseModal();
            LoadStructureBindings();
            upBindings.Update();
        }

        // ==========================================
        // 🔹 UTILS
        // ==========================================
        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            CloseModal();
        }

        private void CloseModal()
        {
            modalOverlay.Visible = false;
            upModals.Update();
        }

        private void DeleteRecord(string table, string pkColumn, string id)
        {
            using (OracleConnection con = new OracleConnection(connStr))
            {
                string sql = $"DELETE FROM {table} WHERE {pkColumn} = :ID";
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.Parameters.Add("ID", id);
                    con.Open();
                    try { cmd.ExecuteNonQuery(); } catch { /* Foreign key safety */ }
                }
            }
        }
    }
}