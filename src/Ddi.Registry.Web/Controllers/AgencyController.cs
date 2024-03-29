﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ddi.Registry.Data;
using Ddi.Registry.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ddi.Registry.Web.Controllers
{
    public class AgencyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgencyController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string agencyName)
        {
            //RegistryProvider provider = new RegistryProvider();

            if (string.IsNullOrEmpty(agencyName)) 
            {
                //Collection<Agency> agencies = provider.GetAgencies(state: ApprovalState.Approved);
                List<Agency> agencies = await _context.Agencies.Where(x => x.ApprovalState == ApprovalState.Approved).ToListAsync();
                return View("AgencyList", agencies);            
            }

            AgencyOverviewModel model = new AgencyOverviewModel();



            //Agency agency = provider.GetAgency(agencyName);
            var agency = await _context.Agencies.SingleOrDefaultAsync(x => 
                    x.AgencyId == agencyName
                    && x.ApprovalState == ApprovalState.Approved);

            if (agency == null || agency.ApprovalState != ApprovalState.Approved)
            {
				UnknownAgencyModel unknownModel = new UnknownAgencyModel()
				{
					AgencyName = agencyName
				};
                return View("UnknownAgency", unknownModel);
            }



            await _context.Entry(agency)
                .Reference(x => x.AdminContact)
                .LoadAsync();
            await _context.Entry(agency)
                .Reference(x => x.TechnicalContact)
                .LoadAsync();
            await _context.Entry(agency)
                .Reference(x => x.Creator)
                .LoadAsync();

            await _context.Entry(agency)
                .Collection(x => x.Assignments)
                .LoadAsync();

            model.Agency = agency;
            model.AdminContact = agency.AdminContact;
            model.TechnicalContact = agency.TechnicalContact;
            model.Assignments = agency.Assignments;

            foreach (Assignment a in model.Assignments)
            {
                await _context.Entry(a)
                    .Collection(x => x.Services)
                    .LoadAsync();
                await _context.Entry(a)
                    .Collection(x => x.Delegations)
                    .LoadAsync();
                await _context.Entry(a)
                    .Collection(x => x.HttpResolvers)
                    .LoadAsync();

                model.Services[a.AssignmentId] = a.Services;
                model.Delegations[a.AssignmentId] = a.Delegations;
                model.HttpResolvers[a.AssignmentId] = a.HttpResolvers;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SearchModel model)
        {
            if (model != null && model.Term != null)
            {

                string search = model.Term.ToLowerInvariant();
                if (search.StartsWith("urn:ddi:"))
                {
                    return await Lookup(search);
                }
                return await Search(model.Term);
            }
            else
            {
                //RegistryProvider provider = new RegistryProvider();
                List<Agency> agencies = await _context.Agencies.Where(x => x.ApprovalState == ApprovalState.Approved).ToListAsync();
                return View("AgencyList", agencies);
            }
        }


        private async Task<IActionResult> Lookup(string urn)
        {
            if (string.IsNullOrWhiteSpace(urn))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ResolverModel model = new ResolverModel();

                DdiUrn ddiurn = null;
                if (!DdiUrn.TryParse(urn, out ddiurn))
                {
                    return RedirectToAction("Index", "Home");
                }
                model.Urn = ddiurn;

                Assignment assignment = await _context.Assignments.Where(x => x.AssignmentId == ddiurn.Agency).FirstOrDefaultAsync();
                model.Assignment = assignment;

                // load HTTP resolvers
                await _context.Entry(assignment)
                    .Collection(x => x.HttpResolvers)
                    .LoadAsync();

                model.HttpResolvers = assignment.HttpResolvers;

                return View("Resolver", model);
            }
        }

        private async Task<IActionResult> Search(string term)
        {
            //RegistryProvider provider = new RegistryProvider();

            if (ModelState.IsValid)
            {
                if (term == null)
                {
                    List<Agency> agencies = await _context.Agencies.Where(x => x.ApprovalState == ApprovalState.Approved).OrderBy(x=>x.AgencyId).ToListAsync();
                    return View("AgencyList", agencies);
                }

                string search = term.ToLowerInvariant();

                var agency = await _context.Agencies.SingleOrDefaultAsync(x => x.AgencyId == search);
                if (agency == null)
                {
                    List<Agency> agencies = await _context.Agencies.Where(x => x.ApprovalState == ApprovalState.Approved 
                        && (x.AgencyId.Contains(term) || x.Label.Contains(term))).ToListAsync();
                    return View("AgencyList", agencies);
                }

                return RedirectToAction("Index", "Agency", new { agencyName = search });
            }
            return RedirectToAction("Index", "Agency");
        }


    }
}
