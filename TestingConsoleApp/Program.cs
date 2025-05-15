using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Numerics;
using SQLHandling;

namespace TestingConsoleApp
{
    internal class Program
    {
        // List of Launcher IDs
        public static List<string> launcherID = new List<string>();

        // SpaceFarmers DataCollection
        public static DataCollection.SpaceFarmers.FarmerDetails farmerDetails = new DataCollection.SpaceFarmers.FarmerDetails();
        public static DataCollection.SpaceFarmers.FarmerBlocks farmerBlocks = new DataCollection.SpaceFarmers.FarmerBlocks();
        public static DataCollection.SpaceFarmers.FarmerPayouts farmerPayouts = new DataCollection.SpaceFarmers.FarmerPayouts();
        public static DataCollection.SpaceFarmers.FarmerPayoutBatches farmerPayoutBatches = new DataCollection.SpaceFarmers.FarmerPayoutBatches();
        public static DataCollection.SpaceFarmers.FarmerPartials farmerPartials = new DataCollection.SpaceFarmers.FarmerPartials();
        public static DataCollection.SpaceFarmers.FarmerPlots farmerPlots = new DataCollection.SpaceFarmers.FarmerPlots();

        // SQL 
        public static SQLHandling.Common sQLHandling = new SQLHandling.Common();

        static void Main(string[] args)
        {


            launcherID.Add("0a6b93e2a21bb611c252f6129c45cd1b5f354bb9676d38b15e5c9e9a990616c3");

            Console.WriteLine("Launcher IDs Found: " + launcherID.Count);




            // Get Database Location
            Console.WriteLine("Getting Database Location...");
            Console.WriteLine(DatabaseFunctions.DataBasePath);

            // Test SQL Connection
            Console.WriteLine("Testing SQL Connection...");

            if (DatabaseFunctions.TestSQLConnection(DatabaseFunctions.DataBasePath))
            {
                Console.WriteLine("SQL Connection Successful");
            }
            else
            {
                Console.WriteLine("SQL Connection Failed");

                // Create Database
                Console.WriteLine("Creating Database...");
                DatabaseFunctions.CreateDatabase(DatabaseFunctions.DataBasePath);
                Console.WriteLine("Database Created");

            }





            // Get Farmer Details
            Console.WriteLine("Getting Farmer Details...");

            SQLHandling.Common.sqlLines.AddRange(farmerDetails.getFarmerDetails(launcherID).Result);



            // Get Farmer Blocks
            Console.WriteLine("Getting Farmer Blocks...");
            SQLHandling.Common.sqlLines.AddRange(farmerBlocks.getFarmerBlocks(launcherID).Result);

            // Get Farmer Payouts
           // Console.WriteLine("Getting Farmer Payouts...");
           // SQLHandling.Common.sqlLines.AddRange(farmerPayouts.getFarmerPayouts(launcherID).Result);

            // Get Farmer Payout Batches
          //  Console.WriteLine("Getting Farmer Payout Batches...");
          //  SQLHandling.Common.sqlLines.AddRange(farmerPayoutBatches.getFarmerPayoutBatches(launcherID).Result);

            // Get Farmer Partials
            Console.WriteLine("Getting Farmer Partials...");
            SQLHandling.Common.sqlLines.AddRange(farmerPartials.getFarmerPartials(launcherID).Result);


            // Get Farmer Plots
            Console.WriteLine("Getting Farmer Plots...");
            SQLHandling.Common.sqlLines.AddRange(farmerPlots.getFarmerPlots(launcherID).Result);






            // Write SQL to Console
            Console.WriteLine("SQL Commands Generated...");
            foreach (string line in SQLHandling.Common.sqlLines)
            {
                Console.WriteLine(line);
            }


            Console.ReadLine();

        }
    }
}
