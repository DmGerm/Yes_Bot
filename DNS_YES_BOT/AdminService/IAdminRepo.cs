namespace DNS_YES_BOT.UserService
{
    public interface IAdminRepo
    {
        public Task AddAdminAsync(long userId);
        public Task<bool> UserIdExistsAsync(long userId);
        public Task RemoveUserIdAsync(long userId);
        public Task<List<long>> GetAllUserIdsAsync();
        public Task<bool> UserListIsEmptyAsync();

    }
}
