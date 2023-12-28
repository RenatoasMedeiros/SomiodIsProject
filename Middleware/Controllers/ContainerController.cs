using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Middleware.Models;

namespace Middleware.Controllers
{
    public class ContainerController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        // GET api/Containers
        [HttpGet]
        [Route("api/somiod/{application}")]
        public IEnumerable<Container> GetAllContainers()
        {
            List<Container> containers = new List<Container>();
            string sql = "SELECT * FROM Containers";
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Container c = new Container
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Creation_dt = (DateTime)reader["Creation_dt"],
                        Parent = (int)reader["Parent"]
                    };
                    containers.Add(c);
                }
                reader.Close();
                conn.Close();
                return containers;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return containers;
            }
        }

        // GET api/<controller>/5
        [HttpGet]
        [Route("api/containers/{id}")]
        public IHttpActionResult GetContainerById(int id)
        {
            Container container = null;
            string sql = "SELECT * FROM Containers WHERE Id = @Id";
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    container = new Container();
                    {
                        container.Id = (int)reader["Id"];
                        container.Name = (string)reader["Name"];
                        container.Creation_dt = (DateTime)reader["Creation_dt"];
                        container.Parent = (int)reader["Parent"];
                    };
                }

                reader.Close();
                conn.Close();

                if (container == null)
                {
                    return NotFound();
                }
                return Ok(container);
            }
            catch (Exception)
            {
                //fechar a ligação à BD
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return NotFound();
            }

        }

        
        public IHttpActionResult GetContainerByName(string name)
        {
            Container container = null;
            string sql = "SELECT * FROM Containers WHERE UPPER(Name) = UPPER(@Name)";
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        container = new Container();
                        {
                            container.Id = (int)reader["Id"];
                            container.Name = (string)reader["Name"];
                            container.Creation_dt = (DateTime)reader["Creation_dt"];
                            container.Parent = (int)reader["Parent"];
                        };
                    }
                }
                reader.Close();
                conn.Close();

                if (container == null)
                {
                    return NotFound();
                }
                return Ok(container);
            }
            catch (Exception)
            {
                //fechar a ligação à BD
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return NotFound();
            }
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}