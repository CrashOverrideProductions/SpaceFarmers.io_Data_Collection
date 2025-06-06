﻿using SQLHandling;
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
    public class FarmerPlots
    {
        // Get API Data
        public async Task<List<string>> getFarmerPlots(List<string> launcherID)
        {
            while (Common.IsAPICallActive) // Wait until the API call is not active
            {
                Logging.Common.AddLogItem("FarmerPlots: An API Call is already active, waiting for it to finish before making a new call.", "Info", "FarmerPlots");
                await Task.Delay(10000); // Wait for 10 seconds before checking again
            }

            Common.IsAPICallActive = true; // Set the API call as active


            // Define Return Object
            List<string> farmerplotsList = new List<string>();

            long collectionTimeStamp = Common.ConvertToUnixEpoch(DateTime.UtcNow);

            foreach (string launcher in launcherID)
            {
                try
                {
                    // Data Object
                    List<FarmerPlotsDatum> farmerPlotResponse = new List<FarmerPlotsDatum>();

                    // Make API Call
                    string nextUrl = $"https://spacefarmers.io/api/farmers/{launcher}/plots";

                    // Counter
                    int counter = 0;
                    int totalpages = 0;

                    using (var client = new HttpClient())
                    {
                        while (!string.IsNullOrEmpty(nextUrl))
                        {
                            var response = await client.GetAsync(nextUrl);
                            response.EnsureSuccessStatusCode();
                            var jsonString = await response.Content.ReadAsStringAsync();

                            var pageData = JsonSerializer.Deserialize<FarmerPlotsResponse>(jsonString);

                            if (pageData?.data != null)
                                farmerPlotResponse.AddRange(pageData.data);

                            totalpages = pageData?.links?.total_pages ?? 0;
                            counter++;

                            Logging.Common.AddLogItem("Retrieved " + (pageData?.data?.Count ?? 0) + " plots from API for launcher: " + launcher, "Info", "FarmerPlots");

                            nextUrl = pageData?.links?.next;
                        }
                    }

                    Common.IsAPICallActive = false; // Set the API call as inactive

                    foreach (var plot in farmerPlotResponse)
                    {
                        // Prepare SQLite Insert
                        string sqlLine = "INSERT OR REPLACE INTO FarmerPlots " +
                                         "(id, type, plot_filename, proofs, points, last_seen, avg_proof_time_ms, k_size, harvester_id, harvester_name, collection_time_stamp) " +
                                         "VALUES ('" + plot.id + "','" +
                                                      plot.type + "','" +
                                                      plot.attributes.plot_filename + "','" +
                                                      plot.attributes.proofs + "','" +
                                                      plot.attributes.points + "','" +
                                                      plot.attributes.last_seen + "','" +
                                                      plot.attributes.avg_proof_time_ms + "','" +
                                                      plot.attributes.k_size + "','" +
                                                      plot.attributes.harvester_id + "','" +
                                                      plot.attributes.harvester_name + "','" +
                                                      collectionTimeStamp + "')";

                        // Add SQL Line to List
                        farmerplotsList.Add(sqlLine);
                    }


                }
                catch (Exception ex)
                {
                    // Log the error
                    Logging.Common.AddLogItem("Error Retrieving Farmer Plots for Launcher: " + launcher + " - " + ex.Message, "Error", "FarmerPlots");

                    Common.IsAPICallActive = false; // Set the API call as inactive

                    // Add to LogFile

                }
            }
            return farmerplotsList;
        }
    }


    public class FarmerPlotsAttributes
    {
        public string plot_filename { get; set; }
        public int proofs { get; set; }
        public int points { get; set; }
        public DateTime last_seen { get; set; }
        public int avg_proof_time_ms { get; set; }
        public int k_size { get; set; }
        public string harvester_id { get; set; }
        public string harvester_name { get; set; }
    }

    public class FarmerPlotsDatum
    {
        public string id { get; set; }
        public string type { get; set; }
        public FarmerPlotsAttributes attributes { get; set; }
    }

    public class FarmerPlotsLinks
    {
        public int total_pages { get; set; }
        public object prev { get; set; }
        public string next { get; set; }
    }

    public class FarmerPlotsResponse
    {
        public List<FarmerPlotsDatum> data { get; set; }
        public FarmerPlotsLinks links { get; set; }
    }


}
