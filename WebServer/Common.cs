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
                    var rawPath = ctx.Request.Url.LocalPath.TrimStart('/').Replace("/", "\\");
                    var requestedPath = Path.Combine(webServerSettings.WebServerPath, rawPath);

                    Console.WriteLine($"Request for: {requestedPath}");

                    string pageToServe = requestedPath;

                    // If it's a directory, look for index.html or index.php
                    if (Directory.Exists(requestedPath))
                    {
                        string indexHtml = Path.Combine(requestedPath, "index.html");
                        string indexPhp = Path.Combine(requestedPath, "index.php");

                        if (File.Exists(indexHtml)) pageToServe = indexHtml;
                        else if (File.Exists(indexPhp)) pageToServe = indexPhp;
                        else
                        {
                            ctx.Response.StatusCode = 403;
                            await RespondWithText(ctx, "<h2>403 - Directory access is forbidden.</h2>");
                            continue;
                        }
                    }

                    if (File.Exists(pageToServe))
                    {
                        string ext = Path.GetExtension(pageToServe).ToLower();
                        string mimeType = GetMimeType(ext);
                        ctx.Response.ContentType = mimeType;

                        if (ext == ".php")
                        {
                            string result = ProcessPhpPage(pageToServe);
                            await RespondWithText(ctx, result);
                        }
                        else if (IsBinaryMimeType(mimeType))
                        {
                            byte[] buffer = File.ReadAllBytes(pageToServe);
                            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            string content = File.ReadAllText(pageToServe);
                            await RespondWithText(ctx, content);
                        }
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        await RespondWithText(ctx, "<h2 style=\"color:red;\">404 - File Not Found</h2>");
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

        private async Task RespondWithText(HttpListenerContext ctx, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            ctx.Response.ContentType = "text/html";
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private string GetMimeType(string extension)
        {
            switch (extension)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/x-icon";
                case ".svg":
                    return "image/svg+xml";
                case ".woff":
                    return "font/woff";
                case ".woff2":
                    return "font/woff2";
                case ".ttf":
                    return "font/ttf";
                case ".otf":
                    return "font/otf";
                case ".eot":
                    return "application/vnd.ms-fontobject";
                case ".xml":
                    return "application/xml";
                case ".pdf":
                    return "application/pdf";
                default:
                    return "application/octet-stream";
            }
        }

        private bool IsBinaryMimeType(string mimeType)
        {
            return mimeType.StartsWith("image/") ||
                   mimeType.StartsWith("application/") && !mimeType.Contains("javascript") && !mimeType.Contains("json") && !mimeType.Contains("xml") ||
                   mimeType.StartsWith("font/");
        }
    }
}