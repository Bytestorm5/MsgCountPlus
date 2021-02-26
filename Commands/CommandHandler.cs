using Discord;
using Discord.Commands;
using Discord.WebSocket;
//using MsgCountPlusNETF.Commands.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MsgCountPlusNET.Commands
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            //_client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }
        public async Task threadCommand(SocketMessage msg)
        {
            var TTh = new Thread(async delegate () { await this.HandleCommandAsync(msg); });
            TTh.Start();
        }
        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            //if (messageParam.Author.Id != 374280713387900938)
            //{
            //    return;
            //}
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            //MCPLUSDB.addMessageAsync(new SocketCommandContext(_client, message));
            //Backbone.LogMessage(message);

            // Create a number to track where the prefix ends and the command begins
            int argPos = 2;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix(Globals.CMD_PREFIX, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            if (Globals.isDebugging) {
                //await message.Channel.SendMessageAsync("Message Count + is not available at this time. Reason provided:\n" +
                //    "```The bot is currently in development mode and is unable to take your command. Please check back later.```");
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            Console.WriteLine("Command received: " + message.Content);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            try
            {
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: null);
            }
            catch (Exception ex)
            {
                //await Backbone.Log(ex.Message);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
