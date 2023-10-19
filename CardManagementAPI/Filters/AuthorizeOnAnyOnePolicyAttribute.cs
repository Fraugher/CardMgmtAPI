using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CardManagementAPI.Filters
{
    public class AuthorizeOnAnyOnePolicyAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the AuthorizeOnAnyOnePolicyAttribute class.
        /// </summary>
        /// <param name="policies">A comma delimited list of policies that are allowed to access the resource.</param>
        public AuthorizeOnAnyOnePolicyAttribute(string policies) : base(typeof(AuthorizeOnAnyOnePolicyFilter))
        {
            Regex commaDelimitedWhitespaceCleanup = new Regex("\\s+,\\s+|\\s+,|,\\s+",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            Arguments = new object[] { commaDelimitedWhitespaceCleanup.Replace(policies, ",") };
        }
    }
}
