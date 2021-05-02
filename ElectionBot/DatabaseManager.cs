using ElectionBot.Databases;
using System.Threading.Tasks;

namespace ElectionBot
{
    public static class DatabaseManager
    {
        public static readonly ElectionDatabase electionDatabase = new();

        public static async Task InitAsync() =>
            await Task.WhenAll(
                electionDatabase.InitAsync()
            );

        public static async Task CloseAsync() =>
            await Task.WhenAll(
                electionDatabase.CloseAsync()
            );
    }
}