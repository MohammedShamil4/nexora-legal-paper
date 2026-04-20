<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="usercreationform.aspx.cs" Inherits="nexora.users.usercreationform" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Creation Form</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="stylesheet" href="<%= ResolveUrl("~/assets/admindashboard/css/style.css") %>" />

    <style>
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

        /* =========================
           FORM CONTAINER
        ========================= */
        .container {
            max-width: 900px;
            margin: 40px auto;
            padding: 20px;
        }

        .container h1 {
            font-size: 26px;
            margin-bottom: 25px;
            color: #ffffff;
        }

        .box {
            margin-bottom: 25px;
        }

        .form-label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #dddddd;
        }

        .dropdown {
            width: 100%;
            padding: 10px 12px;
            font-size: 15px;
            border-radius: 6px;
            border: 1px solid rgba(255,255,255,0.25);
            background-color: #141414;
            color: #ffffff;
        }

        .CARD {
            background-color: #111111;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 6px 18px rgba(0, 0, 0, 0.6);
            margin-bottom: 25px;
            border: 1px solid rgba(255,255,255,0.08);
        }

        .dynamic-form {
            display: grid;
            grid-template-columns: 1fr;
            gap: 20px;
        }

        .dynamic-form input,
        .dynamic-form select,
        .dynamic-form textarea {
            width: 100%;
            padding: 10px 12px;
            font-size: 14px;
            border-radius: 6px;
            background-color: #1a1a1a;
            color: #ffffff;
            border: 1px solid rgba(255,255,255,0.2);
        }

        .dynamic-form input:focus,
        .dynamic-form select:focus,
        .dynamic-form textarea:focus {
            border-color: #00ffa2;
            outline: none;
        }

        .CARDform-actions {
            display: flex;
            justify-content: flex-end;
            gap: 10px;
        }

        .btn {
            padding: 10px 22px;
            font-size: 15px;
            border-radius: 6px;
            border: none;
            cursor: pointer;
        }

        .btn-primary {
            background-color: #4f46e5;
            color: #fff;
        }

        .btn-primary:hover {
            background-color: #4338ca;
        }

        .btn-secondary {
            background-color: #333;
            color: #fff;
        }

        .btn-secondary:hover {
            background-color: #444;
        }
    </style>
</head>

<body>
<form id="form1" runat="server">

    <!-- NAVBAR -->
    <nav class="navbar">
        <div class="nav-logo">NEXORA</div>
        <ul class="nav-links">
            <li><a href="../userdashboard.aspx">Dashboard</a></li>
            <li><a href="#" class="active">Forms</a></li>
            <li><a href="../logout.aspx" class="logout">Logout</a></li>
        </ul>
    </nav>

    <!-- MAIN CONTAINER -->
    <div class="container">
        <h1>Welcome, <asp:Label ID="lblUserName" runat="server"></asp:Label></h1>
        <div class="welcome-section">
                <p>Access your assigned reports and forms below. Location: <asp:Label ID="lblLocation" runat="server"></asp:Label></p>
            </div>

            
        <!-- MESSAGE PANEL -->
        <asp:Panel ID="pnlMessage" runat="server" Visible="false">
            <asp:Label ID="lblMessage" runat="server" CssClass="alert"></asp:Label>
        </asp:Panel>

        <!-- SELECT FORM -->
        <div class="box">
            <label class="form-label">Select Form Type</label>
            <asp:DropDownList ID="ddlFormType" runat="server"
                CssClass="dropdown"
                AutoPostBack="true"
                OnSelectedIndexChanged="ddlFormType_SelectedIndexChanged" />
        </div>

        <!-- DYNAMIC FORM -->
        <div class="CARD">
            <div class="dynamic-form">
                <asp:PlaceHolder ID="phDynamicForm" runat="server"></asp:PlaceHolder>
            </div>
        </div>

        <!-- BUTTONS -->
        <div class="CARDform-actions">
            
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
        </div>
    </div>
</form>
</body>
</html>