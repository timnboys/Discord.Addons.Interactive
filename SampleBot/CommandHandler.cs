using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace SampleBot
{
    public class CommandHandler
    {
        public static int Commands;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        public IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();

            _client.MessageReceived += _client_MessageReceived;
            _client.Ready += Client_Ready;
        }


        private async Task Client_Ready()
        {
            var application = await _client.GetApplicationInfoAsync();
            Console.WriteLine($"Invite: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591");
        }

        private async Task _client_MessageReceived(SocketMessage SocketMessage)
        {
            //Check to ensure that we are only receiving valid messages from users only!
            if (!(SocketMessage is SocketUserMessage message)) return;
            var context = new SocketCommandContext(_client, message);
            if (context.User.IsBot) return;

            var argPos = 0;
            //Ensure that we filter out all messages that do not start with the bot prefix

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(".", ref argPos))) return;


            var result = await _commands.ExecuteAsync(context, argPos, Provider);
            if (result.IsSuccess)
            {
                Console.WriteLine("Command Success");
            }
            else
            {
                try
                {
                    await context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = $"{context.User.Username.ToUpper()} ERROR",
                        Description = result.ErrorReason
                    }.Build());
                }
                catch
                {
                    //
                }
            }
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}