using System;
using System.Collections.Generic;
using System.Linq;
using Ddi.Registry.Data;
using Ddi.Registry.Web.Models;
using System.Globalization;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Ddi.Registry.Web.Controllers
{
    public class AdminController : Controller
	{
        private readonly ApplicationDbContext _context;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _email;

        public AdminController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IEmailSender email)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _email = email;
        }

        #region Approval

        [Authorize(Roles= "admin,SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            //RegistryProvider provider = new RegistryProvider();
            //string username = User.Identity.Name;

            ApproveModel model = new ApproveModel();
            model.Agencies = await _context.Agencies.Where(x => x.ApprovalState == ApprovalState.Requested)
                .Include(i => i.Creator)
                .Include(i => i.AdminContact)
                .Include(i => i.TechnicalContact)
                .ToListAsync();

            Dictionary<string, ApplicationUser> people = new Dictionary<string, ApplicationUser>();
            foreach (Agency agency in model.Agencies)
            {
                if (!string.IsNullOrWhiteSpace(agency.AdminContactId) &&
                    !people.ContainsKey(agency.AdminContactId))
                {
                    people[agency.AdminContactId] = agency.AdminContact;
                }

                if (!string.IsNullOrWhiteSpace(agency.TechnicalContactId) &&
                    !people.ContainsKey(agency.TechnicalContactId))
                {
                    people[agency.TechnicalContactId] = agency.TechnicalContact;
                }

                if (!string.IsNullOrWhiteSpace(agency.CreatorId) &&
                    !people.ContainsKey(agency.CreatorId))
                {
                    people[agency.CreatorId] = agency.Creator;
                }
            }
            model.People = people;

            return View(model);
        }

        [Authorize(Roles = "admin,SuperAdmin")]
        public async Task<IActionResult> Approve(string agencyId)
        {
            
            Agency agency = await _context.GetAgency(agencyId);
            if (agency != null)
            {
                agency.ApprovalState = ApprovalState.Approved;
                agency.DateApproved = DateTime.UtcNow;
                agency.LastModified = DateTime.UtcNow;

                Assignment assignment = new Assignment()
                {
                    Agency = agency,
                    AgencyId = agency.AgencyId,
                    IsDelegated = false
                };

                _context.Add(assignment);

                await _context.SaveChangesAsync();

                var creator = await _context.Users.FindAsync(agency.CreatorId);
                if(creator != null)
                {
                    await SendApprovedEmail(creator, agencyId);
                }                

                return RedirectToAction("Index", "Admin");
            }
            return Forbid();
        }

        public async Task SendApprovedEmail(ApplicationUser user, string agencyName)
        {
            var bodyHtml = $"<p>The following agency identifier has been approved:</<p><p>{agencyName}</p><p>Thank you,<br/>The DDI Alliance</p>";
            var subject = $"DDI Registry - Agency Approved: {agencyName}";

            await _email.SendEmailAsync(user.Email, subject, bodyHtml);
        }
        public async Task SendDeniedEmail(ApplicationUser user, string agencyName, string reason)
        {
			string bodyHtml = $"<p>The following request for an agency identifier has been denied:</p><p>{agencyName}</p><p>The reason given was:</p><p>{reason}</p><p>Thank you,<br/>The DDI Alliance</p>";
            var subject = $"DDI Registry - Agency Denied: {agencyName}";

            await _email.SendEmailAsync(user.Email, subject, bodyHtml);
        }

        [Authorize(Roles = "admin,SuperAdmin")]
        public async Task<IActionResult> Delete(string agencyId)
        {            
            Agency agency = await _context.GetAgency(agencyId);
            if (agency != null)
            {
				DeleteAgencyRequestModel model = new DeleteAgencyRequestModel(agency);
				return View(model);
            }
            return Forbid();
        }

		[Authorize(Roles = "admin,SuperAdmin")]
		[HttpPost]
		public async Task<IActionResult> Delete(DeleteAgencyRequestModel model)
		{
			
            if (ModelState.IsValid &&
				model != null &&
				model.Agency != null)
            {
				Agency agency = await _context.GetAgency(model.Agency.AgencyId);
                if (agency != null)
                {                    
                    _context.Remove(agency);
                    await _context.SaveChangesAsync();

                    var creator = await _context.Users.FindAsync(agency.CreatorId);
                    if (creator != null)
                    {
                        await SendDeniedEmail(creator, agency.AgencyId, model.Reason);
                    }
                    
                    return RedirectToAction("Index", "Admin");
                }
            }
			return Forbid();
		}

		#endregion

		#region Membership

		[Authorize(Roles="SuperAdmin")]
		public async Task<IActionResult> ShowMembers()
		{
            List<ApplicationUser> members = await _context.Users.OrderBy(x => x.Name).ThenBy(x => x.Organization).ThenBy(x => x.NormalizedUserName).ToListAsync();

			return View(members);
		}        

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ImpersonateUser(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var impersonatedUser = await _userManager.FindByIdAsync(id);

            var userPrincipal = await _signInManager.CreateUserPrincipalAsync(impersonatedUser);

            userPrincipal.Identities.First().AddClaim(new Claim("OriginalUserId", currentUserId));
            userPrincipal.Identities.First().AddClaim(new Claim("IsImpersonating", "true"));

            // sign out the current user
            await _signInManager.SignOutAsync();

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal); 

            return RedirectToAction("Index", "Home");
        }
        /*
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> CreateMember()
		{
			return View();
		}

		[Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateMember(string userName, string email, string password, string confirmPassword)
		{
			try
			{
				ViewBag.PasswordLength = Membership.MinRequiredPasswordLength;

				if (ValidateRegistration(userName, email, password, confirmPassword))
				{
					// Attempt to register the user
					MembershipUser user = Membership.CreateUser(userName, password, email);

					if (user != null)
					{
						return RedirectToAction("ShowMembers", new { id = user.ProviderUserKey });
					}
					else
					{
						ModelState.AddModelError("_FORM", "Error creating user");
					}
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("_FORM", ex.Message);
			}

			// If we got this far, something failed, redisplay form
			return View();
		}
        

		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> EditMember(string id)
		{
            var user = await _context.Users.FindAsync(id);
			return View(user);
		}

		[Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> EditMember(string id, FormCollection collection)
		{
			try
			{
				MembershipUser user = Membership.GetUser(new Guid(id.ToString()), false);

				UpdateModel(user);

				foreach (String role in Roles.GetAllRoles())
				{
					if (collection[role].Contains("true"))
					{
						if (!Roles.IsUserInRole(user.UserName, role)) Roles.AddUserToRole(user.UserName, role);
					}
					else
					{
						if (Roles.IsUserInRole(user.UserName, role)) Roles.RemoveUserFromRole(user.UserName, role);
					}
				}

				Membership.UpdateUser(user);

				return RedirectToAction("ShowMembers");
			}
			catch (Exception ex)
			{
				ViewData.ModelState.AddModelError("_FORM", ex.Message);
				return View();
			}
		}

		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> DeleteMember(Guid id)
		{
			MembershipUser user = Membership.GetUser(id, false);
			return View(user);
		}

		[Authorize(Roles = "SuperAdmin")]
		[AcceptVerbs(HttpVerbs.Post)]
		public async Task<IActionResult> DeleteMember(Guid id, FormCollection collection)
		{
			try
			{
				MembershipUser user = Membership.GetUser(new Guid(id.ToString()), false);
				Membership.DeleteUser(user.UserName);

				return RedirectToAction("ShowMembers");
			}
			catch (Exception ex)
			{
				ViewData.ModelState.AddModelError("ERROR", ex.Message);
				return View();
			}
		}

		[Authorize(Roles = "SuperAdmin")]
		private bool ValidateRegistration(string userName, string email, string password, string confirmPassword)
		{
			if (String.IsNullOrEmpty(userName))
			{
				ModelState.AddModelError("username", "You must specify a username.");
			}
			if (String.IsNullOrEmpty(email))
			{
				ModelState.AddModelError("email", "You must specify an email address.");
			}
			if (password == null || password.Length < Membership.MinRequiredPasswordLength)
			{
				ModelState.AddModelError("password",
					String.Format(CultureInfo.CurrentCulture,
						 "You must specify a password of {0} or more characters.",
						 Membership.MinRequiredPasswordLength));
			}
			if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
			{
				ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
			}
			return ModelState.IsValid;
		}
        */
        #endregion

    }

    public class RoleItem
	{
		public String Role { get; set; }

		public RoleItem()
		{
		}

		public RoleItem(String role)
		{
			Role = role;
		}
	}
}
