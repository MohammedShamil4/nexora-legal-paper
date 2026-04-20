using System;

namespace nexora
{
    public partial class logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Clear all session data
            Session.Clear();
            Session.Abandon();

            // Redirect to login page
            Response.Redirect("login.aspx");
        }
    }
}