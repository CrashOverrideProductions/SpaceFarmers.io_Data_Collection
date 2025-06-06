﻿using SQLHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCollection.SpaceFarmers
{
    public class FarmerPayouts
    {
        SQLHandling.DatabaseFunctions sql = new SQLHandling.DatabaseFunctions();

        // Get API Data
        public async Task<List<string>> getFarmerPayouts(Settings.ApplicationSettings appSettings)
        {
            while (Common.IsAPICallActive) // Wait until the API call is not active
            {
                Logging.Common.AddLogItem("FarmerPayouts: An API Call is already active, waiting for it to finish before making a new call.", "Info", "FarmerBlocks");
                await Task.Delay(10000); // Wait for 10 seconds before checking again
            }

            Common.IsAPICallActive = true; // Set the API call as active


            // Database Path
            string databasePath = Path.Combine(appSettings.Database.DatabasePath, appSettings.Database.DatabaseName);

            // Define Return Object
            List<string> farmerBlocksList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in appSettings.Harvester.HarvesterIDs)
            {
                
                try
                {
                    int counter = 0;
                    int totalpages = 0;
                    int payoutid = 0;

                    // Get Last Update TimeStamp
                    long lastUpdateTimeStamp = sql.getLastUpdateFarmerPayouts(databasePath, launcher);

                    // Data Object
                    List<FarmerPayoutDatum> farmerBlocksResponse = new List<FarmerPayoutDatum>();

                    // Make API Call
                    string nextUrl = $"https://spacefarmers.io/api/farmers/{launcher}/payouts";

                    using (var client = new HttpClient())
                    {
                        while (!string.IsNullOrEmpty(nextUrl))
                        {
                            try
                            {
                                var response = await client.GetAsync(nextUrl);
                                response.EnsureSuccessStatusCode();
                                var jsonString = await response.Content.ReadAsStringAsync();

                                var pageData = JsonSerializer.Deserialize<FarmerPayoutResponse>(jsonString);

                                if (pageData?.data != null)
                                    farmerBlocksResponse.AddRange(pageData.data);

                                payoutid = pageData?.data?.Count > 0 ? pageData.data.Max(x => int.Parse(x.id)) : 0;
                                totalpages = pageData?.links?.total_pages ?? 0;
                                counter++;

                                nextUrl = pageData?.links?.next;

                                Logging.Common.AddLogItem("Retrieved " + (pageData?.data?.Count ?? 0) + " payouts from API for launcher: " + launcher, "Info", "FarmerPayouts");

                                // Minor delay to avoid hitting the API too fast and upsetting SpaceFarmers Dev Team
                                System.Threading.Thread.Sleep(125);

                                // check if the data is older than the last update timestamp
                                if (pageData?.data != null && pageData.data.Count > 0)
                                {
                                    foreach (var payout in pageData.data)
                                    {
                                        if (payout.attributes.timestamp < lastUpdateTimeStamp)
                                        {
                                            Logging.Common.AddLogItem("Farmer Payout data is older than last update timestamp, stopping API calls at page " + counter + " of " + totalpages, "Info", "FarmerPayouts");

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
                            catch (Exception ex)
                            {
                                nextUrl = null; // Set to null to exit loop
                                Logging.Common.AddLogItem("Error retrieving farmer payouts for launcher: " + launcher + " - " + ex.Message, "Error", "FarmerPayouts");

                            }
                        }
                    }

                    Common.IsAPICallActive = false; // Set the API call as inactive

                    foreach (var payout in farmerBlocksResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT OR REPLACE INTO FarmerPayouts " +
                                                    "(id,type,launcher_id,block_id,amount,fee,fee_amount,transaction_id,payout_batch_id,status,datetime,timestamp,xch_usd,coin,collection_time_stamp) " +
                                                    "VALUES ('" + payout.id + "','" +
                                                                  payout.type + "','" +
                                                                  payout.attributes.launcher_id + "','" +
                                                                  payout.attributes.block_id + "','" +
                                                                  payout.attributes.amount + "','" +
                                                                  payout.attributes.fee + "','" +
                                                                  payout.attributes.fee_amount + "','" +
                                                                  payout.attributes.transaction_id + "','" +
                                                                  payout.attributes.payout_batch_id + "','" +
                                                                  payout.attributes.status + "','" +
                                                                  payout.attributes.datetime + "','" +
                                                                  payout.attributes.timestamp + "','" +
                                                                  payout.attributes.xch_usd + "','" +
                                                                  payout.attributes.coin + "','" +
                                                                  collectionTimeStamp + "')";

                        // Add SQL Line to List
                        farmerBlocksList.Add(sqlLine);
                    }


                }
                catch (Exception ex)
                {
                    // Temp for testing
                    Logging.Common.AddLogItem("Error Retreiving Launcher Payouts: " + launcher, "Error", "FarmerBlocks");
                    Logging.Common.AddLogItem("Error Details: " + ex.Message, "Error", "FarmerBlocks");

                    Common.IsAPICallActive = false; // Set the API call as inactive


                    // Add to LogFile

                }
            }
            return farmerBlocksList;
        }
    }

    public class FarmerPayoutAttributes
    {
        public string launcher_id { get; set; }
        public long block_id { get; set; }
        public long amount { get; set; }
        public string fee { get; set; }
        public int fee_amount { get; set; }
        public string transaction_id { get; set; }
        public long? payout_batch_id { get; set; }
        public string status { get; set; }
        public DateTime datetime { get; set; }
        public long timestamp { get; set; }
        public double xch_usd { get; set; }
        public string coin { get; set; }
    }

    public class FarmerPayoutDatum
    {
        public string id { get; set; }
        public string type { get; set; }
        public FarmerPayoutAttributes attributes { get; set; }
    }



    public class FarmerPayoutLinks
    {
        public string related { get; set; }
        public int total_pages { get; set; }
        public object prev { get; set; }
        public string next { get; set; }
    }

    public class FarmerPayoutResponse
    {
        public List<FarmerPayoutDatum> data { get; set; }
        public FarmerPayoutLinks links { get; set; }
    }
}
