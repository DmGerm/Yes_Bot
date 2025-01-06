using Interface.Models;
using Interface.VoteStorage;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController(IVoteService voteService) : ControllerBase
    {
        private readonly IVoteService _voteService = voteService;

        [HttpPost("vote_link")]
        [IgnoreAntiforgeryToken]
        public IActionResult GetVoteResultLink([FromBody] VoteEntity vote)
        {
            var voteResultLink = _voteService.GetPageUrl(vote);

            if (string.IsNullOrEmpty(voteResultLink))
            {
                return NotFound("Vote result link not found.");
            }
            return Ok(voteResultLink);
        }

        [HttpPost("shop_sync")]
        public IActionResult SyncShopList([FromBody] List<string> shops)
        {
            try
            {
                _voteService.SyncShops(shops);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Shops synced successfully.");
        }

        [HttpGet("csrf-token")]
        public IActionResult GetCsrfToken()
        {
            var antiforgery = HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }
    }
}