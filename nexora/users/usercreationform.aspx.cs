using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace nexora.users
{
    public partial class usercreationform : System.Web.UI.Page
    {
        // Allowed file extensions
        private readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
        private const int MaxFileSizeMB = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if logged in
            if (!IsUserLoggedIn())
            {
                Response.Redirect("~/login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Display user info
                lblUserName.Text = Session["EMP_NAME"]?.ToString() ?? "User";
                lblLocation.Text = Session["DEFAULT_LOCATION"]?.ToString() ?? "";

                LoadFormDropdown();
            }
            else
            {
                // Recreate dynamic controls on postback
                if (!string.IsNullOrEmpty(ddlFormType.SelectedValue))
                {
                    int formNo = Convert.ToInt32(ddlFormType.SelectedValue);
                    LoadDynamicFields(formNo);
                }
            }
        }

        // ===============================
        // CHECK IF USER IS LOGGED IN
        // ===============================
        private bool IsUserLoggedIn()
        {
            return Session["IS_LOGGED_IN"] != null && (bool)Session["IS_LOGGED_IN"] == true;
        }

        // ===============================
        // LOAD DROPDOWN - ONLY USER'S ACCESSIBLE REPORTS
        // ===============================
        private void LoadFormDropdown()
        {
            // Get user's accessible report IDs from session
            string reportIds = Session["REPORT_IDS"]?.ToString();

            if (string.IsNullOrEmpty(reportIds))
            {
                ShowMessage("No reports assigned. Contact administrator.", false);
                btnSave.Enabled = false;
                return;
            }

            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                // Only show reports user has access to
                OracleCommand cmd = new OracleCommand($@"
                    SELECT SL_NO, NAME
                    FROM NEXORA_LEGAL_MASTER_FILE
                    WHERE ACTIVE = 'Y'
                      AND SL_NO IN ({reportIds})
                    ORDER BY SL_NO", con);

                con.Open();

                ddlFormType.DataSource = cmd.ExecuteReader();
                ddlFormType.DataTextField = "NAME";
                ddlFormType.DataValueField = "SL_NO";
                ddlFormType.DataBind();
            }

            ddlFormType.Items.Insert(0, new ListItem("-- Select Form --", ""));
        }

        // ===============================
        // DROPDOWN CHANGE
        // ===============================
        protected void ddlFormType_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlMessage.Visible = false;
            phDynamicForm.Controls.Clear();

            if (string.IsNullOrEmpty(ddlFormType.SelectedValue))
                return;

            int formNo = Convert.ToInt32(ddlFormType.SelectedValue);
            LoadDynamicFields(formNo);
        }

        // ===============================
        // BUILD DYNAMIC FIELDS
        // ===============================
        private void LoadDynamicFields(int formNo)
        {
            phDynamicForm.Controls.Clear();

            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                OracleCommand cmd = new OracleCommand(@"
                    SELECT 
                        r.COLUMN_NAME,
                        r.DATA_TYPE,
                        r.TABLE_NAME,
                        l.MANDATORY
                    FROM NEXORA_LEGAL_PAPERS_DATATYPE l
                    JOIN NEXORA_REPORT_PAPER_DATATYPE r
                        ON l.DATATYPE_NO = r.SL_NO
                    WHERE l.FL_NO = :FL_NO
                    ORDER BY l.ORDER_NO", con);

                cmd.Parameters.Add(":FL_NO", formNo);

                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string columnName = dr["COLUMN_NAME"].ToString();
                    string dataType = dr["DATA_TYPE"].ToString().ToUpper();
                    string tableName = dr["TABLE_NAME"]?.ToString();
                    bool mandatory = dr["MANDATORY"].ToString() == "Y";

                    // LABEL
                    Label lbl = new Label();
                    lbl.Text = columnName.Replace("_", " ");
                    lbl.CssClass = "form-label";
                    phDynamicForm.Controls.Add(lbl);

                    // CHECK IF COLUMN NAME CONTAINS "_UPLOAD"
                    if (IsUploadField(columnName))
                    {
                        CreateFileUploadControl(columnName, mandatory);
                    }
                    // DROP DOWN FIELD
                    else if (dataType == "DROP DOWN")
                    {
                        CreateDropdown(columnName, tableName);
                    }
                    // DATE FIELD
                    else if (dataType.Contains("DATE"))
                    {
                        CreateDateControl(columnName, mandatory);
                    }
                    // TEXT/VARCHAR FIELD
                    else
                    {
                        CreateTextControl(columnName, dataType, mandatory);
                    }
                }
            }
        }

        // ===============================
        // CHECK IF UPLOAD FIELD
        // ===============================
        private bool IsUploadField(string columnName)
        {
            string upperName = columnName.ToUpper();
            return upperName.Contains("_UPLOAD");
        }

        // ===============================
        // CREATE FILE UPLOAD CONTROL
        // ===============================
        private void CreateFileUploadControl(string columnName, bool mandatory)
        {
            FileUpload fileUpload = new FileUpload();
            fileUpload.ID = "file_" + columnName;
            fileUpload.CssClass = "form-control file-upload";
            fileUpload.AllowMultiple = false;
            phDynamicForm.Controls.Add(fileUpload);

            Label hint = new Label();
            hint.Text = "Allowed: PDF, JPG, PNG, DOC, DOCX, XLS, XLSX (Max 10MB)";
            hint.CssClass = "file-hint";
            phDynamicForm.Controls.Add(hint);

            if (mandatory)
            {
                Label reqLabel = new Label();
                reqLabel.ID = "req_" + columnName;
                reqLabel.Text = " * Required";
                reqLabel.CssClass = "file-required-hint";
                phDynamicForm.Controls.Add(reqLabel);
            }
        }

        // =========================
        // CREATE DROP DOWN
        // ===============================

        private void CreateDropdown(string columnName, string tableName)
        {
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddl_" + columnName;
            ddl.CssClass = "form-control";

            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                con.Open();

                // Get first two columns of the table
                string columnQuery = @"
                SELECT COLUMN_NAME 
                FROM USER_TAB_COLUMNS 
                WHERE TABLE_NAME = :TABLE_NAME
                ORDER BY COLUMN_ID";

                OracleCommand colCmd = new OracleCommand(columnQuery, con);
                colCmd.Parameters.Add(":TABLE_NAME", tableName.ToUpper());

                OracleDataReader colReader = colCmd.ExecuteReader();

                string valueColumn = "";
                string textColumn = "";
                int count = 0;

                while (colReader.Read())
                {
                    if (count == 0)
                        valueColumn = colReader["COLUMN_NAME"].ToString();
                    else if (count == 1)
                        textColumn = colReader["COLUMN_NAME"].ToString();

                    count++;

                    if (count == 2)
                        break;
                }

                // Build dynamic query
                string query = $"SELECT {valueColumn}, {textColumn} FROM {tableName}";

                OracleCommand cmd = new OracleCommand(query, con);

                ddl.DataSource = cmd.ExecuteReader();
                ddl.DataTextField = textColumn;
                ddl.DataValueField = valueColumn;
                ddl.DataBind();
            }

            ddl.Items.Insert(0, new ListItem("-- Select --", ""));

            phDynamicForm.Controls.Add(ddl);
        }

        // ===============================
        // CREATE DATE CONTROL
        // ===============================
        private void CreateDateControl(string columnName, bool mandatory)
        {
            TextBox txt = new TextBox();
            txt.ID = "txt_" + columnName;
            txt.CssClass = "form-control";
            txt.TextMode = TextBoxMode.Date;
            phDynamicForm.Controls.Add(txt);

            if (mandatory)
            {
                AddRequiredValidator(txt.ID);
            }
        }

        // ===============================
        // CREATE TEXT CONTROL
        // ===============================
        private void CreateTextControl(string columnName, string dataType, bool mandatory)
        {
            TextBox txt = new TextBox();
            txt.ID = "txt_" + columnName;
            txt.CssClass = "form-control";

            if (dataType.Contains("300") || dataType.Contains("500") ||
                dataType.Contains("CLOB") || dataType.Contains("TEXT"))
            {
                txt.TextMode = TextBoxMode.MultiLine;
                txt.Rows = 3;
            }

            phDynamicForm.Controls.Add(txt);

            if (mandatory)
            {
                AddRequiredValidator(txt.ID);
            }
        }

        // ===============================
        // ADD REQUIRED FIELD VALIDATOR
        // ===============================
        private void AddRequiredValidator(string controlId)
        {
            RequiredFieldValidator req = new RequiredFieldValidator();
            req.ControlToValidate = controlId;
            req.ErrorMessage = "* Required";
            req.ForeColor = System.Drawing.Color.Red;
            req.Display = ValidatorDisplay.Dynamic;
            phDynamicForm.Controls.Add(req);
        }

        // ===============================
        // CLEAR BUTTON CLICK
        // ===============================
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlFormType.SelectedIndex = 0;
            phDynamicForm.Controls.Clear();
            pnlMessage.Visible = false;
        }

        // ===============================
        // SAVE BUTTON CLICK
        // ===============================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlFormType.SelectedValue))
            {
                ShowMessage("Please select a form type.", false);
                return;
            }

            if (!Page.IsValid)
                return;

            // PRE-VALIDATION: Check Start Date and End Date
            TextBox txtStart = null;
            TextBox txtEnd = null;

            foreach (Control ctrl in phDynamicForm.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    string upperId = txt.ID.ToUpper();
                    if (upperId.Contains("START_DATE"))
                        txtStart = txt;
                    else if (upperId.Contains("END_DATE"))
                        txtEnd = txt;
                }
            }

            if (txtStart != null && txtEnd != null && 
                !string.IsNullOrEmpty(txtStart.Text) && !string.IsNullOrEmpty(txtEnd.Text))
            {
                if (DateTime.TryParse(txtStart.Text, out DateTime startDate) && 
                    DateTime.TryParse(txtEnd.Text, out DateTime endDate))
                {
                    if (startDate > endDate)
                    {
                        ShowMessage("Start Date cannot be greater than End Date.", false);
                        return;
                    }
                }
            }

            int flNo = Convert.ToInt32(ddlFormType.SelectedValue);
            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                con.Open();
                OracleTransaction txn = con.BeginTransaction();

                try
                {
                    string documentDtl = GenerateUniqueDocumentNumber(con, txn);

                    if (!ValidateMandatoryFileUploads(flNo))
                    {
                        txn.Rollback();
                        return;
                    }

                    foreach (Control ctrl in phDynamicForm.Controls)
                    {
                        // ================= FILE UPLOAD =================
                        if (ctrl is FileUpload fileUpload)
                        {
                            string columnName = fileUpload.ID.Replace("file_", "");
                            string filePath = "";

                            if (fileUpload.HasFile)
                            {
                                filePath = SaveUploadedFile(fileUpload, documentDtl, columnName);

                                if (filePath == null)
                                {
                                    txn.Rollback();
                                    return;
                                }
                            }

                            InsertData(con, txn, flNo, columnName, filePath, documentDtl);
                        }

                        // ================= TEXTBOX =================
                        else if (ctrl is TextBox txt)
                        {
                            InsertData(con, txn, flNo,
                                txt.ID.Replace("txt_", ""),
                                txt.Text.Trim().ToUpper(),
                                documentDtl);
                        }

                        // ================= DROPDOWN (FIX) =================
                        else if (ctrl is DropDownList ddl)
                        {
                            string columnName = ddl.ID.Replace("ddl_", "");
                            string selectedText = ddl.SelectedItem.Text;

                            // Skip default empty selection
                            if (!string.IsNullOrEmpty(selectedText))
                            {
                                InsertData(con, txn, flNo, columnName, selectedText.ToUpper(), documentDtl);
                            }
                        }
                    }

                    // Audit info
                    InsertAuditInfo(con, txn, flNo, documentDtl);

                    txn.Commit();

                    ddlFormType.SelectedIndex = 0;
                    phDynamicForm.Controls.Clear();

                    ShowMessage($"Data saved successfully! Document: {documentDtl}", true);
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    ShowMessage("Error saving data: " + ex.Message, false);
                }
            }
        }

        // ===============================
        // VALIDATE MANDATORY FILE UPLOADS
        // ===============================
        private bool ValidateMandatoryFileUploads(int formNo)
        {
            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                OracleCommand cmd = new OracleCommand(@"
                    SELECT r.COLUMN_NAME
                    FROM NEXORA_LEGAL_PAPERS_DATATYPE l
                    JOIN NEXORA_REPORT_PAPER_DATATYPE r
                        ON l.DATATYPE_NO = r.SL_NO
                    WHERE l.FL_NO = :FL_NO
                      AND l.MANDATORY = 'Y'
                    ORDER BY l.ORDER_NO", con);

                cmd.Parameters.Add(":FL_NO", formNo);
                con.Open();

                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string columnName = dr["COLUMN_NAME"].ToString();

                    if (IsUploadField(columnName))
                    {
                        FileUpload fileUpload = phDynamicForm.FindControl("file_" + columnName) as FileUpload;

                        if (fileUpload != null && !fileUpload.HasFile)
                        {
                            ShowMessage($"{columnName.Replace("_", " ")} is required. Please upload a file.", false);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // ===============================
        // SAVE UPLOADED FILE
        // ===============================
        private string SaveUploadedFile(FileUpload fileUpload, string documentDtl, string columnName)
        {
            try
            {
                string fileName = Path.GetFileName(fileUpload.FileName);
                string fileExtension = Path.GetExtension(fileName).ToLower();
                int fileSizeKB = fileUpload.PostedFile.ContentLength / 1024;
                int fileSizeMB = fileSizeKB / 1024;

                // Validate file extension
                if (Array.IndexOf(AllowedExtensions, fileExtension) < 0)
                {
                    ShowMessage($"Invalid file type: {fileExtension}. Allowed: {string.Join(", ", AllowedExtensions)}", false);
                    return null;
                }

                // Validate file size
                if (fileSizeMB > MaxFileSizeMB)
                {
                    ShowMessage($"File size exceeds {MaxFileSizeMB}MB limit.", false);
                    return null;
                }

                // Create subfolder based on upload type
                string uploadType = columnName.ToUpper().Replace("_UPLOAD", "").ToLower();
                string mediaFolder = Server.MapPath($"~/media/{uploadType}/");

                if (!Directory.Exists(mediaFolder))
                {
                    Directory.CreateDirectory(mediaFolder);
                }

                // Generate unique file name
                string uniqueFileName = documentDtl + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;
                string fullPath = Path.Combine(mediaFolder, uniqueFileName);

                // Save file
                fileUpload.SaveAs(fullPath);

                // Return relative path
                return $"~/media/{uploadType}/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                ShowMessage("Error uploading file: " + ex.Message, false);
                return null;
            }
        }

        // ===============================
        // INSERT DATA TO DATABASE
        // ===============================
        private void InsertData(OracleConnection con, OracleTransaction txn, int flNo, string colName, string data, string documentDtl)
        {
            OracleCommand cmd = new OracleCommand(@"
                INSERT INTO NEXORA_LEGAL_PAPERS_DATA_APP
                (FL_TYPE, COL_NAME, DATAS, DOCUMENT_DTL, STATUS)
                VALUES
                (:FL_TYPE, :COL_NAME, :DATAS, :DOCUMENT_DTL, 'PENDING')
            ", con);

            cmd.Transaction = txn;

            cmd.Parameters.Add(":FL_TYPE", flNo);
            cmd.Parameters.Add(":COL_NAME", colName);
            cmd.Parameters.Add(":DATAS", data);
            cmd.Parameters.Add(":DOCUMENT_DTL", documentDtl);

            cmd.ExecuteNonQuery();
        }

        // ===============================
        // INSERT AUDIT INFO (WHO CREATED)
        // ===============================
        private void InsertAuditInfo(OracleConnection con, OracleTransaction txn, int flNo, string documentDtl)
        {
            try
            {
                string empCode = Session["EMP_CODE"]?.ToString().ToUpper() ?? "";
                string empName = Session["EMP_NAME"]?.ToString().ToUpper() ?? "";
                string location = Session["DEFAULT_LOCATION"]?.ToString().ToUpper() ?? "";

                InsertData(con, txn, flNo, "CREATED_BY_CODE", empCode, documentDtl);
                InsertData(con, txn, flNo, "CREATED_BY_NAME", empName, documentDtl);
                InsertData(con, txn, flNo, "CREATED_LOCATION", location, documentDtl);
                InsertData(con, txn, flNo, "CREATED_DATE", DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss").ToUpper(), documentDtl);
            }
            catch
            {
                // Continue even if audit fails
            }
        }

        // ===============================
        // GENERATE UNIQUE DOCUMENT NUMBER
        // ===============================
        private string GenerateUniqueDocumentNumber(OracleConnection con, OracleTransaction txn)
        {
            string documentDtl;
            bool isUnique = false;
            int maxAttempts = 10;
            int attempts = 0;

            do
            {
                documentDtl = "DOC-" + DateTime.Now.ToString("yyyyMMdd") + "-" +
                              Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                OracleCommand checkCmd = new OracleCommand(@"
                    SELECT COUNT(*) 
                    FROM NEXORA_LEGAL_PAPERS_DATA_APP 
                    WHERE DOCUMENT_DTL = :DOCUMENT_DTL
                ", con);

                checkCmd.Transaction = txn;
                checkCmd.Parameters.Add(":DOCUMENT_DTL", documentDtl);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                isUnique = (count == 0);

                attempts++;

            } while (!isUnique && attempts < maxAttempts);

            if (!isUnique)
            {
                throw new Exception("Unable to generate unique document number. Please try again.");
            }

            return documentDtl;
        }

        // ===============================
        // SHOW MESSAGE
        // ===============================
        private void ShowMessage(string message, bool isSuccess)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "alert alert-success" : "alert alert-error";
        }
    }
}