using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApiAngular.Helpers;
using WebApiAngular.Models;

namespace WebApiAngular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signManager;
        private readonly AppSettings _appsettings;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signManager, IOptions<AppSettings> appsettings)
        {
            _userManager = userManager;
            _signManager = signManager;
            _appsettings = appsettings.Value;
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel formdata)
        {
            //will hold all the errors related to registration

            List<string> errorList = new List<string>();

            var user = new IdentityUser
            {
                Email = formdata.Email,
                UserName = formdata.Username,
                //security stamp is user when ever a user updates his information the security stamp will be updated that why we are using it
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, formdata.Password);

            if (result.Succeeded)
            {
                //once a user is created we assign a default role customer
                await _userManager.AddToRoleAsync(user, "Customer");

                //sending confirmation email

                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Successful" });
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    // key, value
                    ModelState.AddModelError("", error.Description);
                    errorList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));

         
        }


        [HttpPost("[action]")]

        public async Task<IActionResult> Login([FromBody]LoginViewModel formdata)
        {
            var user = await _userManager.FindByNameAsync(formdata.Username);

            //to get the user role
            var roles = await _userManager.GetRolesAsync(user);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appsettings.Secret));

            double tokenExpiryTime = Convert.ToDouble(_appsettings.ExpireTime);

            if(user != null && await _userManager.CheckPasswordAsync(user, formdata.Password))
            {
                //Email Confirmation


                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    // create claims of what u want to validate against
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, formdata.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim("LoggedOn", DateTime.Now.ToString())
                    }),

                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature ),
                    Issuer = _appsettings.Site,
                    Audience = _appsettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                };

                //Generate Token

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new { token = tokenHandler.WriteToken(token), expiration = token.ValidTo, username = user.UserName, userRole = roles.FirstOrDefault() });
            }

            ModelState.AddModelError("", "User/Password not found");
            return Unauthorized(new {LoginError= "please check THE login Credentials - Invalid username/password was entered" });
        }

    }
}