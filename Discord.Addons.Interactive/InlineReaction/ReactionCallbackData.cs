using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class ReactionCallbackData
    {
        private readonly ICollection<ReactionCallbackItem> items;

        public bool ExpiresAfterUse { get; }
        public bool SingleUsePerUser { get; }
        public List<ulong> ReactorIDs { get; }
        public string Text { get; }
        public Embed Embed { get; }
        public TimeSpan? Timeout { get; }
        public Func<SocketCommandContext, Task> TimeoutCallback { get; }
        public IEnumerable<ReactionCallbackItem> Callbacks => items;

        public ReactionCallbackData(string text, Embed embed = null, bool expuresafteruse = true, bool singleuseperuser = true, TimeSpan? timeout = null, Func<SocketCommandContext, Task> timeoutCallback = null)
        {
            SingleUsePerUser = singleuseperuser;
            ExpiresAfterUse = expuresafteruse;
            ReactorIDs = new List<ulong>();
            Text = text;
            Embed = embed;
            Timeout = timeout;
            TimeoutCallback = timeoutCallback;
            items = new List<ReactionCallbackItem>();
        }

        public ReactionCallbackData WithCallback(IEmote reaction, Func<SocketCommandContext, SocketReaction, Task> callback)
        {
            var item = new ReactionCallbackItem(reaction, callback);
            items.Add(item);
            return this;
        }
    }
}