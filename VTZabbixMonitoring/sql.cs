﻿using System;
using System.Data.SqlClient;
using System.Data;

namespace VTZabbixMonitoring
{
    internal class sql
    {
        static string connectionString = $@"Data Source={Service.sqlSource};Initial Catalog=AVTO;User Id={Service.sqlUser};Password={Service.sqlPassword};Connection Timeout=60";

        static object SQLQuery(string query)
        {
            object response = -1;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    response = command.ExecuteScalar();
                }
                catch (SqlException)
                {
                    connection.Close();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return response;
        }

        static UInt32 DateTimeToSecondes(string dt) 
        { 
            if (dt == "-1")
            {
                dt = "01.01.2000 00:00:00";
            }

            DateTime converDateTime = DateTime.ParseExact(dt, "d.M.yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture).Add(+Service.localZone);
            return Convert.ToUInt32(DateTime.Now.Subtract(converDateTime).TotalSeconds);
        }

        public static UInt32 LastReplicationSeconds()
        {
            string sqlQuery = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS ORDER BY CARS_ID DESC";
            return DateTimeToSecondes(SQLQuery(sqlQuery).ToString());
        }

        public static UInt32 UnprocessedViolationsCount()
        {
            string sqlQuery = "SELECT COUNT_BIG(CARS_ID) FROM AVTO.dbo.CARS where PROCESSED = 0";
            return Convert.ToUInt32(SQLQuery(sqlQuery));
        }

        public static UInt32 UnprocessedViolationsSeconds()
        {
            string sqlQuery = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS where PROCESSED = 0";
            return DateTimeToSecondes(SQLQuery(sqlQuery).ToString());
        }

        public static UInt32 UnexportedCount()
        {
            string sqlQuery = "SELECT COUNT_BIG(CARS_ID) FROM AVTO.dbo.CARS_VIOLATIONS where EXPORT2 = 0";
            return Convert.ToUInt32(SQLQuery(sqlQuery));
        }

        public static UInt32 UnexportedSeconds()
        {
            string sqlQuery = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS_VIOLATIONS where EXPORT2 = 0";
            return DateTimeToSecondes(SQLQuery(sqlQuery).ToString());
        }

        public static string ArchiveDepthSeconds()
        {
            string oldEntry = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS";
            string lastEntry = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS ORDER BY CARS_ID DESC";

            DateTime archiveOld = DateTime.ParseExact(SQLQuery(oldEntry).ToString(), "d.M.yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture).Add(+Service.localZone);
            DateTime archiveLast = DateTime.ParseExact(SQLQuery(lastEntry).ToString(), "d.M.yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture).Add(+Service.localZone);

            return archiveLast.Subtract(archiveOld).TotalSeconds.ToString();
        }

        public static UInt32 ArchiveDepthCount()
        {
            string sqlQuery = "SELECT COUNT_BIG(CARS_ID) FROM AVTO.dbo.CARS";
            return Convert.ToUInt32(SQLQuery(sqlQuery));
        }


    }
}
