using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using static System.Net.Mime.MediaTypeNames;


namespace SQLHandling
{
    public class DatabaseFunctions
    {
        internal static string DatabaseName = "FarmerData.db";
        internal static string DatabaseFolder = AppDomain.CurrentDomain.BaseDirectory;
        public static string DataBasePath = Path.Combine(DatabaseFolder, DatabaseName);

        public static bool TestSQLConnection(string DataBasePath)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DataBasePath}"))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
                return false;
            }
        }

        public static void CreateDatabase(string path)
        {
            try
            {
                // Create the file if it doesn't exist
                if (!File.Exists(path))
                {
                    using (var connection = new SqliteConnection($"Data Source={path}"))
                    {
                        connection.Open();

                        foreach (var sqlStatement in CreateSQLStatements())
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = sqlStatement;
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
                int test = 0;
            }
        }

        public void InsertDataToDatabase(string path, List<string> sqlStatements)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={path}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        foreach (var sqlStatement in sqlStatements)
                        {
                            command.CommandText = sqlStatement;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
            }
        }

        public static List<string> CreateSQLStatements()
        {
            // Define the Return Object
            List<string> sqlOutput = new List<string>();

            // Create the CompanyDetails Table
            sqlOutput.Add("CREATE TABLE FarmerStatus (" +
                        "id                     TEXT," +
                        "type                   TEXT," +
                        "points_24h             INTEGER," +
                        "farmer_name            TEXT," +
                        "ratio_24h              NUMERIC," +
                        "tib_24h                NUMERIC," +
                        "current_effort         NUMERIC," +
                        "estimated_win_seconds  INTEGER," +
                        "payout_threshold_mojos INTEGER," +
                        "collection_time_stamp  INTEGER);");

            sqlOutput.Add("CREATE TABLE FarmerBlocks (" +
                "id                               INTEGER UNIQUE," +
                "type                             TEXT," +
                "height                           INTEGER," +                                 
                "datetime                         TEXT," +
                "amount                           INTEGER," +
                "effort                           NUMERIC," +
                "farmer_effort                    NUMERIC," +
                "launcher_id                      TEXT," +
                "farmer_name                      TEXT," +
                "payouts                          INTEGER," +
                "timestamp                        INTEGER," +
                "farmer_reward                    INTEGER," +
                "farmer_reward_taken_by_gigahorse INTEGER," +
                "collection_time_stamp            INTEGER);");

            sqlOutput.Add("CREATE TABLE FarmerPayouts (" +
                "id                     INTEGER UNIQUE," +
                "type                   TEXT," +
                "launcher_id            TEXT," +
                "block_id               INTEGER," +
                "amount                 INTEGER," +
                "fee                    TEXT," +
                "fee_amount             INTEGER," +
                "transaction_id         TEXT," +
                "payout_batch_id        TEXT," +
                "status                 TEXT," +
                "datetime               INTEGER," +
                "timestamp              INTEGER," +
                "xch_usd                NUMERIC," +
                "coin                   TEXT," +
                "collection_time_stamp  INTEGER);");

            sqlOutput.Add("CREATE TABLE FarmerPayoutBatches (" +
                "id                     INTEGER UNIQUE," +
                "type                   TEXT," +
                "payout_batch_id        INTEGER," +
                "launcher_id            TEXT," +
                "amount                 INTEGER," +
                "fee                    TEXT," +
                "fee_amount             INTEGER," +
                "transaction_id         TEXT," +
                "payout_count           INTEGER," +
                "status                 TEXT," +
                "timestamp              INTEGER," +
                "coin                   TEXT," +
                "collection_time_stamp  INTEGER);");

            sqlOutput.Add("CREATE TABLE FarmerPartials (" +
                "id                     INTEGER UNIQUE," +
                "type                   TEXT," +
                "time                   TEXT," +
                "harvester_id           TEXT," +
                "sp_hash                TEXT," +
                "plot_filename          TEXT," +
                "block                  INTEGER," +
                "timestamp              INTEGER," +
                "harvester_name         TEXT," +
                "error_code             TEXT," +
                "points                 INTEGER," +
                "time_taken             INTEGER," +
                "plot_id                TEXT," +
                "collection_time_stamp  INTEGER);");

            sqlOutput.Add("CREATE TABLE FarmerPlots (" +
                "id                TEXT UNIQUE," +
                "type              TEXT," +
                "plot_filename     TEXT," +
                "proofs            INTEGER," +
                "points            INTEGER," +
                "last_seen         TEXT," +
                "avg_proof_time_ms INTEGER," +
                "k_size            INTEGER," +
                "harvester_id      TEXT," +
                "harvester_name    TEXT," +
                "collection_time_stamp  INTEGER);");

            // Return the SQL statements
            return sqlOutput;
        }

        public long getLastUpdateFarmerBlocks(string path, string launch)
        { 
            // Get the last update time for FarmerBlocks
            long lastUpdate = 0;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={path}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(datetime) FROM FarmerBlocks WHERE launcher_id = @launcher_id";
                        command.Parameters.AddWithValue("@launcher_id", launch);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastUpdate = Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
            }

            return lastUpdate;
        }

        public long getLastUpdateFarmerPayouts(string path, string launch)
        {
            // Get the last update time for FarmerBlocks
            long lastUpdate = 0;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={path}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(datetime) FROM FarmerPayouts WHERE launcher_id = @launcher_id";
                        command.Parameters.AddWithValue("@launcher_id", launch);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastUpdate = Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
            }

            return lastUpdate;
        }

        public long getLastUpdateFarmerPayoutBatch(string path, string launch)
        {
            // Get the last update time for FarmerBlocks
            long lastUpdate = 0;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={path}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(datetime) FROM FarmerPayoutBatches WHERE launcher_id = @launcher_id";
                        command.Parameters.AddWithValue("@launcher_id", launch);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastUpdate = Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
            }

            return lastUpdate;
        }

        public long getLastUpdateFarmerPartials(string path, string launch)
        {
            // Get the last update time for FarmerBlocks
            long lastUpdate = 0;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={path}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(datetime) FROM FarmerPartials WHERE launcher_id = @launcher_id";
                        command.Parameters.AddWithValue("@launcher_id", launch);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastUpdate = Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
            }

            return lastUpdate;
        }

        public long getLastUpdateFarmerPlots(string path, string launch)
        {
            // Get the last update time for FarmerBlocks
            long lastUpdate = 0;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={path}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(datetime) FROM FarmerPlots WHERE launcher_id = @launcher_id";
                        command.Parameters.AddWithValue("@launcher_id", launch);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastUpdate = Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
            }

            return lastUpdate;
        }
    }
}
