using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Settings
{
    public class InitalSetup
    {
        public static void startInitalStartup(ApplicationSettings settings)
        {
            // Create Folder Structure
            EnsureFolderStructure(settings);

            // Move index.php to WebServer path
            MoveIndexFile(settings);
                        
            // Extract PHP Files
            ExtractPHPFiles(settings);
                        
        }










        // Move wwwroot index.php to WebServer path
        private static void MoveIndexFile(ApplicationSettings settings)
        {
            if (!Common.ifSettingsFileExists(settings.WebServer.WebServerPath + "\\index.php"))
            {
                if (File.Exists(Environment.CurrentDirectory + "\\index.php"))
                {
                    // Move File
                    string sourcePath = Environment.CurrentDirectory + "\\index.php";
                    string destinationPath = settings.WebServer.WebServerPath + "\\index.php";
                    File.Move(sourcePath, destinationPath);
                }
                else
                {
                    // Log error if index.php file does not exist
                    Logging.Common.AddLogItem("index.php file not found in the current directory.", "Error", "InitalSetup");
                }
            }
        }

        // Extract PHP files
        private static void ExtractPHPFiles(ApplicationSettings settings)
        {
            if (!Common.ifSettingsFileExists(settings.WebServer.PhpCompilerPath + "\\php.exe"))
            {
                if (File.Exists(Environment.CurrentDirectory + "\\php.zip"))
                {
                    // Extract PHP files
                    string zipPath = Environment.CurrentDirectory + "\\php.zip";
                    string extractPath = settings.WebServer.PhpCompilerPath;
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                }
                else
                {
                    Logging.Common.AddLogItem("php.zip file not found in the current directory.", "Error", "InitalSetup");
                }
            }
        }

        // Ensure folder structure
        private static void EnsureFolderStructure(ApplicationSettings settings)
        {
            // Setting ini path
            EnsureDirectory(Common.settingsFilePath);

            // Logging path
            EnsureDirectory(settings.Logging.LogFilePath);

            // Database path
            EnsureDirectory(settings.Database.DatabasePath);

            // PHP compiler directory
            EnsureDirectory(settings.WebServer.PhpCompilerPath);

            // Web server root
            EnsureDirectory(settings.WebServer.WebServerPath);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


    }
}
