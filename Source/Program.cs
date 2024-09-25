using Microsoft.Extensions.Configuration;

namespace Source
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