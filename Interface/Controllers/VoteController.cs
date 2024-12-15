using Microsoft.AspNetCore.Mvc;

namespace Interface.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class VoteController : ControllerBase
    {
        [HttpGet] 
        public IActionResult Get()
        {
            return Ok(new { Message = "Hello from VoteController API!" });
        }
    }
}