using System.Collections.Generic;
using YamlDotNet.Serialization;
using YorkDevelopers.Shared;

namespace YorkDevelopers.MergeFeeds
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = new List<string>() { "_data/EventBrite.yml", "_data/Meetup.yml", "_data/YPS.yml" };

            const string TARGETFILENAME = "_data/Events.yml";

            var gitHub = new GitHub();
            var deserializer = new Deserializer();

            var allEvents = new List<Common>();
            foreach (var file in files)
            {
                var yaml = gitHub.ReadFileFromGitHub(file);
                var extraEvents = deserializer.Deserialize<List<Common>>(yaml);
                allEvents.AddRange(extraEvents);
            }

            var serializer = new Serializer();
            var yamlAll = serializer.Serialize(allEvents);

            gitHub.WriteFileToGitHub(TARGETFILENAME, yamlAll);

        }
    }
}
