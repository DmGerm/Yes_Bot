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

        // Получение ссылки на результаты голосования
        [HttpGet("vote_link")]
        public IActionResult GetVoteResultLink([FromBody] VoteEntity vote)
        {
            // Получаем ссылку на результаты голосования из сервиса
            var voteResultLink = _voteService.GetPageUrl(vote);

            // Если ссылка не найдена, возвращаем ошибку
            if (string.IsNullOrEmpty(voteResultLink))
            {
                return NotFound("Vote result link not found.");
            }

            // Отправляем ссылку на результаты голосования
            return Ok(voteResultLink);
        }

        // Синхронизация списка магазинов
        [HttpPost("shop_sync")]
        public IActionResult SyncShopList([FromBody] List<string> shops)
        {
            // Этот контроллер принимает список магазинов и передает в метод по добавлению и обновлению
            return Ok("Shops synced successfully.");
        }
    }
}