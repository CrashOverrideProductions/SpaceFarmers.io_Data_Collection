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
        

        static void Main(string[] args)
        {

            // Statup Routine

            // Get Application Settings
            Console.WriteLine("Inital Startup...");

            Settings.Common settings = new Settings.Common();
            appSettings = Settings.Common.getApplicationSettings();

            // Test SQL Connection
            Console.WriteLine("Testing SQL Connection...");

            if (DatabaseFunctions.TestSQLConnection(appSettings.Database))
            {
                Console.WriteLine("SQL Connection Successful");
            }
            else
            {
                Console.WriteLine("SQL Connection Failed");

                // Create Database
                Console.WriteLine("Creating Database...");
                DatabaseFunctions.CreateDatabase(appSettings.Database);
                Console.WriteLine("Database Created");

            }

            // Check for Harvester IDs
            Console.WriteLine("Checking for Harvester IDs...");
            if (appSettings.Harvester.HarvesterIDs.Count == 0)
            {
                Console.WriteLine("No Harvester IDs Found");
                Console.WriteLine("Please add Harvester IDs to the appsettings.json file");
                Console.ReadLine();
                return;

                // TBD
                // Stop Service
            }
            else
            {
                Console.WriteLine("Harvester IDs Found: " + appSettings.Harvester.HarvesterIDs.Count);
            }

            // Start WebServer
            WebServer.Common webServer = new WebServer.Common(appSettings.WebServer);
            webServer.StartWebServer();


            // ================ Do Stuff

            // Background Workers for Data Collection
            Console.WriteLine("Starting Background Workers...");

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
                    //Console.WriteLine("Getting SQL Lines to Insert...");
                    List<string> lines = new List<string>();

                    lines.AddRange(SQLHandling.Common.sqlLines);

                    // Remove any lines from sqlLines that exist in lines
                    SQLHandling.Common.sqlLines.RemoveAll(x => lines.Contains(x));

                    //// Add Lines to SQL
                    //Console.WriteLine("Adding SQL Lines to Database...");

                    DatabaseFunctions.InsertDataToDatabase(appSettings.Database, lines);
                }
            }



            Console.WriteLine("Starting Background Workers...");

            farmerDetailsBGWorker.RunWorkerAsync(appSettings);
            farmerBlocksBGWorker.RunWorkerAsync(appSettings);
            farmerPayoutsBGWorker.RunWorkerAsync(appSettings);
            farmerPayoutBatchesBGWorker.RunWorkerAsync(appSettings);
            farmerPartialsBGWorker.RunWorkerAsync(appSettings);
            farmerPlotsBGWorker.RunWorkerAsync(appSettings);
            sqlBGWorker.RunWorkerAsync(appSettings);

            Console.WriteLine("Background Workers Started");



            Console.WriteLine("End of Line");

            Console.ReadLine();

        }
    }
}
