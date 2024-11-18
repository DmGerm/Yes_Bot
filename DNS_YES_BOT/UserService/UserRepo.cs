using System.Text.Json;

namespace DNS_YES_BOT.UserService
{
    internal class UserRepo : IUserRepo
    {
        //Добавить репозиторий пользователей, связать пользователей с магазинами по ID магазина
        private readonly HashSet<long> _adminsID = [];

        public UserRepo()
        {
            LoadUserIds();
        }

        public Task AddAdminAsync(long userId)
        {
            _adminsID.Add(userId);
            SaveUserIds();
            return Task.CompletedTask;
        }

        public Task<bool> UserIdExistsAsync(long userId) => Task.FromResult(_adminsID.Contains(userId));

        public Task RemoveUserIdAsync(long userId)
        {
            _adminsID.Remove(userId);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<long>> GetAllUserIdsAsync() => Task.FromResult(_adminsID.AsEnumerable());

        public Task<bool> UserListIsEmptyAsync() => Task.FromResult(_adminsID.Count == 0);

        private void LoadUserIds()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json")))
            {
                var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json"));
                _adminsID.Clear();
                _adminsID.UnionWith(JsonSerializer.Deserialize<HashSet<long>>(json) ?? []);
            }
        }

        private void SaveUserIds()
        {
            var json = JsonSerializer.Serialize(_adminsID);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json"), json);
        }
    }
}
