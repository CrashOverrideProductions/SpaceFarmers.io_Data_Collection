using SQLHandling;
using System;
using System.Collections.Generic;
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
        public async Task<List<string>> getFarmerPayouts(List<string> launcherID)
        {
            // Define Return Object
            List<string> farmerBlocksList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in launcherID)
            {
                try
                {
                    // Get Last Update TimeStamp
                    long lastUpdateTimeStamp = sql.getLastUpdateFarmerPayouts(DatabaseFunctions.DataBasePath, launcher);

                    // Data Object
                    List<FarmerPayoutDatum> farmerBlocksResponse = new List<FarmerPayoutDatum>();

                    // Make API Call
                    string nextUrl = $"https://spacefarmers.io/api/farmers/{launcher}/payouts";

                    using (var client = new HttpClient())
                    {
                        while (!string.IsNullOrEmpty(nextUrl))
                        {
                            var response = await client.GetAsync(nextUrl);
                            response.EnsureSuccessStatusCode();
                            var jsonString = await response.Content.ReadAsStringAsync();

                            var pageData = JsonSerializer.Deserialize<FarmerPayoutResponse>(jsonString);

                            if (pageData?.data != null)
                                farmerBlocksResponse.AddRange(pageData.data);

                            nextUrl = pageData?.links?.next;

                            // check if the data is older than the last update timestamp
                            if (pageData?.data != null && pageData.data.Count > 0)
                            {
                                foreach (var payout in pageData.data)
                                {
                                    if (payout.attributes.timestamp < lastUpdateTimeStamp)
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

                    foreach (var payout in farmerBlocksResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT INTO FarmerPayouts (id,type,launcher_id,block_id,amount,fee,fee_amount,transaction_id,payout_batch_id,status,datetime,timestamp,xch_usd,coin,collection_time_stamp) " +
                                                    "VALUES ('" + payout.id + "','" +
                                                                  payout.type + "','" +
                                                                  payout.attributes.launcher_id + "','" +
                                                                  payout.attributes.block_id + "','" +
                                                                  payout.attributes.amount + "'," +
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
                    Console.WriteLine("Error Retreiving Launcher Payouts: " + launcher);
                    Console.WriteLine("Error Details: " + ex.Message);

                    // Add to LogFile

                }
            }
            return farmerBlocksList;
        }
    }

    public class FarmerPayoutAttributes
    {
        public string launcher_id { get; set; }
        public int block_id { get; set; }
        public int amount { get; set; }
        public string fee { get; set; }
        public int fee_amount { get; set; }
        public object transaction_id { get; set; }
        public object payout_batch_id { get; set; }
        public string status { get; set; }
        public DateTime datetime { get; set; }
        public int timestamp { get; set; }
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
