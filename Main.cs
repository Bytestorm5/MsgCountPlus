using System;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Net.Http;
using MsgCountPlusNET.json;
using System.Collections.Generic;
using MsgCountPlusNET.Commands;
using System.Runtime.InteropServices;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MsgCountPlusNETF.json;
using System.Linq.Expressions;
using System.Diagnostics;

namespace MsgCountPlusNET
{
    public static class Globals
    {        
        public static readonly bool isDebugging = false;

        public static bool Closing = false;
        public static int MAX_THREADS = 32;
        public static string CMD_PREFIX = "mc+";

        //public static List<jGuild> globalData = new List<jGuild>();
        //List of color preferences per user
        public static List<jColor> globalColors = new List<jColor>();
        public static List<settingsUser> openSettingsChannels = new List<settingsUser>();
        //public static bool runUpdates = true;

        public static IMongoClient mongoClient = new MongoClient();
        public static IMongoDatabase db = mongoClient.GetDatabase("MCPlusDB");

        //public static ISocketMessageChannel errorLogCh;

        public static IMongoCollection<jGuild> globalData() {
            return Globals.db.GetCollection<jGuild>("globalData");
        }

        public static List<ulong> unIndexedIDs = new List<ulong>();

        //Server performance stuff, doesn't seem to help much but I'm saving these
        //for a rainy day
        public static bool isCPUOverloaded() {
            PerformanceCounter total_cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float p = total_cpu.NextValue();
            Thread.Sleep(500);
            p = total_cpu.NextValue();
            //Console.WriteLine(@"##### CPU AT " + p + "%");
            return p >= 90;
        }
        public static void waitForLessLoad() {
            if (Globals.isCPUOverloaded()) { Console.WriteLine(@"///\\\ CPU Overloaded, pausing new threads ///\\\"); }
            while (Globals.isCPUOverloaded())
            {
                Thread.Sleep(5);
            }
        }

        //Amount of times the generateUniqueID() command has been called. USed to ensure that everything is unique aside form general randomness
        private static int uses = 0;
        public static List<Thread> threads = new List<Thread>();

        //Generates a unique id in string form
        public static string generateUniqueID()
        {
            string output = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond;
            Random r = new Random();
            int r2 = (int)(r.NextDouble() * r.Next());
            output += r2;
            if (r.Next() % 2 == 0)
            {
                output += DateTime.Today.ToLongDateString();
            }
            else
            {
                output += DateTime.IsLeapYear(r.Next(1, 9998));
            }
            output += uses;
            uses++;
            return output;
        }
        
        /**A bunch of stuff for making mongo easier. Did not make mongo easier. */

        //public static IMongoCollection<jGuild> globalData = Globals.db.GetCollection<jGuild>("globalData")
        public async static Task<List<jGuild>> requestDataWhereAsync(Expression<Func<jGuild, bool>> p) {
            //IMongoCollection<jGuild> globalData = Globals.db.GetCollection<jGuild>("globalData");
            IMongoCollection<jGuild> globalData = Globals.db.GetCollection<jGuild>("globalData");
            //Expression<Func<jGuild, bool>> p1 = new Expression<Func<jGuild, bool>>(p);
            var result = globalData.Find(p).ToList();
            return result;
        }
        public static async Task<List<jGuild>> GetAllGuilds()
        {
            IMongoCollection<jGuild> globalData = Globals.db.GetCollection<jGuild>("globalData");
            var result = globalData.Find(_ => true).ToList();
            return result;
        }
        public async static Task<jGuild> requestDataFindAsync(Expression<Func<jGuild, bool>> p)
        {
            IEnumerable<jGuild> results = await requestDataWhereAsync(p);
            return results.First();
        }
        public static jGuild requestDataFind(Expression<Func<jGuild, bool>> p)
        {
            return requestDataFindAsync(p).Result;
        }
        public static List<jGuild> requestDataWhere(Expression<Func<jGuild, bool>> p)
        {
            Task<List<jGuild>> t = requestDataWhereAsync(p);
            t.Wait();
            return t.Result;
        }
        public static void addMsg(IMessage msg, SocketCommandContext Context) {
            IMongoCollection<jGuild> storedGlobalData = Globals.db.GetCollection<jGuild>("globalData");
            // Get current user, technically does the same thing as loopign through everytihng and is probably optimized by the system
            jUser u = requestDataFind(item => item.Id == Context.Guild.Id).Channels.Find(item => item.Id == Context.Channel.Id).Users.Find(item => item.Id == Context.User.Id);
            // add this single message to the user
            u.Timestamps.Add(msg.Timestamp);
            //Update user
            var update = Builders<jGuild>.Update
                //        ---------------Get current user, again so it points to the on in the guild obj-------------------new user obj
                .Set(p => p.Channels.Find(item => item.Id == Context.Channel.Id).Users.Find(item => item.Id == Context.User.Id), u);
            storedGlobalData.FindOneAndUpdate(item => item.Id == Context.Guild.Id, update);
        }
        public static void addGuild(jGuild g) {
            IMongoCollection<jGuild> storedGlobalData = Globals.db.GetCollection<jGuild>("globalData");
            if (requestDataWhere(item => item.Id == g.Id).Count > 0)
            {
                storedGlobalData.FindOneAndReplace(item => item.Id == g.Id, g);
            }
            else {
                storedGlobalData.InsertOne(g);
            }
        }
        public static void addGuild(SocketGuild G) {
            jGuild g = new jGuild()
            {
                Name = G.Name,
                Id = G.Id,
                Channels = new List<jChannel>()
            };
            foreach (SocketTextChannel C in G.TextChannels) {
                if (C == null) { continue; }
                jChannel c = new jChannel()
                {
                    Name = C.Name,
                    Id = C.Id,
                    Users = new List<jUser>()
                };
                g.Channels.Add(c);
            }
            Globals.addGuild(g);
        }
        public static void indexServer(SocketGuild guild)
        {
            Console.WriteLine("---Started guild index: " + guild.Name + "---");
            //jSettings s = null;
            //await Context.Channel.SendMessageAsync("Indexing...");
            //await MCPLUSDB.indexGuild(Context.Guild);
            if (Globals.requestDataWhere(item => item.Id == guild.Id).Count > 0)
            {
                //Guild exists
                IMongoCollection<jGuild> collection = Globals.globalData();
                collection.FindOneAndDeleteAsync(item => item.Id == guild.Id);
            }
            jGuild g = new jGuild(guild, true);
            Globals.addGuild(g);
            //await Context.Channel.SendMessageAsync("Done.");
            Console.WriteLine("---Finished guild index: " + guild.Name + "---");
        }

    }
    public class Backbone
    {
        public static readonly HttpClient HTTPclient = new HttpClient();

        static private DiscordSocketClient client;
        static private CommandService Commands;
        static private CommandHandler commandHandler;


        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnProcessExit);
            Console.CancelKeyPress += OnProcessExit;

            Console.WriteLine("Starting...");
            Globals.waitForLessLoad();
            //Starts the thread queuer
            var ts = new ThreadStart(backgroundLoop);
            var backgroundThread = new Thread(ts);
            backgroundThread.Start();            
            //Console.WriteLine("Thread queue started");

            new Backbone().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            client = new DiscordSocketClient();

            client.Log += Log;

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Error //Set to Error or Critical at Release
            });
            Console.WriteLine("Logging In...");
            await client.LoginAsync(TokenType.Bot, Tokens.token_bot);
            await client.StartAsync();
            Console.WriteLine("Logged In");
            
            //Console.WriteLine("Thread queue started");

            await client.SetGameAsync("in startup | commands may not be available", type: ActivityType.Playing);

            Commands = new CommandService();

            client.Ready += ClientReady;

            //Put stuff here
            client.MessageReceived += MessageReceived;
            client.JoinedGuild += JoinedGuild;
            client.LeftGuild += LeftGuild;
            client.ChannelCreated += ChannelCreated;
            client.ChannelDestroyed += ChannelRemoved;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        static private async Task LeftGuild(SocketGuild guild)
        {
            //Update status message
            await client.SetGameAsync("for mc+help | " + client.Guilds.Count + " Guilds and counting", type: ActivityType.Watching);
        }

        static private async Task ChannelRemoved(SocketChannel channel)
        {
            SocketGuildChannel gChannel = channel as SocketGuildChannel;
            jGuild currentguild = Globals.requestDataFind(item => item.Id == gChannel.Guild.Id);
            if (currentguild.Channels.Where(item => item.Id == gChannel.Id).Count() > 0)
            {
                currentguild.Channels.Remove(currentguild.Channels.Where(item => item.Id == gChannel.Id).First());
                Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Channel removed");
            }
            else
            {
                return;
            }
        }

        static private async Task ChannelCreated(SocketChannel channel)
        {
            SocketGuildChannel gChannel = channel as SocketGuildChannel;
            jGuild currentguild = Globals.requestDataFind(item => item.Id == gChannel.Guild.Id);
            if (currentguild.Channels.Where(item => item.Id == gChannel.Id).Count() > 0)
            {
                return;
            }
            else
            {
                currentguild.Channels.Add(new jChannel
                {
                    Name = gChannel.Name,
                    Id = gChannel.Id,
                    Users = new List<jUser>()
                });
                Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Channel created");
            }

        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Globals.Closing = true;
            Console.WriteLine("Closing..");
            client.LogoutAsync();
            Console.WriteLine("Logged Out");
            Environment.Exit(0);
        }

        bool Initialized = false;
        private async Task ClientReady()
        {
            //Initial start routine
            if (!Initialized)
            {
                //jColor
                var jColorMap = BsonClassMap.RegisterClassMap<jColor>();
                jColorMap.AutoMap();
                jColorMap.MapProperty(c => c.Id).SetIsRequired(true);
                jColorMap.SetIdMember(jColorMap.GetMemberMap(c => c.Id));

                Console.WriteLine("Checking collections...");
                try
                {
                    //Check that the collections exist
                    IMongoQueryable<jGuild> storedGlobalData = Globals.db.GetCollection<jGuild>("globalData").AsQueryable();
                    IMongoQueryable<jColor> storedColors = Globals.db.GetCollection<jColor>("colorSettings").AsQueryable();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("ERROR: " + exception.Message + "\n Stack: " + exception.StackTrace);
                    return;
                }                
                //Console.WriteLine("File data retreived");
                //Start command handler
                commandHandler = new CommandHandler(client, Commands);
                await commandHandler.InstallCommandsAsync();

                // Sometihng I tried out some time ago, to re-index all servers at each start
                // That went about as well as you imagine it would.
                if (Globals.isDebugging)
                {
                    Console.WriteLine("Updating data from last start...");
                    //indexAll();
                    Console.WriteLine("Data updated");
                    //System.Environment.Exit(1);
                }

                //Console.WriteLine("Getting Autoreport Channel...");
                //Globals.errorLogCh = client.GetChannel(623158782742102036) as ISocketMessageChannel;
                //await Globals.errorLogCh.SendMessageAsync("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Bot Started " + DateTime.Now.ToLocalTime().ToShortDateString());                
            }
            await client.SetGameAsync("for mc+help | " + client.Guilds.Count + " Guilds and counting", type: ActivityType.Watching);
            Initialized = true;
            Console.WriteLine("Ready");

        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            //LegacyCommandHandler commandhandler = new LegacyCommandHandler();
            //commandhandler.internal_index(guild);
            //await client.SetGameAsync("for mc+help | " + client.Guilds.Count + " Guilds and counting", type: ActivityType.Watching);

            //Get past message data from the server
            Globals.indexServer(guild);            
        }

        public async Task MessageReceived(SocketMessage message)
        {
            if (!Initialized)
            {
                while (!Initialized)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }            

            var Message = message as SocketUserMessage;
            var Context = new SocketCommandContext(client, Message);

            //When debugging, ignore messages not from testing server
            if (Globals.isDebugging && Context.Guild.Id != 614202470289244171)
            {
                return;                               
            }

            if (message.Content == null || message.Content == "") { return; }

            if (message.Author.IsBot) { return; }

            if (Context.Guild == null) {
                //if the user is changing bot settings, go through all this stuff
                if (Globals.openSettingsChannels.Exists(item => item.Id == message.Author.Id))
                {
                    settingsUser channel = Globals.openSettingsChannels.Find(item => item.Id == message.Author.Id);
                    jGuild Guild = Globals.requestDataFind(item => item.Id == channel.guild);
                    switch (channel.step) {
                        case 0:
                            if (message.Content == "START")
                            {
                                await message.Author.SendMessageAsync("Would you like to enable users to view their activity scores? \n(respond with Y for yes or N for no)");
                                channel.step++;
                            }
                            else if (message.Content == "EXIT" || message.Content == "CANCEL")
                            {
                                await message.Author.SendMessageAsync("Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else {
                                await message.Author.SendMessageAsync("I didn't understand that. Commands are case-sensitive, so make sure that it is spelled and capitalized properly");
                            }
                            break;
                        case 1:
                            if (message.Content == "Y")
                            {
                                await message.Author.SendMessageAsync("Understood. Users will be able to view their activity scores.");
                                Guild.settings.allowScoring = true;
                                await message.Author.SendMessageAsync("Would you like to allow users to view their level? (levels are based on activity score) \n(respond with Y for yes or N for no)");
                                channel.step++;
                            }
                            else if (message.Content == "N")
                            {
                                await message.Author.SendMessageAsync("Understood. Users will not be able to view their activity scores.");
                                Guild.settings.allowScoring = false;
                                await message.Author.SendMessageAsync("No other adjustable settings found. Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                                //channel.step += 3;
                            }
                            else if (message.Content == "EXIT")
                            {
                                await message.Author.SendMessageAsync("Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else if (message.Content == "CANCEL") {
                                await message.Author.SendMessageAsync("Closing session and reverting settings.");
                                Guild.settings = channel.priorSettings;
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else
                            {
                                await message.Author.SendMessageAsync("I didn't understand that. Commands are case-sensitive, so make sure that it is spelled and capitalized properly");
                            }
                            break;
                        case 2:
                            if (message.Content == "Y")
                            {
                                await message.Author.SendMessageAsync("Understood. Users will be able to view their level.");
                                Guild.settings.allowLevels = true;
                                await message.Author.SendMessageAsync("Would you like to enable rank rewards? Rank rewards would give a rank to members based on either their activity score or their level. \n(respond with Y for yes or N for no)");
                                channel.step++;
                            }
                            else if (message.Content == "N")
                            {
                                await message.Author.SendMessageAsync("Understood. Users will not be able to view their level.");
                                Guild.settings.allowLevels = false;
                                Guild.settings.rewardOnLevel = false;
                                await message.Author.SendMessageAsync("Would you like to enable rank rewards? Rank rewards would give a rank to members based on either their activity score. \n(respond with Y for yes or N for no)");
                                channel.step++;
                            }
                            else if (message.Content == "EXIT")
                            {
                                await message.Author.SendMessageAsync("Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else if (message.Content == "CANCEL")
                            {
                                await message.Author.SendMessageAsync("Closing session and reverting settings.");
                                Guild.settings = channel.priorSettings;
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else {
                                await message.Author.SendMessageAsync("I didn't understand that. Commands are case-sensitive, so make sure that it is spelled and capitalized properly");
                            }
                            break;
                        case 3:
                            if (message.Content == "Y") {
                                await message.Author.SendMessageAsync("Understood. Rank rewards will be enabled. You will be asked to set the rank rewards shortly.");
                                Guild.settings.allowRankRewards = true;
                                await message.Author.SendMessageAsync("Would you like rank rewards to be based on activity score or level? \n(A for activity score, L for level)");
                                channel.step++;
                            }
                            else if (message.Content == "N") {
                                await message.Author.SendMessageAsync("Understood. Rank rewards will be disabled.");
                                Guild.settings.allowRankRewards = false;
                                await message.Author.SendMessageAsync("No other adjustable settings found. Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else if (message.Content == "EXIT")
                            {
                                await message.Author.SendMessageAsync("Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else if (message.Content == "CANCEL")
                            {
                                await message.Author.SendMessageAsync("Closing session and reverting settings.");
                                Guild.settings = channel.priorSettings;
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else
                            {
                                await message.Author.SendMessageAsync("I didn't understand that. Commands are case-sensitive, so make sure that it is spelled and capitalized properly");
                            }
                            break;
                        case 4:
                            if (message.Content == "A")
                            {
                                await message.Author.SendMessageAsync("Understood. Rank rewards will be determined based on activity score.");
                                Guild.settings.rewardOnLevel = false;
                                await message.Author.SendMessageAsync("Please enter the ID of the role you would like to award, followed by the activityscore you would like it to be awarded at (positive integers only). \n Format: `[role ID] [score]` (not including brackets)\n_To get the role ID, enable developer tools and right-click roles in your server's role list. You should see an option to \"Copy ID\"._");
                                await message.Author.SendMessageAsync("Send \"DONE\" when you are done entering rank rewards.");
                                channel.step++;
                            }
                            else if (message.Content == "L") {
                                await message.Author.SendMessageAsync("Understood. Rank rewards will be determined based on level");
                                Guild.settings.rewardOnLevel = true;
                                await message.Author.SendMessageAsync("Please enter the ID of the role you would like to award, followed by the level you would like it to be awarded at (positive integers only). \n Format: `[role ID] [level]` (not including brackets)\n_To get the role ID, enable developer tools and right-click roles in your server's role list. You should see an option to \"Copy ID\"._");
                                await message.Author.SendMessageAsync("Send \"DONE\" when you are done entering rank rewards.");
                                channel.step++;
                            }
                            else if (message.Content == "EXIT")
                            {
                                await message.Author.SendMessageAsync("Closing session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else if (message.Content == "CANCEL")
                            {
                                await message.Author.SendMessageAsync("Closing session and reverting settings.");
                                Guild.settings = channel.priorSettings;
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else
                            {
                                await message.Author.SendMessageAsync("I didn't understand that. Commands are case-sensitive, so make sure that it is spelled and capitalized properly");
                            }
                            break;
                        case 5:
                            if (message.Content == "DONE" || message.Content == "EXIT")
                            {
                                await message.Author.SendMessageAsync("Understood.\nNo more adjustable settings found. Closing Session.");
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else if (message.Content == "CANCEL") {
                                await message.Author.SendMessageAsync("Understood, reverting all settings and exiting.");
                                Guild.settings = channel.priorSettings;
                                Globals.openSettingsChannels.RemoveAll(item => item.Id == message.Author.Id);
                            }
                            else
                            {
                                ulong ID = 0;
                                int threshold = -1;
                                string[] split = message.Content.Split(' ');
                                if (ulong.TryParse(split[0], out ID) && int.TryParse(split[1], out threshold))
                                {
                                    Guild.settings.rankRewards.Add(new jRankReward { Id = ID, scoreThreshold = threshold });
                                }
                            }
                            break;
                    }
                }
                else {
                    return;
                }
            }

            LogMessage(message);

            //int argPos = 0;
            //if (Message.HasStringPrefix(Globals.CMD_PREFIX, ref argPos))
            //{
            //    //Send to CommandHandler
            //    //CommandHandler commandHandler_ = new CommandHandler();
            //    //var TTh = new Thread(delegate () { commandHandler_.ExecuteCommand(message, message.Content.Split(' '), Context, client); });
            //    //TTh.Start();
            //    //commandHandler_.ExecuteCommand(message, message.Content.Split(' '), Context, client);
            //    await commandHandler.threadCommand(message);
            //}            

            jGuild targetGuild = Globals.requestDataFind(item => item.Id == Context.Guild.Id);

            //Rank reward handling
            if (targetGuild.settings.allowRankRewards) {
                LegacyCommandHandler ch = new LegacyCommandHandler();
                int usingScore;
                if (targetGuild.settings.rewardOnLevel)
                {
                    usingScore = ch.internal_level(message, Context);
                }
                else {
                    usingScore = ch.internal_score(message, Context);
                }

                IEnumerable<SocketRole> roles = Context.Guild.Roles.AsEnumerable();
                SocketGuildUser user = message.Author as SocketGuildUser;
                foreach (SocketRole role in Context.Guild.Roles) {
                    if (targetGuild.settings.rankRewards.Where(item => item.Id == role.Id).Count() > 0) {
                        if (usingScore >= targetGuild.settings.rankRewards.Find(item => item.Id == role.Id).scoreThreshold)
                        {
                            await user.AddRoleAsync(role);
                        }
                        else if (user.Roles.Contains(role)) {
                            await user.RemoveRoleAsync(role);
                        }
                    }
                }
            }

            //try
            //{
            //CharHandler charhandler = new CharHandler();

            //dynamic bytlof = DateTime.Now.AddDays(-14);


            //HTTPclient.DefaultRequestHeaders.Add("Authorization", "Bot NTc0OTcxNDM5MjQxODg3NzQ1.XSvYjA.QQ9Z4mGlsodwLiJC_V8-8t36Nl0");
            //HttpResponseMessage response = await HTTPclient.GetAsync("http://discordapp.com/api/channels/508359180931825683/messages");
            //response.EnsureSuccessStatusCode();

            //string responseBody = await response.Content.ReadAsStringAsync();

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public static void LogMessage(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }
            if (message.Content == "" || message.Content == null)
            {
                return;
            }
            var Message = message as SocketUserMessage;
            var Context = new SocketCommandContext(client, Message);

            string time = message.Timestamp.DateTime.ToLocalTime().ToShortTimeString();

            if (Globals.requestDataWhere(item => item.Id == Context.Guild.Id).Count() == 0)
            {
                //Globals.globalData.Add(new jGuild
                //{
                //    Name = Context.Guild.Name,
                //    Id = Context.Guild.Id,
                //    Channels = new List<jChannel>()
                //});
                Globals.addGuild(Context.Guild);
                Console.WriteLine("[" + time + "]: Guild created");                
            }

            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();

            if (currentGuild.Channels.Where(item => item.Id == message.Channel.Id).Count() == 0)
            {
                currentGuild.Channels.Add(new jChannel
                {
                    Name = message.Channel.Name,
                    Id = message.Channel.Id,
                    Users = new List<jUser>()
                });
                Console.WriteLine("[" + time + "]: Channel created");
            }

            jChannel currentChannel = currentGuild.Channels.Where(item => item.Id == message.Channel.Id).First();

            if (currentChannel.Users.Where(item => item.Id == message.Author.Id).Count() == 0)
            {
                currentChannel.Users.Add(new jUser
                {
                    Name = message.Author.Username,
                    Id = message.Author.Id,
                    Timestamps = new List<DateTimeOffset>()
                });
                Console.WriteLine("[" + time + "]: User created");
                currentChannel.Users.Last().Timestamps.Add(message.Timestamp);
                Console.WriteLine("[" + time + "]: Message Logged successfully");
                return;
            }

            jUser currentUser = currentChannel.Users.Where(item => item.Id == message.Author.Id).First();
            if (!currentUser.Timestamps.Contains(message.Timestamp))
            {
                currentUser.Timestamps.Add(message.Timestamp);
                Console.WriteLine("[" + time + "]: Message Logged successfully");
            }
            else
            {
                Console.WriteLine("[" + time + "]: Existing Message Already Logged");
            }

        }
        public static void LogMessage(IMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }
            if (message.Content == "" || message.Content == null)
            {
                return;
            }
            var Context = client.GetChannel(message.Channel.Id) as SocketGuildChannel;

            if (Globals.requestDataWhere(item => item.Id == Context.Guild.Id).Count() == 0)
            {
                //Globals.globalData.Add(new jGuild
                //{
                //    Name = Context.Guild.Name,
                //    Id = Context.Guild.Id,
                //    Channels = new List<jChannel>()
                //});
                Globals.addGuild(Context.Guild);
                //Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Guild created");
            }

            jGuild currentGuild = Globals.requestDataWhere(item => item.Id == Context.Guild.Id).First();

            if (currentGuild.Channels.Where(item => item.Id == message.Channel.Id).Count() == 0)
            {
                currentGuild.Channels.Add(new jChannel
                {
                    Name = message.Channel.Name,
                    Id = message.Channel.Id,
                    Users = new List<jUser>()
                });
                //Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Channel created");
            }

            jChannel currentChannel = currentGuild.Channels.Where(item => item.Id == message.Channel.Id).First();

            if (currentChannel.Users.Where(item => item.Id == message.Author.Id).Count() == 0)
            {
                currentChannel.Users.Add(new jUser
                {
                    Name = message.Author.Username,
                    Id = message.Author.Id,
                    Timestamps = new List<DateTimeOffset>()
                });
                //Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: User created");
                currentChannel.Users.Last().Timestamps.Add(message.Timestamp);
                //Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Message Logged successfully");
                return;
            }

            jUser currentUser = currentChannel.Users.Where(item => item.Id == message.Author.Id).First();
            if (!currentUser.Timestamps.Contains(message.Timestamp))
            {
                currentUser.Timestamps.Add(message.Timestamp);
                //Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Message Logged successfully");
            }
            else
            {
                //Console.WriteLine("[" + DateTime.Now.ToLocalTime().ToShortTimeString() + "]: Existing Message Already Logged");
            }
        }

        public void indexAll()
        {
            //WARNING: WILL REPLACE ALL GIVEN DATA
            foreach (SocketGuild guild in client.Guilds)
            {               
                SocketGuild g = guild;
                try
                {                        
                    Globals.unIndexedIDs.Add(g.Id);
                    Globals.indexServer(g);
                    Globals.unIndexedIDs.Remove(g.Id);
                }
                catch {
                    Globals.unIndexedIDs.Remove(g.Id);
                    Console.WriteLine("Error indexing guild " + g.Name);
                }
                //Globals.threads.Add(t);
            }
        }     
        
        public static void backgroundLoop()
        {           
            while (!Globals.Closing)
            {
                System.Threading.Thread.Sleep(10);
                List<Thread> pendingThreads = Globals.threads.Where(item => (item.ThreadState == System.Threading.ThreadState.Unstarted)).ToList();
                int running = Globals.threads.Count - pendingThreads.Count;
                if (pendingThreads.Count > 0 && running < Globals.MAX_THREADS)
                {
                    Thread luckyWinner = pendingThreads.First();
                    Console.WriteLine("Starting Thread " + luckyWinner.Name);
                    luckyWinner.Start();
                }                
            };
        }
    }
}