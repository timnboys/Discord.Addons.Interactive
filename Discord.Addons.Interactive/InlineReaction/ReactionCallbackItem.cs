using Discord.Commands;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class ReactionCallbackItem
    {
        public IEmote Reaction { get; }
        public Func<SocketCommandContext, SocketReaction, Task> Callback { get; }

        public ReactionCallbackItem(IEmote reaction, Func<SocketCommandContext, SocketReaction, Task> callback)
        {
            Reaction = reaction;
            Callback = callback;
        }
    }
}