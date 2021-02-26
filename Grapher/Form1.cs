using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace MCPlusForm
{
    public partial class GraphHandler : Form
    {
        public GraphHandler()
        {
            InitializeComponent();
            
        }

        private void plotView1_Click(object sender, EventArgs e)
        {

        }

        public void HourGraph(int[] input, string OrderID, byte[] bg, byte[] line)
        {
            if (input.Length != 24)
            {
                return;
            }

            PlotModel plot = new PlotModel();

            var barSeries = new BarSeries
            {
                ItemsSource = new List<BarItem>(new[]
                {
                new BarItem{ Value = (input[23]) },
                new BarItem{ Value = (input[22]) },
                new BarItem{ Value = (input[21]) },
                new BarItem{ Value = (input[20]) },
                new BarItem{ Value = (input[19]) },
                new BarItem{ Value = (input[18]) },
                new BarItem{ Value = (input[17]) },
                new BarItem{ Value = (input[16]) },
                new BarItem{ Value = (input[15]) },
                new BarItem{ Value = (input[14]) },
                new BarItem{ Value = (input[13]) },
                new BarItem{ Value = (input[12]) },
                new BarItem{ Value = (input[11]) },
                new BarItem{ Value = (input[10]) },
                new BarItem{ Value = (input[9]) },
                new BarItem{ Value = (input[8]) },
                new BarItem{ Value = (input[7]) },
                new BarItem{ Value = (input[6]) },
                new BarItem{ Value = (input[5]) },
                new BarItem{ Value = (input[4]) },
                new BarItem{ Value = (input[3]) },
                new BarItem{ Value = (input[2]) },
                new BarItem{ Value = (input[1]) },
                new BarItem{ Value = (input[0]) }
                }),
                LabelPlacement = LabelPlacement.Inside,
                FillColor = OxyColor.FromArgb(line[3], line[0], line[1], line[2])
            };
            plot.Series.Add(barSeries);

            plot.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "HourAxis",
                ItemsSource = new[]
                    {
                "12AM",
                "11PM",
                "10PM",
                "9PM",
                "8PM",
                "7PM",
                "6PM",
                "5PM",
                "4PM",
                "3PM",
                "2PM",
                "1PM",
                "12PM",
                "11AM",
                "10AM",
                "9AM",
                "8AM",
                "7AM",
                "6AM",
                "5AM",
                "4AM",
                "3AM",
                "2AM",
                "1AM",
            }
            });

            var pngExporter = new PngExporter { Width = 600, Height = 600, Background = OxyColor.FromArgb(bg[3], bg[0], bg[1], bg[2]) };
            pngExporter.ExportToFile(plot, OrderID + ".png");
        }
        public void ChannelGraph(string[] names, int[] values, string OrderID, byte[] bg)
        {
            if (names.Length != values.Length)
            {
                return;
            }

            PlotModel plot = new PlotModel();

            int total = values.Sum();

            var pieSeries = new PieSeries { Title = "ChannelGraph: " + OrderID + "\n Messages referenced: " + total};
            pieSeries.AreInsideLabelsAngled = true;
            for (int i = 0; i < names.Length; i++)
            {

                decimal percent = decimal.Divide(values[i], total) * 100;
                if (values[i] == 0 || percent < 1)
                {
                    continue;
                }
                if (percent < 2)
                {
                    pieSeries.Slices.Add(new PieSlice(null, values[i]));
                }
                else
                {
                    pieSeries.Slices.Add(new PieSlice(names[i], values[i]));
                }
            }
            plot.Series.Add(pieSeries);
            plot.TitleFontSize = 40;

            var pngExporter = new PngExporter { Width = 1200, Height = 1000, Background = OxyColor.FromArgb(bg[3], bg[0], bg[1], bg[2]) };
            pngExporter.ExportToFile(plot, OrderID + ".png");
        }
        public void MessageGraph(DateTimeOffset[] messages, string OrderID, byte[] bg, byte[] line)
        {
            Array.Sort(messages);

            PlotModel plot = new PlotModel();

            plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = DateTimeAxis.ToDouble(messages[0].DateTime), Maximum = DateTimeAxis.ToDouble(DateTime.Now) });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left });
            var lineSeries = new LineSeries { Title = "Activity", MarkerType = MarkerType.None, Color = OxyColor.FromArgb(line[3], line[0], line[1], line[2]) };
            for (int i = 0; i < messages.Length; i++)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(messages[i].DateTime), i + 1));
            }
            plot.Series.Add(lineSeries);

            var pngExporter = new PngExporter { Width = 1800, Height = 600, Background = OxyColor.FromArgb(bg[3], bg[0], bg[1], bg[2]) };
            pngExporter.ExportToFile(plot, OrderID + ".png");
        }
        public void ActivityGraph(DateTimeOffset[] messages, string interval, string OrderID, byte[] bg, byte[] line, double repeat)
        {
            Array.Sort(messages);
            TimeSpan logLength = messages.Max().Subtract(messages.Min());

            PlotModel plot = new PlotModel();

            plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = DateTimeAxis.ToDouble(messages.Min().DateTime), Maximum = DateTimeAxis.ToDouble(messages.Max().DateTime) });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0 });
            var lineSeries = new LineSeries { MarkerType = MarkerType.Diamond, MarkerFill = OxyColor.FromArgb(line[3], line[0], line[1], line[2]), Color = OxyColor.FromArgb(line[3], line[0], line[1], line[2]), Smooth = true };

            DateTimeOffset lowEdge = DateTimeOffset.MinValue;
            DateTimeOffset highEdge = DateTimeOffset.MaxValue;

            switch (interval)
            {
                case "h":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddHours(i * repeat);
                        highEdge = lowEdge.AddHours(repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "d":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddDays(i * repeat);
                        highEdge = lowEdge.AddDays(repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "w":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddDays(i * 7 * repeat);
                        highEdge = lowEdge.AddDays(7 * repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "m":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddMonths(i * (int)repeat);
                        highEdge = lowEdge.AddMonths(1 * (int)repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "y":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddYears(i * (int)repeat);
                        highEdge = lowEdge.AddYears(1 * (int)repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                default:
                    return;
            }
            if (messages.Max() > highEdge)
            {
                plot.Axes.Where(item => item.Maximum == DateTimeAxis.ToDouble(messages.Max().DateTime)).First().Maximum = DateTimeAxis.ToDouble(highEdge.DateTime);
            }
            lineSeries.Title = "Activity - " + lineSeries.Points.Count + " Datapoints";
            plot.Series.Add(lineSeries);


            var pngExporter = new PngExporter { Width = 1800, Height = 600, Background = OxyColor.FromArgb(bg[3], bg[0], bg[1], bg[2]) };
            pngExporter.ExportToFile(plot, OrderID + ".png");
        }
        public LineSeries internal_ActivityGraph(DateTimeOffset[] messages, string interval, OxyColor line, double repeat)
        {
            if (messages == null || messages.Length == 0)
            {
                return null;
            }

            Array.Sort(messages);
            TimeSpan logLength = messages[messages.Length - 1].Subtract(messages[0]);

            DateTimeOffset lowEdge = DateTimeOffset.MinValue;
            DateTimeOffset highEdge = DateTimeOffset.MaxValue;

            var lineSeries = new LineSeries { Title = "Activity", MarkerType = MarkerType.None, Color = line };

            switch (interval)
            {
                case "h":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddHours(i * repeat);
                        highEdge = lowEdge.AddHours(repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "d":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddDays(i * repeat);
                        highEdge = lowEdge.AddDays(repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "w":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddDays(i * 7 * repeat);
                        highEdge = lowEdge.AddDays(7 * repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "m":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddMonths(i * (int)repeat);
                        highEdge = lowEdge.AddMonths(1 * (int)repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                case "y":
                    for (int i = 0; lowEdge < DateTimeOffset.Now; i++)
                    {
                        lowEdge = messages.Min().AddYears(i * (int)repeat);
                        highEdge = lowEdge.AddYears(1 * (int)repeat);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(lowEdge.DateTime), messages.Where(item => item.DateTime >= lowEdge && item.DateTime < highEdge).Count()));
                    }
                    break;
                default:
                    return null;
            }
            lineSeries.Smooth = true;
            return lineSeries;
        }
        public void CompareGraph(List<List<DateTimeOffset>> messages, string interval, string OrderID, byte[] bg, string[] names, double repeat)
        {
            PlotModel plot = new PlotModel();

            plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Maximum = DateTimeAxis.ToDouble(DateTime.Now) });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            int step = 0;
            foreach (List<DateTimeOffset> submessages in messages)
            {
                OxyColor color = OxyColor.FromHsv(25 * step + 25, 75, 100);
                LineSeries result = internal_ActivityGraph(submessages.ToArray(), interval, color, repeat);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    result.Title = names[step] + " - " + result.Points.Count + " Datapoints";
                    plot.Series.Add(result);
                }
                step++;
            }

            var pngExporter = new PngExporter { Width = 1800, Height = 600, Background = OxyColor.FromArgb(bg[3], bg[0], bg[1], bg[2]) };
            pngExporter.ExportToFile(plot, OrderID + ".png");
        }

        private void GraphHandler_Load(object sender, EventArgs e)
        {

        }
    }
}
