using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Middleware.Models;
using Middleware.XML;


namespace Middleware.Controllers
{
    public class ContainerController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        // GET api/somiod/app
        [HttpGet]
        [Route("api/somiod/applications/{application}/containers")]
        public IEnumerable<Container> GetAllContainers([FromUri] string application)

        {
            var discoverHeader = Request.Headers.GetValues("somiod-discover");
            List<Container> containers = new List<Container>();

            if (discoverHeader != null && discoverHeader.Contains("container"))
            {
                int parentId = -1;
                string queryString = "SELECT Id FROM Applications WHERE name = @name";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@name", application);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                parentId = (int)reader["Id"];
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        return (IEnumerable<Container>)ex ;
                    }

                    queryString = "SELECT * FROM Containers WHERE parent = @Parent";
                    

                    command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Parent", parentId);

                    try
                    {
                        SqlDataReader reader = command.ExecuteReader();
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
                        connection.Close();
                        return containers;
                    }
                    catch (Exception ex)
                    {
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            connection.Close();
                        }
                        return containers;
                    }

                }
            }
            return containers;
        }


        public List<string> DiscoverContainers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Your implementation to return a list of application names
                    using (SqlCommand cmd = new SqlCommand("SELECT Name FROM Containers", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> containersNames = new List<string>();
                            while (reader.Read())
                            {
                                containersNames.Add((string)reader["Name"]);
                            }
                            return containersNames;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error discovering applications: {ex.Message}");
                throw;
            }
        }

        // GET api/somiod/
        
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
        

        //Get apenas dos containers desta aplicação 
        [HttpGet]
        [Route("api/somiod/applications/{application}/containers/{container}")]
        public IHttpActionResult GetContainerByName([FromUri] string application, [FromUri] string container)
        {

            //encontrar o parent para adicionar à query 

            Container containerToFind = null;
            string sql = "SELECT * FROM Containers WHERE UPPER(Name) = UPPER(@Name)";
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", container);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        containerToFind = new Container();
                        {
                            containerToFind.Id = (int)reader["Id"];
                            containerToFind.Name = (string)reader["Name"];
                            containerToFind.Creation_dt = (DateTime)reader["Creation_dt"];
                            containerToFind.Parent = (int)reader["Parent"];
                        };
                    }
                }
                reader.Close();
                conn.Close();

                if (containerToFind == null)
                {
                    return NotFound();
                }
                return Ok(containerToFind);
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

        // POST Container
        [HttpPost]
        [Route("api/somiod/applications/{application}/containers")]
        public IHttpActionResult PostContainer(HttpRequestMessage request, string application)
        {
            //TODO - receber o container por xml

            if (request.Content == null)
            {
                return BadRequest("Invalid data. The request body cannot be empty.");
            }

            try
            {
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader != null && discoverHeader.Contains("container"))
                {
                    #region Verificar se parent existe
                    int parentId = -1;
                    string queryString = "SELECT Id FROM Applications WHERE name = @name";
                    //verificar se parent existe
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@name", application);
                        try
                        {
                            command.Connection.Open();
                            SqlDataReader reader = command.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    parentId = (int)reader["Id"];
                                }
                                reader.Close();
                            }
                           
                            if(parentId == -1)
                            {
                                //parent n existe
                                return NotFound();
                            }
                        }
                        catch (Exception ex)
                        {
                            return InternalServerError();
                        }
                    }
                    #endregion


                    #region XML
                    HandlerXML handler = new HandlerXML();

                    string requestXML = request.Content.ReadAsStringAsync().Result
                        .Replace(System.Environment.NewLine, String.Empty);


                    if (!handler.IsValidXML(requestXML))
                    {
                        Debug.Print("[DEBUG] 'String is not XML' | Post() in ContainerController");
                        return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
                    }

                    if (!handler.IsValidContainerSchema(requestXML))
                    {
                        Debug.Print("[DEBUG] 'Invalid Schema in XML' | Post() in ContainerController");
                        return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
                    }

                    Container container = new Container();
                    container.Name = handler.ContainerRequest();
                    container.Parent = parentId;

                    #endregion
                    queryString = "INSERT INTO Containers (name, creation_dt, parent) VALUES (@Name, GETDATE() , @Parent)";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@Name", container.Name);
                        command.Parameters.AddWithValue("@Parent", parentId);

                        try
                        {
                            command.Connection.Open();
                            int rows = command.ExecuteNonQuery();
                            Debug.Print("[DEBUG] 'Rows : " + rows + " ' | Post() in ContainerController");
                            if (rows > 0)
                                return Ok();
                            else
                                return NotFound();
                        }
                        catch (Exception ex)
                        {
                            return InternalServerError();
                        }
                    }

                }

                return BadRequest("Invalid or missing somiod-discover header.");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error discovering resources: {ex.Message}");
                return InternalServerError();
            }


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