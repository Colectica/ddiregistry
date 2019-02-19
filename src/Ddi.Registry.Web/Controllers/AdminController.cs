using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ddi.Registry.Data;
using Ddi.Registry.Web.Models;
using System.Web.Security;
using System.Globalization;
using System.Net.Mail;

namespace Ddi.Registry.Web.Controllers
{
    public class AdminController : Controller
	{
		#region Approval

		[Authorize(Roles="admin")]
        public ActionResult Index()
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            ApproveModel model = new ApproveModel();
            model.Agencies = provider.GetAgenciesByApprovalState(ApprovalState.Requested);

            Dictionary<Guid, Person> people = new Dictionary<Guid, Person>();
            foreach (Agency agency in model.Agencies)
            {
                if (agency.AdminContactId != default(Guid) &&
                    !people.ContainsKey(agency.AdminContactId))
                {
                    Person p = provider.GetPerson(agency.AdminContactId);
                    if (p != null)
                    {
                        people[agency.AdminContactId] = p;
                    }
                }

                if (agency.TechnicalContactId != default(Guid) &&
                        !people.ContainsKey(agency.TechnicalContactId))
                {
                    Person p = provider.GetPerson(agency.TechnicalContactId);
                    if (p != null)
                    {
                        people[agency.TechnicalContactId] = p;
                    }
                }
            }
            model.People = people;

            return View(model);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Approve(Guid agencyId)
        {
            RegistryProvider provider = new RegistryProvider();
            Agency agency = provider.GetAgency(agencyId);
            if (agency != null)
            {
                agency.ApprovalState = ApprovalState.Approved;
                agency.DateApproved = DateTime.UtcNow;
                agency.LastModified = DateTime.UtcNow;
                provider.Update(agency);

                Assignment assignment = new Assignment()
                {
                    Name = agency.AgencyName,
                    AgencyId = agency.AgencyId,
                    IsDelegated = false
                };
                provider.Add(assignment);

                MembershipUser user = Membership.GetUser(agency.Username);
                SendApprovedEmail(user, agency.AgencyName);

                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("NoAccess", "Error");
        }

        public void SendApprovedEmail(MembershipUser user, string agencyName)
        {
			var message = new MailMessage("ddiregistry@ddialliance.org", user.Email)
            {
                Subject = "DDI Registry - Agency Approved: " + agencyName,
                Body = string.Format("The following agency identifier has been approved:\n\n  {0}\n\nThank you,\n\nThe DDI Alliance", agencyName)
            };
			message.ReplyToList.Add("ddiregistry@ddialliance.org");
			message.From = new MailAddress("ddiregistry@ddialliance.org", "DDI Registry");
            var client = new SmtpClient();
            client.Send(message);
        }
        public void SendDeniedEmail(MembershipUser user, string agencyName, string reason)
        {
			string text = string.Format("The following request for an agency identifier has been denied:\n\n  {0}\n\nThe reason given was:\n\n  {1}\n\nThank you,\n\nThe DDI Alliance", agencyName, reason);
			var message = new MailMessage("ddiregistry@ddialliance.org", user.Email)
            {
                Subject = "DDI Registry - Agency Denied: " + agencyName,
                Body = text
            };
			message.ReplyToList.Add("ddiregistry@ddialliance.org");
			message.From = new MailAddress("ddiregistry@ddialliance.org", "DDI Registry");
            var client = new SmtpClient();
            client.Send(message);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Delete(Guid agencyId)
        {
            RegistryProvider provider = new RegistryProvider();
            Agency agency = provider.GetAgency(agencyId);
            if (agency != null)
            {
				DeleteAgencyRequestModel model = new DeleteAgencyRequestModel(agency);
				return View(model);
            }
            return RedirectToAction("NoAccess", "Error");
        }

		[Authorize(Roles = "admin")]
		[HttpPost]
		public ActionResult Delete(DeleteAgencyRequestModel model)
		{
			RegistryProvider provider = new RegistryProvider();
            if (ModelState.IsValid &&
				model != null &&
				model.Agency != null)
            {
				Agency agency = model.Agency;
                agency = provider.GetAgency(agency.AgencyId);
                if (agency != null)
                {                    
                    provider.Remove(agency);
                    
                    MembershipUser user = Membership.GetUser(agency.Username);
                    SendDeniedEmail(user, agency.AgencyName, model.Reason);
                    return RedirectToAction("Index", "Admin");
                }
            }
			return RedirectToAction("NoAccess", "Error");
		}

		#endregion

		#region Membership

		[Authorize(Roles="SuperAdmin")]
		public ActionResult ShowMembers()
		{
			int records = 0;

			MembershipUserCollection members = Membership.GetAllUsers(0, Int32.MaxValue, out records);

			return View(members);
		}

		[Authorize(Roles = "SuperAdmin")]
		public ActionResult CreateMember()
		{
			return View();
		}

		[Authorize(Roles = "SuperAdmin")]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult CreateMember(string userName, string email, string password, string confirmPassword)
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
		public ActionResult EditMember(Guid id)
		{
			MembershipUser user = Membership.GetUser(id, false);
			return View(user);
		}

		[Authorize(Roles = "SuperAdmin")]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult EditMember(Guid id, FormCollection collection)
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
		public ActionResult DeleteMember(Guid id)
		{
			MembershipUser user = Membership.GetUser(id, false);
			return View(user);
		}

		[Authorize(Roles = "SuperAdmin")]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult DeleteMember(Guid id, FormCollection collection)
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
