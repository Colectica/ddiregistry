using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Ddi.Registry.Web.Models;
using Microsoft.Web.Helpers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Net.Http;

namespace Ddi.Registry.Web.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            return View();
        }

        public ActionResult Confirmation()
        {
            return View();
        }

        public ActionResult Verify(string ID)
        {
            if (string.IsNullOrEmpty(ID) || (!Regex.IsMatch(ID, @"[0-9a-f]{8}\-([0-9a-f]{4}\-){3}[0-9a-f]{12}")))
            {
                TempData["tempMessage"] = "The user account is not valid. Please try clicking the link in your email again.";
                return View();
            }

            else
            {
                MembershipUser user = MembershipService.GetUser(new Guid(ID));

                if (!user.IsApproved)
                {
                    user.IsApproved = true;
                    Membership.UpdateUser(user);
                    FormsService.SignIn(user.UserName, false);
                    return RedirectToAction("Index", "Manage");
                }
                else
                {
                    FormsService.SignOut();
                    TempData["tempMessage"] = "You have already confirmed your email address... please log in.";
                    return RedirectToAction("LogOn");
                }
            }
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {
                    FormsService.SignIn(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            FormsService.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {

            string reCaptchaPrivateKey = System.Configuration.ConfigurationManager.AppSettings["ReCaptchaPrivateKey"];


            bool captchaValid = true;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.google.com/recaptcha/api/siteverify");
                var paramaters = new Dictionary<string, string>();
                paramaters["secret"] = reCaptchaPrivateKey;
                paramaters["response"] = Request.Form["g-recaptcha-response"];
                paramaters["remoteip"] = HttpContext.Request.UserHostAddress;
                request.Content = new FormUrlEncodedContent(paramaters);

                HttpClient client = new HttpClient();
                var resp = client.SendAsync(request).Result;
                resp.EnsureSuccessStatusCode();

                var responseText = resp.Content.ReadAsStringAsync().Result;
                var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaValidationResponse>(responseText);
                if (!recaptchaResponse.Success)
                {
                    captchaValid = false;
                    ModelState.AddModelError("reCAPTCHA", "invalid reCAPTCHA " + string.Join(":", recaptchaResponse.ErrorCodes));
                }
            }
            catch (Exception e)
            {
                captchaValid = false;
                ModelState.AddModelError("reCAPTCHA", "invalid reCAPTCHA");
            }

            if (captchaValid)
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        //FormsService.SignIn(model.UserName, false /* createPersistentCookie */);

                        MembershipUser user = MembershipService.User(model.UserName);
                        MembershipService.SendConfirmationEmail(user);

                        return RedirectToAction("Confirmation", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "The Captcha did not match, please try again");
            }
            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

    }

    public class RecaptchaValidationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTimeStamp { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }

    }
}
