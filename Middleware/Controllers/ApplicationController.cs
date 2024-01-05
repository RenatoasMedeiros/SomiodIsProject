using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Middleware.Models;
using System.Data.SqlClient;
using Middleware.XML;
using System.Diagnostics;

namespace Middleware.Controllers
{
    public class ApplicationController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;


        // GET: api/Application
        [HttpGet]
        [Route("api/somiod")]
        public HttpResponseMessage GetApplications()
        {
            try
            {
                // Check if the somiod-discover header is present
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader != null && discoverHeader.Contains("application"))
                {
                    var applicationNames = DiscoverApplications();
                    return applicationNames;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering applications: {ex.Message}");
                throw;
            }
        }

        // GET: api/Application/1
        [HttpGet]
        [Route("api/somiod/applications/{name}")]
        public IHttpActionResult GetApplication(string name)
        {
            Application application = GetApplicationByName(name);
            if (application != null)
            {
                var response = Request.CreateResponse(application);
                response.Content = new ObjectContent<Application>(application, new System.Net.Http.Formatting.XmlMediaTypeFormatter());
                return Ok(response);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult PostApplication(HttpRequestMessage request)
        {
            HandlerXML handler = new HandlerXML();

            string teste = Request.Content.ToString();
            //Retiro todos os caracteres e espaços desnecessários
            string rawXml = request.Content.ReadAsStringAsync().Result
                .Replace(System.Environment.NewLine, String.Empty);

            //Verifico se a string que veio do request é XML
            if (!handler.IsValidXML(rawXml))
            {
                Debug.Print("[DEBUG] 'String is not XML' | Post() in ApplicationController");
                return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
            }

            //Verifica se o ficheiro XML está de acordo o XSD
            if (!handler.IsValidApplicationSchemaXML(rawXml))
            {
                Debug.Print("[DEBUG] 'Invalid Schema in XML' | Post() in ApplicationController");
                return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
            }

            //Começo o tratamento de dados
            Application application = new Application();

            application.Name = handler.DealRequestApplication();

            if (String.IsNullOrEmpty(application.Name))
            {
                return Content(HttpStatusCode.BadRequest, "Name is Empty", Configuration.Formatters.XmlFormatter);
            }

            //Verifico se o nome da aplicação já existe
            string sqlVerifyApplication = "SELECT COUNT(*) FROM Applications WHERE UPPER(Name) = UPPER(@Name)";
            string sqlGetApplication = "SELECT * FROM Applications WHERE UPPER(Name) = UPPER(@Name)";
            string sqlPostApplication = "INSERT INTO Applications (Name, Creation_dt) VALUES (@Name, @Creation_dt)";

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmdExists = new SqlCommand(sqlVerifyApplication, conn);
                cmdExists.Parameters.AddWithValue("@Name", application.Name);

                if ((int)cmdExists.ExecuteScalar() > 0)
                {
                    conn.Close();
                    return Content(HttpStatusCode.BadRequest, "Application name already exists", Configuration.Formatters.XmlFormatter);
                }

                SqlCommand cmdPost = new SqlCommand(sqlPostApplication, conn);
                cmdPost.Parameters.AddWithValue("@Name", application.Name);
                cmdPost.Parameters.AddWithValue("@Creation_dt", DateTime.Now);

                int numRows = cmdPost.ExecuteNonQuery();

                if (numRows > 0)
                {
                    //Vou fazer um select para ir buscar o id o creation_dt
                    SqlCommand cmdGet = new SqlCommand(sqlGetApplication, conn);
                    cmdGet.Parameters.AddWithValue("@Name", application.Name);

                    SqlDataReader reader = cmdGet.ExecuteReader();
                    while (reader.Read())
                    {
                        application.Id = (int)reader["Id"];
                        application.Name = (string)reader["Name"];
                        application.Creation_dt = (DateTime)reader["Creation_dt"];
                    }

                    reader.Close();
                    conn.Close();

                    return Content(HttpStatusCode.OK, "Application created successfully", Configuration.Formatters.XmlFormatter);
                }
                return InternalServerError();
            }
            catch (Exception e)
            {
                Debug.Print("[DEBUG] 'Exception in Post() in ApplicationController' | " + e.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }

        //Put method
        [Route("api/somiod/applications/{name}")]
        public IHttpActionResult PutApplication(int id, HttpRequestMessage request)
        {

            HandlerXML handler = new HandlerXML();

            //Retiro todos os caracteres e espaços desnecessários
            string rawXml = request.Content.ReadAsStringAsync().Result
                .Replace(System.Environment.NewLine, String.Empty);

            //Verifico se a string que veio do request é XML
            if (!handler.IsValidXML(rawXml))
            {
                Debug.Print("[DEBUG] 'String is not XML' | Put() in ApplicationController");
                return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
            }

            //Verifica se o ficheiro XML está de acordo o XSD
            if (!handler.IsValidApplicationSchemaXML(rawXml))
            {
                Debug.Print("[DEBUG] 'Invalid Schema in XML' | Put() in ApplicationController");
                return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
            }

            //Começo o tratamento de dados
            Application application = new Application();

            application.Name = handler.DealRequestApplication();

            if (String.IsNullOrEmpty(application.Name))
            {
                return Content(HttpStatusCode.BadRequest, "Name is Empty", Configuration.Formatters.XmlFormatter);
            }

            //Verifico se o nome da aplicação já existe
            string sqlVerifyApplication = "SELECT COUNT(*) FROM Applications WHERE UPPER(Name) = UPPER(@Name)";
            string sqlGetApplication = "SELECT * FROM Applications WHERE UPPER(Name) = UPPER(@Name)";
            string sql = "UPDATE Applications SET Name = @Name WHERE Id = @Id";

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmdExists = new SqlCommand(sqlVerifyApplication, conn);
                cmdExists.Parameters.AddWithValue("@Name", application.Name);

                if ((int)cmdExists.ExecuteScalar() > 0)
                {
                    conn.Close();
                    return Content(HttpStatusCode.BadRequest, "Application name already exists", Configuration.Formatters.XmlFormatter);
                }

                SqlCommand cmdPut = new SqlCommand(sql, conn);
                cmdPut.Parameters.AddWithValue("@Name", application.Name);
                cmdPut.Parameters.AddWithValue("@Id", id);
                int numRows = cmdPut.ExecuteNonQuery();

                if (numRows > 0)
                {
                    //Vou fazer um select para ir buscar o id o creation_dt
                    SqlCommand cmdGet = new SqlCommand(sqlGetApplication, conn);
                    cmdGet.Parameters.AddWithValue("@Name", application.Name);

                    SqlDataReader reader = cmdGet.ExecuteReader();
                    while (reader.Read())
                    {
                        application.Id = (int)reader["Id"];
                        application.Name = (string)reader["Name"];
                        application.Creation_dt = (DateTime)reader["Creation_dt"];
                    }
                    reader.Close();
                    conn.Close();

                    return Content(HttpStatusCode.OK, "Application update successfully", Configuration.Formatters.XmlFormatter);
                }
                return Content(HttpStatusCode.BadRequest, "Application does not exist", Configuration.Formatters.XmlFormatter);
            }
            catch (Exception e)
            {
                Debug.Print("[DEBUG] 'Exception in Put() in ApplicationController' | " + e.Message);
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }

        //Delete method
        [Route("api/somiod/applications/{name}")]
        public IHttpActionResult DeleteApplication(string name)
        {

            HandlerXML handler = new HandlerXML();

            string sqlGetApplication = "SELECT * FROM Applications WHERE name = @Name";
            string sqlDeleteApplication = "DELETE FROM Applications WHERE name = @Name";
            
            string sqlSelectContainer = "SELECT id FROM Containers WHERE parent = @Parent";
            string sqlDeleteContainer = "DELETE FROM Containers WHERE Id = @Id";

            string sqlDeleteSubscription = "DELETE FROM Subscriptions WHERE parent = @Parent";

            string sqlDeleteData = "DELETE FROM Data WHERE Parent = @Parent";
            SqlConnection conn = null;
            Application application = new Application();

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmdGet = new SqlCommand(sqlGetApplication, conn);
                cmdGet.Parameters.AddWithValue("@Name", name);
                SqlDataReader reader = cmdGet.ExecuteReader();
                while (reader.Read())
                {
                    application.Id = (int)reader["Id"];
                    application.Name = (string)reader["Name"];
                    application.Creation_dt = (DateTime)reader["Creation_dt"];
                }

                reader.Close();


                SqlCommand cmdIdContainers = new SqlCommand(sqlSelectContainer, conn);

                cmdIdContainers.Parameters.AddWithValue("@Parent", application.Id);

                SqlDataReader readerMod = cmdIdContainers.ExecuteReader();
                List<int> ContainersIds = new List<int>();
                List<int> SubscriptionIds = new List<int>();
                List<int> DataIds = new List<int>();
                while (readerMod.Read())
                {
                    ContainersIds.Add((int)readerMod["Id"]);
                }
                readerMod.Close();

                foreach (int idContainer in ContainersIds)
                {
                    SqlCommand cmdDeleteData = new SqlCommand(sqlDeleteData, conn);
                    cmdDeleteData.Parameters.AddWithValue("@Parent", idContainer);
                    cmdDeleteData.ExecuteNonQuery();

                    SqlCommand cmdDeleteContainer = new SqlCommand(sqlDeleteContainer, conn);
                    cmdDeleteContainer.Parameters.AddWithValue("@Id", idContainer);
                    cmdDeleteContainer.ExecuteNonQuery();

                    ////Executar query Delete Subscription
                    //SqlCommand cmdDeleteSubscription = new SqlCommand(sqlDeleteSubscription, conn);
                    //cmdDeleteSubscription.Parameters.AddWithValue("@Parent", idContainer);
                    //cmdDeleteSubscription.ExecuteNonQuery();

                }

                SqlCommand cmd = new SqlCommand(sqlDeleteApplication, conn);
                cmd.Parameters.AddWithValue("@Name", application.Name);
                int numRows = cmd.ExecuteNonQuery();
                if (numRows > 0)
                {
                    conn.Close();
                    return Content(HttpStatusCode.OK, "Application delete successfully", Configuration.Formatters.XmlFormatter);
                }
                return Content(HttpStatusCode.BadRequest, "Application does not exist", Configuration.Formatters.XmlFormatter);
            }
            catch (Exception e)
            {
                Debug.Print("[DEBUG] 'Exception in Delete() in ApplicationController' | " + e.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }

        public HttpResponseMessage DiscoverApplications()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Your implementation to return a list of application names
                    using (SqlCommand cmd = new SqlCommand("SELECT Name FROM Applications", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<Application> applicationNames = new List<Application>();
                            while (reader.Read())
                            {
                                Application application = new Application
                                {
                                    Name = (string)reader["Name"],
                                };
                                applicationNames.Add(application);
                            }

                            var response = Request.CreateResponse(applicationNames);
                            response.Content = new ObjectContent<List<Application>>(applicationNames, new System.Net.Http.Formatting.XmlMediaTypeFormatter());
                            return response;
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

        private Application GetApplicationByName(string name)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Your implementation to retrieve and return an application by ID
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @Name", connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Application
                                {
                                    //Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    //Creation_dt = (DateTime)reader["Creation_dt"]
                                };
                            }
                            return null; // Application not found
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting application: {ex.Message}");
                throw;
            }
        }
    }
}
