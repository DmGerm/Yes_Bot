using Interface.Models;
using Interface.VoteStorage;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController(IVoteService _votesStorage) : ControllerBase
    {
        [HttpPost("sync")]
        public IActionResult SyncVotes([FromBody] Dictionary<long, VoteEntity> votes)
        {
            if (votes == null || !votes.Any())
            {
                return BadRequest("No votes received.");
            }

            try
            {
                _votesStorage.SyncVotes(votes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error syncing votes: {ex.Message}");
            }

            return Ok("Votes synced successfully.");
        }
    }
}