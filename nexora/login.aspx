<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="nexora.login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
  <title>Admin Free Code </title>

  <script src="https://cdn.tailwindcss.com"></script>

  <link rel="preconnect" href="https://fonts.googleapis.com"/>
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;700&display=swap" rel="stylesheet"/>

   <link href="<%= ResolveUrl("~/assets/css/login.css") %>" rel="stylesheet" />

</head>
<body class="text-gray-200">

  <form id="form1" runat="server">

    <canvas id="background-canvas"></canvas>

    <main class="min-h-screen flex items-center justify-center p-4">

      <div class="w-full max-w-md p-8 space-y-6 rounded-2xl shadow-2xl glass-card">

        <div class="text-center">
          <h1 class="text-3xl font-bold text-white tracking-tight">
            <span class="brand">
              <span class="brand-main">NEXORA</span>
              <sup class="brand-sup">ERP</sup>
            </span>
          </h1>
        </div>

        <!-- LOGIN FORM CONTENT -->
        <div class="space-y-6">

          <div>
            <div class="relative">
              <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                <svg class="h-5 w-5 text-gray-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                  <path d="M10 8a3 3 0 100-6 3 3 0 000 6zM3.465 14.493a1.23 1.23 0 00.41 1.412A9.957 9.957 0 0010 18c2.31 0 4.438-.784 6.131-2.095a1.23 1.23 0 00.41-1.412A9.992 9.992 0 0010 12a9.992 9.992 0 00-6.535 2.493z" />
                </svg>
              </div>

              <asp:TextBox
                ID="txtEmail"
                runat="server"
                CssClass="form-input block w-full rounded-lg border-0 bg-white/5 py-3 pl-10 text-white ring-1 ring-inset ring-white/10 focus:ring-2 focus:ring-cyan-500"
                Placeholder="admin@mail.com" />
            </div>
          </div>

          <div>
            <div class="relative">
              <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                <svg class="h-5 w-5 text-gray-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M10 1a4.5 4.5 0 00-4.5 4.5V9H5a2 2 0 00-2 2v6a2 2 0 002 2h10a2 2 0 002-2v-6a2 2 0 00-2-2h-.5V5.5A4.5 4.5 0 0010 1z" clip-rule="evenodd" />
                </svg>
              </div>

              <asp:TextBox
                ID="txtPassword"
                runat="server"
                TextMode="Password"
                CssClass="form-input block w-full rounded-lg border-0 bg-white/5 py-3 pl-10 text-white ring-1 ring-inset ring-white/10 focus:ring-2 focus:ring-cyan-500"
                Placeholder="Password" />
            </div>
          </div>

          <asp:Button
            ID="btnLogin"
            runat="server"
            Text="Log in"
            CssClass="w-full rounded-lg bg-gradient-to-r from-cyan-500 to-blue-600 px-4 py-3 text-sm font-semibold text-white hover:scale-105 transition"
            OnClick="btnLogin_Click" />

          <asp:Label
            ID="lblMessage"
            runat="server"
            CssClass="block text-center text-red-400" />

        </div>

      </div>
    </main>

  </form>

<script src="<%= ResolveUrl("~/assets/js/login.js") %>"></script>


   
</body>
</html>