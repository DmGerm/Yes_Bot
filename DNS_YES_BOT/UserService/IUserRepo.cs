namespace DNS_YES_BOT.UserService
{
    public interface IUserRepo
    {
        public Task AddUserIdAsync(long userId);
        public Task<bool> UserIdExistsAsync(long userId);
        public Task RemoveUserIdAsync(long userId);
        public Task<IEnumerable<long>> GetAllUserIdsAsync();

    }
}
