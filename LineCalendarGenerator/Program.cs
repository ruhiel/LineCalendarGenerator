using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LineCalendarGenerator
{
    static class Program
    {
        public static async Task<HashSet<DateTime>> GetHolidaysAsync(int year)
        {
            var key = Environment.GetEnvironmentVariable("CalendarAPIKey");
            var holidaysId = "japanese__ja@holiday.calendar.google.com";
            var startDate = new DateTime(year, 1, 1).ToString("yyyy-MM-dd") + "T00%3A00%3A00.000Z";
            var endDate = new DateTime(year, 12, 31).ToString("yyyy-MM-dd") + "T00%3A00%3A00.000Z";
            var maxCount = 30;

            var url = $"https://www.googleapis.com/calendar/v3/calendars/{holidaysId}/events?key={key}&timeMin={startDate}&timeMax={endDate}&maxResults={maxCount}&orderBy=startTime&singleEvents=true";
            using (var client = new WebClient() { Encoding = Encoding.UTF8 })
            {
                var json = await client.DownloadStringTaskAsync(url);
                var o = JObject.Parse(json);
                var days = o["items"].Select(i => DateTime.Parse(i["start"]["date"].ToString()));
                return new HashSet<DateTime>(days);
            }
        }

        static async Task Main(string[] args)
        {
            var options = new Options();
            var parseResult = CommandLine.Parser.Default.ParseArguments<Options>(args);

            if (parseResult.Tag == CommandLine.ParserResultType.Parsed)
            {
                var parsed = (CommandLine.Parsed<Options>)parseResult;

                var result = await GetHolidaysAsync(parsed.Value.Year);

                var dt = new DateTime(parsed.Value.Year, parsed.Value.Month, 1);

                var imageWidth = parsed.Value.Vertical ? 200 : 1920;
                var imageHight = parsed.Value.Vertical ? 1920 : 200;

                using (var img = new Bitmap(imageWidth, imageHight))
                using (var g = Graphics.FromImage(img))
                {
                    var dayPosX = 100;
                    var dayPosY = 100;

                    var sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    if (parsed.Value.Vertical)
                    {
                        g.DrawString(dt.ToString("yyyy", CultureInfo.GetCultureInfo("en-US")), new Font(parsed.Value.Font, 16), Brushes.Black, 50, 40, sf);
                        g.DrawString(dt.ToString("MMM", CultureInfo.GetCultureInfo("en-US")), new Font(parsed.Value.Font, 24), Brushes.Black, 110, 40, sf);
                    }
                    else
                    {
                        g.DrawString(dt.ToString("yyyy", CultureInfo.GetCultureInfo("en-US")), new Font(parsed.Value.Font, 16), Brushes.Black, 50, 40, sf);
                        g.DrawString(dt.ToString("MMM", CultureInfo.GetCultureInfo("en-US")), new Font(parsed.Value.Font, 24), Brushes.Black, 50, 80, sf);
                    }



                    while (dt.Month == parsed.Value.Month)
                    {
                        var brush = Brushes.Black;
                        if (result.Contains(dt))
                        {
                            brush = Brushes.Red;
                        }
                        else
                        {
                            if (dt.DayOfWeek == DayOfWeek.Sunday)
                            {
                                brush = Brushes.Red;
                            }
                            else if (dt.DayOfWeek == DayOfWeek.Saturday)
                            {
                                brush = Brushes.Blue;
                            }
                            else
                            {
                                brush = Brushes.Black;
                            }
                        }

                        if(parsed.Value.Vertical)
                        {
                            g.DrawString(dt.ToString("ddd", CultureInfo.GetCultureInfo("en-US")), new Font(parsed.Value.Font, 16), brush, 50, dayPosY, sf);
                            g.DrawString(dt.Day.ToString(), new Font(parsed.Value.Font, 24), brush, 90, dayPosY, sf);

                            dayPosY += parsed.Value.Interval;
                        }
                        else
                        {
                            var posX = dayPosX + 20;

                            g.DrawString(dt.ToString("ddd", CultureInfo.GetCultureInfo("en-US")), new Font(parsed.Value.Font, 16), brush, posX, 40, sf);
                            g.DrawString(dt.Day.ToString(), new Font(parsed.Value.Font, 24), brush, posX, 80, sf);

                            dayPosX += parsed.Value.Interval;
                        }



                        dt = dt.AddDays(1);


                    }


                    img.Save(parsed.Value.OutputFile, ImageFormat.Png);
                }

            }
            else
            {
                Console.WriteLine("コマンドライン引数を間違えてないかぃ？");
            }
        }
    }
}
