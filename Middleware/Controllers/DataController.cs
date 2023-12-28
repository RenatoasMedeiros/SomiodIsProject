using Middleware.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;

namespace Middleware.Controllers
{
    public class DataController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        // GET: Data
        // GET -H “content-type: application/xml” -H “somiod-discover: data” http://<domain:9876>/api/somiod/app1 ➔ returns all the data records(names) that are child from app1.
        [HttpGet]
        [Route("api/somiod/{application}")]
        public IEnumerable<Data> GetAllData(string application)
        {
            List<Data> data = new List<Data>();
            string sql = "SELECT d.* FROM Data d " +
                 "JOIN Container c ON d.parent = c.id " +
                 "JOIN Application a ON c.parent = a.id " +
                 "WHERE a.name = @ApplicationName";
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ApplicationName", application);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Data p = new Data
                    {
                        Name = (string)reader["name"],
                    };
                    data.Add(p);
                }
                reader.Close();
                conn.Close();
                return data;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return data;
            }
        }
    }
}
