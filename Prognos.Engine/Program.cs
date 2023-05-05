using Akka.Actor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prognos.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("prognos");
            var scraperManagerActor = actorSystem.ActorOf(ScraperManagerActor.Props());

            while (true)
            {
                var input = Console.ReadLine();
                Match match = _scrapCmdRegex.Match(input);
                if (match.Success)
                {
                    long eventId = long.Parse(match.Groups[1].Value);
                    scraperManagerActor.Tell(new ScrapingCommand(
                        new Core.Winamax.WinamaxEvent(eventId, Core.Sport.Football, "", "", new DateTime()),
                        new ScrapingOptions(
                            scrapInterval: TimeSpan.FromSeconds(5))));
                }
                else
                {
                    Console.WriteLine("Invalid command");
                }
            }
        }

        private static Regex _scrapCmdRegex = new Regex(@"^/scrap\s+winamax\s+(\d+)\s*$");

    //    static void Main(string[] args)
    //    {
    //        //Console.ReadLine();
    //        var task = StartBrowser("19410904");
    //        task.Wait();
    //        Console.ReadLine();
    //    }

    //    static async Task StartBrowser(string matchId)
    //    {
    //        Console.WriteLine("Downloading Browser...");
    //        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
    //        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    //        {
    //            Headless = true
    //        });

    //        Console.WriteLine("New Page...");
    //        var page = await browser.NewPageAsync();

    //        var client = await page.Target.CreateCDPSessionAsync();
    //        await client.SendAsync("Network.enable");
    //        await client.SendAsync("Page.enable");

    //        client.MessageReceived += (sender, e) =>
    //        {
    //            if (e.MessageID == "Network.webSocketFrameReceived")
    //            {
    //                var payloadData = e.MessageData["response"]["payloadData"].ToString();

    //                PlatformEvent match = _updateRegex.Match(payloadData);
    //                if (match.Success)
    //                {
    //                    var data = match.Groups[1].Value;

    //                    try
    //                    {
    //                        dynamic json = JsonConvert.DeserializeObject(data);

    //                        JToken matches = json["matches"];
    //                        if (matches != null)
    //                        {
    //                            var matchTime = matches[matchId]["matchTimeExtended"];
    //                        }

    //                        JToken odds = json["odds"];
    //                        Dictionary<string, float> oddsDict = null;

    //                        if (odds != null)
    //                        {
    //                            oddsDict = new Dictionary<string, float>();
    //                            foreach (var child in odds.Children())
    //                            {
    //                                if (child.Type == JTokenType.Property)
    //                                {
    //                                    var prop = child as JProperty;
    //                                    string rateStr = prop.Value.ToString();
    //                                    if (float.TryParse(rateStr, out float rate)) 
    //                                        oddsDict[prop.Name] = rate;
    //                                }
    //                            }
    //                        }
                                
    //                        Console.WriteLine(data);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.WriteLine($"Parse error: {data}.");
    //                    }
    //                }
    //            }
    //        };

    //        Console.WriteLine("Go to page");
    //        await page.GoToAsync($"https://www.winamax.fr/paris-sportifs/match/live/{matchId}");
    //        //await page.PdfAsync(@"C:\\Users\\maxime.laudrin\\Documents\\test.pdf");
    //        //Console.WriteLine("PDF done");
    //    }

    //    private static Regex _updateRegex = new Regex(@"^42\[""m"",(.*)\]$");

    //    private static void Client_MessageReceived(object sender, MessageEventArgs e)
    //    {
    //    }
    }
}
