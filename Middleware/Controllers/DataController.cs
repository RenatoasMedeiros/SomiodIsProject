using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;
using Middleware.Models;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Middleware.Controllers
{
    public class DataController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        #region GET Data

        // GET -H “content-type: application/xml” -H “somiod-discover: data” http://<domain:9876>/api/somiod/app1 ➔ returns all the data records(names) that are child from app1.
        [HttpGet]
        [Route("api/somiod/{application}/data")] // não é suposto ser data
        public IEnumerable<Data> GetAllData(string application)
        {
            List<Data> data = new List<Data>();
            string sql = "SELECT d.* FROM Data d " +
                 "JOIN Containers c ON d.parent = c.id " +
                 "JOIN Applications a ON c.parent = a.id " +
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
                Debug.Print("[DEBUG] 'Exception in Get() in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return data;
            }
        }
        #endregion

        #region GET Data by Id

        //[HttpGet]
        //[Route("api/somiod/{application}/{container}/data/{id:int}")]
        //public IEnumerable<Data> GetDatabyId(int id)
        //{
        //    List<Data> data = new List<Data>();
        //    string sql = "SELECT * FROM Data Where Id = @id";
        //    SqlConnection conn = null;


        //}

        #endregion

        #region POST Data

        [HttpPost]
        [Route("api/somiod/{application}/{container}")]
        public IHttpActionResult Post(string application, string container, [FromBody] Data newData)
        {

            if (newData == null)
            {
                return BadRequest("Invalid data. The request body cannot be empty.");
            }

            int containerId = -1;

            #region Descobrir contentId
            string sql = "SELECT c.id FROM Containers c WHERE c.name = @container";

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@container", container);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        containerId = (int)reader["id"];
                    }
                    reader.Close();
                }
                conn.Close();

                if (containerId == 0)
                    return NotFound();
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception in Post() in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return NotFound();
            }
            #endregion

            sql = "INSERT INTO Data (name, content, creation_dt, parent) VALUES (@name, @content, GETDATE(), @parent)";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@name", newData.Name);
                cmd.Parameters.AddWithValue("@content", newData.Content);
                cmd.Parameters.AddWithValue("@parent", containerId);

                int rows = cmd.ExecuteNonQuery();
                conn.Close();
                if (rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception in Post() in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }
        #endregion


        #region PUT Data

        //public IHttpActionResult Put(int id, [FromBody] Data editedData)
        //{
            

        //    string queryString = "UPDATE Data SET name=@name, content=@content, Price=@price WHERE Id=@id ";
        //}
        #endregion

        #region DELETE Data
        //Delete data
        [Route("api/somiod/data/{id}")]
        public IHttpActionResult Delete(int id)
        {
            string sql = "DELETE Data WHERE Id=@id ";

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                int numRows = cmd.ExecuteNonQuery();
                conn.Close();

                if (numRows > 0)
                {
                    return Content(HttpStatusCode.OK, "Data delete successfully", Configuration.Formatters.XmlFormatter);
                }
                return Content(HttpStatusCode.BadRequest, "Data does not exist", Configuration.Formatters.XmlFormatter);
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception in Delete() in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }
        #endregion

    }
}
