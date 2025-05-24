using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;
using Microsoft.SqlServer.Server;
using SQLHandling;
using System.Reflection;
using System.IO;

namespace DataCollection.SpaceFarmers
{
    public class FarmerBlocks
    {
        SQLHandling.DatabaseFunctions sql = new SQLHandling.DatabaseFunctions();


        // Get API Data
        public async Task<List<string>> getFarmerBlocks(Settings.ApplicationSettings appSettings)
        {
            while (Common.IsAPICallActive) // Wait until the API call is not active
            {
                Console.WriteLine("FarmerBlocks: An API Call is already active, waiting for it to finish before making a new call.");
                await Task.Delay(10000); // Wait for 10 seconds before checking again
            }
            
            Common.IsAPICallActive = true; // Set the API call as active

            // Define Return Object
            List<string> farmerBlocksList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in appSettings.Harvester.HarvesterIDs)
            {
                // Database Path
                string databasePath = Path.Combine(appSettings.Database.DatabasePath, appSettings.Database.DatabaseName);
                

                try
                {
                    int counter = 0;
                    int totalpages = 0;

                    // Get Last Update TimeStamp
                    long lastUpdateTimeStamp = sql.getLastUpdateFarmerBlocks(databasePath, launcher);

                    // Data Object
                    List<FarmerBlocksDatum> farmerBlocksResponse = new List<FarmerBlocksDatum>();

                    // Make API Call
                    string nextUrl = $"https://spacefarmers.io/api/farmers/{launcher}/blocks";

                    using (var client = new HttpClient())
                    {
                        while (!string.IsNullOrEmpty(nextUrl))
                        {
                            var response = await client.GetAsync(nextUrl);
                            response.EnsureSuccessStatusCode();
                            var jsonString = await response.Content.ReadAsStringAsync();

                            var pageData = JsonSerializer.Deserialize<FarmerBlocksResponse>(jsonString);

                            if (pageData?.data != null)
                                farmerBlocksResponse.AddRange(pageData.data);

                            Console.WriteLine("Farmer Blocks Data, Page " + counter + " of " + totalpages);

                            totalpages = pageData?.links?.total_pages ?? 0;
                            counter++;

                            nextUrl = pageData?.links?.next;

                            // check if the data is older than the last update timestamp
                            if (pageData?.data != null && pageData.data.Count > 0)
                            {
                                foreach (var block in pageData.data)
                                {
                                    if (block.attributes.timestamp < lastUpdateTimeStamp)
                                    {
                                        Console.WriteLine("Farmer Blocks Data is older than last update timestamp, stopping API calls at page " + counter + " of " + totalpages);
                                        nextUrl = null;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                nextUrl = null;
                            }

                        }
                    }

                    // Set API Call as not active
                    Common.IsAPICallActive = false;

                    foreach (var block in farmerBlocksResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT OR REPLACE INTO FarmerBlocks " +
                                                    "(id,type,datetime,height,amount,effort,farmer_effort,launcher_id,farmer_name,payouts,timestamp,farmer_reward,farmer_reward_taken_by_gigahorse,collection_time_stamp) " +
                                                    "VALUES ('" + block.id + "','" +
                                                                  block.type + "','" +
                                                                  block.attributes.datetime + "','" +
                                                                  block.attributes.height + "','" +
                                                                  block.attributes.amount + "','" +
                                                                  block.attributes.effort + "','" +
                                                                  block.attributes.farmer_effort + "','" +
                                                                  block.attributes.launcher_id + "','" +
                                                                  block.attributes.farmer_name + "','" +
                                                                  block.attributes.payouts + "','" +
                                                                  block.attributes.timestamp + "','" +
                                                                  block.attributes.farmer_reward + "','" +
                                                                  block.attributes.farmer_reward_taken_by_gigahorse + "','" +
                                                                  collectionTimeStamp + "')";

                        // Add SQL Line to List
                        farmerBlocksList.Add(sqlLine);
                    }


                }
                catch (Exception ex)
                {
                    // Temp for testing
                    Console.WriteLine("Error Retreiving Farmer Blocks: " + launcher);
                    Console.WriteLine("Error Details: " + ex.Message);
                    Common.IsAPICallActive = false; // Set API Call Flag to False

                    // Add to LogFile

                }
            }
            return farmerBlocksList;
        }
    }


    public class FarmerBlocksAttributes
    {
        public DateTime datetime { get; set; }
        public long height { get; set; }
        public object amount { get; set; }
        public double effort { get; set; }
        public double farmer_effort { get; set; }
        public string launcher_id { get; set; }
        public string farmer_name { get; set; }
        public long payouts { get; set; }
        public long timestamp { get; set; }
        public long farmer_reward { get; set; }
        public bool farmer_reward_taken_by_gigahorse { get; set; }
    }

    public class FarmerBlocksDatum
    {
        public string id { get; set; }
        public string type { get; set; }
        public FarmerBlocksAttributes attributes { get; set; }
    }
       
    public class FarmerBlocksLinks
    {
        public string related { get; set; }
        public int total_pages { get; set; }
        public string prev { get; set; }
        public string next { get; set; }
    }

    public class FarmerBlocksResponse
    {
        public List<FarmerBlocksDatum> data { get; set; }
        public FarmerBlocksLinks links { get; set; }
    }


}
