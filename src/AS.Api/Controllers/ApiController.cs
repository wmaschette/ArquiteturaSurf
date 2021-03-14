using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Services.Api.Controllers
{
    [Authorize("Bearer")]
    [ApiController]
    public class ApiController : ControllerBase
    {
    }
}
