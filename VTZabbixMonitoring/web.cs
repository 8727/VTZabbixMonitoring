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
            string value = HttpResponse.Request.QueryString["params"];

            //DateTime endDateTime = DateTime.UtcNow;
            //DateTime sqlDateTime = endDateTime.AddMinutes(-deltamin);
            //int violations = 0;
            //string sqlAlarm = $"SELECT COUNT_BIG(CARS_ID) FROM[AVTO].[dbo].[CARS_VIOLATIONS] WHERE CHECKTIME > '{sqlDateTime:s}'";

            //if (connectSQL)
            //{
            //    using (SqlConnection connection = new SqlConnection(connectionString))
            //    {
            //        try
            //        {
            //            connection.Open();
            //            SqlCommand command = new SqlCommand(sqlAlarm, connection);
            //            SqlDataReader reader = command.ExecuteReader();
            //            if (reader.Read())
            //            {
            //                violations = Convert.ToInt32(reader.GetValue(0));
            //            }
            //            reader.Close();
            //        }
            //        catch (SqlException)
            //        {
            //            connection.Close();
            //        }
            //        finally
            //        {
            //            if (connection.State == ConnectionState.Open)
            //            {
            //                connection.Close();
            //            }
            //        }
            //    }
            //}

            string json = "{\"getDateTime\":\"" + DateTime.Now.ToString() + "\"";

            //if (connectSQL)
            //{
            //    json += ",\"violations\": " + violations;
            //}

            //if (statusServicesReplicator)
            //{
            //    json += ",\"replicator\":[";
            //    int r = 0;
            //    foreach (DictionaryEntry replicatorKey in Replicator)
            //    {
            //        ReplicatorCh repStatus = (ReplicatorCh)replicatorKey.Value;
            //        r++;
            //        json += "{\"host\":\"" + repStatus.host + "\",\"lastReplicator\":\"" + repStatus.LastReplicationLocalTime + "\",\"lastReplicatorSec\":" + repStatus.LagReplication + "}";
            //        if (r < Replicator.Count)
            //        {
            //            json += ",";
            //        }
            //    }
            //    json += "]";
            //}

            //if (statusViewCamera)
            //{
            //    json += ",\"viewCamera\":[";
            //    int c = 0;
            //    foreach (DictionaryEntry ViewCameraKey in ViewCamera)
            //    {
            //        c++;
            //        json += "{\"ip\":\"" + ViewCameraKey.Key + "\",\"status\":" + ViewCameraKey.Value + "}";
            //        if (c < ViewCamera.Count)
            //        {
            //            json += ",";
            //        }
            //    }
            //    json += "]";
            //}

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
