using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prognos.Engine
{
    public interface IPlatformEventScraper
    {
        Page Page { get; }
        Task<OutcomesSnapshot> ScrapPage();
        Task SubscribeToLive(Action<LiveOddsUpdate> callback);
    }

    public class WinamaxEventScraper : IPlatformEventScraper
    {
        public Page Page { get; }
        private readonly PlatformEvent _platformEvent;

        public WinamaxEventScraper(Page page, PlatformEvent platformEvent)
        {
            Page = page;
            _platformEvent = platformEvent;
        }

        public async Task SubscribeToLive(Action<LiveOddsUpdate> callback)
        {
            var client = await Page.Target.CreateCDPSessionAsync();

            await client.SendAsync("Network.enable");
            await client.SendAsync("Page.enable");

            client.MessageReceived += (sender, e) =>
            {
                if (e.MessageID == "Network.webSocketFrameReceived")
                {
                    var payloadData = e.MessageData["response"]["payloadData"].ToString();
                    if (TryParseOdds(payloadData, out var odds))
                        callback.Invoke(odds);
                }
            };

            //await _page.GoToAsync($"https://www.winamax.fr/paris-sportifs/match/live/{_event}");
        }

        public async Task<OutcomesSnapshot> ScrapPage()
        {
            await Page.GoToAsync(PageUri());
            //string content = await Page.GetContentAsync();

            var scripts = await Page.XPathAsync("//script");

            // Get <script> element containing preloaded state as Javascript object
            string jsData = null;
            foreach (var script in scripts)
            {
                var innerText = await script.GetPropertyAsync("innerText");
                var jsonValue = await innerText.JsonValueAsync();
                var text = jsonValue.ToString();

                Match match = _preloadedStateRegex.Match(text);
                if (match.Success)
                {
                    jsData = match.Groups[1].Value;
                    break;
                }
            }

            var liveElem = await Page.XPathAsync("//div[contains(@class, 'sc-ileJJU')]");
            bool isLive = liveElem.Length > 0;

            dynamic json = JsonConvert.DeserializeObject(jsData);
            var oddsDict = ParseOdds(json["odds"]);
            var outcomes = ParseOutcomes(json["outcomes"], oddsDict);

            return new OutcomesSnapshot(
                @event: _platformEvent,
                createdAt: DateTime.Now,
                state: isLive ? PlatformEventState.Live : PlatformEventState.Open,  // @todo
                outcomes: outcomes);
        }

        private readonly Regex _preloadedStateRegex = new Regex(@"^var PRELOADED_STATE =(.*);var");

        public string PageUri()
        {
            return $"https://www.winamax.fr/paris-sportifs/match/live/{_platformEvent.PlatformId}";
        }

        private static Regex _updateRegex = new Regex(@"^42\[""m"",(.*)\]$");

        private bool TryParseOdds(string payloadData, out LiveOddsUpdate oddsUpdate)
        {
            Match match = _updateRegex.Match(payloadData);
            oddsUpdate = null;

            if (!match.Success)
                return false;
            else
            {
                var data = match.Groups[1].Value;

                try
                {
                    dynamic json = JsonConvert.DeserializeObject(data);

                    var node = json["matches"];
                    node = node[_platformEvent.PlatformId];
                    node = node["matchtimeExtended"];
                    string eventTimeStr = node.ToString();

                    string[] parts = eventTimeStr.Split(':');
                    var eventTime = TimeSpan.FromMinutes(int.Parse(parts[0]))
                        + TimeSpan.FromSeconds(int.Parse(parts[1]));
                    //var eventTime = TimeSpan.ParseExact(eventTimeStr, "mm:SS", CultureInfo.InvariantCulture);

                    var oddsDict = ParseOdds(json["odds"]);

                    oddsUpdate = new LiveOddsUpdate(_platformEvent, DateTime.Now, eventTime, oddsDict);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Parse error: {data}.");
                    return false;
                }
            }
        }

        private Dictionary<string, float> ParseOdds(JToken oddsNode)
        {
            Dictionary<string, float> oddsDict = null;

            if (oddsNode != null)
            {
                oddsDict = new Dictionary<string, float>();
                foreach (var child in oddsNode.Children())
                {
                    if (child.Type == JTokenType.Property)
                    {
                        var prop = child as JProperty;
                        string rateStr = prop.Value.ToString();
                        if (float.TryParse(rateStr, out float rate)) 
                            oddsDict[prop.Name] = rate;
                    }
                }
            }

            return oddsDict;
        }

        private Outcome[] ParseOutcomes(JToken outcomesNode, Dictionary<string, float> oddsDict)
        {
            var outcomes = new List<Outcome>();
            var outcomePrefixes = new List<string>();

            foreach (var child in outcomesNode.Children())
            {
                if (child.Type == JTokenType.Property)
                {
                    var prop = child as JProperty;
                    JToken data = prop.Value;

                    outcomes.Add(new Outcome(
                        id: prop.Name,
                        code: data["code"].ToString(),
                        label: data["label"].ToString(),
                        odd: oddsDict[prop.Name]));

                    outcomePrefixes.Add(prop.Name.Substring(0, 6));
                }
            }

            var countDict = outcomePrefixes.GroupBy(p => p)
                .ToLookup(g => g.Count(), g => g.Key);
            int maxFreq = countDict.Max(kv => kv.Key);
            var mostFreqPrefix = countDict[maxFreq].First();

            return outcomes.Where(o => o.Id.StartsWith(mostFreqPrefix)).ToArray();
        }
    }

    public struct Outcome
    {
        public string Id { get; }
        public string Code { get; }
        public string Label { get; }
        public float Odd { get; }

        public Outcome(string id, string code, string label, float odd)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Odd = odd;
        }
    }
}
