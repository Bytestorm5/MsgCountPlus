using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using System.IO;
using MsgCountPlusNET.json;
using MCPlusForm;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OfficeOpenXml;
using MsgCountPlusNETF.json;

namespace MsgCountPlusNET.Commands
{
    class LegacyCommandHandler
    {
        public void ExecuteCommand(SocketMessage message, string[] args, SocketCommandContext Context, DiscordSocketClient client)
        {
            Console.WriteLine("Command received: " + args[0].Remove(0, 3));
            //Check all commands
            try
            {
                if (args[0] == "mc+messages") { servermessages(message, Context); return; }
                if (args[0] == "mc+help") { help(message, args); return; }
                if (args[0] == "mc+servermessages") { servermessages(message, Context); return; }
                if (args[0] == "mc+score") { score(message, Context); return; }
                if (args[0] == "mc+activehours") { activehours(message, args, Context); return; }
                if (args[0] == "mc+invite") { invite(message); return; }
                if (args[0] == "mc+messagegraph") { messagegraph(message, Context, client); return; }
                if (args[0] == "mc+channelgraph") { channelgraph(message, Context, client); return; }
                if (args[0] == "mc+serveractivehours") { serveractivehours(message, args, Context); return; }
                if (args[0] == "mc+servermessagegraph") { servermessagegraph(message, Context, client); return; }
                if (args[0] == "mc+serverchannelgraph") { serverchannelgraph(message, Context, client); return; }
                if (args[0] == "mc+activity") { activity(message, args, Context, client); return; }
                if (args[0] == "mc+serveractivity") { serveractivity(message, args, Context, client); return; }
                if (args[0] == "mc+level") { level(message, Context); return; }
                if (args[0] == "mc+index") { index(message, Context); return; }
                if (args[0] == "mc+workyoustupidmachine") { workyoustupidmachine(message); return; }
                if (args[0] == "mc+supportserver") { supportserver(message); return; }
                if (args[0] == "mc+setcolors") { setcolors(message, args); return; }
                if (args[0] == "mc+colors") { colors(message); return; }
                if (args[0] == "mc+comparegraph") { comparegraph(message, args, Context, client); return; }
                if (args[0] == "mc+pastindex") { hardindex(message, args); return; }
                if (args[0] == "mc+fullpastindex") { fullhardindex(message, args, Context); return; }
                if (args[0] == "mc+reset") { resetMsg(message, Context); return; }
                if (args[0] == "mc+leaderboard") { leaderboard(message, args, Context); return; }
                if (args[0] == "mc+settings") {settings(message, Context); return;}
                if (args[0] == "mc+report") { buildReport(message, Context); return; }
                //Throw error
                else
                {
                    message.Channel.SendMessageAsync("Invalid command | Do mc+help to see all the commands");
                }
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception in command " + args[0] + " : " + exception.Message + "\n Stack: " + exception.StackTrace);
                message.Channel.SendMessageAsync("Internal Error | Do `mc+supportserver` to report this bug");
                //Globals.errorLogCh.SendMessageAsync("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Exception in command " + args[0] + " : " + exception.Message + "\n Stack: " + exception.StackTrace);
            }
        }
        public void supportserver(SocketMessage message)
        {
            message.Channel.SendMessageAsync("Join the support server at: \n https://discord.gg/FPguN2g");
        }
        public void help(SocketMessage message, string[] args)
        {
            if (args.Length < 2)
            {
                //message.Channel.SendMessageAsync("Command deprecated. Use `mc+help <command>` instead.");
                message.Channel.SendMessageAsync(">>> **Message count + Commands:** \n " +
                "`mc+messages` | Get the amount of messages you have sent in the current server \n" +
                "`mc+score` | Get your score, based on a ratio of messages / time \n" +
                "`mc+activehours [timezone] [user]` | Get scores by hour - Enter timezones in UTC format, for example UTC-4 (EST) would become -4 \n" +
                "`mc+invite` | Get the link to add this bot to your own server \n" +
                "`mc+activity [user]` | Get a line graph showing your or another person's messages since the first message logged \n" +
                "`mc+channelgraph [user]` | Get a pie chart showing which channels you are most active in \n" +
                "`mc+serveractivehours [timezone]` | same as Activehours, but for the whole server \n" +
                "`mc+activity [user]` | Get a graph of the activity of a user. Work in progress. \n" +
                "`mc+serveractivity` | Get a graph of the whole server's activity \n" +
                "`mc+level` | Get your level \n" +
                "`mc+index` | Index the server (Internal - prevents most errors from happening and gives better logging accuracy) \n" +
                "`mc+supportserver` | Get the invite to the Message Count + Support server \n" +
                "`mc+setcolors <background RGB> <graph/line RGB>` | Set custom colors for your graphs \n" +
                "`mc+colors [user]` | Get the custom colors set by the specified user.\n" +
                "`mc+comparegraph <interval> <2 or more users>` | Compare the activity of two or more users.\n" +
                "`mc+pastindex <amount>` | Indexes past messages. Requires Manage Messages permissions\n" +
                "`mc+fullpastindex <amount>` | Index Past messages in all channels. Requires Manage Messages Permissons");
            }
            else if(args[1] == "supportserver") { message.Channel.SendMessageAsync(">>> **mc+supportserver** \n Get the invite link to the Message Count + Support server. You will usually be sent to this command in the case of an Internal Error"); }
            else if (args[1] == "servermessages") { message.Channel.SendMessageAsync(">>> **servermessages [user]** \n Get the count of all the logged messages in this server for some user. You can also use `mc+messages` to get the same result."); }
            else if (args[1] == "score") { message.Channel.SendMessageAsync(">>> **mc+score [user]** \n Get the score for some user or yourself. Score is a representation of your activity, based on messages / time"); }
            else if (args[1] == "activehours") { message.Channel.SendMessageAsync(">>> **mc+activehours [timezone] [user]** \n Get the activehours for some user or yourself. \n **Timezone setting:** \n The timezone parameter is formatted as an offset from UTC time, and defaults to UTC. For example, giving `-4` as an argument will format the graph to UTC-4, or EST"); }
            else if (args[1] == "invite") { message.Channel.SendMessageAsync(">>> **mc+invite** \n Get the link to invite this bot to another server"); }
            else if (args[1] == "messagegraph") { message.Channel.SendMessageAsync(">>> **mc+messagegraph [user]** \n Get the message graph of another user or yourself. Message graphs show your progress toward your total message count."); }
            else if (args[1] == "channelgraph") { message.Channel.SendMessageAsync(">>> **mc+channelgraph [user]** \n Get a Pie Chart showing which channels you or some other user talk most in."); }
            else if (args[1] == "serveractivehours") { message.Channel.SendMessageAsync(">>> **mc+serveractivehours [timezone]** \n Get the activehours for the server as a whole. Useful for figuring out when to host server events. \n **Timezone setting:** \n The timezone parameter is formatted as an offset from UTC time, and defaults to UTC. For example, giving `-4` as an argument will format the graph to UTC-4, or EST"); }
            else if (args[1] == "servermessagegraph") { message.Channel.SendMessageAsync(">>> **mc+servermessagegraph** \n Get the message graph of the server as a whole. Message graphs show the server's progress toward its total message count."); }
            else if (args[1] == "serverchannelgraph") { message.Channel.SendMessageAsync(">>> **mc+serverchannelgraph** \n Get a Pie Chart showing which channels the server talks most in."); }
            else if (args[1] == "activity") { message.Channel.SendMessageAsync(">>> **mc+activity <interval> [user]** \n Get a graph of your or another person's activity over some interval \n **Intervals** \n There are 4 intervals you can use: hour, day, month, and year. These are represented by using the first letter of what interval you're using."); }
            else if (args[1] == "serveractivity") { message.Channel.SendMessageAsync(">>> **mc+serveractivity** \n Get a graph of the server's activity over some interval \n **Intervals** \n There are 4 intervals you can use: hour, day, month, and year. These are represented by using the first letter of what interval you're using."); }
            else if (args[1] == "level") { message.Channel.SendMessageAsync(">>> **mc+level** \n Get you or another person's level. Level is based on the amount of times you can take the square root of the user's score until it becomes smaller than 10."); }
            else if (args[1] == "index") { message.Channel.SendMessageAsync(">>> **mc+index** \n Index the server, makes logging a bit more efficient, though there is very little difference whether or not you index the server.\nRequires Manage Server Permissions\n ***FAIR WARNING: Indexing a server resets all user data on that server***"); }
            else if (args[1] == "messages") { message.Channel.SendMessageAsync(">>> **servermessages [user]** \n Get the count of all the logged messages in this server for some user. You can also use `mc+messages` to get the same result."); }
            else if (args[1] == "setcolors") { message.Channel.SendMessageAsync(">>> **mc+setcolors <background color RGB> <graph color RGB>** \n Set custom colors to use in graphs instead of the defualt colors \n **RGB Format** \n Colors are input as RGB, in the format `red,green,blue` where the words are replaced with some number ranging from 0 to 255. Adding a fourth number can add the Alpha channel, which adjusts the transparency of the color, where 0 is transparent, and 255 is opaque."); }
            else if (args[1] == "colors") { message.Channel.SendMessageAsync(">>> **mc+colors [user]** \n Get the custom colors you or another person has set"); }
            else if (args[1] == "comparegraph") { message.Channel.SendMessageAsync(">>> **mc+comparegraph**\nGet a graph comparing two user's activity. \nNote: Will not use custom line color, but will use custom background color.\n**Intervals**\nThere are 4 intervals you can use: hour, day, month, and year. These are represented by using the first letter of what interval you're using."); }
            else if (args[1] == "pastindex") { message.Channel.SendMessageAsync(">>> **mc+pastindex <amount>**\nIndex some amount of messages back, in the current channel\nRequires Manage Messages permissions"); }
            else if (args[1] == "fullpastindex") { message.Channel.SendMessageAsync(">>> **fullpastindex <amount>**\nIndex some amount of messages back for _all_ channels in the current guild\nRequires Manage Messages Permissions"); }
            else if (args[1] == "reset") { message.Channel.SendMessageAsync("**reset <user>**\nReset messages for some user. Requires Manage Messages permission."); }
            else
            {
                message.Channel.SendMessageAsync(">>> **Message count + Commands:** \n " +
                "`mc+messages` | Get the amount of messages you have sent in the current server \n" +
                "`mc+score` | Get your score, based on a ratio of messages / time \n" +
                "`mc+activehours [timezone] [user]` | Get scores by hour - Enter timezones in UTC format, for example UTC-4 (EST) would become -4 \n" +
                "`mc+invite` | Get the link to add this bot to your own server \n" +
                "`mc+activity [user]` | Get a line graph showing your or another person's messages since the first message logged \n" +
                "`mc+channelgraph [user]` | Get a pie chart showing which channels you are most active in \n" +
                "`mc+serveractivehours [timezone]` | same as Activehours, but for the whole server \n" +
                "`mc+activity [user]` | Get a graph of the activity of a user. Work in progress. \n" +
                "`mc+serveractivity` | Get a graph of the whole server's activity \n" +
                "`mc+level` | Get your level \n" +
                "`mc+index` | Index the server (Internal - prevents most errors from happening and gives better logging accuracy) \n" +
                "`mc+supportserver` | Get the invite to the Message Count + Support server \n" +
                "`mc+setcolors <background RGB> <graph/line RGB>` | Set custom colors for your graphs \n" +
                "`mc+colors [user]` | Get the custom colors set by the specified user.\n" +
                "`mc+comparegraph <interval> <2 or more users>` | Compare the activity of two or more users.\n" +
                "`mc+pastindex <amount>` | Indexes past messages. Requires Manage Messages permissions\n" +
                "`mc+fullpastindex <amount>` | Index Past messages in all channels. Requires Manage Messages Permissons");
            }
        }        
        public void servermessages(SocketMessage message, SocketCommandContext Context)
        {
            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            int count = 0;
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                count = count + user.Timestamps.Count;
            }
            
            message.Channel.SendMessageAsync("You have sent " + count.ToString() + " messages in this server");
        }
        public int internal_servermessages(SocketMessage message, SocketCommandContext Context)
        {
            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            int count = 0;
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                count = count + user.Timestamps.Count;
            }

            return count;
        }
        public void score(SocketMessage message, SocketCommandContext Context)
        {
            List<DateTimeOffset> thirdcup = new List<DateTimeOffset>();

            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            if (!currentGuild.settings.allowScoring) {
                return;
            }
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                foreach (DateTimeOffset timestamp in user.Timestamps)
                {
                    thirdcup.Add(timestamp);
                }
            }

            if (thirdcup != null)
            {
                thirdcup.Sort();
                double time = thirdcup.Last().Subtract(thirdcup.First()).TotalDays; //DateTime.Now.Subtract(thirdcup[0].DateTime).TotalDays;
                int count_ = thirdcup.Count;
                if ((int)time <= 0) { time = 1; }
                int score = count_ / (int)time;
                message.Channel.SendMessageAsync("Your score is currently: " + score.ToString());
            }
            else
            {
                message.Channel.SendMessageAsync("Your score is currently 0");
            }
        }
        public int internal_score(SocketMessage message, SocketCommandContext Context)
        {
            List<DateTimeOffset> thirdcup = new List<DateTimeOffset>();

            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                foreach (DateTimeOffset timestamp in user.Timestamps)
                {
                    thirdcup.Add(timestamp);
                }
            }

            if (thirdcup != null)
            {
                thirdcup.Sort();
                double time = thirdcup.Last().Subtract(thirdcup.First()).TotalDays; //DateTime.Now.Subtract(thirdcup[0].DateTime).TotalDays;
                int count_ = thirdcup.Count;
                if ((int)time <= 0) { time = 1; }
                int score = count_ / (int)time;
                return score;
            }
            else
            {
                return 0;
            }
        }
        public void level(SocketMessage message, SocketCommandContext Context)
        {
            //Rank Rewards
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            if (!currentGuild.settings.allowLevels)
            {
                return;
            }
            double Score = (double)internal_score(message, Context);
            int level = 0;
            for (int i = 0; true; i++)
            {
                if (Score <= 10)
                {
                    level = i;
                    break;
                }
                else
                {
                    Score = Math.Sqrt(Score);
                }
            }
            message.Channel.SendMessageAsync("You are currently at level " + level.ToString());
        }
        public int internal_level(SocketMessage message, SocketCommandContext Context)
        {
            //Rank Rewards
            double Score = (double)internal_score(message, Context);
            int level = 0;
            for (int i = 0; true; i++)
            {
                if (Score <= 10)
                {
                    level = i;
                    break;
                }
                else
                {
                    Score = Math.Sqrt(Score);
                }
            }
            return level;
        }
        public void activehours(SocketMessage message, string[] args, SocketCommandContext Context)
        {
            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            if (Globals.requestDataWhere(item => item.Id == Context.Guild.Id).Count() < 1)
            {
                message.Channel.SendMessageAsync("No data found for this server");
                return;
            }
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                foreach (DateTimeOffset timestamp in user.Timestamps)
                {
                    fullTimestamps.Add(timestamp);
                }
            }
            #endregion

            int timeOffset = 0;

            if (args.Length > 1 && args[1] != null)
            {
                    timeOffset = int.Parse(args[1]);
            }

            int[] hours = new int[24];

            // 0 = 12AM 23 = 11PM | Basically just military time

            foreach (DateTimeOffset time in fullTimestamps)
            {
                int locIndex = time.Hour + timeOffset;
                if (locIndex < 0)
                {
                    locIndex = 24 + locIndex;
                }
                if (locIndex >= 24)
                {
                    locIndex = 24 - locIndex;
                }
                hours[locIndex]++;
            }

            //Get earliest logged message
            DateTimeOffset smallestDate = fullTimestamps.Min(p => p);
            DateTimeOffset biggestDate = fullTimestamps.Max(p => p);

            double OtherTime = biggestDate.Subtract(smallestDate).TotalDays;
            for (int i = 0; i < hours.Length; i++)
            {
                if ((int)OtherTime <= 0)
                {
                    OtherTime = 1;
                }
                hours[i] = hours[i] / (int)OtherTime;
            }

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.HourGraph(hours, OutputID, userColor.bgColor, userColor.lineColor);
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void invite(SocketMessage message)
        {
            message.Channel.SendMessageAsync("https://discordapp.com/api/oauth2/authorize?client_id=" + Tokens.token_clientID + "&permissions=268536912&scope=bot");
        }
        public void messagegraph(SocketMessage message, SocketCommandContext Context, DiscordSocketClient client)
        {
            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                foreach (DateTimeOffset timestamp in user.Timestamps)
                {
                    fullTimestamps.Add(timestamp);
                }
            }
            #endregion
            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.MessageGraph(fullTimestamps.ToArray(), OutputID, userColor.bgColor, userColor.lineColor);
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void channelgraph(SocketMessage message, SocketCommandContext Context, DiscordSocketClient client)
        {
            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();

            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            List<string> cNames = new List<string>();
            List<int> cCounts = new List<int>();
            
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                if (cNames.Contains(channel.Name))
                {
                    cCounts[cNames.IndexOf(cNames.Find(item => item == channel.Name))] += user.Timestamps.Count;
                    continue;
                }
                else
                {
                    cNames.Add(channel.Name);
                    cCounts.Add(user.Timestamps.Count);
                }
                //cNames.Add(channel.Name);
                //cCounts.Add(user.Timestamps.Count);
            }

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.ChannelGraph(cNames.ToArray(), cCounts.ToArray(), OutputID, userColor.bgColor);
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void serveractivehours(SocketMessage message, string[] args, SocketCommandContext Context)
        {
            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            if (Globals.requestDataWhere(item => item.Id == Context.Guild.Id).Count() < 1)
            {
                message.Channel.SendMessageAsync("No data found for this server");
                return;
            }
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (jChannel channel in currentGuild.Channels)
            {
                foreach (jUser user in channel.Users)
                {
                    foreach (DateTimeOffset timestamp in user.Timestamps)
                    {
                        fullTimestamps.Add(timestamp);
                    }
                }
            }
            #endregion

            int timeOffset = 0;

            if (args.Length > 1 && args[1] != null)
            {
                    timeOffset = int.Parse(args[1]);
            }

            int[] hours = new int[24];

            // 0 = 12AM 23 = 11PM | Basically just military time

            foreach (DateTimeOffset time in fullTimestamps)
            {
                int locIndex = time.Hour + timeOffset;
                if (locIndex < 0)
                {
                    locIndex = 24 + locIndex;
                }
                if (locIndex >= 24)
                {
                    locIndex = 24 - locIndex;
                }
                hours[locIndex]++;
            }

            //Get earliest logged message
            DateTimeOffset smallestDate = fullTimestamps.Min(p => p);
            DateTimeOffset biggestDate = fullTimestamps.Max(p => p);

            double OtherTime = biggestDate.Subtract(smallestDate).TotalDays;
            for (int i = 0; i < hours.Length; i++)
            {
                if ((int)OtherTime <= 0)
                {
                    OtherTime = 1;
                }
                hours[i] = hours[i] / (int)OtherTime;
            }

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.HourGraph(hours, OutputID, userColor.bgColor, userColor.lineColor);
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void servermessagegraph(SocketMessage message, SocketCommandContext Context, DiscordSocketClient client)
        {
            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (jChannel channel in currentGuild.Channels)
            {
                foreach (jUser user in channel.Users)
                {
                    foreach (DateTimeOffset timestamp in user.Timestamps)
                    {
                        fullTimestamps.Add(timestamp);
                    }
                }
            }
            #endregion
            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.MessageGraph(fullTimestamps.ToArray(), OutputID, userColor.bgColor, userColor.lineColor);
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void serverchannelgraph(SocketMessage message, SocketCommandContext Context, DiscordSocketClient client)
        {
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();

            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            List<string> cNames = new List<string>();
            List<int> cCounts = new List<int>();

            foreach (jChannel channel in currentGuild.Channels)
            {
                foreach (jUser user in channel.Users)
                {
                    if (cNames.Contains(channel.Name))
                    {
                        cCounts[cNames.IndexOf(cNames.Find(item => item == channel.Name))] += user.Timestamps.Count;
                        continue;
                    }
                    else {
                        cNames.Add(channel.Name);
                        cCounts.Add(user.Timestamps.Count);
                    }
                }
            }

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.ChannelGraph(cNames.ToArray(), cCounts.ToArray(), OutputID, userColor.bgColor);
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void activity(SocketMessage message, string[] args, SocketCommandContext Context, DiscordSocketClient client)
        {
            if (args.Length < 2 || args[1] == null)
            {
                message.Channel.SendMessageAsync("You need to specify an interval! i.e: m = month, d = day, y = year, h = hour, w = week");
            }
            string interval = args[1];
            double repeat = 1;
            if (!args[1].EndsWith("m") && !args[1].EndsWith("d") && !args[1].EndsWith("y") && !args[1].EndsWith("h") && !args[1].EndsWith("w"))
            {
                message.Channel.SendMessageAsync("Invalid Interval; Valid intervals are m = month, d = day, y = year, h = hour, w = week");
                return;
            }
            else
            {
                interval = args[1].Last().ToString();
                CharHandler charHandler = new CharHandler();
                args[1] = charHandler.remLastChar(args[1]);
                if (args[1] == null || args[1] == "")
                {
                    repeat = 1;
                }
                else
                {
                    try
                    {
                        repeat = double.Parse(args[1]);
                    }
                    catch
                    {
                        message.Channel.SendMessageAsync("You have an invalid Character in your interval.");
                        return;
                    }
                }
            }

            if ((interval == "m" || interval == "y") && (int)repeat != repeat)
            {
                message.Channel.SendMessageAsync("Fractional intervals for months and/or years are not currently supported. Please only use whole numbers when working with years or months.");
                return;
            }

            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0) { targetUser = message.MentionedUsers.First(); }

            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            if (Globals.requestDataWhere(item => item.Id == Context.Guild.Id).Count() < 1)
            {
                message.Channel.SendMessageAsync("Error: No data for this server.");
                return;
            }

            message.Channel.SendMessageAsync("Command Received. Activity graphs can take time depending on how many messages are logged, so give it a bit. It should, however, finish in a few minutes at most.");
            IDisposable isTyping = message.Channel.EnterTypingState();

            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (jChannel channel in currentGuild.Channels)
            {
                if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                foreach (DateTimeOffset timestamp in user.Timestamps)
                {
                    fullTimestamps.Add(timestamp);
                }
            }
            #endregion

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.ActivityGraph(fullTimestamps.ToArray(), interval, OutputID, userColor.bgColor, userColor.lineColor, repeat);
            if (!File.Exists(OutputID + ".png"))
            {
                message.Channel.SendMessageAsync("Insufficient data for this interval, try a smaller interval if possible.");
                message.Channel.SendMessageAsync("`If you believe this is incorrect, report it at the support server (mc+supportserver)`");
                return;
            }
            message.Channel.SendFileAsync(OutputID + ".png");
            isTyping.Dispose();
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void serveractivity(SocketMessage message, string[] args, SocketCommandContext Context, DiscordSocketClient client)
        {
            if (args.Length < 2 || args[1] == null)
            {
                message.Channel.SendMessageAsync("You need to specify an interval! i.e: m = month, d = day, y = year, h = hour, w = week");
            }
            string interval = args[1];
            double repeat = 1;
            if (!args[1].EndsWith("m") && !args[1].EndsWith("d") && !args[1].EndsWith("y") && !args[1].EndsWith("h") && !args[1].EndsWith("w"))
            {
                message.Channel.SendMessageAsync("Invalid Interval; Valid intervals are m = month, d = day, y = year, h = hour, w = week");
                return;
            }
            else
            {
                interval = args[1].Last().ToString();
                CharHandler charHandler = new CharHandler();
                args[1] = charHandler.remLastChar(args[1]);
                if (args[1] == null || args[1] == "")
                {
                    repeat = 1;
                }
                else
                {
                    try
                    {
                        repeat = double.Parse(args[1]);
                    }
                    catch
                    {
                        message.Channel.SendMessageAsync("You have an invalid Character in your interval.");
                        return;
                    }
                }
            }

            if ((interval == "m" || interval == "y") && (int)repeat != repeat)
            {
                message.Channel.SendMessageAsync("Fractional intervals for months and/or years are not currently supported. Please only use whole numbers when working with years or months.");
                return;
            }

            message.Channel.SendMessageAsync("Command Received. Activity graphs can take time depending on how many messages are logged, so give it a bit. It should, however, finish in a few minutes at most.");
            IDisposable isTyping = message.Channel.EnterTypingState();

            List<DateTimeOffset> fullTimestamps = new List<DateTimeOffset>();
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (jChannel channel in currentGuild.Channels)
            {
                foreach (jUser user in channel.Users)
                {
                    foreach (DateTimeOffset timestamp in user.Timestamps)
                    {
                        fullTimestamps.Add(timestamp);
                    }
                }
            }
            #endregion

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.ActivityGraph(fullTimestamps.ToArray(), interval, OutputID, userColor.bgColor, userColor.lineColor, repeat);
            if (!File.Exists(OutputID + ".png"))
            {
                message.Channel.SendMessageAsync("Insufficient data for this interval, try a smaller interval if possible.");
                message.Channel.SendMessageAsync("`If you believe this is incorrect, report it at the support server (mc+supportserver)`");
                return;
            }
            else
            {
                message.Channel.SendFileAsync(OutputID + ".png");
            }
            isTyping.Dispose();
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public string generateOrderID(SocketMessage message)
        {
            string output = message.Author.Id.ToString();
            output = output + message.Author.Discriminator;
            output = output + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            return output;
        }
        public void internal_index(SocketGuild guild)
        {
            Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Index started - " + guild.Name + " | " + guild.Id.ToString());
            jGuild currentGuild = new jGuild();
            if (Globals.requestDataWhere(item => item.Id == guild.Id).Count() < 1)
            {
                currentGuild = new jGuild
                {
                    Name = guild.Name,
                    Id = guild.Id,
                    Channels = new List<jChannel>()
                };
                Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Guild created");
            }
            else
            {
                currentGuild = Globals.requestDataWhere(item => item.Id == guild.Id).First();
                currentGuild.Channels = null;
                currentGuild.Channels = new List<jChannel>();
                Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Guild Already found; Resetting");
            }
            foreach (SocketGuildChannel channel in guild.Channels)
            {
                currentGuild.Channels.Add(new jChannel {
                    Name = channel.Name,
                    Id = channel.Id,
                    Users = new List<jUser>()
                });
            }
            Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Guild Channels added");
            Globals.addGuild(currentGuild);
            Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Guild added to global data - Index Completed");
        }
        public void index(SocketMessage message, SocketCommandContext Context)
        {
            SocketGuildUser user = message.Author as SocketGuildUser;
            if (!user.GuildPermissions.ManageGuild)
            {
                message.Channel.SendMessageAsync("You do not have permission to use this command.\nRequired Permission: Manage Server");
                return;
            }
            message.Channel.SendMessageAsync("Indexing server... (This may take a while, depending on the size of the server");
            internal_index(Context.Guild);
            message.Channel.SendMessageAsync("Server Indexed.");
        }
        public void workyoustupidmachine(SocketMessage message)
        {
            Random random = new Random();
            int dice = random.Next() % 2;
            if (dice == 0)
            {
                message.Channel.SendMessageAsync("As you wish, mortal.");
            }
            if (dice == 1)
            {
                message.Channel.SendMessageAsync("No.");
            }
        }
        public void setcolors(SocketMessage message, string[] args)
        {
            if (args.Length < 3)
            {
                message.Channel.SendMessageAsync("You are missing arguments! Please check if you have put colors for both the background and line.");
            }
            jColor color = new jColor
            {
                Id = message.Author.Id,
                Name = message.Author.Username
            };

            string[] bgRGB = args[1].Split(',');
            string[] lineRGB = args[2].Split(',');

            if (bgRGB.Length < 3)
            {
                message.Channel.SendMessageAsync("Not enough values provided for: Background Color");
                return;
            }
            if (lineRGB.Length < 3)
            {
                message.Channel.SendMessageAsync("Not enough values provided for: Line Color");
                return;
            }

            if (bgRGB.Length > 3)
            {
                color.bgColor = new byte[] { byte.Parse(bgRGB[0]), byte.Parse(bgRGB[1]), byte.Parse(bgRGB[2]), byte.Parse(bgRGB[3]) };
            }
            else
            {
                color.bgColor = new byte[] { byte.Parse(bgRGB[0]), byte.Parse(bgRGB[1]), byte.Parse(bgRGB[2]), 255 };
            }
            if (lineRGB.Length > 3)
            {
                color.lineColor = new byte[] { byte.Parse(lineRGB[0]), byte.Parse(lineRGB[1]), byte.Parse(lineRGB[2]), byte.Parse(lineRGB[3]) };
            }
            else
            {
                color.lineColor = new byte[] { byte.Parse(lineRGB[0]), byte.Parse(lineRGB[1]) , byte.Parse(lineRGB[2]) , 255};
            }

            Globals.globalColors.RemoveAll(item => item.Id == message.Author.Id);
            Globals.globalColors.Add(color);
            message.Channel.SendMessageAsync("Color set Successfully.");
        }
        public void colors(SocketMessage message)
        {
            SocketUser targetUser = message.Author;
            if (message.MentionedUsers.Count > 0)
            {
                targetUser = message.MentionedUsers.First();
            }
            if (Globals.globalColors.Where(item => item.Id == targetUser.Id).Count() == 0)
            {
                message.Channel.SendMessageAsync("You currently have default color settings");
            }
            else
            {
                jColor color = Globals.globalColors.Where(item => item.Id == targetUser.Id).First();
                message.Channel.SendMessageAsync($"**Colors for {message.Author.Username}** \n" +
                    $"Background: `{color.bgColor[0]},{color.bgColor[1]},{color.bgColor[2]},{color.bgColor[3]}`\n" +
                    $"Line/Graph: `{color.lineColor[0]},{color.lineColor[1]},{color.lineColor[2]},{color.lineColor[3]}`");
            }
        }
        public jColor internal_colors(SocketMessage message)
        {
            if (Globals.globalColors.Where(item => item.Id == message.Author.Id).Count() == 0)
            {
                return new jColor {
                    Name = message.Author.Username,
                    Id = message.Author.Id,
                    bgColor = new byte[] {245, 245, 220, 255},
                    lineColor = new byte[] {78, 154, 6, 255}
                };
            }
            else
            {
                return Globals.globalColors.Where(item => item.Id == message.Author.Id).First();
            }
        }
        public void comparegraph(SocketMessage message, string[] args, SocketCommandContext Context, DiscordSocketClient client)
        {
            if (args.Length < 2 || args[1] == null)
            {
                message.Channel.SendMessageAsync("You need to specify an interval! i.e: m = month, d = day, y = year, h = hour, w = week");
            }
            string interval = args[1];
            double repeat = 1;
            if (!args[1].EndsWith("m") && !args[1].EndsWith("d") && !args[1].EndsWith("y") && !args[1].EndsWith("h") && !args[1].EndsWith("w"))
            {
                message.Channel.SendMessageAsync("Invalid Interval; Valid intervals are m = month, d = day, y = year, h = hour, w = week");
                return;
            }
            else
            {
                interval = args[1].Last().ToString();
                CharHandler charHandler = new CharHandler();
                args[1] = charHandler.remLastChar(args[1]);
                if (args[1] == null || args[1] == "")
                {
                    repeat = 1;
                }
                else
                {
                    try
                    {
                        repeat = double.Parse(args[1]);
                    }
                    catch
                    {
                        message.Channel.SendMessageAsync("You have an invalid Character in your interval.");
                        return;
                    }
                }
            }

            if ((interval == "m" || interval == "y") && (int)repeat != repeat)
            {
                message.Channel.SendMessageAsync("Fractional intervals for months and/or years are not currently supported. Please only use whole numbers when working with years or months.");
                return;
            }
            IReadOnlyCollection<SocketUser> targetUsers = message.MentionedUsers;
            if(targetUsers.Count < 2)
            {
                message.Channel.SendMessageAsync("Please specify 2 or more users to compare");
                return;
            }

            List<List<DateTimeOffset>> fullTimestamps = new List<List<DateTimeOffset>>();
            List<string> Names = new List<string>();
            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();
            #region Get Full Timestamp array
            foreach (SocketUser targetUser in targetUsers)
            {
                List<DateTimeOffset> userMessages = new List<DateTimeOffset>();
                foreach (jChannel channel in currentGuild.Channels)
                {
                    if (channel.Users.Where(item => item.Id == targetUser.Id).Count() < 1) { continue; }
                    jUser user = channel.Users.Where(item => item.Id == targetUser.Id).First();
                    foreach (DateTimeOffset timestamp in user.Timestamps)
                    {
                        userMessages.Add(timestamp);
                    }
                }
                fullTimestamps.Add(userMessages);
                Names.Add(targetUser.Username);
            }
            #endregion

            GraphHandler graphhandler = new GraphHandler();
            string OutputID = generateOrderID(message);
            jColor userColor = internal_colors(message);
            graphhandler.CompareGraph(fullTimestamps, interval, OutputID, userColor.bgColor, Names.ToArray(), repeat);
            if (!File.Exists(OutputID + ".png"))
            {
                message.Channel.SendMessageAsync("Insufficient data for this interval, try a smaller interval if possible.");
                message.Channel.SendMessageAsync("`If you believe this is incorrect, report it at the support server (mc+supportserver)`");
                return;
            }
            message.Channel.SendFileAsync(OutputID + ".png");
            System.Threading.Thread.Sleep(1000);
            File.Delete(OutputID + ".png");
        }
        public void hardindex(SocketMessage message, string[] args)
        {
            SocketGuildUser author = message.Author as SocketGuildUser;
            if (author == null)
            {
                return;
            }
            if (!author.GuildPermissions.ManageMessages)
            {
                message.Channel.SendMessageAsync("You do not have permission to use this command.\n(Required Permission: `Manage Messages`)");
            }
            if (args.Length < 2)
            {
                message.Channel.SendMessageAsync("Please specify how many messages back you want to index.\nIf you want to index _all_ messages, search for messages after some very early date, and look for something that says `[number] results`. Inputting that will then index all the messages in the channel.");
                return;
            }
            message.Channel.SendMessageAsync("Indexing...\nDepending on the amount of messages you are indexing, this can take a very long time. ~1 million messages takes around half an hour to 40 minutes.");
            //Globals.runUpdates = false;
            var messages = message.Channel.GetMessagesAsync(int.Parse(args[1])).ToEnumerable();
            int errors = 0;

            foreach (IReadOnlyCollection<IMessage> MessageCollection in messages)
            {
                foreach (IMessage IntMessage in MessageCollection)
                {
                    try
                    {
                        Backbone.LogMessage(IntMessage);
                    }
                    catch (Exception e) {
                        errors++;
                        continue;
                    }
                }
            }
            //Globals.runUpdates = true;
            Console.WriteLine("Index Complete with " + errors + " errors");
        }
        public void fullhardindex(SocketMessage message, string[] args, SocketCommandContext Context)
        {
            SocketGuildUser author = message.Author as SocketGuildUser;
            if (author == null)
            {
                return;
            }
            if (!author.GuildPermissions.ManageMessages)
            {
                message.Channel.SendMessageAsync("You do not have permission to use this command.\n(Required Permission: `Manage Messages`)");
            }
            if (args.Length < 2)
            {
                message.Channel.SendMessageAsync("Please specify how many messages back you want to index.\nIf you want to index _all_ messages, search for messages after some very early date, and look for something that says `[number] results`. Inputting that will then index all the messages in the channel.");
                return;
            }
            message.Channel.SendMessageAsync("Indexing...\nDepending on the amount of messages you are indexing, this can take a very long time. ~1 million messages takes around half an hour to 40 minutes.");
            //Globals.runUpdates = false;
            int errors = 0;
            foreach (SocketChannel channel in Context.Guild.Channels)
            {
                ISocketMessageChannel Channel = channel as ISocketMessageChannel;
                if (Channel == null) { continue; }
                var messages = Channel.GetMessagesAsync(int.Parse(args[1])).ToEnumerable();

                foreach (IReadOnlyCollection<IMessage> MessageCollection in messages)
                {
                    foreach (IMessage IntMessage in MessageCollection)
                    {
                        try
                        {
                            Backbone.LogMessage(IntMessage);
                        }
                        catch (Exception e) {
                            errors++;
                            continue;
                        }
                    }
                }
            }
            message.Channel.SendMessageAsync("Index Complete with " + errors + " errors");
            //Globals.runUpdates = true;
        }
        public void settings(SocketMessage message, SocketCommandContext Context)
        {
            jGuild targetGuild = Globals.requestDataFind(item => item.Id == Context.Guild.Id);
            SocketGuildUser targetUser = message.Author as SocketGuildUser;

            if (!targetUser.GuildPermissions.ManageMessages) {
                message.Channel.SendMessageAsync("You lack the required permissions: `Manage Messages`");
                return;
            }

            targetUser.SendMessageAsync("Welcome to Message Count + settings. \nType CANCEL at any time to revert the settings and exit settings, or EXIT to save the settings you set and exit settings. \nType START to begin.");
            settingsUser user = new settingsUser
            {
                name = targetUser.Username,
                Id = targetUser.Id,
                step = 0,
                waitForResponse = true,
                guild = targetGuild.Id,
                priorSettings = targetGuild.settings
            };
            Globals.openSettingsChannels.Add(user);

            #region Old
            //var eb = new EmbedBuilder();
            //eb.WithTitle("Message Count + Settings");
            //eb.AddField("Levels allowed", targetGuild.settings.allowLevels);
            //eb.AddField("Scoring Enabled", targetGuild.settings.allowScoring);
            //eb.AddField("`Messages / Time` scoring enabled", targetGuild.settings.specialScoreSwitch);
            //eb.WithFooter("Click the reactions to adjust the settings");

            //IUserMessage set = message.Channel.SendMessageAsync("", false, eb.Build()).Result;
            //int timeOut = 0;
            //set.AddReactionAsync(new Emoji("🇱"));
            //set.AddReactionAsync(new Emoji("🇸"));
            //set.AddReactionAsync(new Emoji("🇹"));
            //while (timeOut <= 480) {
            //    System.Threading.Thread.Sleep(250);

            //    //if (set.Reactions.Count > 3)
            //    //{
            //    //    //List<IEmote> reactions = set.Reactions.Where(item => item.);
            //    //}
            //    int tester;
            //    try
            //    {
            //        tester = set.Reactions[new Emoji("🇱")].ReactionCount;
            //        tester = set.Reactions[new Emoji("🇸")].ReactionCount;
            //        tester = set.Reactions[new Emoji("🇹")].ReactionCount;
            //    }
            //    catch (Exception ex) {
            //        continue;
            //    }

            //    if (set.Reactions[new Emoji("🇱")].ReactionCount > 1) {
            //        targetGuild.settings.allowLevels = !targetGuild.settings.allowLevels;
            //        eb.Fields.Find(item => item.Name == "Leveling Enabled").Value = targetGuild.settings.allowLevels;
            //        set.RemoveAllReactionsAsync();
            //        set.AddReactionAsync(new Emoji("🇱"));
            //        set.AddReactionAsync(new Emoji("🇸"));
            //        set.AddReactionAsync(new Emoji("🇹"));
            //    }
            //    if (set.Reactions[new Emoji("🇸")].ReactionCount > 1)
            //    {
            //        targetGuild.settings.allowScoring = !targetGuild.settings.allowScoring;
            //        eb.Fields.Find(item => item.Name == "Scoring Enabled").Value = targetGuild.settings.allowScoring;
            //        set.RemoveAllReactionsAsync();
            //        set.AddReactionAsync(new Emoji("🇱"));
            //        set.AddReactionAsync(new Emoji("🇸"));
            //        set.AddReactionAsync(new Emoji("🇹"));
            //    }
            //    if (set.Reactions[new Emoji("🇹")].ReactionCount > 1)
            //    {
            //        targetGuild.settings.specialScoreSwitch = !targetGuild.settings.specialScoreSwitch;
            //        eb.Fields.Find(item => item.Name == "Messages / Time` scoring enabled").Value = targetGuild.settings.specialScoreSwitch;
            //        set.RemoveAllReactionsAsync();
            //        set.AddReactionAsync(new Emoji("🇱"));
            //        set.AddReactionAsync(new Emoji("🇸"));
            //        set.AddReactionAsync(new Emoji("🇹"));
            //    }

            //    //eb.Fields.Find(item => item.Name == "Scoring Enabled").Value = !targetGuild.settings.allowScoring;
            //    //targetGuild.settings.allowScoring = !targetGuild.settings.allowScoring;
            //    set.ModifyAsync(msg => msg.Embed = eb.Build());
            //    timeOut++;
            //}
            #endregion
        }
        public void buildReport(SocketMessage message, SocketCommandContext Context) {
            jGuild targetGuild = Globals.requestDataFind(item => item.Id == Context.Guild.Id);

            message.Channel.SendMessageAsync("Note: This command is still in beta and there may still be issues");

            ExcelPackage report = new ExcelPackage();
            report.Workbook.Worksheets.Add("Activity");
            var activitySheet = report.Workbook.Worksheets["Activity"];
            report.Workbook.Worksheets.Add("Evaluations");
            var evalSheet = report.Workbook.Worksheets["Evaluations"];
            
            var header = new List<string[]> {
                new string[] { "Activity for " + targetGuild.Name,"", DateTime.Now.Month.ToString(),"", DateTime.Now.AddMonths(-1).Month.ToString(),"", DateTime.Now.AddMonths(-2).Month.ToString(),"", DateTime.Now.AddMonths(-3).Month.ToString(),""},
                new string[] { "Name", "ID","Activity Levels","Note", "Activity Levels", "Note", "Activity Levels", "Note","Activity Levels","Note"}
            };
            string headerRange = "A1:" + Char.ConvertFromUtf32(header[0].Length + 64) + "2";
            activitySheet.Cells[headerRange].LoadFromArrays(header);
            activitySheet.Cells["A2:N2"].Style.Font.Bold = true;
            activitySheet.Cells["A1"].Style.Font.Size = 18;

            var header2 = new List<string[]> {
                new string[] { "Change in Activity for " + targetGuild.Name},
                new string[] { "Name", "ID","Average Activity","Change from " + monthString(DateTime.Now.AddMonths(-3).Month) + " to " + monthString(DateTime.Now.AddMonths(-2).Month), monthString(DateTime.Now.AddMonths(-2).Month) + " to " + monthString(DateTime.Now.AddMonths(-1).Month), monthString(DateTime.Now.AddMonths(-1).Month) + " to " + monthString(DateTime.Now.Month)}
            };
            string headerRange2 = "A1:" + Char.ConvertFromUtf32(header2[0].Length + 64) + "2";
            evalSheet.Cells[headerRange2].LoadFromArrays(header2);
            evalSheet.Cells["A2:N2"].Style.Font.Bold = true;
            evalSheet.Cells["A1"].Style.Font.Size = 18;

            List<jUser> members = targetGuild.Members();
            List<object[]> acti = new List<object[]>();
            List<object[]> eval = new List<object[]>();
            int r = 3;
            foreach (jUser member in members)
            {
                List<object> memberData = new List<object>();

                //
                //ACTIVITY SHEET
                //

                //memberData.Add(member.Name); //A
                //memberData.Add(member.Id.ToString()); //B
                //memberData.Add(member.Timestamps.Where(item => item.Month == DateTime.Now.Month && item.Year == DateTime.Now.Year).Count()); //C
                ////memberData.Add("IFS(C"+r+">=800,\"Active\",C"+r+"<=400,\"Inactive\",AND(400<=C"+r+",C"+r+"<=800),\"Semi - active\")"); // D
                //memberData.Add(null);
                //memberData.Add(member.Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-1).Month && item.Year == DateTime.Now.AddMonths(-1).Year).Count()); //E
                ////memberData.Add("IFS(E" + r + ">=800,\"Active\",E" + r + "<=400,\"Inactive\",AND(400<=E" + r + ",E" + r + "<=800),\"Semi - active\")"); //F
                //memberData.Add(null);
                //memberData.Add(member.Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-2).Month && item.Year == DateTime.Now.AddMonths(-2).Year).Count()); //G
                ////memberData.Add("IFS(G" + r + ">=800,\"Active\",G" + r + "<=400,\"Inactive\",AND(400<=G" + r + ",G" + r + "<=800),\"Semi - active\")"); //H
                //memberData.Add(null);
                //memberData.Add(member.Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-3).Month && item.Year == DateTime.Now.AddMonths(-3).Year).Count()); //I
                ////memberData.Add("IFS(I" + r + ">=800,\"Active\",I" + r + "<=400,\"Inactive\",AND(400<=I" + r + ",I" + r + "<=800),\"Semi - active\")"); //J
                //memberData.Add(null);

                acti.Add(memberData.ToArray());

                //
                //CHANGE/EVAL SHEET
                //

                
                List<object> evalData = new List<object>();
                evalData.Add(member.Name);
                evalData.Add(member.Id.ToString());
                //memberData.Add(String.Empty);
                //memberData.Add(String.Empty);
                //memberData.Add(String.Empty);
                //memberData.Add(String.Empty);

                //evalData.Add("AVERAGE(Activity!C" + r + ",Activity!E" + r + ",Activity!G" + r + ",Activity!I" + r + ")");
                //evalData.Add("SUM(Activity!E" + r + "-Activity!C" + r + ")");
                //evalData.Add("SUM(Activity!G" + r + "-Activity!E" + r + ")");
                //evalData.Add("SUM(Activity!I" + r + "-Activity!G" + r + ")");

                eval.Add(evalData.ToArray());
                r++;
            }
            //activitySheet.Cells[3, 1].LoadFromArrays(acti);
            evalSheet.Cells[3, 1].LoadFromArrays(eval);
            
            for (int i = 3; i < r; i++) {
                //cellToFormula(activitySheet.Cells[i, 4]);
                //cellToFormula(activitySheet.Cells[i, 6]);
                //cellToFormula(activitySheet.Cells[i, 8]);
                //cellToFormula(activitySheet.Cells[i, 10]);
                //if (i - 3 > 158) {
                //    int h = 0;
                //}
                //cellToFormula(evalSheet.Cells[i, 3]);
                //cellToFormula(evalSheet.Cells[i, 4]);
                //cellToFormula(evalSheet.Cells[i, 5]);
                //cellToFormula(evalSheet.Cells[i, 6]);


                //activitySheet.Cells[i, 10].Formula = "AVERAGE(Activity!C" + i + ",Activity!E" + i + ",Activity!G" + i + ",Activity!I" + i + ")";

                activitySheet.Cells[i, 1].Value = members[i - 3].Name;
                activitySheet.Cells[i, 2].Value = members[i - 3].Id.ToString();                
                activitySheet.Cells[i, 3].Value = members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.Month && item.Year == DateTime.Now.Year).Count();
                activitySheet.Cells[i, 4].Value = activityString(members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.Month && item.Year == DateTime.Now.Year).Count());
                activitySheet.Cells[i, 5].Value = members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-1).Month && item.Year == DateTime.Now.AddMonths(-1).Year).Count();
                activitySheet.Cells[i, 6].Value = activityString(members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-1).Month && item.Year == DateTime.Now.AddMonths(-1).Year).Count());
                activitySheet.Cells[i, 7].Value = members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-2).Month && item.Year == DateTime.Now.AddMonths(-2).Year).Count();
                activitySheet.Cells[i, 8].Value = activityString(members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-2).Month && item.Year == DateTime.Now.AddMonths(-2).Year).Count());
                activitySheet.Cells[i, 9].Value = members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-3).Month && item.Year == DateTime.Now.AddMonths(-3).Year).Count();
                activitySheet.Cells[i, 10].Value = activityString(members[i - 3].Timestamps.Where(item => item.Month == DateTime.Now.AddMonths(-3).Month && item.Year == DateTime.Now.AddMonths(-3).Year).Count());

                evalSheet.Cells[i, 3].Formula = "AVERAGE(Activity!C" + i + ",Activity!E" + i + ",Activity!G" + i + ",Activity!I" + i + ")";
                evalSheet.Cells[i, 4].Formula = "SUM(Activity!E" + i + "-Activity!C" + i + ")";
                evalSheet.Cells[i, 5].Formula = "SUM(Activity!G" + i + "-Activity!E" + i + ")";
                evalSheet.Cells[i, 5].Formula = "SUM(Activity!I" + i + "-Activity!G" + i + ")";

                //activitySheet.Cells[i, 12].Formula = "SUM(C" + i + ", E" + i + ", G" + i + ", I" + i + ")";
            }

            report.Workbook.FullCalcOnLoad = true;
            report.Workbook.CalcMode = ExcelCalcMode.Automatic;
            activitySheet.Calculate();
            evalSheet.Calculate();
            
            string fileName = generateOrderID(message) + ".xlsx";
            FileInfo reportFile = new FileInfo(fileName);
            report.SaveAs(reportFile);
            message.Channel.SendFileAsync(fileName);
            System.Threading.Thread.Sleep(1000);
            reportFile.Delete();
        }
        public string activityString(int count) {
            if (count >= 1000)
            {
                return "Activity";
            }
            else if (count >= 500)
            {
                return "Semi-Active";
            }
            else {
                return "Inactive";
            }
        }
        public void resetMsg(SocketMessage message, SocketCommandContext Context) {
            SocketUser target = null;
            SocketGuildUser user = message.Author as SocketGuildUser;
            if (!user.GuildPermissions.ManageMessages)
            {
                message.Channel.SendMessageAsync("You do not have permission to use this command.\nRequired Permission: Manage Messages");
                return;
            }
            if (message.MentionedUsers.Count > 0) {
                target = message.MentionedUsers.First();
                if (target.IsBot) { return; }
            }
            if (target == null) { return; }
            jGuild targetGuild = Globals.requestDataFind(item => item.Id == Context.Guild.Id);
            foreach (jChannel channel in targetGuild.Channels) {
                channel.Lookup(target.Id).Timestamps = new List<DateTimeOffset>();
            }
        }
        public void leaderboard(SocketMessage message, string[] args, SocketCommandContext Context)
        {
            int n = 5;
            if (args.Length > 1) {
                if (int.TryParse(args[1], out n))
                {
                }
                else {
                    n = 5;
                }
            }
            jGuild targetGuild = Globals.requestDataFind(item => item.Id == Context.Guild.Id);
            List<jUser> Members = targetGuild.Members();

            //SortedList<int, string> CountData = new SortedList<int, string>();
            List<string> countNames = new List<string>();
            List<int> countCounts = new List<int>();

            foreach (jUser Member in Members) {
                //CountData.Add(Member.Timestamps.Count, Member.Name);
                countNames.Add(Member.Name);
                countCounts.Add(Member.Timestamps.Count);
            }

            string output = "**Leaderboard for " + targetGuild.Name + "**\n" +
                "```";
            for (int i = 1; i < n+1; i++) {
                if (countCounts.Count == 0) {
                    break;
                }
                string name = countNames[countCounts.IndexOf(countCounts.Max())];
                countNames.Remove(name);
                countCounts.Remove(countCounts.Max());
                //CountData[0].GetByIndex();
                output += i + " :: " + name + "\n";
            }
            output += "```";
            message.Channel.SendMessageAsync(output);
        }
        public string monthString(int month) {
            switch (month) {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    return null;
            }
        }
        public void cellToFormula(ExcelRange cell) {
            string store = cell.Text;
            cell.Value = "";
            cell.Formula = store;
        }
    }
}
