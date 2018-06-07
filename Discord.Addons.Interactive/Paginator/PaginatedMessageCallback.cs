using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }

        public TimeSpan? Timeout => options.Timeout;

        private readonly PaginatedMessage _pager;

        private PaginatedAppearanceOptions options => _pager.Options;
        private readonly int pages;
        private int page = 1;
        

        public PaginatedMessageCallback(InteractiveService interactive, 
            SocketCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;
            pages = _pager.Pages.Count();
        }

        public async Task DisplayAsync(ReactionList Reactions)
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                if (Reactions.First) await message.AddReactionAsync(options.First);
                if (Reactions.Backward) await message.AddReactionAsync(options.Back);
                if (Reactions.Forward) await message.AddReactionAsync(options.Next);
                if (Reactions.Last) await message.AddReactionAsync(options.Last);


                var manageMessages = Context.Channel is IGuildChannel guildChannel &&
                                     (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages;

                if (Reactions.Jump)
                {
                    if (options.JumpDisplayOptions == JumpDisplayOptions.Always || options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages)
                    {
                        await message.AddReactionAsync(options.Jump);
                    }
                }

                if (Reactions.Trash)
                {
                    await message.AddReactionAsync(options.Stop);
                }

                if (Reactions.Info)
                {
                    if (options.DisplayInformationIcon) await message.AddReactionAsync(options.Info);
                }
            });
            if (Timeout.HasValue)
            {
                Displaytimeout(message, Message);
            }
        }

        public void Displaytimeout(RestUserMessage M1, IUserMessage M2)
        {
            if (Timeout.HasValue)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(M1);
                    M2.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(options.First))
                page = 1;
            else if (emote.Equals(options.Next))
            {
                if (page >= pages)
                    return false;
                ++page;
            }
            else if (emote.Equals(options.Back))
            {
                if (page <= 1)
                    return false;
                --page;
            }
            else if (emote.Equals(options.Last))
                page = pages;
            else if (emote.Equals(options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, options.Stop.Name);
                        return;
                    }
                    page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, options.InformationText, timeout: options.InfoTimeout);
                return false;
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }
        
        protected Embed BuildEmbed()
        {
            var current = _pager.Pages.ElementAt(page - 1);

            var builder = new EmbedBuilder
            {
                Author = current.Author ?? _pager.Author,
                Title = current.Title ?? _pager.Title,
                Url = current.Url ?? _pager.Url,
                Description = current.Description ?? _pager.Description,
                ImageUrl = current.ImageUrl ?? _pager.ImageUrl,
                Color = current.Color ?? _pager.Color,
                Fields = current.Fields ?? _pager.Fields,
                Footer = current.FooterOverride ?? _pager.FooterOverride ?? new EmbedFooterBuilder
                {
                    Text = string.Format(options.FooterFormat, page, pages)
                },
                ThumbnailUrl = current.ThumbnailUrl ?? _pager.ThumbnailUrl,
                Timestamp = current.TimeStamp ?? _pager.TimeStamp
            };

            /*var builder = new EmbedBuilder()
                .WithAuthor(_pager.Author)
                .WithColor(_pager.Color)
                .WithDescription(_pager.Pages.ElementAt(page - 1).Description)
                .WithImageUrl(current.ImageUrl ?? _pager.DefaultImageUrl)
                .WithUrl(current.Url)
                .WithFooter(f => f.Text = string.Format(options.FooterFormat, page, pages))
                .WithTitle(current.Title ?? _pager.Title);*/
            builder.Fields = _pager.Pages.ElementAt(page - 1).Fields;

            return builder.Build();
        }
        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
