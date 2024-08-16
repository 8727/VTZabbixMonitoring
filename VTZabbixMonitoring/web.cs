using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace VTZabbixMonitoring
{
    internal class web
    {
        static HttpListener serverWeb;
        public static Thread WEBServer = new Thread(ThreadWEBServer);

        static void ThreadWEBServer()
        {
            serverWeb = new HttpListener();
            serverWeb.Prefixes.Add(@"http://+:8090/");
            serverWeb.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            serverWeb.Start();
            while (Service.statusWeb)
            {
                ProcessRequest();
            }
        }

        static void ProcessRequest()
        {
            var result = serverWeb.BeginGetContext(ListenerCallback, serverWeb);
            var startNew = Stopwatch.StartNew();
            result.AsyncWaitHandle.WaitOne();
            startNew.Stop();
        }

        static void ListenerCallback(IAsyncResult result)
        {
            var HttpResponse = serverWeb.EndGetContext(result);
            string key = HttpResponse.Request.QueryString["key"];
            string json = "{\"getDateTime\":\"" + DateTime.Now.ToString() + "\"";

            switch (key.ToLower())
            {
                case "replicator":
                    
                    break;
                case "violation":

                    break;
                case "export":
                    
                    break;
                default:
                    json += ",\"replicationSeconds\":\"" + Service.StatuseJson["LastReplicationSeconds"] + "\"";
                    json += ",\"violationsCount\":\"" + Service.StatuseJson["UnprocessedViolationsCount"] + "\"";
                    json += ",\"violationsSeconds\":\"" + Service.StatuseJson["UnprocessedViolationsSeconds"] + "\"";
                    json += ",\"unexportedCount\":\"" + Service.StatuseJson["UnexportedCount"] + "\"";
                    json += ",\"unexportedSeconds\":\"" + Service.StatuseJson["UnexportedSeconds"] + "\"";
                    break;
            }

            json += "}";

            HttpResponse.Response.Headers.Add("Content-Type", "application/json");
            HttpResponse.Response.StatusCode = 200;
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            HttpResponse.Response.ContentLength64 = buffer.Length;
            HttpResponse.Response.OutputStream.Write(buffer, 0, buffer.Length);
            HttpResponse.Response.Close();
        }
    }
}
