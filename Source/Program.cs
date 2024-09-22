using Microsoft.Extensions.Configuration;

namespace leviubot.Source
{
    public class Program
    {
        public static IConfiguration config;
        public static string baseDir;

        private static void Main(string[] args) { }
        public static void Setup()
        {
            baseDir = GetBaseDir();
            config = GetConfig();
        }

        //public static string GetLastCommit()
        //{
        //    try
        //    {
        //        string gitFolderPath = Path.Combine(baseDir, ".git");

        //        string headFilePath = Path.Combine(gitFolderPath, "HEAD");
        //        string headContent = File.ReadAllText(headFilePath).Trim();

        //        string lastCommitHash;

        //        if (headContent.StartsWith("ref:"))
        //        {
        //            string referencePath = headContent.Substring(5).Trim();
        //            string refFilePath = Path.Combine(gitFolderPath, referencePath);

        //            lastCommitHash = File.ReadAllText(refFilePath).Trim();
        //        }
        //        else lastCommitHash = headContent;

        //        using (var repo = new Repository(baseDir))
        //        {
        //            Commit commit = repo.Lookup<Commit>(lastCommitHash);

        //            if (commit != null)
        //            {
        //                DateTimeOffset commitTime = commit.Committer.When;
        //                return $"Last commit time: {commitTime.LocalDateTime}";
        //            }
        //            else return "Commit with hash not found.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Print(ex.Message, Logtype.Error, "Error");
        //        return "Error?";
        //    }
        //}
        private static string GetBaseDir()
        {
            string baseDir = AppContext.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(baseDir);

            while (directory != null)
            {
                string configFilePath = Path.Combine(directory.FullName, "config.yml");

                if (File.Exists(configFilePath)) return directory.FullName;
                directory = directory.Parent;
            }
            throw new Exception("config.yml does not exist");
        }
        private static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddYamlFile("config.yml")
                .Build();
        }
    }
}