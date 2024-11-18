using System.Text.Json;

namespace DNS_YES_BOT.UserService
{
    internal class UserRepo : IUserRepo
    {
        private readonly HashSet<long> _userIds = [];

        public UserRepo()
        {
            LoadUserIds();
        }

        public Task AddUserIdAsync(long userId)
        {
            _userIds.Add(userId);
            SaveUserIds();
            return Task.CompletedTask;
        }

        public Task<bool> UserIdExistsAsync(long userId) => Task.FromResult(_userIds.Contains(userId));

        public Task RemoveUserIdAsync(long userId)
        {
            _userIds.Remove(userId);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<long>> GetAllUserIdsAsync() => Task.FromResult(_userIds.AsEnumerable());

        public Task<bool> UserListIsEmptyAsync() => Task.FromResult(_userIds.Count == 0);

        private void LoadUserIds()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json")))
            {
                var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json"));
                _userIds.Clear();
                _userIds.UnionWith(JsonSerializer.Deserialize<HashSet<long>>(json) ?? []);
            }
        }

        private void SaveUserIds()
        {
            var json = JsonSerializer.Serialize(_userIds);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json"), json);
        }
    }
}
