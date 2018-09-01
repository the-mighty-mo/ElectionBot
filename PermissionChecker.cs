using System.Threading.Tasks;
using Discord.WebSocket;

namespace ElectionBot
{
    public class PermissionChecker
    {
        public static bool HasSend(SocketGuild g, SocketTextChannel channel)
        {
            return g.GetUser(SecurityInfo.botClient).GetPermissions(channel).ReadMessages
                    && g.GetUser(SecurityInfo.botClient).GetPermissions(channel).SendMessages;
        }
    }
}
