using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Settings
{
    public class Common
    {
        // Default File Paths
        internal const string settingsFilePath = "C:\\ChiaReporting\\config";
        internal const string LogFilePath = "C:\\ChiaReporting\\logs";
        internal const string DatabaseFilePath = "C:\\ChiaReporting\\db";
        internal const string DatabaseName = "FarmerData.db";

        public static bool ifSettingsFileExists(string filePath)
        {
            return System.IO.File.Exists(filePath);
        }


        public static bool createSettingsFile() 
        {
            // Do Somethin
            if (!ifSettingsFileExists(settingsFilePath))
            {
                
                // Create Default Settings Object
                ApplicationSettings applicationSettings = new ApplicationSettings();

                // Logging Settings
                applicationSettings.Logging = new LoggingSettings
                {
                    LoggingEnabled = true,
                    LogFilePath = LogFilePath
                };

                // Harvester Settings
                applicationSettings.Harvester = new HarvesterSettings
                {
                    HarvesterIDs = new List<string>()
                };

                // Database Settings
                applicationSettings.Database = new DatabaseSettings
                {
                    DatabaseName = DatabaseName,
                    DatabasePath = DatabaseFilePath
                };

                // Web Server Settings

                applicationSettings.WebServer = new WebServerSettings
                {
                    EnableWebserver = true,
                    WebServerPort = 8080,
                    PhpCompilerPath = "C:\\ChiaReporting\\webserver\\php",
                    WebServerPath = "C:\\ChiaReporting\\webserver\\wwwroot"
                };

                // Data Collection Intervals
                applicationSettings.DataCollection = new DataCollectionIntervals
                {
                    FarmerDetails = "60",
                    FarmerBlocks = "60",
                    FarmerPlots = "60",
                    FarmerPayouts = "60",
                    FarmerPayoutBatches = "60",
                    FarmerPartials = "60"
                };

                // Inital Setup
                InitalSetup.startInitalStartup(applicationSettings);

                // Serialise Settings Object
                string settingsString = JsonSerializer.Serialize(applicationSettings, new JsonSerializerOptions { WriteIndented = true });

                // Write Settings to File
                string settingsPath = settingsFilePath + "\\settings.ini";
                System.IO.File.WriteAllText(settingsPath, settingsString);

                // Check if the Database Exists and Create if not


                return true;

            }
            else
            {
                return false;
            }
        }

        public static ApplicationSettings getApplicationSettings(string path = settingsFilePath)
        {
            // Define Return Object 
            ApplicationSettings settings = new ApplicationSettings();

            string settingsPath = settingsFilePath + "\\settings.ini";

            // Read Settings File
            if (ifSettingsFileExists(settingsPath))
            {
                // Read File
                string settingsString = System.IO.File.ReadAllText(settingsPath);

                // Deserialise Output
                settings = JsonSerializer.Deserialize<ApplicationSettings>(settingsString);
            }
            else
            {
                // Create Default Settings
                createSettingsFile();
            }

            // Return Settings Object
            return settings;
        }


    }

    public class LoggingSettings
    {
        public bool LoggingEnabled { get; set; }
        public string LogFilePath { get; set; }
    }

    public class HarvesterSettings
    {
        public List<string> HarvesterIDs { get; set; }
    }

    public class DatabaseSettings
    {
        public string DatabaseName { get; set; }
        public string DatabasePath { get; set; }
    }

    public class WebServerSettings
    {
        public bool EnableWebserver { get; set; }
        public int WebServerPort { get; set; }
        public string PhpCompilerPath { get; set; }
        public string WebServerPath { get; set; }
    }

    public class DataCollectionIntervals
    {
        public string FarmerDetails { get; set; }
        public string FarmerBlocks { get; set; }
        public string FarmerPlots { get; set; }
        public string FarmerPayouts { get; set; }
        public string FarmerPayoutBatches { get; set; }
        public string FarmerPartials { get; set; }
    }

    public class  ApplicationSettings
    {
        public LoggingSettings Logging { get; set; }
        public HarvesterSettings Harvester { get; set; }
        public DatabaseSettings Database { get; set; }
        public WebServerSettings WebServer { get; set; }
        public DataCollectionIntervals DataCollection { get; set; }
    }
}
