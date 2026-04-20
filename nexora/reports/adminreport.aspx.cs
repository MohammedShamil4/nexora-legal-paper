using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace nexora.reports
{
    public partial class adminreport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // We expect SL_NO (FL_TYPE / report id) in session
            if (Session["SL_NO"] == null)
            {
                lblPendingInfo.Text = "SL_NO missing in session. Please select a report first.";
                gvPending.Visible = false;
                gv.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                int flNo = Convert.ToInt32(Session["SL_NO"]);
                LoadPendingReport(flNo);  // from APP (PENDING)
                LoadApprovedReport(flNo); // from DATA
            }
        }

        // ---------------- APPROVED (main table) ----------------
        private void LoadApprovedReport(int flNo)
        {
            string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();

                // 1) Dynamic columns
                DataTable dtCols = GetDynamicColumns(con, flNo);

                // 2) Data from main table - only LATEST version per document
                DataTable dtData = new DataTable();
                string dataSql = @"
                    SELECT DOCUMENT_DTL, COL_NAME, DATAS, DTL_ID
                    FROM NEXORA_LEGAL_PAPERS_DATA
                    WHERE FL_TYPE = :FL_NO
                      AND DTL_ID IN (
                          SELECT MAX(DTL_ID) 
                          FROM NEXORA_LEGAL_PAPERS_DATA 
                          WHERE FL_TYPE = :FL_NO 
                          GROUP BY DOCUMENT_DTL
                      )";

                using (OracleDataAdapter daData = new OracleDataAdapter(dataSql, con))
                {
                    daData.SelectCommand.BindByName = true;
                    daData.SelectCommand.Parameters.Add("FL_NO", OracleDbType.Int32).Value = flNo;
                    daData.Fill(dtData);
                }

                if (dtData.Rows.Count == 0)
                {
                    gv.Visible = false;
                    lblApprovedInfo.Text = "No approved records found.";
                    return;
                }

                // 3) Pivot
                DataTable dtResult = BuildResultTable(dtCols, dtData);

                // 4) Sort logic
                string sortExp = "";
                bool hasEndDate = dtResult.Columns.Contains("END_DATE");
                bool hasDtlId = dtResult.Columns.Contains("DTL_ID");

                if (hasEndDate && hasDtlId) sortExp = "END_DATE DESC, DTL_ID DESC";
                else if (hasEndDate) sortExp = "END_DATE DESC";
                else if (hasDtlId) sortExp = "DTL_ID DESC";

                if (!string.IsNullOrEmpty(sortExp))
                {
                    DataView dv = dtResult.DefaultView;
                    dv.Sort = sortExp;
                    dtResult = dv.ToTable();
                }

                gv.Visible = true;
                lblApprovedInfo.Text = "";
                gv.DataSource = dtResult;
                gv.DataBind();
            }
        }

        // ---------------- PENDING (APP table) ----------------
        private void LoadPendingReport(int flNo)
        {
            string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();

                // 1) Dynamic columns
                DataTable dtCols = GetDynamicColumns(con, flNo);

                // 2) Data from APP table (PENDING only)
                DataTable dtData = new DataTable();
                string dataSql = @"
                    SELECT DOCUMENT_DTL, COL_NAME, DATAS
                    FROM NEXORA_LEGAL_PAPERS_DATA_APP
                    WHERE FL_TYPE = :FL_NO
                      AND STATUS = 'PENDING'
                    ORDER BY DOCUMENT_DTL";

                using (OracleDataAdapter daData = new OracleDataAdapter(dataSql, con))
                {
                    daData.SelectCommand.BindByName = true;
                    daData.SelectCommand.Parameters.Add("FL_NO", OracleDbType.Int32).Value = flNo;
                    daData.Fill(dtData);
                }

                if (dtData.Rows.Count == 0)
                {
                    gvPending.Visible = false;
                    lblPendingInfo.Text = "No pending records.";
                    return;
                }

                // 3) Pivot
                DataTable dtResult = BuildResultTable(dtCols, dtData);

                // 4) Sort logic
                if (dtResult.Columns.Contains("END_DATE"))
                {
                    DataView dv = dtResult.DefaultView;
                    dv.Sort = "END_DATE DESC";
                    dtResult = dv.ToTable();
                }

                gvPending.Visible = true;
                lblPendingInfo.Text = "";
                gvPending.DataSource = dtResult;
                gvPending.DataBind();
            }
        }

        // ---------------- Helper: get dynamic columns for a form ----------------
        private DataTable GetDynamicColumns(OracleConnection con, int flNo)
        {
            DataTable dtCols = new DataTable();

            string colSql = @"
                SELECT RPD.COLUMN_NAME
                FROM NEXORA_LEGAL_PAPERS_DATATYPE LPD
                JOIN NEXORA_REPORT_PAPER_DATATYPE RPD
                  ON LPD.DATATYPE_NO = RPD.SL_NO
                WHERE LPD.FL_NO = :FL_NO
                ORDER BY LPD.ORDER_NO";

            using (OracleDataAdapter daCols = new OracleDataAdapter(colSql, con))
            {
                daCols.SelectCommand.BindByName = true;
                daCols.SelectCommand.Parameters.Add("FL_NO", OracleDbType.Int32).Value = flNo;
                daCols.Fill(dtCols);
            }

            return dtCols;
        }

        // ---------------- Helper: pivot DOCUMENT_DTL + columns ----------------
        private DataTable BuildResultTable(DataTable dtCols, DataTable dtData)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("DOCUMENT_DTL");

            foreach (DataRow r in dtCols.Rows)
            {
                string colName = r["COLUMN_NAME"].ToString();
                if (!dtResult.Columns.Contains(colName))
                    dtResult.Columns.Add(colName);
            }

            DataView view = new DataView(dtData);
            DataTable dtDocs = view.ToTable(true, "DOCUMENT_DTL");

            foreach (DataRow doc in dtDocs.Rows)
            {
                string docId = doc["DOCUMENT_DTL"].ToString();
                DataRow newRow = dtResult.NewRow();
                newRow["DOCUMENT_DTL"] = docId;

                string safeDocId = docId.Replace("'", "''");
                DataRow[] rows = dtData.Select($"DOCUMENT_DTL = '{safeDocId}'");

                foreach (DataRow d in rows)
                {
                    string colName = d["COL_NAME"].ToString();
                    if (dtResult.Columns.Contains(colName))
                        newRow[colName] = d["DATAS"];
                }

                dtResult.Rows.Add(newRow);
            }
            return dtResult;
        }
        // ---------------- History Button Handler (APPROVED grid) ----------------
        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "History")
            {
                string docId = e.CommandArgument.ToString();

                LoadHistory(docId);
                
                int flNo = Convert.ToInt32(Session["SL_NO"]);
                LoadApprovedReport(flNo);
                LoadPendingReport(flNo);

                ClientScript.RegisterStartupScript(this.GetType(), "popupHistory", "openHistoryModal();", true);
            }
        }

        private void LoadHistory(string docId)
        {
            int flNo = Convert.ToInt32(Session["SL_NO"]);
            string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();

                // 1. GET COLUMNS
                DataTable dtCols = GetDynamicColumns(con, flNo);

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
                    da.SelectCommand.BindByName = true;
                    da.SelectCommand.Parameters.Add("DOC", OracleDbType.Varchar2).Value = docId;
                    da.SelectCommand.Parameters.Add("FL_NO", OracleDbType.Int32).Value = flNo;
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

                // 6. SORT LOGIC
                string historySort = "";
                bool hHasEndDate = dtResult.Columns.Contains("END_DATE");
                bool hHasDtlId = dtResult.Columns.Contains("DTL_ID");

                if (hHasEndDate && hHasDtlId) historySort = "END_DATE DESC, DTL_ID DESC";
                else if (hHasEndDate) historySort = "END_DATE DESC";
                else if (hHasDtlId) historySort = "DTL_ID DESC";

                if (!string.IsNullOrEmpty(historySort))
                {
                    DataView sortView = dtResult.DefaultView;
                    sortView.Sort = historySort;
                    dtResult = sortView.ToTable();
                }

                gvHistory.DataSource = dtResult;
                gvHistory.DataBind();
            }
        }

        // ---------------- RowDataBound: turn *_UPLOAD into Download link ----------------
        protected void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            GridView grid = (GridView)sender;
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null)
                return;

            DataTable table = drv.Row.Table;

            // Count how many TemplateFields are at the left (Action column in gvPending)
            int templateFieldCount = 0;
            foreach (DataControlField field in grid.Columns)
            {
                if (field is TemplateField)
                    templateFieldCount++;
                else
                    break; // assume any TemplateFields are at the start
            }

            // Loop through underlying data columns
            for (int colIndex = 0; colIndex < table.Columns.Count; colIndex++)
            {
                string columnName = table.Columns[colIndex].ColumnName;

                // Only care about *_UPLOAD columns
                if (!columnName.ToUpper().Contains("_UPLOAD"))
                    continue;

                // Determine which cell in the GridView row corresponds to this column
                int cellIndex = templateFieldCount + colIndex;
                if (cellIndex >= e.Row.Cells.Count)
                    continue;

                TableCell cell = e.Row.Cells[cellIndex];

                string filePath = Convert.ToString(drv[columnName])?.Trim();
                if (string.IsNullOrEmpty(filePath))
                    continue;

                // Make sure URL is correct
                string url = filePath;
                if (url.StartsWith("~"))
                {
                    // e.g. "~/media/file/..."
                    url = ResolveUrl(url);
                }
                else if (!url.StartsWith("/") &&
                         !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // e.g. "media/file/..." => make it app-relative
                    url = ResolveUrl("~/" + url.TrimStart('/'));
                }

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

        // ---------------- Approve button handler (PENDING grid) ----------------
        protected void gvPending_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ApproveDoc")
            {
                string documentDtl = e.CommandArgument.ToString();
                int flNo = Convert.ToInt32(Session["SL_NO"]);

                try
                {
                    ApproveDocument(flNo, documentDtl);
                    LoadPendingReport(flNo);
                    LoadApprovedReport(flNo);
                    lblPendingInfo.Text = "Document " + documentDtl + " approved.";
                }
                catch (Exception ex)
                {
                    lblPendingInfo.Text = "Error approving document: " + ex.Message;
                }
            }
            else if (e.CommandName == "RejectDoc")
            {
                string documentDtl = e.CommandArgument.ToString();
                int flNo = Convert.ToInt32(Session["SL_NO"]);

                try
                {
                    RejectDocument(flNo, documentDtl);
                    LoadPendingReport(flNo);
                    LoadApprovedReport(flNo);
                    lblPendingInfo.Text = "Document " + documentDtl + " rejected.";
                }
                catch (Exception ex)
                {
                    lblPendingInfo.Text = "Error rejecting document: " + ex.Message;
                }
            }
        }

        // ---------------- Approve logic: move from APP to DATA ----------------

        private string GenerateDTLID()
        {
            return "DTL-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private void ApproveDocument(int flNo, string documentDtl)
        {
            string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();
                OracleTransaction txn = con.BeginTransaction();

                try
                {
                    // Select from APP excluding CREATED_* fields
                    string selectSql = @"
                        SELECT FL_TYPE, COL_NAME, DATAS, DOCUMENT_DTL
                        FROM NEXORA_LEGAL_PAPERS_DATA_APP
                        WHERE FL_TYPE = :FL_TYPE
                          AND DOCUMENT_DTL = :DOCUMENT_DTL
                          AND STATUS = 'PENDING'
                          AND COL_NAME NOT IN (
                                'CREATED_BY_CODE',
                                'CREATED_BY_NAME',
                                'CREATED_LOCATION',
                                'CREATED_DATE'
                          )";

                    using (OracleCommand cmdSel = new OracleCommand(selectSql, con))
                    {
                        cmdSel.Transaction = txn;
                        cmdSel.BindByName = true;
                        cmdSel.Parameters.Add("FL_TYPE", OracleDbType.Int32).Value = flNo;
                        cmdSel.Parameters.Add("DOCUMENT_DTL", OracleDbType.Varchar2).Value = documentDtl;
                        string dtlId = GenerateDTLID();

                        using (OracleDataReader rdr = cmdSel.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                using (OracleCommand cmdIns = new OracleCommand(@"
                                    INSERT INTO NEXORA_LEGAL_PAPERS_DATA
                                        (FL_TYPE, COL_NAME, DATAS, DOCUMENT_DTL, DTL_ID)
                                    VALUES
                                        (:FL_TYPE, :COL_NAME, :DATAS, :DOCUMENT_DTL, :DTL_ID)
                                ", con))
                                {
                                    cmdIns.Transaction = txn;
                                    cmdIns.BindByName = true;
                                    cmdIns.Parameters.Add(":DTL_ID", dtlId);
                                    cmdIns.Parameters.Add("FL_TYPE", OracleDbType.Int32).Value = rdr["FL_TYPE"];
                                    cmdIns.Parameters.Add("COL_NAME", OracleDbType.Varchar2).Value = rdr["COL_NAME"];
                                    cmdIns.Parameters.Add("DATAS", OracleDbType.Varchar2).Value = rdr["DATAS"];
                                    cmdIns.Parameters.Add("DOCUMENT_DTL", OracleDbType.Varchar2).Value = rdr["DOCUMENT_DTL"];

                                    cmdIns.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Mark APP rows as APPROVED
                    using (OracleCommand cmdUpd = new OracleCommand(@"
                        UPDATE NEXORA_LEGAL_PAPERS_DATA_APP
                        SET STATUS = 'APPROVED'
                        WHERE FL_TYPE = :FL_TYPE
                          AND DOCUMENT_DTL = :DOCUMENT_DTL
                          AND STATUS = 'PENDING'
                    ", con))
                    {
                        cmdUpd.Transaction = txn;
                        cmdUpd.BindByName = true;
                        cmdUpd.Parameters.Add("FL_TYPE", OracleDbType.Int32).Value = flNo;
                        cmdUpd.Parameters.Add("DOCUMENT_DTL", OracleDbType.Varchar2).Value = documentDtl;
                        cmdUpd.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
                catch
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        private void RejectDocument(int flNo, string documentDtl)
        {
            string connStr = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;

            using (OracleConnection con = new OracleConnection(connStr))
            {
                con.Open();
                
                // Update status to REJECTED in staging table
                string sql = @"
                    UPDATE NEXORA_LEGAL_PAPERS_DATA_APP
                    SET STATUS = 'REJECTED'
                    WHERE FL_TYPE = :FL_NO
                      AND DOCUMENT_DTL = :DOC
                      AND (STATUS = 'PENDING' OR STATUS = 'APPROVED')";

                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("FL_NO", OracleDbType.Int32).Value = flNo;
                    cmd.Parameters.Add("DOC", OracleDbType.Varchar2).Value = documentDtl;
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}