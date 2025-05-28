using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Numerics;
using SQLHandling;
using System.Configuration;
using System.ComponentModel;
using Settings;

namespace TestingConsoleApp
{
    internal class Program
    {
        // Settings
        public static Settings.ApplicationSettings appSettings = new Settings.ApplicationSettings();
        
        // Logging
        public static Logging.Common logging = new Logging.Common();
        
        // WebServer
        public static Settings.WebServerSettings webServerSettings = new Settings.WebServerSettings();

        // SpaceFarmers DataCollection
        public static DataCollection.SpaceFarmers.FarmerBlocks farmerBlocks = new DataCollection.SpaceFarmers.FarmerBlocks();
        public static DataCollection.SpaceFarmers.FarmerPayouts farmerPayouts = new DataCollection.SpaceFarmers.FarmerPayouts();
        public static DataCollection.SpaceFarmers.FarmerPayoutBatches farmerPayoutBatches = new DataCollection.SpaceFarmers.FarmerPayoutBatches();
        public static DataCollection.SpaceFarmers.FarmerPartials farmerPartials = new DataCollection.SpaceFarmers.FarmerPartials();
        public static DataCollection.SpaceFarmers.FarmerPlots farmerPlots = new DataCollection.SpaceFarmers.FarmerPlots();

        // SQL 
        public static SQLHandling.Common sQLHandling = new SQLHandling.Common();

        public static void StopService() 
        {
            // Log the stopping of the service
            Logging.Common.AddLogItem("Stopping Service", "Info", "Program.StopService");

            // Check the data collection background workers and cancel them
            
            // Check the SQL background worker and cancel it

            // Check the logging background worker and cancel it

            // Actually Stop the Service

        }

        static void Main(string[] args)
        {
            // Statup Routine

            // Get Application Settings
            Logging.Common.AddLogItem("Inital Startup", "Info", "Program.Main");

            Settings.Common settings = new Settings.Common();
            appSettings = Settings.Common.getApplicationSettings();



            // The First thing - Start the Logging BG worker
            Logging.Common.AddLogItem("Starting Logging Background Worker", "Info", "Program.Main");

            BackgroundWorker loggingBGWorker = new BackgroundWorker();
            loggingBGWorker.WorkerReportsProgress = true;
            loggingBGWorker.WorkerSupportsCancellation = true;
            loggingBGWorker.DoWork += loggingBGWorker_DoWork;

            // Start the Logging Background Worker
            loggingBGWorker.RunWorkerAsync(appSettings);


            

            // Test SQL Connection
            Logging.Common.AddLogItem("Testing SQL Connection", "Info", "Program.Main");
            
            if (DatabaseFunctions.TestSQLConnection(appSettings.Database))
            {
                Logging.Common.AddLogItem("SQL Connection Successful", "Info", "Program.Main");
            }
            else
            {
                Logging.Common.AddLogItem("SQL Connection Failed", "Error", "Program.Main");

                // Create Database
                Logging.Common.AddLogItem("Creating Database", "Info", "Program.Main");
                DatabaseFunctions.CreateDatabase(appSettings.Database);
            }

            // Check for Harvester IDs
            Logging.Common.AddLogItem("Checking for Harvester IDs", "Info", "Program.Main");
            if (appSettings.Harvester.HarvesterIDs.Count == 0)
            {
                Logging.Common.AddLogItem("No Harvester IDs found in appsettings.json", "Error", "Program.Main");
                
                // Stop Service
                Logging.Common.AddLogItem("Stopping Service", "Info", "Program.Main");
                StopService();
            }
            else
            {
                Logging.Common.AddLogItem("Harvester IDs found in appsettings.json", "Info", "Program.Main");
                // Log the Harvester IDs
                foreach (var harvesterID in appSettings.Harvester.HarvesterIDs)
                {
                    Logging.Common.AddLogItem($"Harvester ID: {harvesterID}", "Info", "Program.Main");
                }
            }

            // Start WebServer
            Logging.Common.AddLogItem("Starting WebServer", "Info", "Program.Main");

            WebServer.Common webServer = new WebServer.Common(appSettings.WebServer);
            webServer.StartWebServer();


            // ================ Do Stuff

            // Background Workers for Data Collection
            Logging.Common.AddLogItem("Starting Background Workers", "Info", "Program.Main");
            
            // Background Workers
            BackgroundWorker farmerDetailsBGWorker = new BackgroundWorker();
            farmerDetailsBGWorker.WorkerReportsProgress = true;
            farmerDetailsBGWorker.WorkerSupportsCancellation = true;
            farmerDetailsBGWorker.DoWork += farmerDetailsBGWorker_DoWork;
            farmerDetailsBGWorker.ProgressChanged += farmerDetailsBGWorker_ProgressChanged;

            BackgroundWorker farmerBlocksBGWorker = new BackgroundWorker();
            farmerBlocksBGWorker.WorkerReportsProgress = true;
            farmerBlocksBGWorker.WorkerSupportsCancellation = true;
            farmerBlocksBGWorker.DoWork += farmerBlocksBGWorker_DoWork;
            farmerBlocksBGWorker.ProgressChanged += farmerBlocksBGWorker_ProgressChanged;

            BackgroundWorker farmerPayoutsBGWorker = new BackgroundWorker();
            farmerPayoutsBGWorker.WorkerReportsProgress = true;
            farmerPayoutsBGWorker.WorkerSupportsCancellation = true;
            farmerPayoutsBGWorker.DoWork += farmerPayoutsBGWorker_DoWork;
            farmerPayoutsBGWorker.ProgressChanged += farmerPayoutsBGWorker_ProgressChanged;

            BackgroundWorker farmerPayoutBatchesBGWorker = new BackgroundWorker();
            farmerPayoutBatchesBGWorker.WorkerReportsProgress = true;
            farmerPayoutBatchesBGWorker.WorkerSupportsCancellation = true;
            farmerPayoutBatchesBGWorker.DoWork += farmerPayoutBatchesBGWorker_DoWork;
            farmerPayoutBatchesBGWorker.ProgressChanged += farmerPayoutBatchesBGWorker_ProgressChanged;

            BackgroundWorker farmerPartialsBGWorker = new BackgroundWorker();
            farmerPartialsBGWorker.WorkerReportsProgress = true;
            farmerPartialsBGWorker.WorkerSupportsCancellation = true;
            farmerPartialsBGWorker.DoWork += farmerPartialsBGWorker_DoWork;
            farmerPartialsBGWorker.ProgressChanged += farmerPartialsBGWorker_ProgressChanged;
            
            BackgroundWorker farmerPlotsBGWorker = new BackgroundWorker();
            farmerPlotsBGWorker.WorkerReportsProgress = true;
            farmerPlotsBGWorker.WorkerSupportsCancellation = true;
            farmerPlotsBGWorker.DoWork += farmerPlotsBGWorker_DoWork;
            farmerPlotsBGWorker.ProgressChanged += farmerPlotsBGWorker_ProgressChanged;

            // SQL Background Worker
            BackgroundWorker sqlBGWorker = new BackgroundWorker();
            sqlBGWorker.WorkerReportsProgress = true;
            sqlBGWorker.WorkerSupportsCancellation = true;
            sqlBGWorker.DoWork += sqlBGWorker_DoWork;
            sqlBGWorker.ProgressChanged += sqlBGWorker_ProgressChanged;


            // Background Worker Event Handlers

            // Farmer Details
            void farmerDetailsBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                DataCollection.SpaceFarmers.FarmerDetails farmerDetails = new DataCollection.SpaceFarmers.FarmerDetails();

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (Convert.ToInt32(ApplicationSettings.DataCollection.FarmerDetails) * 1000) * 60;

                    // Farmer Details
                    List<string> farmerDetailsList = new List<string>();

                    // Get Farmer Details
                    farmerDetailsList = (farmerDetails.getFarmerDetails(ApplicationSettings.Harvester.HarvesterIDs).Result);

                    // Report Progress
                    worker.ReportProgress(0, farmerDetailsList);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                } 
                e.Cancel = true; // Cancel the worker
            };

            void farmerDetailsBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result from the BackgroundWorker
                List<string> farmerDetailsList = (List<string>)e.UserState;

                // Add the Result to the SQL Lines
                SQLHandling.Common.sqlLines.AddRange(farmerDetailsList);
            }

            // Farmer Blocks Background Worker
            void farmerBlocksBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (Convert.ToInt32(ApplicationSettings.DataCollection.FarmerBlocks) * 1000) * 60;

                    // Farmer Blocks
                    List<string> farmerBlocksList = new List<string>();

                    // Get Farmer Blocks
                    farmerBlocksList = (farmerBlocks.getFarmerBlocks(ApplicationSettings).Result);

                    // Report Progress
                    worker.ReportProgress(0, farmerBlocksList);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                }
                e.Cancel = true; // Cancel the worker
            };

            void farmerBlocksBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result from the BackgroundWorker
                List<string> farmerBlocksList = (List<string>)e.UserState;

                // Add the Result to the SQL Lines
                SQLHandling.Common.sqlLines.AddRange(farmerBlocksList);
            }


            // Farmer Payouts Background Worker
            void farmerPayoutsBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (Convert.ToInt32(ApplicationSettings.DataCollection.FarmerPayouts) * 1000) * 60;

                    // Farmer Payouts
                    List<string> farmerPayoutsList = new List<string>();

                    // Get Farmer Payouts
                    farmerPayoutsList = (farmerPayouts.getFarmerPayouts(ApplicationSettings).Result);

                    // Report Progress
                    worker.ReportProgress(0, farmerPayoutsList);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                }
                e.Cancel = true; // Cancel the worker
            };

            void farmerPayoutsBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result from the BackgroundWorker
                List<string> farmerPayoutsList = (List<string>)e.UserState;

                // Add the Result to the SQL Lines
                SQLHandling.Common.sqlLines.AddRange(farmerPayoutsList);
            }

            // Farmer Payout Batches Background Worker
            void farmerPayoutBatchesBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (Convert.ToInt32(ApplicationSettings.DataCollection.FarmerPayoutBatches) * 1000) * 60;

                    // Farmer Payout Batches
                    List<string> farmerPayoutBatchesList = new List<string>();

                    // Get Farmer Payout Batches
                    farmerPayoutBatchesList = (farmerPayoutBatches.getFarmerPayoutBatches(ApplicationSettings).Result);

                    // Report Progress
                    worker.ReportProgress(0, farmerPayoutBatchesList);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                }
                e.Cancel = true; // Cancel the worker
            };

            void farmerPayoutBatchesBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result from the BackgroundWorker
                List<string> farmerPayoutBatchesList = (List<string>)e.UserState;

                // Add the Result to the SQL Lines
                SQLHandling.Common.sqlLines.AddRange(farmerPayoutBatchesList);
            }

            // Farmer Partials Background Worker
            void farmerPartialsBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (Convert.ToInt32(ApplicationSettings.DataCollection.FarmerPartials) * 1000) * 60;

                    // Farmer Partials
                    List<string> farmerPartialsList = new List<string>();

                    // Get Farmer Partials
                    farmerPartialsList = (farmerPartials.getFarmerPartials(ApplicationSettings).Result);

                    // Report Progress
                    worker.ReportProgress(0, farmerPartialsList);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                }
                e.Cancel = true; // Cancel the worker
            };

            void farmerPartialsBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result from the BackgroundWorker
                List<string> farmerPartialsList = (List<string>)e.UserState;

                // Add the Result to the SQL Lines
                SQLHandling.Common.sqlLines.AddRange(farmerPartialsList);
            }

            // Farmer Plots Background Worker
            void farmerPlotsBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (Convert.ToInt32(ApplicationSettings.DataCollection.FarmerPlots) * 1000) * 60;

                    // Farmer Plots
                    List<string> farmerPlotsList = new List<string>();

                    // Get Farmer Plots
                    farmerPlotsList = (farmerPlots.getFarmerPlots(ApplicationSettings.Harvester.HarvesterIDs).Result);

                    // Report Progress
                    worker.ReportProgress(0, farmerPlotsList);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                }
                e.Cancel = true; // Cancel the worker
            };

            void farmerPlotsBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result from the BackgroundWorker
                List<string> farmerPlotsList = (List<string>)e.UserState;

                // Add the Result to the SQL Lines
                SQLHandling.Common.sqlLines.AddRange(farmerPlotsList);
            }

            // SQL Background Worker
            void sqlBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings ApplicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Calulate Sleep milliseconds
                    int sleepMilliseconds = (30000);

                    // Report Progress
                    worker.ReportProgress(0, null);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(sleepMilliseconds);
                }
                e.Cancel = true; // Cancel the worker
            };

            void sqlBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // Get the Result
                List<string> farmerDetailsList = SQLHandling.Common.sqlLines;

                if (farmerDetailsList.Count > 0)
                {
                    // Add the Result to the SQL Lines
                    SQLHandling.Common.sqlLines.AddRange(farmerDetailsList);

                    //// Get SQL Lines to Insert
                    List<string> lines = new List<string>();

                    lines.AddRange(SQLHandling.Common.sqlLines);

                    // Remove any lines from sqlLines that exist in lines
                    SQLHandling.Common.sqlLines.RemoveAll(x => lines.Contains(x));

                    //// Add Lines to SQL
                    DatabaseFunctions.InsertDataToDatabase(appSettings.Database, lines);
                }
            }

            // Logging Background Worker
            void loggingBGWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = sender as BackgroundWorker;

                // Get Settings
                ApplicationSettings applicationSettings = (ApplicationSettings)e.Argument;

                // Do Work Loop
                while (!worker.CancellationPending)
                {
                    // Temp Holder for Log Items
                    List<Logging.LogItem> logItemsTemp = new List<Logging.LogItem>();

                    // Get Log Items
                    logItemsTemp.AddRange(Logging.Common.LogItems);

                    // Remove all LogItems that have been logged
                    Logging.Common.LogItems.RemoveAll(x => logItemsTemp.Contains(x));

                    // Add Log Items to Log File
                    Logging.Logging.LogMessage(logItemsTemp, applicationSettings.Logging.LogFilePath);

                    // Sleep for Collection Interval
                    System.Threading.Thread.Sleep(1000);
                }
                e.Cancel = true; // Cancel the worker
            };

            Logging.Common.AddLogItem("Logging Background Worker Started", "Info", "Program.Main");
            farmerDetailsBGWorker.RunWorkerAsync(appSettings);
            farmerBlocksBGWorker.RunWorkerAsync(appSettings);
            farmerPayoutsBGWorker.RunWorkerAsync(appSettings);
            farmerPayoutBatchesBGWorker.RunWorkerAsync(appSettings);
            farmerPartialsBGWorker.RunWorkerAsync(appSettings);
            farmerPlotsBGWorker.RunWorkerAsync(appSettings);
            sqlBGWorker.RunWorkerAsync(appSettings);

            Logging.Common.AddLogItem("Background Workers Started", "Info", "Program.Main");



            Console.WriteLine("End of Line");

            Console.ReadLine();

        }
    }
}
