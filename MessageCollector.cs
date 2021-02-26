using Discord;
using Discord.WebSocket;
using MsgCountPlusNET.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MsgCountPlusNET.Commands
{
    class MessageCollector
    {
        public static void indexChannel(ISocketMessageChannel channel, jChannel outChannel)
        {
            Console.WriteLine("Starting " + channel.Name + " index");
            if (outChannel == null) {
                outChannel = new jChannel(channel);
            }
            var messages = new List<IReadOnlyCollection<IMessage>>();
            try
            {
                messages = channel.GetMessagesAsync(int.MaxValue).ToEnumerable().ToList();
            }
            catch (Exception ex) {
                return;
            }
            Console.WriteLine("Got messages for " + channel.Name);
            int threadCount = 0;
            foreach (IReadOnlyCollection<IMessage> collection in messages)
            {
                foreach (IMessage m in collection) {
                    if (!outChannel.Users.Exists(item => item.Id == m.Author.Id))
                    {
                        jUser newU = new jUser()
                        {
                            Id = m.Author.Id,
                            Name = m.Author.Username,
                            Timestamps = new List<DateTimeOffset>()
                        };
                        newU.Timestamps.Add(m.Timestamp);
                        outChannel.Users.Add(newU);
                    }
                    else {
                        outChannel.Users.Find(item => item.Id == m.Author.Id).Timestamps.Add(m.Timestamp);
                    }                    
                }
                #region old_threaded
                //threadCount++;
                //Thread TTh = new Thread(async delegate ()
                //{
                //    foreach (IMessage m in collection)
                //    {
                //        try
                //        {
                //            //if (m.Author.IsBot || m.Author.IsWebhook || m.Content == "" || m.Conten)
                //            if (m == null || m.Author == null) { continue; }
                //            if (outChannel.Users.Exists(item => item.Id == m.Author.Id))
                //            {
                //                try
                //                {
                //                    outChannel.Users.Find(item => item.Id == m.Author.Id).Timestamps.Add(m.Timestamp);
                //                }
                //                catch (Exception ex)
                //                {
                //                    continue;
                //                }
                //            }
                //            else
                //            {
                //                if (adding.Contains(m.Author.Id))
                //                {
                //                    while (adding.Contains(m.Author.Id))
                //                    {
                //                        Thread.Sleep(5);
                //                    }
                //                    try
                //                    {
                //                        outChannel.Users.Find(item => item.Id == m.Author.Id).Timestamps.Add(m.Timestamp);
                //                    }
                //                    catch (Exception ex)
                //                    {
                //                        continue;
                //                    }
                //                    continue;
                //                }
                //                adding.Add(m.Author.Id);
                //                jUser newU = new jUser()
                //                {
                //                    Id = m.Author.Id,
                //                    Name = m.Author.Username,
                //                    Timestamps = new List<DateTimeOffset>()
                //                };
                //                newU.Timestamps.Add(m.Timestamp);
                //                outChannel.Users.Add(newU);
                //                adding.RemoveAll(item => item == m.Author.Id);
                //            }
                //        }
                //        catch (Exception ex) {
                //            continue;
                //        }
                //    }
                //});
                //TTh.Name = channel.Name + " | " + threadCount + " | " + channel.Id;
                //Globals.threads.Add(TTh);                
                //Globals.waitForLessLoad();
                //TTh.Start();
                #endregion
            }
            Console.WriteLine("Finished " + channel.Name + " index");
        }
        public async static Task<List<DateTimeOffset>> indexChannel(ISocketMessageChannel channel)
        {
            Console.WriteLine("Starting " + channel.Name + " collection");
            var messages = channel.GetMessagesAsync(int.MaxValue).ToEnumerable().ToList();
            //Console.WriteLine("Got messages for " + channel.Name);
            List<DateTimeOffset> l = new List<DateTimeOffset>();            
            foreach (IMessage m in messages) {
                l.Add(m.Timestamp);
            }
            return l;
        }
        public async static Task<List<DateTimeOffset>> indexGuild(SocketGuild guild)
        {
            List<DateTimeOffset> output = new List<DateTimeOffset>();
            List<Thread> threads = new List<Thread>();
            foreach (ISocketMessageChannel channel in guild.TextChannels)
            {
                //await Program.Log(channel.Name + " Start");
                try
                {
                    var TTh = new Thread(async delegate ()
                    {
                        output.AddRange(await collectMessages(channel));
                    });
                    threads.Add(TTh);
                    TTh.Start();
                }
                catch (Exception ex)
                {
                    continue;
                }
                //await Program.Log(channel.Name + " End");
            }
            while (threads.Exists(item => item.IsAlive)) {
                Thread.Sleep(5);
            }
            return output;
        }


        public async static Task<List<DateTimeOffset>> collectMessages(ISocketMessageChannel channel)
        {
            jGuild g = await Globals.requestDataFindAsync(item => item.Channels.Count(C => C.Id == channel.Id) > 0);
            jChannel c = g.Channels.Find(item => item.Id == channel.Id);
            return c.fullTimestamps();
        }
        public async static Task<List<DateTimeOffset>> collectMessages(SocketUser user, ISocketMessageChannel channel)
        {
            jGuild g = await Globals.requestDataFindAsync(item => item.Members().Exists(U => U.Id == user.Id) && item.Channels.Exists(C => C.Id == channel.Id));
            List<DateTimeOffset> c = g.Channels.Find(C => C.Id == channel.Id).Users.Find(U => U.Id == user.Id).Timestamps;
            return c;
        }
        public async static Task<List<DateTimeOffset>> collectMessages(SocketGuild guild)
        {
            jGuild j = Globals.requestDataFind(item => item.Id == guild.Id);
            return j.FullTimestamps();
        }
        public async static Task<List<DateTimeOffset>> collectMessages(SocketGuildUser user)
        {
            return await collectMessages(user, user.Guild);
        }
        public async static Task<List<DateTimeOffset>> collectMessages(SocketUser user, SocketGuild guild)
        {
            jGuild g = await Globals.requestDataFindAsync(item => item.Id == guild.Id);
            jUser u = g.LookUp(user.Id);
            if (u == null || u.Timestamps == null)
            {
                return new List<DateTimeOffset>();
            }
            else {
                return u.Timestamps;
            }
        }
        public async static Task<List<DateTimeOffset>[]> collectMessages(SocketUser[] users, SocketGuild guild)
        {
            //List<DateTimeOffset> full = await collectMessages(guild);
            List<DateTimeOffset>[] output = new List<DateTimeOffset>[users.Length];
            for (int i = 0; i < users.Length; i++)
            {
                output[i] = await collectMessages(users[i], guild);
            }
            return output;
        }
        public async static Task<List<List<DateTimeOffset>>> collectMessagesList(SocketUser[] users, SocketGuild guild)
        {
            List<List<DateTimeOffset>> output = new List<List<DateTimeOffset>>();
            for (int i = 0; i < users.Length; i++)
            {
                try
                {
                    List<DateTimeOffset> e = await collectMessages(users[i], guild);
                    output.Add(e);
                }
                catch {
                    continue;
                }
            }
            return output;
        }
        public async static Task<List<List<DateTimeOffset>>> collectMessagesList(List<SocketUser> users, SocketGuild guild) {
            return await collectMessagesList(users.ToArray(), guild);
        }
    }
}
