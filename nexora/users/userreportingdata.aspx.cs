using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.IO;

namespace nexora.users
{
    public partial class userreportingdata : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SL_NO"] == null)
            {
                Response.Write("SL_NO missing");
                return;
            }

            if (!IsPostBack)
                LoadReport();
        }

        // ===================== MAIN GRID (ONLY MAIN TABLE) =====================
        private void LoadReport()
        {
            string flNo = Session["SL_NO"].ToString();
            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                con.Open();

                // Get columns
                DataTable dtCols = new DataTable();

                string colSql = @"
                    SELECT RPD.COLUMN_NAME
                    FROM NEXORA_LEGAL_PAPERS_DATATYPE LPD
                    JOIN NEXORA_REPORT_PAPER_DATATYPE RPD
                    ON LPD.DATATYPE_NO = RPD.SL_NO
                    WHERE LPD.FL_NO = :FL_NO
                    ORDER BY LPD.ORDER_NO";

                using (OracleDataAdapter da = new OracleDataAdapter(colSql, con))
                {
                    da.SelectCommand.Parameters.Add(":FL_NO", flNo);
                    da.Fill(dtCols);
                }

                // ONLY MAIN TABLE DATA
                DataTable dtData = new DataTable();

                string dataSql = @"
                    SELECT FL_TYPE, COL_NAME, DATAS, DOCUMENT_DTL
                    FROM NEXORA_LEGAL_PAPERS_DATA
                    WHERE FL_TYPE = :FL_NO";

                using (OracleDataAdapter da = new OracleDataAdapter(dataSql, con))
                {
                    da.SelectCommand.Parameters.Add(":FL_NO", flNo);
                    da.Fill(dtData);
                }

                // Build grid
                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("DOCUMENT_DTL");

                foreach (DataRow r in dtCols.Rows)
                {
                    string col = r["COLUMN_NAME"].ToString();
                    if (!dtResult.Columns.Contains(col))
                        dtResult.Columns.Add(col);
                }

                DataView view = new DataView(dtData);
                DataTable docs = view.ToTable(true, "DOCUMENT_DTL");

                foreach (DataRow d in docs.Rows)
                {
                    string doc = d["DOCUMENT_DTL"].ToString();

                    DataRow row = dtResult.NewRow();
                    row["DOCUMENT_DTL"] = doc;

                    foreach (DataRow x in dtData.Select($"DOCUMENT_DTL = '{doc.Replace("'", "''")}'"))
                    {
                        string col = x["COL_NAME"].ToString();
                        if (dtResult.Columns.Contains(col))
                            row[col] = x["DATAS"];
                    }

                    dtResult.Rows.Add(row);
                }

                gv.DataSource = dtResult;
                gv.DataBind();
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "alert alert-success" : "alert alert-error";
        }

        // ===================== HISTORY BUTTON (MAIN TABLE ONLY) =====================
        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "History")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                string docId = gv.DataKeys[index].Value.ToString();

                LoadHistory(docId);
                LoadReport(); // Rebind main grid so we don't lose links during postback

                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    "popup", "openModal();", true);
            }
        }

        private void LoadHistory(string docId)
        {
            string flNo = Session["SL_NO"].ToString();
            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(cs))
            {
                con.Open();

                // 1. GET COLUMNS
                DataTable dtCols = new DataTable();

                string colSql = @"
            SELECT RPD.COLUMN_NAME
            FROM NEXORA_LEGAL_PAPERS_DATATYPE LPD
            JOIN NEXORA_REPORT_PAPER_DATATYPE RPD
            ON LPD.DATATYPE_NO = RPD.SL_NO
            WHERE LPD.FL_NO = :FL_NO
            ORDER BY LPD.ORDER_NO";

                using (OracleDataAdapter da = new OracleDataAdapter(colSql, con))
                {
                    da.SelectCommand.Parameters.Add(":FL_NO", flNo);
                    da.Fill(dtCols);
                }

                // 2. GET DATA
                DataTable dtData = new DataTable();

                string dataSql = @"
            SELECT FL_TYPE, COL_NAME, DATAS, DOCUMENT_DTL, DTL_ID
            FROM NEXORA_LEGAL_PAPERS_DATA
            WHERE DOCUMENT_DTL = :DOC
            AND FL_TYPE = :FL_NO
            ORDER BY DTL_ID";

                using (OracleDataAdapter da = new OracleDataAdapter(dataSql, con))
                {
                    da.SelectCommand.Parameters.Add(":DOC", docId);
                    da.SelectCommand.Parameters.Add(":FL_NO", flNo);
                    da.Fill(dtData);
                }

                // 3. GET DISTINCT DTL_ID (VERSIONS)
                DataView dv = new DataView(dtData);
                DataTable dtVersions = dv.ToTable(true, "DTL_ID");

                // 4. BUILD RESULT TABLE
                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("DOCUMENT_DTL");

                foreach (DataRow c in dtCols.Rows)
                {
                    string col = c["COLUMN_NAME"].ToString();
                    if (!dtResult.Columns.Contains(col))
                        dtResult.Columns.Add(col);
                }

                // 5. BUILD EACH VERSION ROW
                foreach (DataRow v in dtVersions.Rows)
                {
                    string dtlId = v["DTL_ID"].ToString();

                    DataRow newRow = dtResult.NewRow();
                    newRow["DOCUMENT_DTL"] = docId;

                    foreach (DataRow r in dtData.Select($"DTL_ID = '{dtlId}'"))
                    {
                        string col = r["COL_NAME"].ToString();
                        string val = r["DATAS"].ToString();

                        if (dtResult.Columns.Contains(col))
                            newRow[col] = val;
                    }

                    dtResult.Rows.Add(newRow);
                }

                // 6. SORT BY END_DATE DESC
                if (dtResult.Columns.Contains("END_DATE"))
                {
                    DataView sortView = dtResult.DefaultView;
                    sortView.Sort = "END_DATE DESC";
                    dtResult = sortView.ToTable();
                }

                gvHistory.DataSource = dtResult;
                gvHistory.DataBind();
            }
        }

        // ===================== EDIT (INSERT ONLY INTO APP TABLE) =====================
        protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gv.EditIndex = e.NewEditIndex;
            LoadReport();
        }

        protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gv.EditIndex = -1;
            LoadReport();
        }

        protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            pnlMessage.Visible = false;
            string docId = gv.DataKeys[e.RowIndex].Value.ToString();
            string cs = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            GridViewRow row = gv.Rows[e.RowIndex];

            // --- DATE VALIDATION ---
            string startDateStr = "";
            string endDateStr = "";

            for (int j = 1; j < row.Cells.Count; j++)
            {
                string colHeader = gv.HeaderRow.Cells[j].Text.ToUpper();
                if (colHeader.Contains("START_DATE") || colHeader.Contains("END_DATE"))
                {
                    TextBox txt = row.Cells[j].Controls.Count > 0 ? row.Cells[j].Controls[0] as TextBox : null;
                    if (txt != null)
                    {
                        if (colHeader.Contains("START_DATE")) startDateStr = txt.Text;
                        else endDateStr = txt.Text;
                    }
                }
            }

            if (!string.IsNullOrEmpty(startDateStr) && !string.IsNullOrEmpty(endDateStr))
            {
                if (DateTime.TryParse(startDateStr, out DateTime startDate) &&
                    DateTime.TryParse(endDateStr, out DateTime endDate))
                {
                    if (startDate > endDate)
                    {
                        ShowMessage("Start Date cannot be greater than End Date.", false);
                        gv.EditIndex = -1;
                        LoadReport();
                        return;
                    }
                }
            }
            // -----------------------

            using (OracleConnection con = new OracleConnection(cs))
            {
                con.Open();
                OracleTransaction txn = con.BeginTransaction();

                try
                {
                    int flNo = Convert.ToInt32(Session["SL_NO"]);

                    string newDoc = docId;

                    for (int i = 1; i < row.Cells.Count; i++)
                    {
                        string col = gv.HeaderRow.Cells[i].Text;

                        if (col == "DOCUMENT_DTL" || string.IsNullOrWhiteSpace(col) || col == "&nbsp;")
                            continue;

                        if (col.ToUpper().Contains("_UPLOAD"))
                        {
                            string fuBaseName = "fu_" + col;
                            string hfBaseName = "hf_" + col;

                            string hfKey = Request.Form.AllKeys.FirstOrDefault(k => k != null && k.EndsWith(hfBaseName));
                            string filePath = hfKey != null ? Request.Form[hfKey] : "";

                            string fuKey = Request.Files.AllKeys.FirstOrDefault(k => k != null && k.EndsWith(fuBaseName));

                            if (fuKey != null)
                            {
                                System.Web.HttpPostedFile postedFile = Request.Files[fuKey];
                                if (postedFile != null && postedFile.ContentLength > 0)
                                {
                                    string savedPath = SavePostedFile(postedFile, newDoc, col);
                                    if (!string.IsNullOrEmpty(savedPath))
                                    {
                                        filePath = savedPath;
                                    }
                                }
                            }

                            InsertIntoApp(con, txn, flNo, col, filePath, newDoc);
                        }
                        else
                        {
                            TextBox txt = row.Cells[i].Controls.Count > 0 ? row.Cells[i].Controls[0] as TextBox : null;

                            if (txt != null)
                            {
                                InsertIntoApp(con, txn, flNo, col, txt.Text.Trim().ToUpper(), newDoc);
                            }
                        }
                    }

                    txn.Commit();
                }
                catch
                {
                    txn.Rollback();
                }
            }

            gv.EditIndex = -1;
            LoadReport();
        }

        private readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
        private const int MaxFileSizeMB = 30;

        private string SavePostedFile(System.Web.HttpPostedFile postedFile, string documentDtl, string columnName)
        {
            try
            {
                string fileName = Path.GetFileName(postedFile.FileName);
                string fileExtension = Path.GetExtension(fileName).ToLower();
                int fileSizeKB = postedFile.ContentLength / 1024;
                int fileSizeMB = fileSizeKB / 1024;

                if (Array.IndexOf(AllowedExtensions, fileExtension) < 0) return null;
                if (fileSizeMB > MaxFileSizeMB) return null;

                string uploadType = columnName.ToUpper().Replace("_UPLOAD", "").ToLower();
                string mediaFolder = Server.MapPath($"~/media/{uploadType}/");

                if (!Directory.Exists(mediaFolder))
                {
                    Directory.CreateDirectory(mediaFolder);
                }

                string uniqueFileName = documentDtl + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;
                string fullPath = Path.Combine(mediaFolder, uniqueFileName);

                postedFile.SaveAs(fullPath);
                return $"~/media/{uploadType}/" + uniqueFileName;
            }
            catch
            {
                return null;
            }
        }

        private void InsertIntoApp(OracleConnection con, OracleTransaction txn,
            int flNo, string col, string val, string doc)
        {
            string sql = @"
                INSERT INTO NEXORA_LEGAL_PAPERS_DATA_APP
                (FL_TYPE, COL_NAME, DATAS, DOCUMENT_DTL, STATUS)
                VALUES (:FL_TYPE, :COL_NAME, :DATAS, :DOC, 'PENDING')";

            using (OracleCommand cmd = new OracleCommand(sql, con))
            {
                cmd.Transaction = txn;
                cmd.Parameters.Add(":FL_TYPE", flNo);
                cmd.Parameters.Add(":COL_NAME", col);
                cmd.Parameters.Add(":DATAS", val);
                cmd.Parameters.Add(":DOC", doc);
                cmd.ExecuteNonQuery();
            }
        }

        // ===================== FILE HANDLING =====================
        protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            GridView grid = (GridView)sender;
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv == null)
                return;

            DataTable table = drv.Row.Table;

            int templateFieldCount = 0;
            foreach (DataControlField field in grid.Columns)
            {
                if (field is TemplateField)
                    templateFieldCount++;
                else
                    break;
            }

            // ================= 🔥 EDIT MODE =================
            if (e.Row.RowState.HasFlag(DataControlRowState.Edit))
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string colName = table.Columns[i].ColumnName;

                    int cellIndex = templateFieldCount + i;

                    if (cellIndex >= e.Row.Cells.Count)
                        continue;

                    TableCell cell = e.Row.Cells[cellIndex];

                    // 🔥 UPLOAD FIELD → FILEUPLOAD ONLY
                    if (colName.ToUpper().Contains("_UPLOAD"))
                    {
                        cell.Controls.Clear();

                        FileUpload fu = new FileUpload
                        {
                            ID = "fu_" + colName
                        };

                        cell.Controls.Add(fu);

                        string existingFilePath = Convert.ToString(drv[colName])?.Trim();
                        HiddenField hf = new HiddenField
                        {
                            ID = "hf_" + colName,
                            Value = existingFilePath
                        };
                        cell.Controls.Add(hf);
                    }
                    else
                    {
                        // NORMAL TEXTBOX STYLING
                        foreach (Control ctrl in cell.Controls)
                        {
                            if (ctrl is TextBox txt)
                            {
                                txt.CssClass = "grid-input";
                                if (colName.ToUpper().Contains("DATE"))
                                {
                                    txt.TextMode = TextBoxMode.Date;
                                }
                            }
                        }
                    }
                }
            }

            // ================= 🔥 NORMAL VIEW MODE =================
            if (!e.Row.RowState.HasFlag(DataControlRowState.Edit))
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string colName = table.Columns[i].ColumnName;

                    if (!colName.ToUpper().Contains("_UPLOAD"))
                        continue;

                    int cellIndex = templateFieldCount + i;

                    if (cellIndex >= e.Row.Cells.Count)
                        continue;

                    TableCell cell = e.Row.Cells[cellIndex];

                    string filePath = Convert.ToString(drv[colName])?.Trim();

                    if (string.IsNullOrEmpty(filePath))
                        continue;

                    string url = filePath;

                    if (url.StartsWith("~"))
                        url = ResolveUrl(url);
                    else if (!url.StartsWith("/") &&
                             !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        url = ResolveUrl("~/" + url.TrimStart('/'));

                    HyperLink lnk = new HyperLink
                    {
                        Text = "Download",
                        NavigateUrl = url,
                        Target = "_blank",
                        CssClass = "btn-download"
                    };

                    cell.Controls.Clear();
                    cell.Controls.Add(lnk);
                }
            }
        }
    }
}