<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="usermanagement.aspx.cs" Inherits="nexora.usermanagement" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Manage Users - NEXORA</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/assets/admindashboard/css/style.css") %>" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <style>
        .page-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
        }

        .btn-add-user {
            background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%);
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 8px;
            border: none;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .btn-add-user:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(79, 70, 229, 0.4);
        }

        /* User Grid Styling */
        .user-grid-container {
            background: rgba(255, 255, 255, 0.03);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 12px;
            padding: 20px;
            overflow-x: auto;
        }

        .user-table {
            width: 100%;
            border-collapse: collapse;
            color: #f8fafc;
        }

        .user-table th {
            text-align: left;
            padding: 15px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
            color: #94a3b8;
            font-weight: 500;
        }

        .user-table td {
            padding: 15px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
        }

        .privilege-badge {
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 11px;
            font-weight: 700;
            text-transform: uppercase;
        }

        .privilege-admin { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
        .privilege-user { background: rgba(59, 130, 246, 0.1); color: #3b82f6; }

        .status-active { color: #10b981; }
        .status-inactive { color: #ef4444; }

        /* Modal Overlay */
        .modal-overlay {
            position: fixed;
            inset: 0;
            background: rgba(0, 0, 0, 0.8);
            backdrop-filter: blur(4px);
            display: none; /* Hidden by default */
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }

        .modal-box {
            background: #1e293b;
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 16px;
            width: 100%;
            max-width: 500px;
            padding: 30px;
            color: white;
            position: relative;
        }

        .access-modal-box {
            max-width: 800px;
        }

        .modal-header {
            margin-bottom: 25px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .modal-close {
            background: none;
            border: none;
            color: #94a3b8;
            font-size: 24px;
            cursor: pointer;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            margin-bottom: 8px;
            color: #94a3b8;
            font-size: 14px;
        }

        .form-control {
            width: 100%;
            background: rgba(255, 255, 255, 0.05);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 8px;
            padding: 10px 14px;
            color: white;
            outline: none;
        }

        .form-control:focus {
            border-color: #4f46e5;
        }

        .modal-footer {
            margin-top: 30px;
            display: flex;
            justify-content: flex-end;
            gap: 12px;
        }

        /* Access Mapping Table in Modal */
        .access-table {
            width: 100%;
            margin-top: 20px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 8px;
        }

        .access-table th, .access-table td {
            padding: 10px;
            text-align: left;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
        }

        .action-btn {
            background: none;
            border: none;
            cursor: pointer;
            padding: 5px;
            transition: color 0.2s;
        }

        .btn-edit { color: #3b82f6; }
        .btn-access { color: #f59e0b; }
        .btn-view { color: #10b981; }
        .btn-delete { color: #ef4444; }

        /* ===== SEARCH BAR ===== */
        .search-container {
            position: relative;
            margin-bottom: 20px;
            max-width: 400px;
        }

        .search-container input {
            width: 100%;
            background: rgba(255, 255, 255, 0.05);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 12px;
            padding: 10px 18px 10px 45px;
            color: white;
            font-size: 14px;
            transition: all 0.3s;
        }

        .search-container input:focus {
            border-color: #3b82f6;
            background: rgba(255, 255, 255, 0.08);
        }

        .search-container i {
            position: absolute;
            left: 18px;
            top: 50%;
            transform: translateY(-50%);
            color: #64748b;
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <!-- NAVBAR -->
        <nav class="navbar">
            <div class="nav-logo">NEXORA</div>
            <ul class="nav-links">
                <li><a href="admindashboard.aspx">Dashboard</a></li>
                <li><a href="usermanagement.aspx" class="active">Users</a></li>
                <li><a href="adminallreports.aspx">Reports</a></li>
                <li><a href="adminsettings.aspx">Settings</a></li>
                <li><a href="logout.aspx" class="logout">Logout</a></li>
            </ul>
        </nav>

        <div class="container" style="margin-top: 40px;">
            <asp:UpdatePanel ID="upHiddenFields" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:HiddenField ID="hfSelectedEmpCode" runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="upHeader" runat="server">
                <ContentTemplate>
                    <div class="page-header">
                <h1 class="heading">Security & Access Management</h1>
                <div style="display: flex; gap: 10px;">
                    <button type="button" class="btn-add-user" style="background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%);" onclick="alert('System-wide access log feature coming soon!')">
                        <i class="fa-solid fa-list-check"></i> All Access
                    </button>
                    <asp:LinkButton ID="btnNewUser" runat="server" CssClass="btn-add-user" OnClick="btnNewUser_Click" CausesValidation="false">
                        <i class="fa-solid fa-user-plus"></i> New User
                    </asp:LinkButton>
                </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div class="search-container">
                <i class="fa-solid fa-user-gear"></i>
                <input type="text" id="txtSearchUsers" placeholder="Search by name, ID or handle..." onkeyup="filterUsers(this.value)" />
            </div>

            <div class="user-grid-container">
                <asp:UpdatePanel ID="upUserGrid" runat="server">
                    <ContentTemplate>
                        <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" 
                            CssClass="user-table" BorderStyle="None" GridLines="None"
                            OnRowCommand="gvUsers_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="EMP_CODE" HeaderText="Employee ID" />
                                <asp:BoundField DataField="EMP_NAME" HeaderText="Full Name" />
                                <asp:BoundField DataField="USERNAME" HeaderText="Login Handle" />
                                <asp:TemplateField HeaderText="Role">
                                    <ItemTemplate>
                                        <span class='<%# "privilege-badge privilege-" + Eval("PRIVILEGE").ToString().ToLower() %>'>
                                            <%# Eval("PRIVILEGE") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='<%# Eval("USR_ACTIVE").ToString() == "Y" ? "status-active" : "status-inactive" %>'>
                                            <i class='<%# Eval("USR_ACTIVE").ToString() == "Y" ? "fa-solid fa-circle-check" : "fa-solid fa-circle-xmark" %>'></i>
                                            <%# Eval("USR_ACTIVE").ToString() == "Y" ? "Active" : "Disabled" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="action-btn btn-edit" 
                                            CommandName="EditUser" CommandArgument='<%# Eval("EMP_CODE") %>' tooltip="Edit Profile">
                                            <i class="fa-solid fa-pen-to-square"></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnViewAccess" runat="server" CssClass="action-btn btn-view" 
                                            CommandName="ViewAccess" CommandArgument='<%# Eval("EMP_CODE") %>' tooltip="View Access Rights">
                                            <i class="fa-solid fa-eye"></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnAccess" runat="server" CssClass="action-btn btn-access" 
                                            CommandName="ManageAccess" CommandArgument='<%# Eval("EMP_CODE") %>' tooltip="Manage Permissions">
                                            <i class="fa-solid fa-shield-halved"></i>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <!-- INLINE ACCESS MANAGEMENT SECTION -->
            <asp:UpdatePanel ID="upAccessDetails" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnlAccessDetails" runat="server" CssClass="user-grid-container" style="margin-top: 30px; border-top: 4px solid #3b82f6; display: none;">
                        <div class="page-header" style="margin-bottom: 20px;">
                            <div style="display: flex; align-items: center; gap: 15px;">
                                <div>
                                    <h2 style="color: #3b82f6; margin-bottom: 5px;">Access Policy: <asp:Label ID="lblInlineUser" runat="server"></asp:Label></h2>
                                    <p style="color: #64748b; font-size: 14px;">Define granular report and location permissions for this operative.</p>
                                </div>
                                <div style="background: rgba(59, 130, 246, 0.1); border: 1px solid rgba(59, 130, 246, 0.3); padding: 4px 12px; border-radius: 20px; font-size: 12px; color: #60a5fa;">
                                    <i class="fa-solid fa-fingerprint"></i> <asp:Literal ID="litActiveEmpCode" runat="server" Text="No User Selected"></asp:Literal>
                                </div>
                            </div>
                            <asp:LinkButton ID="btnCloseInline" runat="server" OnClick="btnCloseInline_Click" style="color: #94a3b8; font-size: 20px;"><i class="fa-solid fa-rectangle-xmark"></i></asp:LinkButton>
                        </div>

                        <div style="background: rgba(255,255,255,0.02); padding: 25px; border-radius: 12px; margin-bottom: 25px; border: 1px solid rgba(255,255,255,0.05);">
                            <div style="display: grid; grid-template-columns: 1fr 1fr 1fr auto; gap: 20px; align-items: end;">
                                <div class="form-group" style="margin-bottom:0;">
                                    <label>Active Report Catalog</label>
                                    <asp:DropDownList ID="ddlReportsInline" runat="server" CssClass="form-control" style="background: #0b1120; border-color: #334155;"></asp:DropDownList>
                                </div>
                                <div class="form-group" style="margin-bottom:0;">
                                    <label>Geographic Binding (Location)</label>
                                    <asp:TextBox ID="txtLocationInline" runat="server" CssClass="form-control" placeholder="GLOBAL / HQ / BRANCH" />
                                </div>
                                <div class="form-group" style="margin-bottom:0;">
                                    <label>Resource Path</label>
                                    <asp:TextBox ID="txtTablePathInline" runat="server" CssClass="form-control" Text="~/users/userreportingdata.aspx" />
                                </div>
                                <asp:Button ID="btnGrantInline" runat="server" Text="Initialize Access" CssClass="btn-add-user" style="padding: 10px 24px;" OnClick="btnGrantInline_Click" />
                            </div>
                            <asp:Label ID="lblInlineError" runat="server" ForeColor="#ef4444" style="display:block; margin-top:10px; font-size:13px;"></asp:Label>
                        </div>

                        <asp:GridView ID="gvUserAccessInline" runat="server" AutoGenerateColumns="False" 
                            CssClass="user-table" BorderStyle="None" GridLines="None"
                            OnRowCommand="gvUserAccessInline_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="REPORT_NAME" HeaderText="Security Report" />
                                <asp:BoundField DataField="LOCATION_ID" HeaderText="Allocated Site" />
                                <asp:BoundField DataField="TABLE_NAME" HeaderText="Internal Routing" />
                                <asp:TemplateField HeaderText="Authorization">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnRevokeInline" runat="server" CssClass="action-btn btn-delete" 
                                            CommandName="DeleteAccess" CommandArgument='<%# Eval("REPORT_ID").ToString() + "|" + Eval("LOCATION_ID").ToString() %>'
                                            OnClientClick="return confirm('Immediately revoke this authorization level?');">
                                            <i class="fa-solid fa-user-shield"></i> Revoke
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="gvUsers" EventName="RowCommand" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <!-- USER MODAL (ADD / EDIT) -->
        <div id="userModal" class="modal-overlay">
            <asp:UpdatePanel ID="upUserModal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="modal-box">
                <div class="modal-header">
                    <h2>User Configuration</h2>
                    <button type="button" class="modal-close" onclick="closeUserModal()">&times;</button>
                </div>
                <div class="form-group">
                    <label>Employee Code <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="txtEmpCode" runat="server" CssClass="form-control" required="required" placeholder="e.g. USER001" />
                </div>
                <div class="form-group">
                    <label>Full Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" required="required" placeholder="Full Employee Name" />
                </div>
                <div class="form-group">
                    <label>Username (Login) <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="txtLoginName" runat="server" CssClass="form-control" required="required" />
                </div>
                <div class="form-group">
                    <label>Credentials (Password) <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" required="required" />
                </div>
                <div class="form-group">
                    <label>Privilege Level</label>
                    <asp:DropDownList ID="ddlPrivilege" runat="server" CssClass="form-control" style="background: #0f172a;">
                        <asp:ListItem Text="Standard User" Value="USER" />
                        <asp:ListItem Text="Administrator" Value="ADMIN" />
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Account State</label>
                    <asp:CheckBox ID="chkIsActive" runat="server" Text=" Active & Enabled" Checked="true" ForeColor="#94a3b8" />
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" onclick="closeUserModal()" style="padding: 10px 20px; border-radius: 8px; border:none; background:#334155; color:white;">Cancel</button>
                    <asp:Button ID="btnSaveUser" runat="server" Text="Commit Changes" CssClass="btn-add-user" OnClick="btnSaveUser_Click" />
                </div>
                    <asp:HiddenField ID="hfMode" runat="server" Value="ADD" />
                        </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <!-- ACCESS MODAL (MANAGE PERMISSIONS) -->
        <div id="accessModal" class="modal-overlay">
            <asp:UpdatePanel ID="upModalAccess" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="modal-box access-modal-box">
                        <div class="modal-header" style="border-bottom: 1px solid rgba(255,255,255,0.1); padding-bottom: 15px;">
                            <div>
                                <h2>Access Control Policies</h2>
                                <small style="color: #64748b;">Managing access for: <asp:Label ID="lblAccessUser" runat="server" Font-Bold="true" ForeColor="#3b82f6"></asp:Label></small>
                            </div>
                            <button type="button" class="modal-close" onclick="closeAccessModal()">&times;</button>
                        </div>
                
                <div style="background: rgba(255,255,255,0.02); padding: 20px; border-radius: 12px; margin-bottom: 20px;">
                    <div style="display: grid; grid-template-columns: 1fr 1fr 1fr auto; gap: 15px; align-items: end;">
                        <div class="form-group" style="margin-bottom:0;">
                            <label>Report</label>
                            <asp:DropDownList ID="ddlReports" runat="server" CssClass="form-control" style="background: #0b1120;"></asp:DropDownList>
                        </div>
                        <div class="form-group" style="margin-bottom:0;">
                            <label>Specific Location</label>
                            <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control" placeholder="e.g. HEAD OFFICE" />
                        </div>
                        <div class="form-group" style="margin-bottom:0;">
                            <label>Platform Path</label>
                            <asp:TextBox ID="txtTablePath" runat="server" CssClass="form-control" Text="~/users/userreportingdata.aspx" />
                        </div>
                        <asp:Button ID="btnAddAccess" runat="server" Text="Grant" CssClass="btn-add-user" style="padding: 8px 16px;" OnClick="btnAddAccess_Click" />
                    </div>
                </div>

                <div class="search-container" style="margin: 0 0 15px 0;">
                    <i class="fa-solid fa-shield-halved"></i>
                    <input type="text" id="txtSearchAccess" placeholder="Filter permissions..." onkeyup="filterAccess(this.value)" />
                </div>

                <div class="user-grid-container" style="max-height: 400px; overflow-y: auto; padding: 0;">
                    <asp:GridView ID="gvUserAccess" runat="server" AutoGenerateColumns="False" 
                        CssClass="user-table" BorderStyle="None" GridLines="None"
                        OnRowCommand="gvUserAccess_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="REPORT_NAME" HeaderText="Associated Report" />
                            <asp:BoundField DataField="LOCATION_ID" HeaderText="Assigned Location" />
                            <asp:BoundField DataField="TABLE_NAME" HeaderText="Target Page" />
                            <asp:TemplateField HeaderText="Revoke">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnRevoke" runat="server" CssClass="action-btn btn-delete" 
                                        CommandName="DeleteAccess" CommandArgument='<%# Eval("REPORT_ID").ToString() + "|" + Eval("LOCATION_ID").ToString() %>'
                                        OnClientClick="return confirm('Immediately revoke this permission?');">
                                        <i class="fa-solid fa-trash-can"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

    </form>

    <script>
        function filterUsers(query) {
            query = query.toLowerCase();
            const table = document.getElementById('<%= gvUsers.ClientID %>');
            if (!table) return;
            const rows = table.getElementsByTagName('tr');
            for (let i = 1; i < rows.length; i++) {
                const text = rows[i].textContent.toLowerCase();
                rows[i].style.display = text.includes(query) ? '' : 'none';
            }
        }

        function filterAccess(query) {
            query = query.toLowerCase();
            const table = document.getElementById('<%= gvUserAccess.ClientID %>');
            if (!table) return;
            const rows = table.getElementsByTagName('tr');
            for (let i = 1; i < rows.length; i++) {
                const text = rows[i].textContent.toLowerCase();
                rows[i].style.display = text.includes(query) ? '' : 'none';
            }
        }

        function openUserModal() {
            document.getElementById('userModal').style.display = 'flex';
        }

        function closeUserModal() {
            document.getElementById('userModal').style.display = 'none';
        }

        function openAccessModal() {
            document.getElementById('accessModal').style.display = 'flex';
        }

        function closeAccessModal() {
            document.getElementById('accessModal').style.display = 'none';
        }

        // Close on escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                closeUserModal();
                closeAccessModal();
            }
        });
    </script>
</body>
</html>
