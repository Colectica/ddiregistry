using Ddi.Registry.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ddi.Registry.Web.Services
{
    public class AgencyAuthorizationHandler :
        AuthorizationHandler<AgencyRequirement, Agency>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       AgencyRequirement requirement,
                                                       Agency resource)
        {
            if (context.User.Identity?.Name == resource?.Creator?.UserName)
            {
                context.Succeed(requirement);
            }
            else if (context.User.Identity?.Name == resource?.TechnicalContact?.UserName)
            {
                context.Succeed(requirement);
            }
            if (context.User.Identity?.Name == resource?.AdminContact?.UserName)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class AgencyRequirement : IAuthorizationRequirement { }
}
