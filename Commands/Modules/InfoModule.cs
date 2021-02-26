using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using MsgCountPlusNET;
using MsgCountPlusNET.json;

namespace MsgCountPlusNET.Commands.Modules
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("messages")]
        [Summary("Gives the amount of messages the user has sent in the current server")]
        public async Task messages([Summary("The user to get messages from, defaults to current user")] SocketUser user = null)
        {
            SocketUser User = user;
            if (User == null)
            {
                User = Context.User;
                if (User == null)
                {
                    return;
                }
            }
            List<DateTimeOffset> m = await MessageCollector.collectMessages(User, Context.Guild);
            int mC = m.Count;
            await Context.Channel.SendMessageAsync(User.Username + " has sent " + mC + " messages in this server.");
        }

        [Command("messages")]
        [Summary("Gives the amount of messages the user has sent in the current channel")]
        public async Task messages([Summary("The user to get messages from, defaults to current user")] SocketUser user = null, [Summary("The channel to get messages from, defaults to current channel")] ISocketMessageChannel channel = null)
        {
            SocketUser User = user;
            ISocketMessageChannel Channel = channel;
            if (User == null)
            {
                User = Context.User as SocketGuildUser;
            }
            if (Channel == null)
            {
                Channel = Context.Channel;
            }
            List<DateTimeOffset> m = await MessageCollector.collectMessages(User, Channel);
            int mC = m.Count;
            await Context.Channel.SendMessageAsync(user.Username + " has sent " + mC + " messages in <#" + Channel.Id + ">");
        }

        [Command("servermessages")]
        [Summary("Returns the amount of messages sent in the current server")]
        public async Task servermessages()
        {
            //List<IMessage> m = await MessageCollector.collectMessages(Context.Guild);
            var m = await MessageCollector.collectMessages(Context.Guild);
            int mC = m.Count();
            await Context.Channel.SendMessageAsync(mC + " messages have been sent in this server");
        }

        [Command("channelmessages")]
        [Summary("Returns the amount of messages sent in the given (or current) channel")]
        public async Task channelmessages([Summary("The channel to get messages from, defaults to current channel")] ISocketMessageChannel channel = null)
        {
            ISocketMessageChannel Channel = channel;
            if (Channel == null)
            {
                Channel = Context.Channel;
            }
            //List<IMessage> m = await MessageCollector.collectMessages(Channel);

            var m = await MessageCollector.collectMessages(Channel);
            int mC = m.Count();
            await Context.Channel.SendMessageAsync(mC + " messages have been sent in <#" + Channel.Id + ">");
        }

        [Command("score")]
        [Summary("Gives the user a score based on their messages / time")]
        public async Task score([Summary("The user to get the score of, defaults to current user")] SocketUser user = null)
        {
            SocketUser User = user;
            if (User == null)
            {
                User = Context.User;
            }
            List<DateTimeOffset> m = await MessageCollector.collectMessages(User, Context.Guild);
            int mC = m.Count;
            m.Sort();
            double d = Math.Abs(m.Last().Subtract(m.First()).TotalDays);
            int score = (int)((double)mC / d);
            await Context.Channel.SendMessageAsync("Your score is currently " + score);
        }

        [Command("indexserver")]
        [Alias("index", "serverindex", "pastindex", "fullpastindex", "fullindex", "fullserverindex")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task indexServer()
        {            
            //jSettings s = null;
            await Context.Channel.SendMessageAsync("Indexing...");
            //await MCPLUSDB.indexGuild(Context.Guild);
            if (Globals.requestDataWhere(item => item.Id == Context.Guild.Id).Count > 0)
            {
                //Guild exists
                IMongoCollection<jGuild> collection = Globals.globalData();
                await collection.FindOneAndDeleteAsync(item => item.Id == Context.Guild.Id);
            }
            jGuild g = new jGuild(Context.Guild, true);
            Globals.addGuild(g);
            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("invite")]
        [Summary("Get a link to invite this bot to another server")]
        public async Task invite()
        {
            await Context.Channel.SendMessageAsync("https://discordapp.com/api/oauth2/authorize?client_id=" + Tokens.token_clientID + "&permissions=268536912&scope=bot");
        }

        [Command("supportserver")]
        [Summary("Get a link to join the support server")]
        public async Task supportserver()
        {
            await Context.Channel.SendMessageAsync("Join the support server at: \n https://discord.gg/FPguN2g");
        }
        /**
         * These can no longer work because data is stored by Timestamp rather than with all message data.
         */
        //[Command("wordmessages")]
        //[Summary("Gets the amount of times the user has said a given phrase")]
        //public async Task wordmessages([Summary("The phrase or word to search for, phrases with spaces must be in quotes")] string phrase, [Summary("The user to get messages from")] SocketUser user = null)
        //{
        //    SocketUser User = user;
        //    if (User == null)
        //    {
        //        User = Context.User;
        //    }
        //    List<DateTimeOffset> l = await MessageCollector.collectMessages(User, Context.Guild);
        //    l = l.Where(item => item.Content.ToLower().Contains(phrase.ToLower())).ToList();
        //    await Context.Channel.SendMessageAsync(User.Username + " has said \"" + phrase + "\" " + l.Count + " times.");
        //}

        //[Command("serverwordmessages")]
        //[Summary("Gets the amount of times the given phrase has been said in this server")]
        //public async Task wordmessages([Summary("The phrase or word to search for, phrases with spaces must be in quotes")] string phrase)
        //{
        //    List<DateTimeOffset> l = await MessageCollector.collectMessages(Context.Guild);
        //    l = l.Where(item => item.Content.ToLower().Contains(phrase.ToLower())).ToList();
        //    await Context.Channel.SendMessageAsync("\"" + phrase + "\" has been said " + l.Count + " times in this server.");
        //}

        [Command("leaderboard")]
        [Alias("rankings", "rankingboard", "top", "topusers")]
        [Summary("Get a list of users ordered by total messages")]
        public async Task leaderboard([Summary("The amount of users to show")] int length = 10)
        {
            List<List<DateTimeOffset>> l = await MessageCollector.collectMessagesList(Context.Guild.Users.ToArray(), Context.Guild);
            List<SocketGuildUser> users = Context.Guild.Users.ToList();
            //l = l.OrderBy(item => item.Count).Reverse().ToList();
            List<List<DateTimeOffset>> orderedL = l.OrderBy(item => item.Count).Reverse().ToList();
            string output = "```";
            int offset = 0;
            for (int i = 0; i < length + offset && i < l.Count; i++)
            {
                List<DateTimeOffset> m = orderedL[i];
                SocketGuildUser u = users[l.IndexOf(m)];
                if (u.IsBot) {
                    offset++;
                    continue;
                }
                output += (i + 1 - offset) + ". " + u.Username + " | " + m.Count + " messages\n";
            }
            output += "```";

            await Context.Channel.SendMessageAsync(output);
        }
    }
}
