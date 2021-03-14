using AS.Service.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Services.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ApiController
    {
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Post(
            [FromBody] AccessCredentials credenciais,
            [FromServices] AccessManager accessManager)
        {
            if (accessManager.ValidateCredentials(credenciais))
                return Ok(accessManager.GenerateToken(credenciais));
            else
                return Unauthorized();
        }
    }
}
