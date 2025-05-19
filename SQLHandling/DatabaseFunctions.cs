using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using static System.Net.Mime.MediaTypeNames;


namespace SQLHandling
{
    public class DatabaseFunctions
    {
        public static bool TestSQLConnection(Settings.DatabaseSettings databaseSettings)
        {
            string databasePath = databaseSettings.DatabasePath;
            string databaseName = databaseSettings.DatabaseName;
            string databaseFullPath = Path.Combine(databasePath, databaseName);

            try
            {
                if (File.Exists(databaseFullPath))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Some form of error handling here
                return false;
            }
            return false;

        }


        public static void CreateDatabase(Settings.DatabaseSettings databaseSettings)
        {
            string databasePath = databaseSettings.DatabasePath;
            string databaseName = databaseSettings.DatabaseName;
            string databaseFullPath = Path.Combine(databasePath, databaseName);

            try
            {
                if (!File.Exists(databaseFullPath))
                {
                    Console.WriteLine($"Database file does not exist at {databaseFullPath}. Creating a new database file...");

                    // Create the empty SQLite file
                    SQLiteConnection.CreateFile(databaseFullPath);
                }

                else
                {
                    Console.WriteLine($"Database file already exists at {databaseFullPath}. Skipping file creation.");
                }

                using (var connection = new SQLiteConnection($"Data Source={databaseFullPath};Version=3;"))
                {
                    connection.Open();

                    foreach (var sql in CreateSQLStatements())
                    {
                        Console.WriteLine($"Executing SQL: {sql.Split('\n')[0]}...");
                        try
                        {
                            using (var command = new SQLiteCommand(sql, connection))
                            {
                                command.ExecuteNonQuery();
                                Console.WriteLine($"Executed successfully: {sql.Split('\n')[0]}...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing SQL: {sql}\nException: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database: {ex.Message}");
            }
        }

        public static void InsertDataToDatabase(Settings.DatabaseSettings databaseSettings, List<string> sqlStatements)
        {
            string databasePath = databaseSettings.DatabasePath;
            string databaseName = databaseSettings.DatabaseName;
            string databaseFullPath = Path.Combine(databasePath, databaseName);

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={databaseFullPath};Version=3;"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        foreach (var sqlStatement in sqlStatements)
                        {
                            try
                            {
                                command.CommandText = sqlStatement;
                                command.ExecuteNonQuery();
                            }
                            catch(Exception ex)
                            { 
                                Console.WriteLine($"Error executing SQL statement: {sqlStatement}\nException: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting data to database: {ex.Message}");
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
                "launcher_id            TEXT," +
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
                using (var connection = new SQLiteConnection($"Data Source={path};Version=3;"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(timestamp) FROM FarmerBlocks WHERE launcher_id = '" + launch + "'";
                        
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
                Console.WriteLine("Error. Unable to get Farmer Last Block Last Update");
                Console.WriteLine("launcher: " + launch);
                Console.WriteLine(ex.Message);
            }

            return lastUpdate;
        }

        public long getLastUpdateFarmerPayouts(string path, string launch)
        {
            // Get the last update time for FarmerBlocks
            long lastUpdate = 0;

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={path};Version=3;"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(timestamp) FROM FarmerPayouts WHERE launcher_id = '" + launch + "'";

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
                using (var connection = new SQLiteConnection($"Data Source={path};Version=3;"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(timestamp) FROM FarmerPayoutBatches WHERE launcher_id = '" + launch + "'";

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
                using (var connection = new SQLiteConnection($"Data Source={path};Version=3;"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(timestamp) FROM FarmerPartials WHERE launcher_id = '" + launch + "'";

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
                using (var connection = new SQLiteConnection($"Data Source={path};Version=3;"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(timestamp) FROM FarmerPlots WHERE launcher_id = '" + launch + "'";

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
