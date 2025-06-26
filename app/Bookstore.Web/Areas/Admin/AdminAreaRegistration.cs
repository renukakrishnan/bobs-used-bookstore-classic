using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bookstore.Web.Areas.Admin
{
    public static class AdminAreaRegistration
    {
        public static string AreaName => "Admin";

        public static void RegisterAdminArea(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapAreaControllerRoute(
                name: "Admin_default",
                areaName: "Admin",
                pattern: "Admin/{controller=Home}/{action=Index}/{id?}",
                defaults: new { area = "Admin" }
            );
        }
    }
}