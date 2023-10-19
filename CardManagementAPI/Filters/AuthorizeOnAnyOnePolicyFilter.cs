using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CardManagementAPI.Filters
{
    public class AuthorizeOnAnyOnePolicyFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService authorization;
        public string Policies { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AuthorizeOnAnyOnePolicyFilter class.
        /// </summary>
        /// <param name="policies">A comma delimited list of policies that are allowed to access the resource.</param>
        /// <param name="authorization">The AuthorizationFilterContext.</param>
        public AuthorizeOnAnyOnePolicyFilter(string policies, IAuthorizationService authorization)
        {
            Policies = policies;
            this.authorization = authorization;
        }

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized.
        /// </summary>
        /// <param name="context">A context for authorization filters i.e. IAuthorizationFilter and IAsyncAuthorizationFilter implementations.</param>
        /// <returns>Sets the context.Result to ForbidResult() if the user fails all of the policies listed.</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            bool authorized = false;
            var role = "Unauthorized";
            var handler = new JwtSecurityTokenHandler();
            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authHeader != null)
            {
                authHeader = authHeader.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(authHeader);
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                role = tokenS.Claims.First(claim => claim.Type == "role").Value;
            }
            else
            {
                Console.WriteLine("Not logged in");
                context.Result = new ForbidResult();
                return;
            }

            var policies = Policies.Split(",").ToList();
            // Loop through policies.  User need only belong to one policy to be authorized.
            foreach (var policy in policies)
            {
                authorized = (policy.ToString() == role);
                if (authorized)
                {
                    Console.WriteLine("Authorization succeeded for policy = " + policy.ToString());
                    return;
                }
            }
            Console.WriteLine("Authorization failed.");
            context.Result = new ForbidResult();
            return;
        }
    }
}
