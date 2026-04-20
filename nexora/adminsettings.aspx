<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="adminsettings.aspx.cs" Inherits="nexora.adminsettings" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title>Settings - NEXORA</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/assets/admindashboard/css/style.css") %>" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

        <style>
        body {
            background: radial-gradient(circle at 20% 20%, rgba(255,255,255,0.05), transparent 40%),
                        radial-gradient(circle at 80% 30%, rgba(255,255,255,0.04), transparent 40%), 
                        radial-gradient(circle at 50% 80%, rgba(255,255,255,0.03), transparent 40%), 
                        linear-gradient(135deg, #0a0a0a, #151515, #0a0a0a);
            color: #ffffff;
            font-family: 'Poppins', sans-serif;
            min-height: 100vh;
        }

        /* ===== Navbar Highlight Symmetry ===== */
        .nav-links li a::after {
            content: '';
            position: absolute;
            left: 0;
            bottom: -6px;
            width: 0;
            height: 2px;
            background: #00ffa2; /* Dashboard Green Accent */
            transition: 0.3s;
        }
        .nav-links li a:hover::after, .nav-links li a.active::after { width: 100%; }

        .container {
            padding: 25px 5% 100px;
            background: rgba(255,255,255,0.01);
            backdrop-filter: blur(12px);
        }

        .main-content {
            max-width: 1200px;
            margin: 0 auto;
        }

        .page-header {
            margin-bottom: 30px;
            border-bottom: 1px solid rgba(255,255,255,0.08);
            padding-bottom: 20px;
        }

        .heading {
            font-size: 32px;
            font-weight: 600;
            margin: 0;
        }

        /* ===== TAB NAVIGATION (MODERNIZED) ===== */
        .tabs-bar {
            display: flex;
            gap: 15px;
            margin-bottom: 40px;
            background: rgba(255, 255, 255, 0.04);
            padding: 12px;
            border-radius: 16px;
            border: 1px solid rgba(255, 255, 255, 0.08);
        }

        .tab-btn {
            color: #94a3b8;
            padding: 12px 24px;
            border-radius: 10px;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            gap: 10px;
            border: 1px solid transparent;
            background: rgba(255, 255, 255, 0.02);
            text-decoration: none;
        }

        .tab-btn:hover {
            background: rgba(255, 255, 255, 0.08);
            color: #f8fafc;
        }

        .tab-btn.active {
            background: rgba(255, 255, 255, 0.12);
            color: #00ffa2; /* Match Dashboard Green */
            border: 1px solid rgba(0, 255, 162, 0.3);
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
        }

        /* ===== GLASS CARD (Dashboard Box Style) ===== */
        .glass-card {
            background: rgba(255,255,255,0.08);
            backdrop-filter: blur(14px);
            border-radius: 20px;
            padding: 35px;
            border: 1px solid rgba(255,255,255,0.15);
            box-shadow: 0 12px 40px rgba(0,0,0,0.6);
            margin-bottom: 35px;
        }

        .section-title {
            font-size: 22px;
            font-weight: 600;
            color: #f8fafc;
            margin: 0;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        /* ===== DATA GRID ===== */
        .custom-grid {
            width: 100%;
            border-collapse: collapse;
            margin-top: 25px;
            color: #e2e8f0;
        }

        .custom-grid th {
            text-align: left;
            padding: 18px;
            border-bottom: 2px solid rgba(255, 255, 255, 0.1);
            color: #94a3b8;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.08em;
        }

        .custom-grid td {
            padding: 18px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
            font-size: 14px;
        }

        .custom-grid tr:hover {
            background: rgba(255, 255, 255, 0.03);
        }

        .badge {
            padding: 5px 12px;
            border-radius: 8px;
            font-size: 11px;
            font-weight: 700;
        }

        .badge-active { background: rgba(16, 185, 129, 0.15); color: #34d399; border: 1px solid rgba(52, 211, 153, 0.3); }
        .badge-inactive { background: rgba(239, 68, 68, 0.15); color: #f87171; border: 1px solid rgba(248, 113, 113, 0.3); }

        .action-btn {
            background: rgba(255,255,255,0.05);
            border: 1px solid rgba(255,255,255,0.1);
            color: #94a3b8;
            cursor: pointer;
            padding: 10px;
            border-radius: 10px;
            transition: all 0.2s;
            margin-right: 8px;
        }

        /* ===== SEARCH BAR ===== */
        .search-container {
            position: relative;
            margin-bottom: 15px;
            max-width: 400px;
        }

        .search-container input {
            width: 100%;
            background: rgba(255, 255, 255, 0.04);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 12px;
            padding: 12px 18px 12px 45px;
            color: white;
            font-size: 14px;
            transition: all 0.3s;
        }

        .search-container input:focus {
            border-color: #00ffa2;
            background: rgba(255, 255, 255, 0.08);
            box-shadow: 0 0 15px rgba(0, 255, 162, 0.1);
        }

        .search-container i {
            position: absolute;
            left: 18px;
            top: 50%;
            transform: translateY(-50%);
            color: #64748b;
            font-size: 14px;
        }

        .btn-edit:hover { background: rgba(59, 130, 246, 0.1); color: #3b82f6; border-color: #3b82f6; }
        .btn-delete:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; border-color: #ef4444; }

            /* Dropdown specific optimization */
            select.form-control {
                background-color: #1e293b !important;
            }

            select.form-control option {
                background-color: #1e293b;
                color: #fff;
            }

        /* ===== MODAL ===== */
        .modal-overlay {
            position: fixed;
            inset: 0;
            background: rgba(0, 0, 0, 0.88);
            backdrop-filter: blur(10px);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 2000;
        }

        .modal-box {
            background: #0f172a;
            border: 1px solid rgba(255, 255, 255, 0.18);
            border-radius: 28px;
            width: 100%;
            max-width: 580px;
            padding: 45px;
            box-shadow: 0 30px 60px rgba(0, 0, 0, 0.9);
        }

        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }

        .modal-close-btn {
            color: #64748b !important;
            font-size: 28px !important;
            transition: all 0.2s !important;
            line-height: 1 !important;
            cursor: pointer !important;
            text-decoration: none !important;
        }

        .modal-close-btn:hover {
            color: #f8fafc !important;
            transform: scale(1.1);
        }

        .form-control {
            width: 100%;
            background: rgba(255, 255, 255, 0.04);
            border: 1px solid rgba(255, 255, 255, 0.12);
            border-radius: 12px;
            padding: 14px 18px;
            color: white;
            margin-bottom: 25px;
            outline: none;
            transition: border-color 0.2s;
        }

        .form-control:focus { border-color: #00ffa2; }

        label {
            display: block;
            margin-bottom: 10px;
            color: #94a3b8;
            font-size: 13px;
            font-weight: 500;
        }

        .btn-primary {
            background: rgba(0, 255, 162, 0.1);
            color: #00ffa2;
            border: 1px solid rgba(0, 255, 162, 0.3);
            padding: 14px 30px;
            border-radius: 40px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.4s;
        }

        .btn-primary:hover {
            background: #00ffa2;
            color: #000;
            box-shadow: 0 0 25px rgba(0, 255, 162, 0.5);
            transform: translateY(-2px);
        }

        .icon-required {
            color: #3b82f6;
            font-size: 1.1rem;
        }

        .icon-optional {
            color: #64748b;
            font-size: 1.1rem;
        }

        .logout {
            background: crimson;
            padding: 6px 14px;
            border-radius: 6px;
            color: white !important;
            font-weight: 600;
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
       <asp:ScriptManager ID="sm" runat="server" />
        <!-- NAVBAR -->
        <nav class="navbar">
            <div class="nav-logo">NEXORA</div>
            <ul class="nav-links">
                <li><a href="admindashboard.aspx">Dashboard</a></li>
                <li><a href="usermanagement.aspx">Users</a></li>
                <li><a href="adminallreports.aspx">Reports</a></li>
                <li><a href="adminsettings.aspx" class="active">Settings</a></li>
                <li><a href="logout.aspx" class="logout">Logout</a></li>
            </ul>
        </nav>

        <div class="main-content container">
            <div class="page-header">
                <div>
                    <h1 style="font-size: 32px; font-weight: 800; margin: 0; color: #f8fafc;">Infrastructure Configuration</h1>
                    <div style="color: #64748b; margin-top: 5px;">Managing metadata for Legal Compliance engine.</div>
                </div>
            </div>

            <!-- HORIZONTAL TABS BAR -->
            <div class="tabs-bar">
                <asp:HiddenField ID="hfActiveTab" runat="server" Value="reports" />
                
                <asp:LinkButton ID="btnTabReports" runat="server" CssClass="tab-btn active" OnClick="TabChange_Click" CommandArgument="reports">
                    <i class="fa-solid fa-file-invoice"></i> Reports Master
                </asp:LinkButton>
                
                <asp:LinkButton ID="btnTabFields" runat="server" CssClass="tab-btn" OnClick="TabChange_Click" CommandArgument="fields">
                    <i class="fa-solid fa-database"></i> Data Dictionary
                </asp:LinkButton>
                
                <asp:LinkButton ID="btnTabBindings" runat="server" CssClass="tab-btn" OnClick="TabChange_Click" CommandArgument="bindings">
                    <i class="fa-solid fa-link"></i> Structure Binding
                </asp:LinkButton>

                <div style="flex-grow: 1;"></div>
                <a href="admindashboard.aspx" class="tab-btn" style="border-color: rgba(255,255,255,0.05);">
                    <i class="fa-solid fa-circle-left"></i> Dashboard
                </a>
            </div>

            <!-- REPORTS MASTER TAB -->
                <asp:UpdatePanel ID="upReports" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlReports" runat="server" Visible="true">
                            <div class="glass-card">
                                <div class="page-header">
                                    <h2 class="section-title"><i class="fa-solid fa-file-circle-check"></i> Legal Master Files</h2>
                                    <asp:Button ID="btnNewReport" runat="server" Text="+ New Report Path" CssClass="btn-primary" OnClick="btnNewReport_Click" />
                                </div>
                                <div class="search-container">
                                    <i class="fa-solid fa-search"></i>
                                    <input type="text" id="txtSearchReports" placeholder="Filter by name or path..." onkeyup="filterGridView('gvReports', this.value)" />
                                </div>
                                <asp:GridView ID="gvReports" runat="server" AutoGenerateColumns="false" CssClass="custom-grid" GridLines="None"
                                    OnRowCommand="gvReports_RowCommand">
                                    <Columns>
                                        <asp:BoundField DataField="SL_NO" HeaderText="ID" />
                                        <asp:BoundField DataField="NAME" HeaderText="Display Name" />
                                        <asp:BoundField DataField="TABLE_NAME" HeaderText="Platform Route" />
                                        <asp:TemplateField HeaderText="Status">
                                            <ItemTemplate>
                                                <span class='<%# "badge " + (Convert.ToString(Eval("ACTIVE")) == "Y" ? "badge-active" : "badge-inactive") %>'>
                                                    <%# Convert.ToString(Eval("ACTIVE")) == "Y" ? "ONLINE" : "OFFLINE" %>
                                                </span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:LinkButton runat="server" CommandName="EditReport" CommandArgument='<%# Eval("SL_NO") %>' CssClass="action-btn btn-edit"><i class="fa-solid fa-pen-nib"></i></asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="DeleteReport" CommandArgument='<%# Eval("SL_NO") %>' CssClass="action-btn btn-delete" OnClientClick="return confirm('Archive this report path?');"><i class="fa-solid fa-trash"></i></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <!-- FIELD DICTIONARY TAB -->
                <asp:UpdatePanel ID="upFields" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlFields" runat="server" Visible="false">
                            <div class="glass-card">
                                <div class="page-header">
                                    <h2 class="section-title"><i class="fa-solid fa-list-check"></i> Field Components</h2>
                                    <asp:Button ID="btnNewField" runat="server" Text="+ Register Datatype" CssClass="btn-primary" OnClick="btnNewField_Click" />
                                </div>
                                <div class="search-container">
                                    <i class="fa-solid fa-search"></i>
                                    <input type="text" id="txtSearchFields" placeholder="Filter by column name..." onkeyup="filterGridView('gvFields', this.value)" />
                                </div>
                                <asp:GridView ID="gvFields" runat="server" AutoGenerateColumns="false" CssClass="custom-grid" GridLines="None"
                                    OnRowCommand="gvFields_RowCommand">
                                    <Columns>
                                        <asp:BoundField DataField="SL_NO" HeaderText="ID" />
                                        <asp:BoundField DataField="COLUMN_NAME" HeaderText="Internal Identifier" />
                                        <asp:BoundField DataField="DATA_TYPE" HeaderText="Logical Type" />
                                        <asp:BoundField DataField="TABLE_NAME" HeaderText="Source Metadata" />
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:LinkButton runat="server" CommandName="EditField" CommandArgument='<%# Eval("SL_NO") %>' CssClass="action-btn btn-edit"><i class="fa-solid fa-pen-nib"></i></asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="DeleteField" CommandArgument='<%# Eval("SL_NO") %>' CssClass="action-btn btn-delete" OnClientClick="return confirm('Immediately remove this data definition? This may fail if it is currently bound to reports.');"><i class="fa-solid fa-trash"></i></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <!-- BINDING MAP TAB -->
                <asp:UpdatePanel ID="upBindings" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlBindings" runat="server" Visible="false">
                            <div class="glass-card">
                                <div class="page-header">
                                    <h2 class="section-title"><i class="fa-solid fa-layer-group"></i> Structure Bindings</h2>
                                    <asp:Button ID="btnNewBinding" runat="server" Text="+ Map Field to Report" CssClass="btn-primary" OnClick="btnNewBinding_Click" />
                                </div>
                                <div class="search-container">
                                    <i class="fa-solid fa-search"></i>
                                    <input type="text" id="txtSearchBindings" placeholder="Filter by report or field..." onkeyup="filterGridView('gvBindings', this.value)" />
                                </div>
                                <asp:GridView ID="gvBindings" runat="server" AutoGenerateColumns="false" CssClass="custom-grid" GridLines="None"
                                    OnRowCommand="gvBindings_RowCommand">
                                    <Columns>
                                        <asp:BoundField DataField="REPORT_NAME" HeaderText="Report Container" />
                                        <asp:BoundField DataField="FIELD_IDENTIFIER" HeaderText="Component" />
                                        <asp:BoundField DataField="ORDER_NO" HeaderText="Sequence" />
                                        <asp:TemplateField HeaderText="Required">
                                            <ItemTemplate>
                                                <i class='<%# (Convert.ToString(Eval("MANDATORY")) == "Y" ? "fa-solid fa-circle-check icon-required" : "fa-solid fa-circle-minus icon-optional") %>'></i>
                                                <span style="font-size: 11px; margin-left: 5px; opacity: 0.8; vertical-align: middle;">
                                                    <%# Convert.ToString(Eval("MANDATORY")) == "Y" ? "REQUIRED" : "OPTIONAL" %>
                                                </span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:LinkButton runat="server" CommandName="EditBinding" CommandArgument='<%# Eval("FL_NO").ToString() + "|" + Eval("DATATYPE_NO").ToString() %>' CssClass="action-btn btn-edit"><i class="fa-solid fa-pen-nib"></i></asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="DeleteBinding" CommandArgument='<%# Eval("FL_NO").ToString() + "|" + Eval("DATATYPE_NO").ToString() %>' CssClass="action-btn btn-delete" OnClientClick="return confirm('Cut this structural link?');"><i class="fa-solid fa-scissors"></i></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
        </div>

        <!-- SHARED MODALS OVERLAY (Simple reveal logic) -->
        <asp:UpdatePanel ID="upModals" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hfSelectedID" runat="server" />
                <div id="modalOverlay" class="modal-overlay" runat="server" visible="false">
                    <div class="modal-box">
                        <div class="modal-header">
                            <h2 style="margin: 0; color: #00ffa2;"><asp:Literal ID="litModalTitle" runat="server"></asp:Literal></h2>
                            <asp:LinkButton ID="btnCloseModal" runat="server" OnClick="btnCloseModal_Click" CssClass="modal-close-btn">&times;</asp:LinkButton>
                        </div>
                        <div style="margin-top: 25px;">
                            <asp:MultiView ID="mvForms" runat="server">
                                <!-- REPORT FORM -->
                                <asp:View ID="vwReport" runat="server">
                                    <label>Report Display Name <span style="color: #ef4444;">*</span></label>
                                    <asp:TextBox ID="txtReportName" runat="server" CssClass="form-control" required="required" placeholder="e.g. Daily Sales Summary" />
                                    <label>Platform Path (~/reports/...) <span style="color: #ef4444;">*</span></label>
                                    <asp:TextBox ID="txtReportPath" runat="server" CssClass="form-control" required="required" placeholder="~/reports/sales_report.aspx" />
                                    <label>FontAwesome Icon Class</label>
                                    <asp:TextBox ID="txtReportIcon" runat="server" CssClass="form-control" />
                                    <asp:CheckBox ID="chkReportActive" runat="server" Text=" Active & Publicly Available" Checked="true" style="margin-bottom: 20px; display: block;" />
                                    <asp:Button ID="btnSaveReport" runat="server" Text="Commit Changes" CssClass="btn-primary" style="width: 100%;" OnClick="btnSaveReport_Click" />
                                </asp:View>

                                <!-- FIELD FORM -->
                                <asp:View ID="vwField" runat="server">
                                    <label>Internal Column Identifier <span style="color: #ef4444;">*</span></label>
                                    <asp:TextBox ID="txtFieldName" runat="server" CssClass="form-control" required="required" placeholder="e.g. OUTLET_NAME" />
                                    <label>Logical Data Type</label>
                                    <asp:DropDownList ID="ddlFieldType" runat="server" CssClass="form-control">
                                        <asp:ListItem Text="TEXT (Basic String)" Value="TEXT" />
                                        <asp:ListItem Text="VARCHAR2 (Long String)" Value="VARCHAR2" />
                                        <asp:ListItem Text="NUMBER (Numeric Value)" Value="NUMBER" />
                                        <asp:ListItem Text="DATE (Calendar)" Value="DATE" />
                                        <asp:ListItem Text="DROP DOWN (Lookup)" Value="DROP DOWN" />
                                    </asp:DropDownList>
                                    <label>Lookup Source Table (Optional)</label>
                                    <asp:TextBox ID="txtFieldSource" runat="server" CssClass="form-control" placeholder="e.g. nexora_outlet_master" />
                                    <asp:Button ID="btnSaveField" runat="server" Text="Update Library" CssClass="btn-primary" style="width: 100%;" OnClick="btnSaveField_Click" />
                                </asp:View>

                                <!-- BINDING FORM -->
                                <asp:View ID="vwBinding" runat="server">
                                    <label>Parent Report</label>
                                    <asp:DropDownList ID="ddlBindReport" runat="server" CssClass="form-control"></asp:DropDownList>
                                    <label>Component field</label>
                                    <asp:DropDownList ID="ddlBindField" runat="server" CssClass="form-control"></asp:DropDownList>
                                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                                        <div>
                                            <label>Sort Order</label>
                                            <asp:TextBox ID="txtBindOrder" runat="server" CssClass="form-control" TextMode="Number" />
                                        </div>
                                        <div style="padding-top: 35px;">
                                            <asp:CheckBox ID="chkBindMandatory" runat="server" Text=" Mandatory field" Checked="true" />
                                        </div>
                                    </div>
                                    <asp:Button ID="btnSaveBinding" runat="server" Text="Establish Link" CssClass="btn-primary" style="width: 100%;" OnClick="btnSaveBinding_Click" />
                                </asp:View>
                            </asp:MultiView>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

    </form>

    <script>
        // Real-time Grid Filtering
        function filterGridView(gridId, query) {
            query = query.toLowerCase();
            
            const tables = document.querySelectorAll('.custom-grid');
            let targetTable = null;
            
            tables.forEach(t => {
                if (t.id && t.id.endsWith(gridId)) targetTable = t;
            });
 
            if (!targetTable) return;
 
            const rows = targetTable.getElementsByTagName('tr');
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                if (row.classList.contains('header-row')) continue;
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(query) ? '' : 'none';
            }
        }

        // Handled via Server-Side logic mostly, but local hotkeys are good
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                // Since modalOverlay is toggled via server-side Visible property,
                // we just check if btnCloseModal exists in the DOM.
                var btnClose = document.querySelector('.modal-close-btn');
                if (btnClose) {
                    btnClose.click();
                }
            }
        });
    </script>
</body>
</html>
