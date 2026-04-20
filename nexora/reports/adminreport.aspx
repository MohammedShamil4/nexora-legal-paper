<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="adminreport.aspx.cs" Inherits="nexora.reports.adminreport" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Admin Report</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/assets/reports/css/style.css") %>" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <style>
        /* ===== MODERN THEME OVERRIDE ===== */
        body {
            background: radial-gradient(circle at 10% 10%, rgba(0, 255, 162, 0.05), transparent 35%),
                        radial-gradient(circle at 90% 90%, rgba(59, 130, 246, 0.05), transparent 35%),
                        linear-gradient(135deg, #050505 0%, #0f172a 100%);
            color: #f8fafc;
            font-family: 'Poppins', sans-serif;
            margin: 0;
            min-height: 100vh;
            overflow-x: hidden;
        }

        .glass-bg {
            padding: 40px 5%;
            animation: fadeIn 0.8s ease-out;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }

        /* ===== ACTION BAR ===== */
        .action-bar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
            background: rgba(255, 255, 255, 0.03);
            padding: 20px 30px;
            border-radius: 20px;
            border: 1px solid rgba(255, 255, 255, 0.08);
            backdrop-filter: blur(10px);
        }

        .report-title {
            font-size: 24px;
            font-weight: 700;
            background: linear-gradient(135deg, #fff 0%, #94a3b8 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin: 0;
        }

        /* ===== GRIDVIEW STYLING ===== */
        .glass-table {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0 8px;
            margin-top: 10px;
        }

        .glass-table th {
            background: transparent !important;
            color: #64748b !important;
            font-size: 12px;
            text-transform: uppercase;
            letter-spacing: 0.1em;
            padding: 15px 20px;
            border: none !important;
            text-align: left;
        }

        .glass-table tr {
            background: rgba(255, 255, 255, 0.03) !important;
            border-radius: 12px;
            transition: all 0.3s ease;
        }

        .glass-table tr:hover:not(.header-row) {
            background: rgba(255, 255, 255, 0.07) !important;
            transform: scale(1.005);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .glass-table td {
            padding: 18px 20px !important;
            border-top: 1px solid rgba(255, 255, 255, 0.05) !important;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05) !important;
            border-left: none !important;
            border-right: none !important;
            font-size: 14px;
            color: #e2e8f0 !important;
        }

        .glass-table td:first-child {
            border-left: 1px solid rgba(255, 255, 255, 0.05) !important;
            border-top-left-radius: 12px;
            border-bottom-left-radius: 12px;
        }

        .glass-table td:last-child {
            border-right: 1px solid rgba(255, 255, 255, 0.05) !important;
            border-top-right-radius: 12px;
            border-bottom-right-radius: 12px;
        }

        /* ===== ACTION PILLS ===== */
        .btn-approve, .btn-history {
            padding: 8px 20px;
            border-radius: 30px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            transition: all 0.3s;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            border: 1px solid transparent;
            text-decoration: none;
        }

        .btn-approve {
            background: rgba(0, 255, 162, 0.1);
            color: #00ffa2;
            border-color: rgba(0, 255, 162, 0.2);
        }

        .btn-approve:hover {
            background: #00ffa2;
            color: #000;
            box-shadow: 0 0 20px rgba(0, 255, 162, 0.4);
        }

        .btn-history {
            background: rgba(59, 130, 246, 0.1);
            color: #3b82f6;
            border-color: rgba(59, 130, 246, 0.2);
        }

        .btn-history:hover {
            background: #3b82f6;
            color: #fff;
            box-shadow: 0 0 20px rgba(59, 130, 246, 0.4);
        }

        .btn-reject {
            background: rgba(239, 68, 68, 0.1);
            color: #ef4444;
            border: 1px solid rgba(239, 68, 68, 0.2);
            padding: 8px 20px;
            border-radius: 30px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            transition: all 0.3s;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            text-decoration: none;
        }

        .btn-reject:hover {
            background: #ef4444;
            color: #fff;
            box-shadow: 0 0 20px rgba(239, 68, 68, 0.4);
        }

        .btn-download {
            background: rgba(45, 212, 191, 0.1);
            color: #2dd4bf;
            border: 1px solid rgba(45, 212, 191, 0.2);
            padding: 8px 20px;
            border-radius: 30px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            transition: all 0.3s;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            text-decoration: none;
        }

        .btn-download:hover {
            background: #2dd4bf;
            color: #000;
            box-shadow: 0 0 20px rgba(45, 212, 191, 0.4);
        }

        .btn-group {
            display: flex;
            gap: 12px;
            align-items: center;
            flex-wrap: nowrap;
        }

        /* ===== MODAL REINVENTED ===== */
        .modal-overlay {
            position: fixed;
            inset: 0;
            background: rgba(0, 0, 0, 0.85);
            backdrop-filter: blur(18px);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 9999;
            animation: modalFade 0.3s ease-out;
        }

        @keyframes modalFade {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        .modal-content {
            background: #0f172a;
            padding: 40px;
            border-radius: 30px;
            width: 98%;
            max-width: 1600px;
            max-height: 85vh;
            overflow: auto;
            border: 1px solid rgba(255, 255, 255, 0.12);
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7);
            position: relative;
            transform-origin: center;
            animation: modalSlide 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
        }

        @keyframes modalSlide {
            from { transform: scale(0.9) translateY(30px); opacity: 0; }
            to { transform: scale(1) translateY(0); opacity: 1; }
        }

        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.08);
        }

        .modal-close {
            color: #64748b;
            font-size: 28px;
            transition: color 0.2s;
            line-height: 1;
            cursor: pointer;
        }

        .modal-close:hover { color: #f8fafc; }

        /* Custom Scrollbar */
        ::-webkit-scrollbar { width: 8px; }
        ::-webkit-scrollbar-track { background: rgba(255, 255, 255, 0.02); }
        ::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
        ::-webkit-scrollbar-thumb:hover { background: rgba(255, 255, 255, 0.2); }
        /* ===== SEARCH CONTAINER ===== */
        .search-container {
            position: relative;
            width: 300px;
        }

        .search-container input {
            width: 100%;
            background: rgba(255, 255, 255, 0.05);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 12px;
            padding: 12px 15px 12px 45px;
            color: #f8fafc;
            font-size: 14px;
            outline: none;
            transition: all 0.3s;
        }

        .search-container input:focus {
            background: rgba(255, 255, 255, 0.08);
            border-color: #00ffa2;
            box-shadow: 0 0 20px rgba(0, 255, 162, 0.15);
        }

        .search-container i {
            position: absolute;
            left: 18px;
            top: 50%;
            transform: translateY(-50%);
            color: #64748b;
            font-size: 14px;
        }

        @media (max-width: 768px) {
            .action-bar { flex-direction: column; gap: 20px; align-items: stretch; }
            .search-container { width: 100%; }
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <!-- NAVBAR -->
    <nav class="navbar">
        <div class="nav-logo">NEXORA</div>
        <ul class="nav-links">
            <li><a href="../admindashboard.aspx" class="active">HOME</a></li>
            <!-- Opens pending-data modal -->
            <li><a href="#" onclick="openDataModal(); return false;">VIEW PENDING</a></li>
            <li><a href="../logout.aspx" class="logout">Logout</a></li>
        </ul>
    </nav>

    <!-- MAIN CONTENT -->
    <div class="glass-bg">
        <div class="action-bar">
            <div class="left-section">
                <h1 class="report-title">Operational Intelligence Portal</h1>
                <div style="color: #64748b; font-size: 14px;">Logged in as: <span style="color: #00ffa2;"><%= Session["USERNAME"] %></span></div>
            </div>
            
            <div class="search-container">
                <i class="fa-solid fa-magnifying-glass"></i>
                <input type="text" id="txtSearch" placeholder="Filter records..." onkeyup="filterGrid('gv', this.value)" />
            </div>
        </div>

        <asp:Label ID="lblApprovedInfo" runat="server" CssClass="msg" style="display:block; margin-bottom:15px; color: #94a3b8;" />
        <asp:GridView ID="gv"
                      runat="server"
                      CssClass="glass-table"
                      AutoGenerateColumns="true"
                      BorderWidth="0"
                      GridLines="None"
                      DataKeyNames="DOCUMENT_DTL"
                      OnRowDataBound="Grid_RowDataBound"
                      OnRowCommand="gv_RowCommand">
            <HeaderStyle CssClass="header-row" />
            <Columns>
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <div class="btn-group">
                            <asp:LinkButton
                                runat="server"
                                CssClass="btn-history" 
                                CommandName="History"
                                CommandArgument='<%# Eval("DOCUMENT_DTL") %>'>
                                <i class="fa-solid fa-clock-rotate-left"></i> History
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <!-- MODAL OVERLAY WITH PENDING GRID -->
    <div id="dataModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2>Pending Submissions</h2>
                <span class="modal-close" onclick="closeDataModal()">&times;</span>
            </div>

            <asp:Label ID="lblPendingInfo" runat="server" CssClass="msg" />

            <asp:GridView ID="gvPending"
                          runat="server"
                          CssClass="glass-table"
                          AutoGenerateColumns="true"
                          BorderWidth="1"
                          GridLines="Both"
                          DataKeyNames="DOCUMENT_DTL"
                          OnRowDataBound="Grid_RowDataBound"
                          OnRowCommand="gvPending_RowCommand">
                <Columns>
                    <%-- Approve button column --%>
                    <asp:TemplateField HeaderText="Administrative Action">
                        <ItemTemplate>
                            <div class="btn-group">
                                <asp:LinkButton ID="btnApprove"
                                                runat="server"
                                                CssClass="btn-approve"
                                                CommandName="ApproveDoc"
                                                CommandArgument='<%# Eval("DOCUMENT_DTL") %>'>
                                                <i class="fa-solid fa-circle-check"></i> Authorize
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnRejectPending"
                                                runat="server"
                                                CssClass="btn-reject"
                                                CommandName="RejectDoc"
                                                CommandArgument='<%# Eval("DOCUMENT_DTL") %>'
                                                OnClientClick="return confirm('Are you sure you want to reject this submission?');">
                                                <i class="fa-solid fa-circle-xmark"></i> Reject
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- MODAL OVERLAY FOR HISTORY -->
    <div id="historyModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2>History</h2>
                <span class="modal-close" onclick="closeHistoryModal()">&times;</span>
            </div>
            
            <asp:GridView ID="gvHistory"
                          runat="server"
                          CssClass="glass-table"
                          AutoGenerateColumns="true"
                          BorderWidth="1"
                          GridLines="Both"
                          OnRowDataBound="Grid_RowDataBound">
            </asp:GridView>
        </div>
    </div>
<script>
    // Global Grid Filtering
    function filterGrid(gridId, query) {
        query = query.toLowerCase();
        // Get the table. GridView with ID 'gv' usually has a client ID ending in 'gv'
        const tables = document.querySelectorAll('.glass-table');
        let targetTable = null;
        
        tables.forEach(t => {
            if (t.id.endsWith(gridId)) targetTable = t;
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

        // Show modal
    function openDataModal() {
        document.getElementById('dataModal').style.display = 'flex';
    }

    // Hide modal
    function closeDataModal() {
        document.getElementById('dataModal').style.display = 'none';
    }

    // Show History Modal
    function openHistoryModal() {
        document.getElementById('historyModal').style.display = 'flex';
    }

    // Hide History Modal
    function closeHistoryModal() {
        document.getElementById('historyModal').style.display = 'none';
    }

    // Close when clicking outside content
    window.onclick = function (event) {
        var modal = document.getElementById('dataModal');
        if (event.target === modal) {
            closeDataModal();
        }
        var hModal = document.getElementById('historyModal');
        if (event.target === hModal) {
            closeHistoryModal();
        }
    };

    // If you want it to auto-open on page load, uncomment:
    // window.onload = function () {
    //     openDataModal();
    // };
</script>
</form>
</body>
</html>