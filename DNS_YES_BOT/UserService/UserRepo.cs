namespace DNS_YES_BOT.UserService
{
    internal class UserRepo : IUserRepo
    {
        private readonly HashSet<long> _userIds = new();

        public Task AddUserIdAsync(long userId)
        {
            _userIds.Add(userId);
            return Task.CompletedTask;
        }

        public Task<bool> UserIdExistsAsync(long userId) => Task.FromResult(_userIds.Contains(userId));

        public Task RemoveUserIdAsync(long userId)
        {
            _userIds.Remove(userId);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<long>> GetAllUserIdsAsync() => Task.FromResult(_userIds.AsEnumerable());
    }
}
