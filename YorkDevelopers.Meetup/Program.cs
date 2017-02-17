using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using YamlDotNet.Serialization;
using YorkDevelopers.Meetup.Models;
using YorkDevelopers.Meetup.Properties;
using YorkDevelopers.Shared;

namespace YorkDevelopers.Meetup
{
    class Program
    {
        static void Main(string[] args)
        {
            const string URL = "https://api.meetup.com";
            
            const string TECH = "292";

            var client = PrepareHttpClient(new Uri(URL));
            var allEvents = new List<YorkDevelopers.Shared.Common>();

            var events = GET<List<Event>>(client, "/recommended/events?sign=true&key=" + Settings.Default.MEETUPTOKEN + "&fields=group_photo&topic_category=" + TECH);

            foreach (var evt in events)
            {
                var common = new Common();
                common.Name = evt.name;
                common.Description = evt.description;
                common.URL = evt.link;
                common.IsFree = ((evt.fee?.amount ?? 0) == 0);
                common.Logo = evt.group.photo?.thumb_link;

                common.Starts = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(evt.time));

                if (evt.duration == null)
                {
                    // If no duration is supplied,  then meetup assumes 3 hours
                    common.Ends = common.Starts.AddHours(3);
                }
                else
                {
                    var duration = TimeSpan.FromMilliseconds(double.Parse(evt.duration));
                    common.Ends = common.Starts + duration;
                }
                common.Venue = evt.venue?.name;
                allEvents.Add(common);
            }

            var serializer = new Serializer();
            var yaml = serializer.Serialize(allEvents);

            // Push the file to git
            var gitHubClient = new GitHub();
            gitHubClient.WriteFileToGitHub("_data/Meetup.yml", yaml);

        }

        private static HttpClient PrepareHttpClient(Uri endPoint)
        {
            var client = new HttpClient();
            client.BaseAddress = endPoint;
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
