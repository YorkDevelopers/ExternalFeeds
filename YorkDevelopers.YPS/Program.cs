using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YorkDevelopers.Shared;

namespace YorkDevelopers.YPS
{
    class Program
    {
        //Speed,  filtering additional pages
        static void Main(string[] args)
        {
            const string URL = "http://www.ypsyork.org/events/?pno=1";

            var client = new WebClient();
            var html = client.DownloadString(URL);

            var document = new Document(html);

            var allEvents = new List<YorkDevelopers.Shared.Common>();
            var article = document.GetNextTag(@"<div class=""article"">");
            while (article != null)
            {
                var common = new Common();
                common.IsFree = false;  //free to members

                var articleTitle = document.GetNextTag(@"<h3 class=""article-title"">", article);
                if (articleTitle != null)
                {
                    var urlTag = document.GetNextTagOfType("a", articleTitle);
                    common.Name = urlTag.Contents;
                    common.URL = document.GetAttribute(urlTag, "href");
                }

                var articleDetails = document.GetNextTag(@"dl class=""article-details""", article);

                var dateLabelTag = document.GetNextTagOfType("dt", articleDetails);
                var dateTag = document.GetNextTagOfType("dd", dateLabelTag);

                var startTimeLabelTag = document.GetNextTagOfType("dt", dateTag);
                var startTimeTag = document.GetNextTagOfType("dd", startTimeLabelTag);

                if (startTimeTag.Contents == "All day event")
                {
                    common.Starts = DateTime.ParseExact(dateTag.Contents, "d MMM yyyy", null);
                    common.Ends = common.Starts.AddHours(23);
                }
                else
                {
                    common.Starts = DateTime.ParseExact(dateTag.Contents + " " + startTimeTag.Contents, "d MMM yyyy h:mm tt", null);
                    common.Ends = common.Starts.AddHours(2);
                }
                var venueLabelTag = document.GetNextTagOfType("dt", startTimeTag);
                var venueTag = document.GetNextTagOfType("dd", venueLabelTag);
                common.Venue = venueTag.Contents;

                var logo = document.GetNextTagOfType("img", venueTag);
                common.Logo = document.GetAttribute(logo, "src");

                common.Description = GetDescription(common.URL, client);
                allEvents.Add(common);


                article = document.GetNextTag(@"<div class=""article"">", article);
            }


            var serializer = new Serializer();
            var yaml = serializer.Serialize(allEvents);

            // Push the file to git
            var gitHubClient = new GitHub();
            gitHubClient.WriteFileToGitHub("_data/YPS.yml", yaml);
        }

        private static string GetDescription(string URL, WebClient client)
        {
            var html = client.DownloadString(URL);

            var document = new Document(html);
            var heading = document.GetNextTag(@"<h3 class=""article-title"">");
            var description = document.GetNextTagOfType("p", heading);
            return description.Contents;
        }
    }
}
