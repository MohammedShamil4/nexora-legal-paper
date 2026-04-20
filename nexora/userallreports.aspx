<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="userallreports.aspx.cs" Inherits="nexora.userallreports" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">

    <head runat="server">
        <title>REPORTS</title>
        <link rel="stylesheet" href="<%= ResolveUrl("~/assets/admindashboard/css/style.css") %>" />

        <!-- SPLINE VIEWER -->
        <script type="module" src="https://unpkg.com/@splinetool/viewer@1.12.46/build/spline-viewer.js">
        </script>

        <style>
            /* Optional: Spline background style */
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
                border: 1px solid #ccc;
                font-size: 14px;
                outline: none;
            }

            .search-input:focus {
                border-color: #4f46e5;
                box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.2);
            }
        </style>
    </head>

    <body>
        <form id="form1" runat="server">
            <!-- NAVBAR -->
            <nav class="navbar">
                <div class="nav-logo">NEXORA</div>
                <ul class="nav-links">
                    <li><a href="admindashboard.aspx">DashBoard</a></li>
                    <li><a href="userallreports.aspx" class="active">Reports</a></li>
                    <li><a href="login.aspx" class="logout">Logout</a></li>
                </ul>
            </nav>

            <!-- MAIN CONTAINER -->
            <div class="container">

                <!-- SPLINE SECTION -->
                <div class="spline-wrapper">
                    <spline-viewer url="https://prod.spline.design/27aAdvI7DkBTzVcJ/scene.splinecode">
                    </spline-viewer>
                </div>

                <div class="dashboard-header">
                    <h1 class="heading">REPORTS</h1>

                    <input type="text" id="searchBox" class="search-input" placeholder="Search..."
                        onkeyup="filterCards()" />
                </div>

                <div class="box-container">
                    <!-- Static Form Creation Card -->
                    <div class="box">
                        <img src='<%= ResolveUrl("~/assets/admindashboard/image/file6.png") %>' alt="icon" />
                        <h3 class="card-title">Form Creation</h3>
                        <p>Form creation</p>
                        <a href='<%= ResolveUrl("~/users/usercreationform.aspx") %>' class="btn">Open</a>
                    </div>
                    <asp:Repeater ID="rptReports" runat="server">
                        <ItemTemplate>
                            <div class="box">
                                <img src='<%= ResolveUrl("~/assets/admindashboard/image/file3.png") %>' alt="icon" />
                                <h3 class="card-title">
                                    <%# Eval("REPORT_NAME") %>
                                </h3>
                                <p>Click to access this report</p>
                                <asp:LinkButton ID="lnkReadMore" runat="server" CssClass="btn" Text="View data"
                                    CommandArgument='<%# Eval("REPORT_ID") %>' OnClick="lnkReadMore_Click"
                                    CausesValidation="false" UseSubmitBehavior="false" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoReports" runat="server" Visible="false" CssClass="no-reports">
                        <div class="box">
                            <img src='<%= ResolveUrl("~/assets/admindashboard/image/file5.png") %>' alt="icon" />
                            <h3 class="card-title">No reports assigned</h3>
                            <p>No reports assigned. Please contact administrator.</p>
                            <a href="userallreports.aspx" class="btn">Open</a>
                        </div>
                    </asp:Panel>
                    <div class="box">
                        <img src='<%= ResolveUrl("~/assets/admindashboard/image/file5.png") %>' alt="icon" />
                        <h3 class="card-title">Coming soon .........</h3>
                        <p>Coming soon .........</p>
                        <a href="userallreports.aspx" class="btn">Open</a>
                    </div>
                    <div class="box">
                        <img src='<%= ResolveUrl("~/assets/admindashboard/image/file5.png") %>' alt="icon" />
                        <h3 class="card-title">Coming soon .........</h3>
                        <p>Coming soon .........</p>
                        <a href="userallreports.aspx" class="btn">Open</a>
                    </div>
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