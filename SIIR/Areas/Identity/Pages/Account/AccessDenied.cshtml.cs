// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;
using SIIR.Utilities;

namespace SIIR.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public void OnGet()
        {
        }

        public string GetDashboardUrl()
        {
            if (User.IsInRole(CNT.AdminRole))
                return "/Admin/Home";
            if (User.IsInRole(CNT.CoachRole))
                return "/Coach/Home";
            if (User.IsInRole(CNT.StudentRole))
                return "/Student/Home";
            return "/";
        }
    }
}
