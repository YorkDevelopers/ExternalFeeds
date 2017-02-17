using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YorkDevelopers.Shared;

namespace YorkDevelopers.MergeFeeds
{
    class Program
    {

        static void Main(string[] args)
        {
            const string FILENAME1 = "_data/EventBrite.yml";
            const string FILENAME2 = "_data/Meetup.yml";
            const string TARGETFILENAME = "_data/Events.yml";

            var gitHub = new GitHub();

            var yaml1 = gitHub.ReadFileFromGitHub(FILENAME1);
            var yaml2 = gitHub.ReadFileFromGitHub(FILENAME2);

            var deserializer = new Deserializer();
            var events1 = deserializer.Deserialize<List<Common>>(yaml1);
            var events2 = deserializer.Deserialize<List<Common>>(yaml2);
            var allEvents = events1.Union(events2);

            var serializer = new Serializer();
            var yaml = serializer.Serialize(allEvents);

            gitHub.WriteFileToGitHub(TARGETFILENAME, yaml);

        }


    }
}
