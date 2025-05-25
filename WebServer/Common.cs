using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WebServer
{
    public class Common
    {
        HttpListener HttpServ;
        Settings.WebServerSettings webServerSettings = new Settings.WebServerSettings();

        public Common(Settings.WebServerSettings Settings)
        {
            // Add WebServer Settings
            webServerSettings = Settings;

            // Start the HTTP Listener
            HttpServ = new HttpListener();
        }


        public async void StartWebServer()
        {
            if (HttpServ.IsListening)
            {
                try
                {
                    HttpServ.Stop();
                    // HttpServ.Abort();
                }
                catch (Exception ex)
                {
                    // Handle Exception

                    return;
                }
            }
            // Start the HTTP Server
            await StartServerWithRestart();
        }

        public void StopWebServer()
        {
            if (HttpServ.IsListening)
            {
                try
                {
                    HttpServ.Stop();
                }
                catch (Exception ex)
                {
                    // Handle Exception
                    return;
                }
            }
        }


        private string ProcessPhpPage(string pageFileName)
        {
            try
            {
                string phpCompilerPath = webServerSettings.PhpCompilerPath;

                var proc = new Process();
                proc.StartInfo.FileName = phpCompilerPath;
                proc.StartInfo.Arguments = $"-d \"display_errors=1\" -d \"error_reporting=E_ALL\" \"{pageFileName}\"";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;

                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    return $"<h2 style=\"color:red;\">PHP Error</h2><pre>{error}</pre>";
                }

                return output;
            }
            catch (Exception ex)
            {
                return $"<h2 style=\"color:red;\">Exception</h2><pre>{ex.Message}</pre>";
            }
        }

        private async Task StartServerWithRestart()
        {
            while (true)
            {
                try
                {
                    await StartServer();
                }
                catch (Exception ex)
                {
                    // Optional: log error
                    Console.WriteLine($"Server error: {ex.Message}");
                }

                // Wait before retrying
                await Task.Delay(1000);
            }
        }

        private async Task StartServer()
        {
            HttpServ = new HttpListener();
            HttpServ.Prefixes.Add("http://localhost:" + webServerSettings.WebServerPort.ToString() + "/");
            HttpServ.Start();

            try
            {
                while (true)
                {
                    var ctx = await HttpServ.GetContextAsync();
                    var page = webServerSettings.WebServerPath;

                    var requestedPath = ctx.Request.Url.LocalPath.TrimStart('/').Replace("/", "\\");
                    page = Path.Combine(webServerSettings.WebServerPath, requestedPath);

                    Console.WriteLine($"Request for: ");
                    Console.WriteLine($"{page}");


                    if (File.Exists(page))
                    {
                        string file;
                        var ext = new FileInfo(page);

                        if (ext.Extension == ".php")
                            file = ProcessPhpPage(page);
                        else
                            file = File.ReadAllText(page);

                        await ctx.Response.OutputStream.WriteAsync(ASCIIEncoding.UTF8.GetBytes(file), 0, file.Length);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        var file = "<h2 style=\"color:red;\">404 File Not Found !!!</h2>";
                        await ctx.Response.OutputStream.WriteAsync(ASCIIEncoding.UTF8.GetBytes(file), 0, file.Length);
                    }

                    ctx.Response.OutputStream.Close();
                    ctx.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Web Server Stopped or Error Occurred. Restarting...");
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                HttpServ.Stop();
                HttpServ.Close();
            }
        }
    }
}