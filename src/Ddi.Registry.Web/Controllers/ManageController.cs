using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ddi.Registry.Data;
using Ddi.Registry.Web.Models;
using System.Web.Security;
using System.Net.Mail;

namespace Ddi.Registry.Web.Controllers
{
    public class ManageController : Controller
    {
        #region Assignment
        [Authorize]
        public ActionResult AddAssignment(Guid agencyId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;


            if (!provider.ManagesAgency(username, agencyId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Agency agency = provider.GetAgency(agencyId);
            AssignmentModel assignment = new AssignmentModel()
            {
                AgencyId = agencyId,
                AgencyName = agency.AgencyName,
                Name = agency.AgencyName + "."
            };
            return View(assignment);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddAssignment(AssignmentModel model)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                if (!provider.ManagesAgency(username, model.AgencyId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                Agency agency = provider.GetAgency(model.AgencyId);

                string assignmentName = model.Name.ToLowerInvariant();
                if (!assignmentName.StartsWith(agency.AgencyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("", "The agency must start with the agency id");
                    model.AgencyId = agency.AgencyId;
                    model.AgencyName = agency.AgencyName;
                    model.Name = agency.AgencyName + ".";
                    return View(model);
                }
                
                if (provider.GetAssignment(assignmentName) != null)
                {
                    ModelState.AddModelError("", "Sub agency already exists: " + assignmentName);
                    model.AgencyId = agency.AgencyId;
                    model.AgencyName = agency.AgencyName;
                    model.Name = agency.AgencyName + ".";                    
                    return View(model);
                }

                Assignment assignment = new Assignment()
                {
                    AgencyId = model.AgencyId,
                    Name = assignmentName
                };

                provider.Add(assignment);

            }
            return RedirectToAction("ViewAgency", "Manage", new { agencyId = model.AgencyId });            
        }

        [Authorize]
        public ActionResult EditAssignment(Guid assignmentId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            Assignment assignment = provider.GetAssignment(assignmentId);
            if (assignment == null || !provider.ManagesAssignment(username, assignment.AssignmentId))
            {
                return RedirectToAction("NoAccess", "Error");
            }

			Agency agency = provider.GetAgency(assignment.AgencyId);
			if (agency == null)
			{
				return RedirectToAction("NoAccess", "Error");
			}

            AssignmentModel model = new AssignmentModel()
            {
                AgencyId = assignment.AgencyId,
                AgencyName = agency.AgencyName,
				Name = assignment.Name,
                Delegated = assignment.IsDelegated,
                AssignmentId = assignment.AssignmentId,
				IsDelegated = assignment.IsDelegated,
				Services = provider.GetServicesForAssignment(assignment.AssignmentId),
				Delegations = provider.GetDelegationsForAssignment(assignment.AssignmentId)
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditAssignment(AssignmentModel model)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                Assignment assignment = provider.GetAssignment(model.AssignmentId);
                if (assignment == null || !provider.ManagesAssignment(username, assignment.AssignmentId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                Agency agency = provider.GetAgency(assignment.AgencyId);

                string assignmentName = model.Name;
                if (assignmentName.Equals(agency.AgencyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    //
                }
                else if (!assignmentName.StartsWith(agency.AgencyName + ".", StringComparison.InvariantCultureIgnoreCase))
                {
                    assignmentName = agency.AgencyName + "." + assignmentName;
                }

                assignment.Name = assignmentName.ToLowerInvariant();
                assignment.IsDelegated = model.Delegated;
                assignment.LastModified = DateTime.UtcNow;
                provider.Update(assignment);

				return RedirectToAction("EditAssignment", "Manage", new { assignmentId = assignment.AssignmentId });
            }
            return View(model);
        }

		[Authorize]
		public ActionResult DeleteAssignment(Guid assignmentId)
		{
			RegistryProvider provider = new RegistryProvider();
			string username = User.Identity.Name;

			Assignment assignment = provider.GetAssignment(assignmentId);
			if (assignment == null || !provider.ManagesAssignment(username, assignment.AssignmentId))
			{
				return RedirectToAction("NoAccess", "Error");
			}

			Agency agency = provider.GetAgency(assignment.AgencyId);
			if (agency == null)
			{
				return RedirectToAction("NoAccess", "Error");
			}

			// Don't allow deleting the top level assignment.
			if (assignment.Name == agency.AgencyName)
			{
				return RedirectToAction("ViewAgency", new { agencyId = agency.AgencyId });
			}

			return View(assignment);
		}

		[Authorize]
		[HttpPost]
		public ActionResult DeleteAssignment(Assignment assignment)
		{
			RegistryProvider provider = new RegistryProvider();
			string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
			    if (assignment == null || !provider.ManagesAssignment(username, assignment.AssignmentId))
			    {
				    return RedirectToAction("NoAccess", "Error");
			    }

				Assignment populatedAssignment = provider.GetAssignment(assignment.AssignmentId);

				Agency agency = provider.GetAgency(populatedAssignment.AgencyId);
			    if (agency == null)
			    {
				    return RedirectToAction("NoAccess", "Error");
			    }

			    // Don't allow deleting the top level assignment.
			    if (populatedAssignment.Name == agency.AgencyName)
			    {
				    return RedirectToAction("ViewAgency", new { agencyId = agency.AgencyId });
			    }

			    provider.Remove(assignment);

			    return RedirectToAction("ViewAgency", new { agencyId = agency.AgencyId });
            }
            return View(assignment);
		}


        #endregion

        #region Service
        [Authorize]
        public ActionResult AddService(Guid assignmentId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;


            if (!provider.ManagesAssignment(username, assignmentId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Service service = new Service()
            {
                AssignmentId = assignmentId
            };
            return View(service);
        }

        [Authorize]
        public ActionResult DeleteService(Guid serviceId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;


            if (!provider.ManagesService(username, serviceId))
            {
                return RedirectToAction("NoAccess", "Error");
            }

            Service service = provider.GetService(serviceId);
            if (service == null) { return RedirectToAction("NotFound", "Error"); }
            Assignment assignment = provider.GetAssignment(service.AssignmentId);
            if (assignment == null) { return RedirectToAction("NotFound", "Error"); }

			return View(service);
        }

		[Authorize]
		[HttpPost]
		public ActionResult DeleteService(Service service)
		{
			RegistryProvider provider = new RegistryProvider();
			string username = User.Identity.Name;

			if (service == null) { return RedirectToAction("NotFound", "Error"); }

			if (!provider.ManagesService(username, service.ServiceId))
			{
				return RedirectToAction("NoAccess", "Error");
			}

			Assignment assignment = provider.GetAssignment(service.AssignmentId);
			if (assignment == null) { return RedirectToAction("NotFound", "Error"); }

			provider.Remove(service);

			return RedirectToAction("EditAssignment", "Manage", new { assignmentId = assignment.AssignmentId });
		}


        [HttpPost]
        [Authorize]
        public ActionResult AddService(Service service)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;
            
            if (ModelState.IsValid)
            {
                if (!provider.ManagesAssignment(username, service.AssignmentId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                Assignment assignment = provider.GetAssignment(service.AssignmentId);

                service.ServiceId = Guid.NewGuid();
                provider.Add(service);

                return RedirectToAction("EditAssignment", "Manage", new { assignmentId = assignment.AssignmentId });
            }
            return View(service);
        }

        [Authorize]
        public ActionResult EditService(Guid serviceId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (!provider.ManagesService(username, serviceId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Service service = provider.GetService(serviceId);
            return View(service);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditService(Service service)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                if (!provider.ManagesService(username, service.ServiceId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                if (!provider.ManagesAssignment(username, service.AssignmentId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                provider.Update(service);

                Assignment assignment = provider.GetAssignment(service.AssignmentId);
                return RedirectToAction("ViewAgency", "Manage", new { agencyId = assignment.AgencyId });
            }
            return View(service);
        }



        #endregion

        #region Delegation
        [Authorize]
        public ActionResult AddDelegation(Guid assignmentId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;


            if (!provider.ManagesAssignment(username, assignmentId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Delegation delegation = new Delegation()
            {
                AssignmentId = assignmentId
            };
            return View(delegation);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddDelegation(Delegation delegation)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                if (!provider.ManagesAssignment(username, delegation.AssignmentId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                Assignment assignment = provider.GetAssignment(delegation.AssignmentId);

                delegation.DelegationId = Guid.NewGuid();
                provider.Add(delegation);

                return RedirectToAction("EditAssignment", "Manage", new { assignmentId = assignment.AssignmentId });
            }
            return View(delegation);
        }

        [Authorize]
        public ActionResult EditDelegation(Guid delegationId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (!provider.ManagesDelegation(username, delegationId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Delegation delegation = provider.GetDelegation(delegationId);
            return View(delegation);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditDelegation(Delegation delegation)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                if (!provider.ManagesDelegation(username, delegation.DelegationId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                if (!provider.ManagesAssignment(username, delegation.AssignmentId))
                {
                    return RedirectToAction("NoAccess", "Error");
                }
                provider.Update(delegation);

                Assignment assignment = provider.GetAssignment(delegation.AssignmentId);
                return RedirectToAction("EditAssignment", "Manage", new { assignmentId = assignment.AssignmentId });
            }
            return View(delegation);
        }

		[Authorize]
		public ActionResult DeleteDelegation(Guid delegationId)
		{
			RegistryProvider provider = new RegistryProvider();
			string username = User.Identity.Name;

			if (!provider.ManagesDelegation(username, delegationId))
			{
				return RedirectToAction("NoAccess", "Error");
			}

			Delegation delegation = provider.GetDelegation(delegationId);
			if (delegation == null) { return RedirectToAction("NotFound", "Error"); }

			Assignment assignment = provider.GetAssignment(delegation.AssignmentId);
			if (assignment == null) { return RedirectToAction("NotFound", "Error"); }

			return View(delegation);
		}


        [Authorize]
		[HttpPost]
        public ActionResult DeleteDelegation(Delegation delegation)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

			if (delegation == null) { return RedirectToAction("NotFound", "Error"); }
			
			Guid delegationId = delegation.DelegationId;
            if (!provider.ManagesDelegation(username, delegationId))
            {
                return RedirectToAction("NoAccess", "Error");
            }

            Assignment assignment = provider.GetAssignment(delegation.AssignmentId);
            if (assignment == null) { return RedirectToAction("NotFound", "Error"); }

            provider.Remove(delegation);

			return RedirectToAction("EditAssignment", "Manage", new { assignmentId = assignment.AssignmentId });
        }

        #endregion

        #region Agency
        [Authorize]
        public ActionResult Index()
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            OverviewModel model = new OverviewModel();
            model.Agencies = provider.GetAgenciesForUser(username);
            model.People = provider.GetPeopleForUser(username);

            return View(model);
        }

        [Authorize]
        public ActionResult AddAgency()
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            ViewBag.People = provider.GetPeopleForUser(username);
            return View();
        }

        [Authorize]
        public ActionResult EditAgency(Guid agencyId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (!provider.ManagesAgency(username, agencyId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Agency agency = provider.GetAgency(agencyId);
            AgencyModel model = new AgencyModel()
            {
                AdminContact = agency.AdminContactId,
                TechnicalContact = agency.TechnicalContactId,
                AgencyId = agency.AgencyId,
                AgencyName = agency.AgencyName
            };
            ViewBag.People = provider.GetPeopleForUser(username);
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditAgency(AgencyModel model)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (!provider.ManagesAgency(username, model.AgencyId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Agency agency = provider.GetAgency(model.AgencyId);

            if (model.AdminContact != default(Guid)) { agency.AdminContactId = model.AdminContact; }
            if (model.TechnicalContact != default(Guid)) { agency.TechnicalContactId = model.TechnicalContact; }
            agency.LastModified = DateTime.UtcNow;
            provider.Update(agency);
            return RedirectToAction("Index", "Manage");
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddAgency(AgencyModel addAgencyModel)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                Agency agency = provider.GetAgency(addAgencyModel.AgencyName);
                if (agency != null)
                {
                    ModelState.AddModelError("", "The agency id already exists, please try again");
                }
                else
                {
                    agency = new Agency() 
                    { 
                        AgencyName = addAgencyModel.AgencyName,
                        ApprovalState = ApprovalState.Requested,
                        Username = username
                    };
                    if (addAgencyModel.AdminContact != default(Guid)) { agency.AdminContactId = addAgencyModel.AdminContact; }
                    if (addAgencyModel.TechnicalContact != default(Guid)) { agency.TechnicalContactId = addAgencyModel.TechnicalContact; }
                    provider.Add(agency);

					// Send email.
					MembershipUser user = Membership.GetUser(this.User.Identity.Name);
					SendConfirmationEmail(user, addAgencyModel.AgencyName);

                    return RedirectToAction("Index", "Manage");
                }
            }

            ViewBag.People = provider.GetPeopleForUser(username);
            return View();
        }

		private void SendConfirmationEmail(MembershipUser user, string agencyName)
		{
			string messageText = string.Format(
				"You submitted the following request for a new agency identifier:\n\n  {0}\n\nYou will receive a separate confirmation when your request has been processed.\n\nThank you,\n\nThe DDI Alliance",
				agencyName);

			var message = new MailMessage("ddiregistry@ddialliance.org", user.Email)
			{
				Subject = "DDI Registry - Agency Request: " + agencyName,
				Body = messageText
			};
			message.ReplyToList.Add("ddiregistry@ddialliance.org");
			message.From = new MailAddress("ddiregistry@ddialliance.org", "DDI Registry");
			var client = new SmtpClient();
			client.Send(message);
		}

        [Authorize]
        public ActionResult ViewAgency(Guid agencyId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

           
            if (!provider.ManagesAgency(username, agencyId))
            {
                return RedirectToAction("NoAccess", "Error");
            }
            Agency agency = provider.GetAgency(agencyId);

            AgencyOverviewModel model = new AgencyOverviewModel();
            model.Agency = provider.GetAgency(agencyId);
            model.AdminContact = provider.GetPerson(agency.AdminContactId);
            model.TechnicalContact = provider.GetPerson(agency.TechnicalContactId);
            model.Assignments = provider.GetAssignmentsForAgency(agencyId);

            foreach (Assignment a in model.Assignments)
            {
                model.Services[a.AssignmentId] = provider.GetServicesForAssignment(a.AssignmentId);
                model.Delegations[a.AssignmentId] = provider.GetDelegationsForAssignment(a.AssignmentId);
            }

            return View(model);
        }
        #endregion

        #region Person
        [Authorize]
        public ActionResult AddPerson()
        {
			Person person = new Person();
            return View(person);
        }

        [Authorize]
        public ActionResult ViewPerson(Guid personId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;            

            if (provider.ManagesPerson(username, personId))
            {
                Person person = provider.GetPerson(personId);
                if (person == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }
                return View(person);
            }
            else
            {
                return RedirectToAction("NoAccess", "Error");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddPerson(Person person)
        {
            if (ModelState.IsValid)
            {
                RegistryProvider provider = new RegistryProvider();
                string username = User.Identity.Name;
                person.PersonId = Guid.NewGuid();
                person.Username = username;
                provider.Add(person);
                return RedirectToAction("Index", "Manage");
            }
            return View();
        }

        [Authorize]
        public ActionResult EditPerson(Guid personId)
        {
            RegistryProvider provider = new RegistryProvider();
            string username = User.Identity.Name;

            
            if (provider.ManagesPerson(username,personId))
            {
                Person person = provider.GetPerson(personId);
                if (person != null)
                {
                    return View(person);
                }
                else
                {
                    return RedirectToAction("NoAccess", "Error");
                }
            }
            else
            {
                return RedirectToAction("NoAccess", "Error");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditPerson(Person person)
        {
            if (ModelState.IsValid)
            {
                RegistryProvider provider = new RegistryProvider();
                string username = User.Identity.Name;

                if (provider.ManagesPerson(username, person.PersonId))
                {
                    person.Username = username;
                    provider.Update(person);
                    return RedirectToAction("Index", "Manage");
                }
                else
                {
                    return RedirectToAction("NoAccess", "Error");
                }
            }
            return View();
        }

		[Authorize]
		public ActionResult DeletePerson(Guid personId)
		{
			RegistryProvider provider = new RegistryProvider();
			string username = User.Identity.Name;

			if (provider.ManagesPerson(username, personId))
			{
				Person person = provider.GetPerson(personId);

				if (person != null)
				{
					return View(person);
				}
				else
				{
					return RedirectToAction("NoAccess", "Error");
				}
			}
			else
			{
				return RedirectToAction("NoAccess", "Error");
			}
		}

		[Authorize]
		[HttpPost]
		public ActionResult DeletePerson(Person person)
		{
			RegistryProvider provider = new RegistryProvider();
			string username = User.Identity.Name;

			if (provider.ManagesPerson(username, person.PersonId))
			{
				provider.Remove(person);
				return RedirectToAction("Index");
			}
			else
			{
				return RedirectToAction("NoAccess", "Error");
			}
		}

        #endregion

    }
}
