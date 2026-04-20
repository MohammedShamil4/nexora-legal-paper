<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="userdashboard.aspx.cs" Inherits="nexora.userdashboard" %>

    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">

    <head runat="server">
        <title>User Dashboard | Nexora</title>

        <link rel="stylesheet" href="<%= ResolveUrl("~/assets/admindashboard/css/style.css") %>" />

        <!-- SPLINE VIEWER -->
        <script type="module" src="https://unpkg.com/@splinetool/viewer@1.12.46/build/spline-viewer.js">
        </script>

        <!-- FONT AWESOME -->
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

        <style>
            .spline-wrapper {
                width: 100%;
                height: 350px;
                margin-bottom: 30px;
                border-radius: 16px;
                overflow: hidden;
            }

            spline-viewer {
                width: 100%;
                height: 100%;
            }

            .dashboard-header {
                display: flex;
                align-items: center;
                justify-content: space-between;
                margin-bottom: 20px;
            }

            .search-input {
                width: 250px;
                padding: 10px 14px;
                border-radius: 8px;
                border: 1px solid rgba(255, 255, 255, 0.1);
                background: rgba(255, 255, 255, 0.05);
                color: white;
                font-size: 14px;
                outline: none;
                backdrop-filter: blur(5px);
            }

            .search-input:focus {
                border-color: #00cca2;
                box-shadow: 0 0 0 2px rgba(0, 204, 162, 0.2);
            }

            /* Summary Widgets */
            .summary-container {
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
                gap: 20px;
                margin-bottom: 20px;
            }

            .stat-box {
                background: rgba(255, 255, 255, 0.05);
                backdrop-filter: blur(10px);
                border: 1px solid rgba(255, 255, 255, 0.1);
                border-radius: 12px;
                padding: 20px;
                display: flex;
                justify-content: space-between;
                align-items: center;
                transition: transform 0.3s ease;
            }

            .stat-box:hover {
                transform: translateY(-5px);
                background: rgba(255, 255, 255, 0.08);
            }

            .stat-label {
                display: block;
                font-size: 14px;
                color: #94a3b8;
                margin-bottom: 5px;
            }

            .stat-value {
                font-size: 28px;
                font-weight: 700;
                color: #f8fafc;
            }

            .stat-icon {
                font-size: 32px;
                opacity: 0.8;
            }

            .stat-box.pending {
                border-left: 4px solid #f59e0b;
            }

            .stat-box.approved {
                border-left: 4px solid #00cca2;
            }

            .card-stats {
                display: flex;
                gap: 10px;
                margin: 15px 0;
                font-size: 13px;
            }

            .c-stat {
                flex: 1;
                padding: 8px;
                border-radius: 6px;
                display: flex;
                flex-direction: column;
                align-items: center;
                background: rgba(255, 255, 255, 0.03);
            }

            .c-stat span {
                font-size: 11px;
                color: #94a3b8;
                margin-bottom: 2px;
            }

            .pending-tag strong {
                color: #fbbf24;
            }

            .approved-tag strong {
                color: #00cca2;
            }

            .expiry-tag.expired strong {
                color: #ef4444;
            }

            .expiry-tag.urgent strong {
                color: #f59e0b;
            }

            .expiry-tag.safe strong {
                color: #94a3b8;
            }

            .expiry-tag.n-a strong {
                color: #475569;
            }

            /* Welcome Message */
            .welcome-section {
                margin-bottom: 30px;
            }

            .welcome-section h1 {
                font-size: 28px;
                color: #fff;
                margin-bottom: 5px;
            }

            .welcome-section p {
                color: #94a3b8;
                font-size: 16px;
            }
        </style>
    </head>

    <body>
        <form id="form1" runat="server">
            <!-- NAVBAR -->
            <nav class="navbar">
                <div class="nav-logo">NEXORA</div>
                <ul class="nav-links">
                    <li><a href="userdashboard.aspx" class="active">Home</a></li>
                    <li><a href="userallreports.aspx">My Reports</a></li>
                    <li><a href="login.aspx" class="logout">Logout</a></li>
                </ul>
            </nav>

            <!-- MAIN CONTAINER -->
            <div class="container">

                <!-- WELCOME -->
                <div class="welcome-section">
                    <h1>Welcome, <asp:Literal ID="litUserName" runat="server">User</asp:Literal>!</h1>
                    <p>Access your assigned operational reports and track submission status below.</p>
                </div>

                <!-- SPLINE SECTION -->
                <div class="spline-wrapper">
                    <spline-viewer url="https://prod.spline.design/HECf2aQl5plWEMfK/scene.splinecode">
                    </spline-viewer>
                </div>

                <!-- SUMMARY STATS -->
                <div class="summary-container">
                    <div class="stat-box pending">
                        <div class="stat-info">
                            <span class="stat-label">My Pending Submissions</span>
                            <asp:Label ID="lblTotalPending" runat="server" CssClass="stat-value">0</asp:Label>
                        </div>
                        <div class="stat-icon"><i class="fa-solid fa-hourglass-half"></i></div>
                    </div>
                    <div class="stat-box approved">
                        <div class="stat-info">
                            <span class="stat-label">My Approved Records</span>
                            <asp:Label ID="lblTotalApproved" runat="server" CssClass="stat-value">0</asp:Label>
                        </div>
                        <div class="stat-icon"><i class="fa-solid fa-clipboard-check"></i></div>
                    </div>
                </div>

                <div class="dashboard-header" style="margin-top: 40px;">
                    <h1 class="heading">My Accessible Reports</h1>

                    <input type="text" id="searchBox" class="search-input" placeholder="Search your reports..."
                        onkeyup="filterCards()" />
                </div>

                <div class="box-container">
                    <asp:Repeater ID="rptReports" runat="server">
                        <ItemTemplate>
                            <div class="box">
                                <img src='<%# ResolveUrl("~/assets/admindashboard/image/file5.png") %>' alt="icon" />
                                <h3 class="card-title">
                                    <%# Eval("NAME") %>
                                </h3>

                                <div class="card-stats">
                                    <div class="c-stat pending-tag">
                                        <span>Pending:</span>
                                        <strong>
                                            <%# Eval("PENDING_COUNT") %>
                                        </strong>
                                    </div>
                                    <div class="c-stat approved-tag">
                                        <span>Approved:</span>
                                        <strong>
                                            <%# Eval("APPROVED_COUNT") %>
                                        </strong>
                                    </div>
                                    <div class='<%# "c-stat expiry-tag " + GetExpiryClass(Eval("DAYS_LEFT")) %>'>
                                        <span>Timeline:</span>
                                        <strong>
                                            <%# GetExpiryText(Eval("DAYS_LEFT")) %>
                                        </strong>
                                    </div>
                                </div>

                                <asp:LinkButton ID="lnkOpenReport" runat="server" CssClass="btn"
                                    CommandArgument='<%# Eval("SL_NO") %>' OnClick="lnkOpenReport_Click">View Report
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

            </div>
        </form>
        <script>
            function filterCards() {
                let input = document.getElementById("searchBox").value.toLowerCase();
                let cards = document.getElementsByClassName("box");

                for (let i = 0; i < cards.length; i++) {
                    let title = cards[i].getElementsByClassName("card-title")[0];
                    let text = title.innerText.toLowerCase();

                    cards[i].style.display = text.includes(input) ? "block" : "none";
                }
            }
        </script>
    </body>

    </html>