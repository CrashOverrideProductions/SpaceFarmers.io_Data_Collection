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

namespace DataCollection.SpaceFarmers
{
    public class FarmerBlocks
    {
        SQLHandling.DatabaseFunctions sql = new SQLHandling.DatabaseFunctions();


        // Get API Data
        public async Task<List<string>> getFarmerBlocks(List<string> launcherID)
        {
            // Define Return Object
            List<string> farmerBlocksList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in launcherID)
            {
                try
                {
                    // Get Last Update TimeStamp
                    long lastUpdateTimeStamp = sql.getLastUpdateFarmerBlocks(DatabaseFunctions.DataBasePath, launcher);

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

                            nextUrl = pageData?.links?.next;

                            // check if the data is older than the last update timestamp
                            if (pageData?.data != null && pageData.data.Count > 0)
                            {
                                foreach (var block in pageData.data)
                                {
                                    if (block.attributes.timestamp < lastUpdateTimeStamp)
                                    {
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

                    foreach (var block in farmerBlocksResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT INTO FarmerBlocks (id,type,height,datetime,amount,effort,farmer_effort,launcher_id,farmer_name,payouts,timestamp,farmer_reward,farmer_reward_taken_by_gigahorse,collection_time_stamp) " +
                                                    "VALUES ('" + block.id + "','" +
                                                                  block.type + "','" +
                                                                  block.attributes.datetime + "," +
                                                                  block.attributes.height + "," +
                                                                  block.attributes.datetime + "," +
                                                                  block.attributes.amount + "," +
                                                                  block.attributes.effort + "," +
                                                                  block.attributes.farmer_effort + ",'" +
                                                                  block.attributes.launcher_id + "'," +
                                                                  block.attributes.farmer_name + "," +
                                                                  block.attributes.payouts + "," +
                                                                  block.attributes.timestamp + "," +
                                                                  block.attributes.farmer_reward + "," +
                                                                  block.attributes.farmer_reward_taken_by_gigahorse +
                                                                  collectionTimeStamp + ")";

                        // Add SQL Line to List
                        farmerBlocksList.Add(sqlLine);
                    }


                }
                catch (Exception ex)
                {
                    // Temp for testing
                    Console.WriteLine("Error Retreiving Farmer Blocks: " + launcher);
                    Console.WriteLine("Error Details: " + ex.Message);

                    // Add to LogFile

                }
            }
            return farmerBlocksList;
        }
    }


    public class FarmerBlocksAttributes
    {
        public DateTime datetime { get; set; }
        public int height { get; set; }
        public object amount { get; set; }
        public double effort { get; set; }
        public double farmer_effort { get; set; }
        public string launcher_id { get; set; }
        public string farmer_name { get; set; }
        public int payouts { get; set; }
        public int timestamp { get; set; }
        public object farmer_reward { get; set; }
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
