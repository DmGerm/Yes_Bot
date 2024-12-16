using DNS_YES_BOT.Models;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class VoteController : ControllerBase
    {
        private Dictionary<long, VoteEntity> _votesStorage = [];

        [HttpGet] 
        public IActionResult Get()
        {
            return Ok(new { Message = "Hello from VoteController API!" });
        }
        [HttpPost]
        public IActionResult AddOrUpdateVote(long chatId, [FromBody] VoteEntity voteEntity)
        {
            if (voteEntity == null)
            {
                return BadRequest("VoteEntity is null.");
            }

            _votesStorage[chatId] = voteEntity;
            return Ok("Vote data added or updated.");
        }

        [HttpGet("{chatId:long}")]
        public IActionResult GetVoteResults(long chatId)
        {
            if (!_votesStorage.TryGetValue(chatId, out var voteEntity))
            {
                return NotFound($"No votes found for ChatId: {chatId}");
            }

            return Ok(voteEntity);
        }
    }
}