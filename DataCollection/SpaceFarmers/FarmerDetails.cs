using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCollection.SpaceFarmers
{
    public class FarmerDetails
    {
        // Get API Data
        public async Task<List<string>> getFarmerDetails(List<string> launcherID)
        {
            while (Common.IsAPICallActive) // Wait until the API call is not active
            {
                Console.WriteLine("FarmerDetails: An API Call is already active, waiting for it to finish before making a new call.");
                await Task.Delay(10000); // Wait for 10 seconds before checking again
            }

            Common.IsAPICallActive = true; // Set the API call as active

            // Define Return Object
            List<string> farmerDetailsList = new List<string>();
            
            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);



            foreach (string launcher in launcherID)
            {
                try
                {
                    // Data Object
                    FarmerDetailsResponse farmerDetailsResponse = new FarmerDetailsResponse();

                    // Make API Call
                    using (var client = new HttpClient())
                    {
                        string apiURL = "https://spacefarmers.io/api/farmers/" + launcher;

                        var response = await client.GetAsync(apiURL);
                        response.EnsureSuccessStatusCode();
                        var jsonString = await response.Content.ReadAsStringAsync();

                        farmerDetailsResponse = JsonSerializer.Deserialize<FarmerDetailsResponse>(jsonString);
                    }

                    Common.IsAPICallActive = false; // Set the API call as inactive

                    //Prepare SQLite Insert
                    string sqlLine = "INSERT INTO FarmerStatus (id,type,points_24h,farmer_name,ratio_24h,tib_24h,current_effort,estimated_win_seconds,payout_threshold_mojos,collection_time_stamp)" +
                                                        "VALUES ('" + farmerDetailsResponse.data.id + "','" +
                                                                      farmerDetailsResponse.data.type + "','" +
                                                                      farmerDetailsResponse.data.attributes.points_24h + "','" +
                                                                      farmerDetailsResponse.data.attributes.farmer_name + "','" +
                                                                      farmerDetailsResponse.data.attributes.ratio_24h + "','" +
                                                                      farmerDetailsResponse.data.attributes.tib_24h + "','" +
                                                                      farmerDetailsResponse.data.attributes.current_effort + "','" +
                                                                      farmerDetailsResponse.data.attributes.estimated_win_seconds + "','" +
                                                                      farmerDetailsResponse.data.attributes.payout_threshold_mojos + "','" +
                                                                      collectionTimeStamp + "') ";

                    // Add SQL Line to List
                    farmerDetailsList.Add(sqlLine);
                }
                catch (Exception ex)
                {
                    // Temp for testing
                    Console.WriteLine("Error Retreiving Farmer Details: ");
                    Console.WriteLine("Launcher ID: " + launcher);
                    Console.WriteLine("Error Details: " + ex.Message);
                    Common.IsAPICallActive = false; // Set the API call as inactive

                    // Add to LogFile

                }
            }
            return farmerDetailsList;
        }
    }


    public class FarmerDetailsResponse
    {
        public FarmerDetailsData data { get; set; }
    }
    public class FarmerDetailsAttributes
    {
        public int points_24h { get; set; }
        public string farmer_name { get; set; }
        public double ratio_24h { get; set; }
        public double tib_24h { get; set; }
        public double current_effort { get; set; }
        public long estimated_win_seconds { get; set; }
        public long payout_threshold_mojos { get; set; }
    }

    public class FarmerDetailsData
    {
        public string id { get; set; }
        public string type { get; set; }
        public FarmerDetailsAttributes attributes { get; set; }
    }




}
