using System.Threading.Tasks;
using ElectionBot.Databases;

namespace ElectionBot
{
    public static class DatabaseManager
    {
        public static readonly VotersDatabase votersDatabase = new VotersDatabase();
        public static readonly ElectionDatabase electionDatabase = new ElectionDatabase();

        public static async Task InitAsync()
        {
            await Task.WhenAll(
                votersDatabase.InitAsync(),
                electionDatabase.InitAsync()
            );
        }

        public static async Task CloseAsync()
        {
            await Task.WhenAll(
                votersDatabase.CloseAsync(),
                electionDatabase.CloseAsync()
            );
        }
    }
}
