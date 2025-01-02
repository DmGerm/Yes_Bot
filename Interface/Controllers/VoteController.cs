using Interface.Models;
using Interface.VoteStorage;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController(IVoteService voteService) : ControllerBase
    {
        private readonly IVoteService _voteService = voteService;

        [HttpPost("vote_link")]
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
    }
}