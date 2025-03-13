using System.Text.Json;

namespace DNS_YES_BOT.UserService
{
    public class AdminRepo : IAdminRepo
    {
        private readonly HashSet<long> _adminsID = [];

        public AdminRepo()
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

        public Task<List<long>> GetAllUserIdsAsync() => Task.FromResult(_adminsID.ToList());

        public Task<bool> UserListIsEmptyAsync() => Task.FromResult(_adminsID.Count == 0);

        private void LoadUserIds()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/userIds.json")))
            {
                var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/userIds.json"));
                _adminsID.Clear();
                _adminsID.UnionWith(JsonSerializer.Deserialize<HashSet<long>>(json) ?? []);
            }
        }

        private void SaveUserIds()
        {
            var json = JsonSerializer.Serialize(_adminsID);
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var filePath = Path.Combine(directory, "userIds.json");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(filePath, json);
        }
    }
}
