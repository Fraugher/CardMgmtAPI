using CardManagementAPI.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CardManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthLoginController : ControllerBase
    {
        [HttpPost, Route("login")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            var secretKey = "4f1feeca525de4cdb064656007da3edac7895a87ff0ea865693300fb8b6e8f9c";
            var key = Encoding.ASCII.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenHandler = new JwtSecurityTokenHandler(); 
            try
            {
                if (string.IsNullOrEmpty(loginDTO.UserName) ||
                string.IsNullOrEmpty(loginDTO.Password))
                    return BadRequest("Username and/or Password not specified");
                string role=String.Empty; ;
                if (loginDTO.UserName.Equals("Administrator") && loginDTO.Password.Equals("password"))
                    role = "Administrator";
                if (loginDTO.UserName.Equals("Employee") && loginDTO.Password.Equals("password"))
                    role = "Employee";
                if (loginDTO.UserName.Equals("Customer") && loginDTO.Password.Equals("password"))
                    role = "Customer";
                if (String.IsNullOrEmpty(role))
                {
                    return Unauthorized();
                }
                else // valid user
                {
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                             {
                                 new Claim(ClaimTypes.Name, loginDTO.UserName),
                                 new Claim(ClaimTypes.Name, loginDTO.Password),
                                 new Claim(ClaimTypes.Role, role)
                             }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = credentials,
                        Issuer = "Fraugher"
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return Ok(tokenHandler.WriteToken(token));
                }
            }
            catch
            {
                return BadRequest("Error generating token.");
            }
        }
    }
}
