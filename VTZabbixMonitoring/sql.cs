using System;
using System.Data.SqlClient;
using System.Data;

namespace VTZabbixMonitoring
{
    internal class sql
    {
        static string connectionString = $@"Data Source={Service.sqlSource};Initial Catalog=AVTO;User Id={Service.sqlUser};Password={Service.sqlPassword};Connection Timeout=60";

        static public object SQLQuery(string query)
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

        static public object SqlUnprocessedViolations()
        {
            string sqlunprocessedviolations = "SELECT COUNT_BIG(CARS_ID) FROM AVTO.dbo.CARS where PROCESSED = 0";
            return SQLQuery(sqlunprocessedviolations);
        }

        static public object SqlLastDate()
        {
            string sqllastdate = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS ORDER BY CARS_ID DESC";
            string sqllastdatest = "SELECT TOP(1) CHECKTIME FROM AVTO.dbo.CARS";

            DateTime replicatorendTime = DateTime.ParseExact(SQLQuery(sqllastdate).ToString(), "d.M.yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture).Add(+Service.localZone);
            string interval = DateTime.Now.Subtract(replicatorendTime).TotalSeconds.ToString();

            DateTime replicatorstartTime = DateTime.ParseExact(SQLQuery(sqllastdatest).ToString(), "d.M.yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture).Add(+Service.localZone);

            string intervald = replicatorendTime.Subtract(replicatorstartTime).TotalSeconds.ToString();

            return intervald;
        }




    }
}
