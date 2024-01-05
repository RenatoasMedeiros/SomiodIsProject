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
using static System.Net.Mime.MediaTypeNames;


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

            List<Container> containers = new List<Container>();
            #region Verificar Header

            var discoverHeader = Request.Headers.GetValues("somiod-discover");

            if (discoverHeader == null || !discoverHeader.Contains("container"))
            {
                return containers;
            }
            #endregion


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
                    return (IEnumerable<Container>)ex;
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
            if (request.Content == null)
            {
                return BadRequest("Invalid data. The request body cannot be empty.");
            }

            try
            {
                #region Verificar Header
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader == null || !discoverHeader.Contains("container"))
                {
                    return BadRequest("Invalid or missing somiod-discover header.");
                }
                #endregion

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

                        if (parentId == -1)
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

                #region Verificar XML Recebido 
                HandlerXML handler = new HandlerXML();

                string requestXML = request.Content.ReadAsStringAsync().Result
                    .Replace(System.Environment.NewLine, String.Empty);


                if (!handler.IsValidXML(requestXML))
                {
                    return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
                }

                if (!handler.ValidateDataSchemaXML(requestXML))
                {
                    return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
                }

                Container container = new Container
                {
                    Name = handler.ContainerRequest(),
                    Parent = parentId,
                    Creation_dt = DateTime.Now
                };
                #endregion

                #region Guardar Dados
                queryString = "INSERT INTO Containers (name, creation_dt, parent) VALUES (@Name, @Creation_dt , @Parent)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Name", container.Name);
                    command.Parameters.AddWithValue("@Parent", container.Parent);
                    command.Parameters.AddWithValue("@Creation_dt", container.Creation_dt);

                    try
                    {
                        command.Connection.Open();
                        int rows = command.ExecuteNonQuery();

                        if (rows < 0)
                            return InternalServerError();

                        queryString = "SELECT Id FROM Containers WHERE name = @Name AND parent = @Parent";

                        command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@Name", container.Name);
                        command.Parameters.AddWithValue("@Parent", container.Parent);

                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                container.Id = (int)reader["Id"];
                            }

                            reader.Close();
                        }
                        connection.Close();

                        return Content(HttpStatusCode.OK, "Container created successfully", Configuration.Formatters.XmlFormatter);
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        //ver se é preciso estes try catch todos ou entao fazer returns mais especificos
                        return InternalServerError();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error discovering resources: {ex.Message}");
                return InternalServerError();
            }
        }

        // PUT Container Alterar rotas
        [HttpPut]
        [Route("api/somiod/applications/{application}/containers/{container}")]
        public IHttpActionResult Put(HttpRequestMessage request, string container)
        {
            if (request.Content == null)
            {
                return BadRequest("Invalid data. The request body cannot be empty.");
            }

            try
            {
                #region Verificar Header
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader == null || !discoverHeader.Contains("container"))
                {
                    return BadRequest("Invalid or missing somiod-discover header.");
                }
                #endregion

                #region Verificar se o container existe
                int containerId = -1;
                int containerParent = -1;
                string queryString = "SELECT Id, Parent FROM Containers WHERE name = @Name";
                //verificar se container existe
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Name", container);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                containerId = (int)reader["Id"];
                                containerParent = (int)reader["Parent"];
                            }
                            reader.Close();
                        }

                        if (containerId == -1)
                        {
                            //container n existe
                            return NotFound();
                        }

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                #endregion

                #region Verificar XML Recebido
                HandlerXML handler = new HandlerXML();

                string requestXML = request.Content.ReadAsStringAsync().Result
                    .Replace(System.Environment.NewLine, String.Empty);

                if (!handler.IsValidXML(requestXML))
                {
                    return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
                }

                if (!handler.ValidateDataSchemaXML(requestXML))
                {
                    return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
                }

                Container containertoUpdate = new Container
                {
                    Id = containerId,
                    Name = handler.ContainerRequest(),
                    Parent = containerParent
                };

                #endregion

                #region Guardar Alterações
                queryString = "UPDATE Containers SET name = @Name WHERE id = @Id";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Name", containertoUpdate.Name);
                    command.Parameters.AddWithValue("@Id", containerId);

                    try
                    {
                        command.Connection.Open();
                        int rows = command.ExecuteNonQuery();
                        if (rows < 0)
                            return NotFound();

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering resources: {ex.Message}");
                return InternalServerError();
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [Route("api/somiod/applications/{application}/containers/{container}")]

        public IHttpActionResult Delete(HttpRequestMessage request, string container)
        {

            try
            {
                #region Verificar Header
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader == null || !discoverHeader.Contains("container"))
                {
                    return BadRequest("Invalid or missing somiod-discover header.");
                }
                #endregion

                #region Verificar se o container existe
                int containerId = -1;
                int containerParent = -1;
                string queryString = "SELECT Id, Parent FROM Containers WHERE name = @Name";
                //verificar se container existe
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Name", container);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                containerId = (int)reader["Id"];
                                containerParent = (int)reader["Parent"];
                            }
                            reader.Close();
                        }

                        if (containerId == -1)
                        {
                            //container n existe
                            return NotFound();
                        }

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                #endregion

                #region Apagar Data do Container

                queryString = "DELETE FROM Data WHERE parent = @Id";
                //verificar se container existe
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Id", containerId);
                    try
                    {
                        command.Connection.Open();
                        int rows = command.ExecuteNonQuery();
                        if (rows < 0)
                            return InternalServerError();

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                #endregion


                #region Apagar Subscriptions do Container

                queryString = "DELETE FROM Subscriptions WHERE parent = @Id";
                //verificar se container existe
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Id", containerId);
                    try
                    {
                        command.Connection.Open();
                        int rows = command.ExecuteNonQuery();
                        if (rows < 0)
                            return InternalServerError();

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                #endregion

                #region Apagar Container

                queryString = "DELETE FROM Containers WHERE id = @Id";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Id", containerId);
                    try
                    {
                        command.Connection.Open();
                        int rows = command.ExecuteNonQuery();
                        if (rows < 0)
                            return InternalServerError();

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError();
                    }
                }
                #endregion

                return Content(HttpStatusCode.OK, "Container Deleted Succefully", Configuration.Formatters.XmlFormatter);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering resources: {ex.Message}");
                return InternalServerError();
            }
        }
    }
}