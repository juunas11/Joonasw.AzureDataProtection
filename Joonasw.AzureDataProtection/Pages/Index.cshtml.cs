using System;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Joonasw.AzureDataProtection.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDataProtector _dataProtector;

        public IndexModel(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Test");
        }

        public string ReadValue { get; set; }
        public string WrittenValue { get; set; }

        public void OnGet()
        {
            const string CookieName = "DataProtectionTestCookie";
            if (Request.Cookies.TryGetValue(CookieName, out var cookieValue))
            {
                ReadValue = Encoding.UTF8.GetString(_dataProtector.Unprotect(Convert.FromBase64String(cookieValue)));
            }
            else
            {
                var value = $"Data written at {DateTime.Now}";
                cookieValue = Convert.ToBase64String(_dataProtector.Protect(Encoding.UTF8.GetBytes(value)));
                Response.Cookies.Append(CookieName, cookieValue, new CookieOptions
                {
                    IsEssential = true
                });
                WrittenValue = value;
            }
        }
    }
}
