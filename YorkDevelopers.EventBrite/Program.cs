using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using YamlDotNet.Serialization;
using YorkDevelopers.EventBrite.Models;
using YorkDevelopers.EventBrite.Properties;
using YorkDevelopers.Shared;

namespace YorkDevelopers.EventBrite
{
    class Program
    {
        static void Main(string[] args)
        {
            const string CATEGORY = "102"; //Science and Tech
            const string URL = "https://www.eventbriteapi.com";
            const string FILENAME = "_data/EventBrite.yml";

            var venues = new Dictionary<string, Venue>();
            var client = PrepareHttpClient(new Uri(URL), Settings.Default.EVENTBRITE_TOKEN);
            var events = GET<Responses>(client, $"/v3/events/search/?location.address=York&categories={CATEGORY}");

            var allEvents = new List<Common>();
            foreach (var evt in events.events)
            {
                var venue = default(Venue);
                if (!venues.TryGetValue(evt.venue_id, out venue))
                {
                    venue = GET<Venue>(client, $"/v3/venues/{evt.venue_id}/");
                    venues.Add(evt.venue_id, venue);
                }

                var common = new Common();
                common.Name = evt.name.text;
                common.Description = evt.description.text;
                common.URL = evt.url;
                common.IsFree = evt.is_free;
                common.Logo = CleanUpURL(evt.logo?.url);
                common.Starts = evt.start.local;
                common.Ends = evt.end.local;
                common.Venue = venue.name;
                allEvents.Add(common);
            }

            var serializer = new Serializer();
            var yaml = serializer.Serialize(allEvents);

            // Push the file to git
            var gitHubClient = new GitHub();
            gitHubClient.WriteFileToGitHub(FILENAME, yaml);

        }

        private static string CleanUpURL(string originalURL)
        {
            const string PREFIX = "https://img.evbuc.com/";

            if (!string.IsNullOrWhiteSpace(originalURL) && originalURL.StartsWith(PREFIX))
            {
                originalURL = originalURL.Substring(PREFIX.Length);
                originalURL = WebUtility.UrlDecode(originalURL);
            }

            return originalURL;
        }

        private static HttpClient PrepareHttpClient(Uri endPoint, string oAuthtoken)
        {
            var client = new HttpClient();
            client.BaseAddress = endPoint;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + oAuthtoken);
            return client;
        }

        public static T GET<T>(HttpClient client, string apiCall)
        {
            // Proxy the call onto our service.
            var httpResponseMessage = client.GetAsync(apiCall).Result;
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception(string.Format("Failed to GET from {0}.   Status {1}.  Reason {2}. {3}", apiCall, (int)httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase, httpResponseMessage.RequestMessage));
            }
            else
            {
                return httpResponseMessage.Content.ReadAsAsync<T>().Result;
            }
        }
    }
}
