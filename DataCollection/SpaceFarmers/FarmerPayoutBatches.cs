using SQLHandling;
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
    public class FarmerPayoutBatches
    {
        SQLHandling.DatabaseFunctions sql = new SQLHandling.DatabaseFunctions();

        // Get API Data
        public async Task<List<string>> getFarmerPayoutBatches(Settings.ApplicationSettings appSettings)
        {
            string databasePath = Path.Combine(appSettings.Database.DatabasePath, appSettings.Database.DatabaseName);

            // Define Return Object
            List<string> farmerPayoutBatchList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in appSettings.Harvester.HarvesterIDs)
            {
                try
                {
                    int counter = 0;
                    int totalpages = 0;

                    // Data Object
                    List<FarmerPayoutBatchDatum> farmerPayoutBatchesResponse = new List<FarmerPayoutBatchDatum>();

                    // Get Last Update TimeStamp
                    long lastUpdateTimeStamp = sql.getLastUpdateFarmerPayoutBatch(databasePath, launcher);

                    // Make API Call
                    string nextUrl = $"https://spacefarmers.io/api/farmers/{launcher}/payout_batches";

                    using (var client = new HttpClient())
                    {
                        while (!string.IsNullOrEmpty(nextUrl))
                        {
                            var response = await client.GetAsync(nextUrl);
                            response.EnsureSuccessStatusCode();
                            var jsonString = await response.Content.ReadAsStringAsync();

                            var pageData = JsonSerializer.Deserialize<FarmerPayoutBatchesResponse>(jsonString);

                            if (pageData?.data != null)
                                farmerPayoutBatchesResponse.AddRange(pageData.data);

                            Console.WriteLine("Farmer Payout Batches Data, Page " + counter + " of " + totalpages);

                            totalpages = pageData?.links?.total_pages ?? 0;
                            counter++;

                            nextUrl = pageData?.links?.next;

                            // check if the data is older than the last update timestamp
                            if (pageData?.data != null && pageData.data.Count > 0)
                            {
                                foreach (var payoutBatch in pageData.data)
                                {
                                    if (payoutBatch.attributes.timestamp < lastUpdateTimeStamp)
                                    {
                                        Console.WriteLine("Farmer Payout Batches Data is older than last update timestamp, stopping API calls at page " + counter + " of " + totalpages);

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

                    foreach (var batch in farmerPayoutBatchesResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT OR REPLACE INTO " +
                                                    "FarmerPayoutBatches (id,type,payout_batch_id,launcher_id,amount,fee,fee_amount,transaction_id,payout_count,status,timestamp,coin,collection_time_stamp) " +
                                                    "VALUES ('" + batch.id + "','" +
                                                                  batch.type + "','" +
                                                                  batch.attributes.payout_batch_id + "','" +
                                                                  batch.attributes.launcher_id + "','" +
                                                                  batch.attributes.amount + "','" +
                                                                  batch.attributes.fee + "','" +
                                                                  batch.attributes.fee_amount + "','" +
                                                                  batch.attributes.transaction_id + "','" +
                                                                  batch.attributes.payout_count + "','" +
                                                                  batch.attributes.status + "','" +
                                                                  batch.attributes.timestamp + "','" +
                                                                  batch.attributes.coin + "','" +
                                                                  collectionTimeStamp + "')";

                        // Add SQL Line to List
                        farmerPayoutBatchList.Add(sqlLine);
                    }


                }
                catch (Exception ex)
                {
                    // Temp for testing
                    Console.WriteLine("Error Retreiving Launcher Payout Batches: " + launcher);
                    Console.WriteLine("Error Details: " + ex.Message);

                    // Add to LogFile

                }
            }
            return farmerPayoutBatchList;
        }
    }

    public class FarmerPayoutBatchAttributes
    {
        public long payout_batch_id { get; set; }
        public string launcher_id { get; set; }
        public long amount { get; set; }
        public string fee { get; set; }
        public long fee_amount { get; set; }
        public string transaction_id { get; set; }
        public long payout_count { get; set; }
        public string status { get; set; }
        public long timestamp { get; set; }
        public string coin { get; set; }
    }

    public class FarmerPayoutBatchDatum
    {
        public string id { get; set; }
        public string type { get; set; }
        public FarmerPayoutBatchAttributes attributes { get; set; }
    }

    public class FarmerPayoutBatchLinks
    {
        public int total_pages { get; set; }
        public object prev { get; set; }
        public string next { get; set; }
    }

    public class FarmerPayoutBatchesResponse
    {
        public List<FarmerPayoutBatchDatum> data { get; set; }
        public FarmerPayoutBatchLinks links { get; set; }
    }


}
