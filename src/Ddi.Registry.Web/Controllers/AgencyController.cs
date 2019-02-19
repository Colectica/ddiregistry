using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ddi.Registry.Data;
using Ddi.Registry.Web.Models;
using System.Collections.ObjectModel;

namespace Ddi.Registry.Web.Controllers
{
    public class AgencyController : Controller
    {
        public ActionResult Index(string agencyName)
        {
            RegistryProvider provider = new RegistryProvider();

            if (string.IsNullOrEmpty(agencyName)) 
            {
                Collection<Agency> agencies = provider.GetAgencies(state: ApprovalState.Approved);
                return View("AgencyList", agencies);            
            }

            AgencyOverviewModel model = new AgencyOverviewModel();

			

            Agency agency = provider.GetAgency(agencyName);
            if (agency == null || agency.ApprovalState != ApprovalState.Approved)
            {
				UnknownAgencyModel unknownModel = new UnknownAgencyModel()
				{
					AgencyName = agencyName
				};
                return View("UnknownAgency", unknownModel);
            }

            model.Agency = agency;
            model.AdminContact = provider.GetPerson(agency.AdminContactId);
            model.TechnicalContact = provider.GetPerson(agency.TechnicalContactId);
            model.Assignments = provider.GetAssignmentsForAgency(agency.AgencyId);

            foreach (Assignment a in model.Assignments)
            {
                model.Services[a.AssignmentId] = provider.GetServicesForAssignment(a.AssignmentId);
                model.Delegations[a.AssignmentId] = provider.GetDelegationsForAssignment(a.AssignmentId);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SearchModel model)
        {
            if (model != null)
            {
                return Search(model.Term);
            }
            else
            {
                RegistryProvider provider = new RegistryProvider();
                Collection<Agency> agencies = provider.GetAgencies(state: ApprovalState.Approved);
                return View("AgencyList", agencies);
            }
        }


        public ActionResult Search(string term)
        {
            RegistryProvider provider = new RegistryProvider();

            if (ModelState.IsValid)
            {
                if (term == null)
                {
                    Collection<Agency> agencies = provider.GetAgencies(state: ApprovalState.Approved);
                    return View("AgencyList", agencies);
                }

                string search = term.ToLowerInvariant();
                if (search.StartsWith("urn:ddi:"))
                {
                    search = search.Replace("urn:ddi:", "");
                    int index = search.IndexOf(":");
                    if (index != -1)
                    {
                        search = search.Substring(0, index);
                    }
                }

                Agency agency = provider.GetAgency(search);
                if (agency == null)
                {
                    Collection<Agency> agencies = provider.GetAgencies(state: ApprovalState.Approved, partialName: search);
                    return View("AgencyList", agencies);
                }

                return RedirectToAction("Index", "Agency", new { agencyName = search });
            }
            return RedirectToAction("Index", "Agency");
        }


    }
}
