using Discord;
using Discord.Commands;
using Discord.WebSocket;
//using MCPLUS2.Data;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
//using OxyPlot.Wpf;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgCountPlusNET.Commands.Modules
{
    //[Group("Graphing")]
    public class GraphModule : ModuleBase<SocketCommandContext>
    {
        private void HourGraph(int[] input, string filename)
        {
            if (input.Length != 24)
            {
                return;
            }

            PlotModel plot = new PlotModel();
            var series = new OxyPlot.Series.BarSeries();
            List<BarItem> l = new List<BarItem>();
            for (int i = 0; i < 24; i++)
            {
                l.Add(new BarItem(value: input[23 - i]));
            }
            series.ItemsSource = l;
            series.LabelPlacement = LabelPlacement.Inside;

            plot.Series.Add(series);
            plot.Axes.Add(new OxyPlot.Axes.CategoryAxis
            {
                ItemsSource = new string[] { "12AM", "11PM", "10PM", "9PM", "8PM", "7PM", "6PM", "5PM", "4PM", "3PM", "2PM", "1PM", "12PM", "11AM", "10AM", "9AM", "8AM", "7AM", "6AM", "5AM", "4AM", "3AM", "2AM", "1AM", },
                Key = "HourAxis",
                Position = AxisPosition.Left
            });

            var png = new PngExporter { Width = 600, Height = 600 };
            png.ExportToFile(plot, filename + ".png");
        }
        private void HourGraph(List<DateTimeOffset> messages, int timezone, string filename)
        {
            int[] h = new int[24];
            foreach (DateTimeOffset message in messages)
            {
                h[wrapTime(message.Hour + timezone)]++;
            }
            HourGraph(h, filename);
        }
        public static int wrapTime(int i)
        {
            int I = i;
            while (I >= 24 || I < 0)
            {
                if (I >= 24)
                {
                    I = I - 24;
                }
                if (I < 0)
                {
                    I = I + 24;
                }
            }
            return I;
        }
        private void ChannelGraph(SocketGuild guild, string filename, SocketUser user = null)
        {
            PlotModel plot = new PlotModel();

            //List<DateTimeOffset> messages = new List<DateTimeOffset>();//MessageCollector.collectMessages(guild);
            int total = 0;
            if (user == null)
            {
                Task<List<DateTimeOffset>> t = MessageCollector.collectMessages(guild);
                t.Wait();
                total = t.Result.Count;
            }
            else {
                Task<List<DateTimeOffset>> t = MessageCollector.collectMessages(user, guild);
                t.Wait();
                total = t.Result.Count;
            }

            //int total = messages.Count;
            

            Dictionary<IMessageChannel, int> organized = new Dictionary<IMessageChannel, int>();
            
            foreach (SocketTextChannel channel in guild.TextChannels)
            {
                Task<List<DateTimeOffset>> t = MessageCollector.collectMessages(channel);
                t.Wait();
                organized.Add(channel, t.Result.Count);
            }

            var pieSeries = new OxyPlot.Series.PieSeries { Title = "ChannelGraph: " + filename + "\n Messages referenced: " + total };
            pieSeries.AreInsideLabelsAngled = true;
            for (int i = 0; i < organized.Count; i++)
            {
                var l = organized.ToList();
                decimal percent = decimal.Divide(l[i].Value, total) * 100;
                if (l[i].Value == 0 || percent < 1)
                {
                    continue;
                }
                if (percent < 2)
                {
                    pieSeries.Slices.Add(new PieSlice(null, l[i].Value));
                }
                else
                {
                    pieSeries.Slices.Add(new PieSlice(l[i].Key.Name, l[i].Value));
                }
            }
            plot.Series.Add(pieSeries);
            plot.TitleFontSize = 40;

            var pngExporter = new PngExporter { Width = 1200, Height = 1000 };
            pngExporter.ExportToFile(plot, filename + ".png");
        }
        private void ActivityGraph(List<DateTimeOffset> messages, TimeSpan interval, string filename)
        {
            TimeSpan Interval = interval;
            if (interval.TotalHours < 1)
            {
                Interval = new TimeSpan(interval.Minutes * 30, 0, 0, 0);
            }
            List<DateTimeOffset> orderedMsg = messages.OrderBy(item => item).ToList();
            TimeSpan difference = orderedMsg.Last().Subtract(orderedMsg.First());
            int r = divideTimeSpan(Interval, difference) + 1;
            int[] vals = new int[r];

            foreach (DateTimeOffset msg in orderedMsg)
            {
                try
                {
                    if (msg == orderedMsg.First())
                    {
                        vals[0]++;
                    }
                    int currentQuotient = divideTimeSpan(Interval, msg.Subtract(orderedMsg.First()));
                    vals[currentQuotient]++;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            PlotModel plot = new PlotModel();
            plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = DateTimeAxis.ToDouble(orderedMsg.Last().DateTime), Maximum = DateTimeAxis.ToDouble(orderedMsg.First().DateTime) });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0 });
            var lineSeries = new LineSeries { MarkerType = MarkerType.Diamond };
            DateTimeOffset c = orderedMsg.First();
            for (int i = 0; i < r; i++)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(c.DateTime), vals[i]));
                c = c.Add(Interval);
            }
            plot.Series.Add(lineSeries);

            var pngExporter = new PngExporter { Width = 1800, Height = 600 };
            pngExporter.ExportToFile(plot, filename + ".png");
        }
        private void CompareGraph(List<DateTimeOffset>[] messages, List<SocketUser> usernames, TimeSpan interval, string filename)
        {
            TimeSpan Interval = interval;
            if (interval.TotalHours < 1)
            {
                Interval = new TimeSpan(interval.Minutes * 30, 0, 0, 0);
            }
            List<DateTimeOffset> orderedMsg = new List<DateTimeOffset>(); //messages.OrderBy(item => item.Timestamp).ToList();
            foreach (List<DateTimeOffset> mL in messages)
            {
                orderedMsg.AddRange(mL);
            }
            orderedMsg.Sort();
            TimeSpan difference = orderedMsg.Last().Subtract(orderedMsg.First());
            int r = divideTimeSpan(Interval, difference) + 1;
            //List<LineSeries> series = new List<LineSeries>();

            PlotModel plot = new PlotModel();
            plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = DateTimeAxis.ToDouble(orderedMsg.Last().DateTime), Maximum = DateTimeAxis.ToDouble(orderedMsg.First().DateTime) });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0 });
            //OxyColor color = OxyColor.FromHsv(0, 1, 1);
            int cursor = 0;
            foreach (List<DateTimeOffset> list in messages)
            {
                var line = new LineSeries
                {
                    Title = usernames[cursor].Username,
                    //Color = color,
                    IsVisible = true,
                    MarkerType = MarkerType.Diamond
                };
                //line.Title = list.First().Author.Username;
                //line.Color = color;
                //line.IsVisible = true;
                int[] vals = new int[r];
                //DateTimeOffset minimum = list.Min(item => item.Timestamp);
                foreach (DateTimeOffset msg in list)
                {
                    try
                    {
                        if (msg == orderedMsg.First())
                        {
                            vals[0]++;
                        }
                        int currentQuotient = divideTimeSpan(Interval, msg.Subtract(orderedMsg.First()));
                        vals[currentQuotient]++;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                DateTimeOffset c = orderedMsg.First();
                for (int i = 0; i < r; i++)
                {
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(c.DateTime), vals[i]));
                    c = c.Add(Interval);
                }
                plot.Series.Add(line);
                cursor++;
                //                                          30/360
                //color = OxyColor.FromHsv(color.ToHsv()[0] + (1.0/12.0), 1, 1);
            }
            foreach (Series s in plot.Series)
            {
                s.IsVisible = true;
            }
            //plot.InvalidatePlot(true);
            var pngExporter = new PngExporter { Width = 1800, Height = 600 };
            pngExporter.ExportToFile(plot, filename + ".png");
        }

        /**
         * Returns the amount of times t1 fits in t2
         * */
        private int divideTimeSpan(TimeSpan t1, TimeSpan t2)
        {
            int d = 0;
            if (t1 > t2)
            {
                return 0;
            }
            if (t1 == t2)
            {
                return 1;
            }
            TimeSpan thirdcup = t2;
            while (thirdcup >= t1)
            {
                thirdcup = thirdcup.Subtract(t1);
                d++;
            }
            return d;
        }

        private static string generateOrderID(IMessage message)
        {
            string output = message.Author.Id.ToString();
            output = output + message.Author.Discriminator;
            output = output + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            return output;
        }

        [Command("activity")]
        [Summary("Gives a graph of the given user's activity over the given interval")]
        public async Task activitygraph([Summary("Interval to group messages by, in days.\nSyntax: [days of interval]d")] TimeSpan interval, [Summary("User to get activity of, defaults to current user")] SocketUser user = null)
        {
            SocketUser User = user;
            if (User == null)
            {
                User = Context.User;
            }
            string name = generateOrderID(Context.Message);
            List<DateTimeOffset> l = await MessageCollector.collectMessages(user, Context.Guild);
            ActivityGraph(l, interval, name);
            await Context.Channel.SendFileAsync(name + ".png");
        }

        [Command("serveractivity")]
        [Summary("Gives a graph of the current server's activity over the given interval")]
        public async Task serveractivity([Summary("Interval to group messages by, in days.\nSyntax: [days of interval]d")] TimeSpan interval)
        {
            string name = generateOrderID(Context.Message);
            List<DateTimeOffset> l = await MessageCollector.collectMessages(Context.Guild);
            ActivityGraph(l, interval, name);
            await Context.Channel.SendFileAsync(name + ".png");
        }

        //[Command("wordactivity")]
        //[Summary("Get the graph of some phrase's usage over time, over the given interval")]
        //public async Task wordactivity([Summary("Interval to group messages by, in days.\nSyntax: [days of interval]d")] TimeSpan interval, [Summary("The phrase or word to search for, phrases with spaces must be in quotes")]string phrase, [Summary("User to get activity of, defaults to current user")]SocketUser user = null)
        //{
        //    string name = generateOrderID(Context.Message);
        //    List<DateTimeOffset> l = new List<DateTimeOffset>();
        //    if (user == null)
        //    {
        //        l = await MessageCollector.collectMessages(Context.Guild);
        //    }
        //    else
        //    {
        //        l = await MessageCollector.collectMessages(user, Context.Guild);
        //    }
        //    l = l.Where(item => item.Content.Contains(phrase)).ToList();
        //    ActivityGraph(l, interval, name);
        //    await Context.Channel.SendFileAsync(name + ".png");
        //}

        [Command("channelgraph")]
        [Summary("Get a pie chart of relative usage of different channels for a given user")]
        public async Task channelgraph([Summary("User to get activity of, defaults to current user")]SocketUser user = null)
        {
            SocketUser User = user;
            if (user == null)
            {
                User = Context.User;
            }
            //List<DateTimeOffset> messages = await MessageCollector.collectMessages(User, Context.Guild);
            string name = generateOrderID(Context.Message);
            ChannelGraph(Context.Guild, name, User);
            await Context.Channel.SendFileAsync(name + ".png");
        }

        [Command("serverchannelgraph")]
        [Summary("Get a pie chart of relative use of different channels across the entire server")]
        public async Task serverchannelgraph()
        {
            //List<DateTimeOffset> messages = await MessageCollector.collectMessages(Context.Guild);
            string name = generateOrderID(Context.Message);
            ChannelGraph(Context.Guild, name);
            await Context.Channel.SendFileAsync(name + ".png");
        }

        [Command("activehours")]
        [Summary("Get the active hours of the given user. Can be adjusted for timezone (adjustments are relative to UTC, for example -5 means UTC-5 or EST)")]
        public async Task activehours([Summary("Timezone to adjust to relative to UTC. For example UTC-5 (EST) would be -5")]int timezone = 0, [Summary("User to get activity of, defaults to current user")]SocketUser user = null)
        {
            SocketUser User = user;
            if (User == null)
            {
                User = Context.User;
            }
            List<DateTimeOffset> l = await MessageCollector.collectMessages(User, Context.Guild);
            string name = generateOrderID(Context.Message);
            HourGraph(l, timezone, name);
            await Context.Channel.SendFileAsync(name + ".png");
        }

        [Command("serveractivehours")]
        [Summary("Get the active hours of the current server. Can be adjusted for timezone (adjustments are relative to UTC, for example -4 means UTC-4 or EST)")]
        public async Task serveractivehours(int timezone = 0)
        {
            List<DateTimeOffset> l = await MessageCollector.collectMessages(Context.Guild);
            string name = generateOrderID(Context.Message);
            HourGraph(l, timezone, name);
            await Context.Channel.SendFileAsync(name + ".png");
        }

        [Command("comparegraph")]
        [Summary("Get a comparison of different users activity based on the given interval")]
        public async Task comparegraph([Summary("Interval to group messages by, in days.\nSyntax: [days of interval]d")] TimeSpan interval, params string[] userList)
        {
            if (Context.Message.MentionedUsers.Count < 2)
            {
                await Context.Channel.SendMessageAsync("Please specify at least 2 users");
                return;
            }
            List<SocketUser> users = Context.Message.MentionedUsers.ToList();
            List<DateTimeOffset>[] msgss = await MessageCollector.collectMessages(users.ToArray(), Context.Guild);
            string n = generateOrderID(Context.Message);
            CompareGraph(msgss, users, interval, n);
            await Context.Channel.SendFileAsync(n + ".png");
        }
    }
}
