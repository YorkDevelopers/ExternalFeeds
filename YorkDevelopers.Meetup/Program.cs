﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using YamlDotNet.Serialization;
using YorkDevelopers.Meetup.Models;
using YorkDevelopers.Meetup.Properties;
using YorkDevelopers.Shared;

namespace YorkDevelopers.Meetup
{
    /// <summary>
    /// The application will download any suitable events from Meetup and then 
    /// write them to a yaml file called Meetup.yml.  This file will then
    /// be pushed into our git hub repro.
    /// </summary>
    class Program
    {
        static void Main()
        {
            const string URL = "https://api.meetup.com";
            const string TECH = "292";

            // Location of the Perky Peacock
            const double LAT = 53.960636138916016;
            const double LON = -1.0860970020294189;
            const double LARGEST_DISTANCE = 25;

            var client = PrepareHttpClient(new Uri(URL));
            var events = GET<List<Event>>(client, "/recommended/events?sign=true&key=" + Settings.Default.MEETUPTOKEN + "&fields=group_photo&topic_category=" + TECH);
            var geoData = new GeoData();

            var allEvents = new List<YorkDevelopers.Shared.Common>();
            foreach (var evt in events)
            {
                // Is this event near to us?
                var distance = geoData.distance(LAT, LON, evt.venue.lat, evt.venue.lon, 'M');
                if (distance <= LARGEST_DISTANCE)
                {
                    var common = new Common();
                    common.Name = evt.name;
                    common.Description = evt.description;
                    common.URL = evt.link;

                    // An event is free unless it contains a fee record with an amount which isn't 0
                    common.IsFree = ((evt.fee?.amount ?? 0) == 0);

                    // Events optionally have a group photo
                    common.Logo = evt.group.photo?.thumb_link;

                    // The start time is held in milliseconds
                    common.Starts = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(evt.time));

                    if (evt.duration == null)
                    {
                        // If no duration is supplied,  then meetup assumes 3 hours
                        common.Ends = common.Starts.AddHours(3);
                    }
                    else
                    {
                        // Duration is also held in milliseconds
                        var duration = TimeSpan.FromMilliseconds(double.Parse(evt.duration));
                        common.Ends = common.Starts + duration;
                    }
                    common.Venue = evt.venue?.name;
                    allEvents.Add(common);
                }
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
