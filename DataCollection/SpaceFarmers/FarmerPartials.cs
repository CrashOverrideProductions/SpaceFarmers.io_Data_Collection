using SQLHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCollection.SpaceFarmers
{
    public class FarmerPartials
    {
        SQLHandling.DatabaseFunctions sql = new SQLHandling.DatabaseFunctions();


        // Get API Data
        public async Task<List<string>> getFarmerPartials(Settings.ApplicationSettings appSettings)
        {
            while (Common.IsAPICallActive) // Wait until the API call is not active
            {
                Logging.Common.AddLogItem("FarmerPartials: An API Call is already active, waiting for it to finish before making a new call.", "Info", "FarmerBlocks");
                await Task.Delay(10000); // Wait for 10 seconds before checking again
            }

            Common.IsAPICallActive = true; // Set the API call as active

            string databasePath = Path.Combine(appSettings.Database.DatabasePath, appSettings.Database.DatabaseName);

            // Define Return Object
            List<string> farmerpartialsList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in appSettings.Harvester.HarvesterIDs)
            {
                try
                {
                    int counter = 0;
                    int totalpages = 0;

                    // Get Last Update TimeStamp
                    long lastUpdateTimeStamp = sql.getLastUpdateFarmerPartials(databasePath, launcher);
                   
                    // Data Object
                    List<FarmerPartialsDatum> farmerPartialsResponse = new List<FarmerPartialsDatum>();

                    // Make API Call
                    string nextUrl = $"https://spacefarmers.io/api/farmers/{launcher}/partials";

                    using (var client = new HttpClient())
                    {
                        while (!string.IsNullOrEmpty(nextUrl))
                        {
                            var response = await client.GetAsync(nextUrl);
                            response.EnsureSuccessStatusCode();
                            var jsonString = await response.Content.ReadAsStringAsync();

                            var pageData = JsonSerializer.Deserialize<FarmerPartialsResponse>(jsonString);

                            if (pageData?.data != null)
                                farmerPartialsResponse.AddRange(pageData.data);

                            Logging.Common.AddLogItem("Farmer Partials Data, Page " + counter + " of " + (pageData?.links?.total_pages ?? 0), "Info", "FarmerPartials");

                            totalpages = pageData?.links?.total_pages ?? 0;
                            counter++;

                            nextUrl = pageData?.links?.next;

                            // check if the data is older than the last update timestamp
                            if (pageData?.data != null && pageData.data.Count > 0)
                            {
                                foreach (var partial in pageData.data)
                                {
                                    if (partial.attributes.timestamp < lastUpdateTimeStamp)
                                    {
                                        // Log that the data is older than the last update timestamp
                                        Logging.Common.AddLogItem("Farmer Partials Data is older than last update timestamp, stopping API calls at page " + counter + " of " + totalpages, "Info", "FarmerPartials");
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

                    // Set API Call Flag to False
                    Common.IsAPICallActive = false;

                    foreach (var partial in farmerPartialsResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT OR REPLACE INTO FarmerPartials " +
                                                    "(id,type,time,harvester_id,sp_hash,plot_filename,block,timestamp,harvester_name,error_code,points,time_taken,plot_id,launcher_id,collection_time_stamp) " +
                                                    "VALUES ('" + partial.id + "','" +
                                                                  partial.type + "','" +
                                                                  partial.attributes.time + "','" +
                                                                  partial.attributes.harvester_id + "','" +
                                                                  partial.attributes.sp_hash + "','" +
                                                                  partial.attributes.plot_filename + "','" +
                                                                  partial.attributes.block + "','" +
                                                                  partial.attributes.timestamp + "','" +
                                                                  partial.attributes.harvester_name + "','" +
                                                                  partial.attributes.error_code + "','" +
                                                                  partial.attributes.points + "','" +
                                                                  partial.attributes.time_taken + "','" +
                                                                  partial.attributes.plot_id + "','" +
                                                                  launcher + "','" +
                                                                  collectionTimeStamp + "')";

                        // Add SQL Line to List
                        farmerpartialsList.Add(sqlLine);
                    }


                }
                catch (Exception ex)
                {
                    // Temp for testing
                    Logging.Common.AddLogItem("Error Retreiving Farmer Partials Batches: " + launcher, "Error", "FarmerBlocks");
                    Logging.Common.AddLogItem("Error Details: " + ex.Message, "Error", "FarmerBlocks");

                    Common.IsAPICallActive = false; // Reset the API call status

                    // Add to LogFile

                }
            }
            return farmerpartialsList;
        }
    }

    public class FarmerPartialsAttributes
    {
        public DateTime time { get; set; }
        public string harvester_id { get; set; }
        public string sp_hash { get; set; }
        public string plot_filename { get; set; }
        public bool block { get; set; }
        public long timestamp { get; set; }
        public string harvester_name { get; set; }
        public string error_code { get; set; }
        public int points { get; set; }
        public int? time_taken { get; set; }
        public string plot_id { get; set; }
    }

    public class FarmerPartialsDatum
    {
        public string id { get; set; }
        public string type { get; set; }
        public FarmerPartialsAttributes attributes { get; set; }
    }

    public class FarmerPartialsLinks
    {
        public int total_pages { get; set; }
        public object prev { get; set; }
        public string next { get; set; }
    }

    public class FarmerPartialsResponse
    {
        public List<FarmerPartialsDatum> data { get; set; }
        public FarmerPartialsLinks links { get; set; }
    }

}
