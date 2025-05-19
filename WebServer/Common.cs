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
            await StartServer();
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
            string phpCompilerPath = webServerSettings.PhpCompilerPath + "\\php.exe";

            Process proc = new Process();
            proc.StartInfo.FileName = phpCompilerPath;
            proc.StartInfo.Arguments = "-d \"display_errors=1\" -d \"error_reporting=E_PARSE\" \"" + pageFileName + "\"";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            string res = proc.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(res))
            {
                res = proc.StandardError.ReadToEnd();
                res = "<h2 style=\"color:red;\">Error!</h2><hr/> <h4>Error Details :</h4> <pre>" + res + "</pre>";
                proc.StandardError.Close();
            }
            if (res.StartsWith("\nParse error: syntax error"))
                res = "<h2 style=\"color:red;\">Error!</h2><hr/> <h4>Error Details :</h4> <pre>" + res + "</pre>";


            proc.StandardOutput.Close();

            proc.Close();
            return res;
        }


        private async Task StartServer()
        {
            HttpServ = new HttpListener();
            HttpServ.Prefixes.Add("http://localhost:" + webServerSettings.WebServerPort.ToString() + "/");
            HttpServ.Start();
            while (true)
            {
                try
                {
                    var ctx = await HttpServ.GetContextAsync();

                    var page = webServerSettings.WebServerPath + "/index.php";

                    if (ctx.Request.Url.LocalPath != "/")
                    {
                        page = webServerSettings.WebServerPath + ctx.Request.Url.LocalPath;
                    }

                    if (File.Exists(page))
                    {
                        string file;
                        var ext = new FileInfo(page);
                        if (ext.Extension == ".php")
                        {
                            file = ProcessPhpPage(page);
                        }
                        else
                        {
                            file = File.ReadAllText(page);
                        }

                        await ctx.Response.OutputStream.WriteAsync(ASCIIEncoding.UTF8.GetBytes(file), 0, file.Length);
                        ctx.Response.OutputStream.Close();
                        ctx.Response.Close();

                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        var file = "<h2 style=\"color:red;\">404 File Not Found !!!</h2>";
                        await ctx.Response.OutputStream.WriteAsync(ASCIIEncoding.UTF8.GetBytes(file), 0, file.Length);
                        ctx.Response.OutputStream.Close();

                        ctx.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    break;
                }
            }


        }
    }
}