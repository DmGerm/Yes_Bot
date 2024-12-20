using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.VoteService
{
    public interface IVoteService
    {
        //ToDo: Бот отправляет 3 сообщения. 1 Всего ответило магазинов N, 2. Не ответили магазины - list, 3. Ответили следующие участники по магазинам Магазин: name,name...
        public Task<bool> AddEntity(long chatId, string shopName, string userName);
        public Task<VoteEntity?> GetVoteEntityByChatId (long chatId);
        public Task<bool> RemoveEntity(long chatId);
        public Task<bool> CheckEntity(long chatId);
        Task<VoteEntity> GetResultsAsync(long id);
    }
}
