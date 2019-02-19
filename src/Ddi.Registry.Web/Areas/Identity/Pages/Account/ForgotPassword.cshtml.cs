using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Ddi.Registry.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using Ddi.Registry.Web.Models;

namespace Ddi.Registry.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }


        [BindProperty(Name = "g-recaptcha-response")]
        public string RecaptchaResponse { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await ValidateRecaptcha();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }


        private async Task ValidateRecaptcha()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.google.com/recaptcha/api/siteverify");
                var paramaters = new Dictionary<string, string>();
                paramaters["secret"] = _configuration["recaptcha:ReCaptchaPrivateKey"];
                paramaters["response"] = RecaptchaResponse;
                paramaters["remoteip"] = HttpContext.Connection.RemoteIpAddress.ToString();
                request.Content = new FormUrlEncodedContent(paramaters);

                HttpClient client = new HttpClient();
                var resp = await client.SendAsync(request);
                resp.EnsureSuccessStatusCode();

                var responseText = await resp.Content.ReadAsStringAsync();
                var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaValidationResponse>(responseText);
                if (!recaptchaResponse.Success)
                {
                    ModelState.AddModelError("reCAPTCHA", "invalid reCAPTCHA " + string.Join(":", recaptchaResponse.ErrorCodes));
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("reCAPTCHA", "invalid reCAPTCHA");
            }
        }
    }
}
