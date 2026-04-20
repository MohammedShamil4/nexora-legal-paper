<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="userreportingdata.aspx.cs" Inherits="nexora.users.userreportingdata" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Report</title>

    <link rel="stylesheet" href="<%= ResolveUrl("~/assets/reports/css/style.css") %>" />

    <style>
        /* ================= MODAL ================= */
        .modal {
            position: fixed;
            top: 0; left: 0;
            width: 100%; height: 100%;
            background: rgba(0,0,0,0.7);
            display: none;
            z-index: 9999;
        }

        .modal-content {
            background: #111;
            width: 85%;
            margin: 5% auto;
            padding: 20px;
            border-radius: 10px;
            color: white;
        }

        .close {
            float: right;
            font-size: 24px;
            cursor: pointer;
        }

        /* ================= BUTTONS ================= */
        .grid-btn {
            padding: 6px 14px;
            border-radius: 6px;
            border: none;
            font-size: 13px;
            cursor: pointer;
            transition: all 0.3s ease;
            font-weight: 500;
            text-decoration: none;
            display: inline-block;
        }

        .grid-btn-edit {
            background: linear-gradient(135deg, #4CAF50, #2e7d32);
            color: #fff;
        }

        .grid-btn-edit:hover {
            transform: scale(1.05);
        }

        .grid-btn-update {
            background: #ff9800;
            color: #fff;
        }

        .grid-btn-cancel {
            background: #e53935;
            color: #fff;
        }

        .grid-btn-history {
            background: linear-gradient(135deg, #2196F3, #0d47a1);
            color: #fff;
        }

        /* ================= INPUT ================= */
        .grid-input {
            width: 100%;
            padding: 6px 10px;
            border-radius: 6px;
            border: 1px solid rgba(255,255,255,0.2);
            background: rgba(255,255,255,0.05);
            color: #fff;
            font-size: 13px;
            outline: none;
            transition: all 0.3s ease;
        }

        .grid-input:focus {
            border-color: #2196F3;
            box-shadow: 0 0 6px rgba(33,150,243,0.5);
        }

        .grid-input::placeholder {
            color: rgba(255,255,255,0.5);
        }

        /* DOWNLOAD BUTTON */
        .btn-download {
            padding: 5px 10px;
            background: linear-gradient(135deg, #9c27b0, #6a1b9a);
            color: #fff !important;
            border-radius: 5px;
            text-decoration: none;
            font-size: 12px;
            transition: 0.3s;
        }

        .btn-download:hover {
            background: linear-gradient(135deg, #ba68c8, #8e24aa);
        }

        /* =========================
           SUCCESS/ERROR MESSAGES
        ========================= */
        .alert {
            padding: 15px 20px;
            border-radius: 6px;
            margin-bottom: 20px;
            font-weight: 500;
            display: block;
        }

        .alert-success {
            background-color: rgba(0, 255, 162, 0.15);
            border: 1px solid #00ffa2;
            color: #00ffa2;
        }

        .alert-error {
            background-color: rgba(255, 77, 77, 0.15);
            border: 1px solid #ff4d4d;
            color: #ff4d4d;
        }
    </style>

    <script>
        function openModal() {
            document.getElementById('<%= pnlHistory.ClientID %>').style.display = 'block';
        }

        function closeModal() {
            document.getElementById('<%= pnlHistory.ClientID %>').style.display = 'none';
        }
    </script>

</head>

<body>

<form id="form1" runat="server" enctype="multipart/form-data">

    
    <nav class="navbar">
        <div class="nav-logo">NEXORA</div>
        <ul class="nav-links">
            <li><a href="../userdashboard.aspx">Dashboard</a></li>
            <li><a href="#" class="active">Forms</a></li>
            <li><a href="../logout.aspx" class="logout">Logout</a></li>
        </ul>
    </nav>

    <br /><br />
    
    <div style="max-width: 90%; margin: 0 auto;">
         <!-- MESSAGE PANEL -->
        <asp:Panel ID="pnlMessage" runat="server" Visible="false">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert"></asp:Label>
        </asp:Panel>
    </div>

    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    
    <asp:GridView
        ID="gv"
        runat="server"
        AutoGenerateColumns="true"
        CssClass="glass-table"
        DataKeyNames="DOCUMENT_DTL"
        OnRowEditing="gv_RowEditing"
        OnRowCancelingEdit="gv_RowCancelingEdit"
        OnRowUpdating="gv_RowUpdating"
        OnRowDataBound="gv_RowDataBound"
        OnRowCommand="gv_RowCommand">

        <Columns>

           
            <asp:TemplateField HeaderText="Actions">
                <ItemTemplate>
                    <asp:LinkButton 
                        runat="server"
                        CommandName="Edit"
                        Text="Edit"
                        CssClass="grid-btn grid-btn-edit" />
                </ItemTemplate>

                <EditItemTemplate>
                    <asp:LinkButton 
                        runat="server"
                        CommandName="Update"
                        Text="Update"
                        CssClass="grid-btn grid-btn-update" />

                    &nbsp;

                    <asp:LinkButton 
                        runat="server"
                        CommandName="Cancel"
                        Text="Cancel"
                        CssClass="grid-btn grid-btn-cancel" />
                </EditItemTemplate>
            </asp:TemplateField>

            
            <asp:TemplateField HeaderText="History">
                <ItemTemplate>
                    <asp:Button
                        runat="server"
                        Text="History"
                        CssClass="grid-btn grid-btn-history"
                        CommandName="History"
                        CommandArgument="<%# Container.DataItemIndex %>" />
                </ItemTemplate>
            </asp:TemplateField>

        </Columns>

    </asp:GridView>

   
    <asp:Panel ID="pnlHistory" runat="server" CssClass="modal">
        <div class="modal-content">
            <span class="close" onclick="closeModal()">&times;</span>
            <h2>History</h2>

            <asp:GridView
                ID="gvHistory"
                runat="server"
                AutoGenerateColumns="true"
                OnRowDataBound="gv_RowDataBound"
                CssClass="glass-table" />
        </div>
    </asp:Panel>

</form>

</body>
</html>