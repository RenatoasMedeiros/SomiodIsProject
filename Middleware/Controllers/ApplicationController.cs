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
        public IHttpActionResult GetApplications()
        {
            //List<string> applicationNames = DiscoverApplications();
            return Ok();//(applicationNames);
        }

        // GET: api/Application/1
        [HttpGet]
        [Route("api/somiod/applications/{name}")]

        /*[HttpGet]
        [Route("api/somiod/{application}")]
        public IHttpActionResult GetApplication(int id)
        {
            Application application = GetApplicationByName(name);
            if (application != null)
            {
                return Ok(application);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult PostApplication(HttpRequestMessage request)
        }*/
       

        // POST: api/Application
        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult PostApplication(Application application)
        {
            HandlerXML handler = new HandlerXML();

            string teste = Request.Content.ToString();
            //Retiro todos os caracteres e espaços desnecessários
            string rawXml = request.Content.ReadAsStringAsync().Result
                .Replace(System.Environment.NewLine, String.Empty);

            //Verifico se a string que veio do request é XML
            if (!handler.IsValidXML(rawXml))
            {
                
                return CreatedAtRoute("DefaultApi", new { id = application.Id }, application);
            }
            return BadRequest(ModelState);
        }

        // PUT: api/Application/1
        [HttpPut]
        [Route("api/somiod/{application}")]
        public IHttpActionResult PutApplication(int id, Application application)
        {
            if (id != application.Id)
            {
                return BadRequest("Mismatched Ids");
            }

            if (ModelState.IsValid)
            {
                UpdateApplication(application);
                return Ok(application);
            }

            return BadRequest(ModelState);
        }

        // DELETE: api/Application/1
        [HttpDelete]
        [Route("api/somiod/{application}")]
        public IHttpActionResult DeleteApplication(int id)
        {
           // Application application = GetApplicationById(id);
            /*if (application != null)
            {
                DeleteApplicationById(id);
                return Ok(application);
            }*/
            return NotFound();
        }

        ////[HttpPost]
        ////[Route("api/somiod")]
        //private void AddApplication(Application application)
        //{
        //    HandlerXML handler = new HandlerXML();

        //    string teste = Request.Content.ToString();
        //    //Retiro todos os caracteres e espaços desnecessários
        //    string rawXml = request.Content.ReadAsStringAsync().Result
        //        .Replace(System.Environment.NewLine, String.Empty);

        //    //Verifico se a string que veio do request é XML
        //    if (!handler.IsValidXML(rawXml))
        //    {
        //        Debug.Print("[DEBUG] 'String is not XML' | Post() in ApplicationController");
        //        return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
        //    }

        //    //Verifica se o ficheiro XML está de acordo o XSD
        //    if (!handler.IsValidApplicationSchemaXML(rawXml))
        //    {
        //        Debug.Print("[DEBUG] 'Invalid Schema in XML' | Post() in ApplicationController");
        //        return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
        //    }

        //    //Começo o tratamento de dados
        //    Application application = new Application();

        //    application.Name = handler.DealRequestApplication();

        //    if (String.IsNullOrEmpty(application.Name))
        //    {
        //        return Content(HttpStatusCode.BadRequest, "Name is Empty", Configuration.Formatters.XmlFormatter);
        //    }

        //    //Verifico se o nome da aplicação já existe
        //    string sqlVerifyApplication = "SELECT COUNT(*) FROM Applications WHERE UPPER(Name) = UPPER(@Name)";
        //    string sqlGetApplication = "SELECT * FROM Applications WHERE UPPER(Name) = UPPER(@Name)";
        //    string sqlPostApplication = "INSERT INTO Applications (Name, Creation_dt) VALUES (@Name, @Creation_dt)";

        //    SqlConnection conn = null;

        //    try
        //    {
        //        conn = new SqlConnection(connectionString);
        //        conn.Open();

        //        SqlCommand cmdExists = new SqlCommand(sqlVerifyApplication, conn);
        //        cmdExists.Parameters.AddWithValue("@Name", application.Name);

        //        if ((int)cmdExists.ExecuteScalar() > 0)
        //        {
        //            conn.Close();
        //            return Content(HttpStatusCode.BadRequest, "Application name already exists", Configuration.Formatters.XmlFormatter);
        //        }

        //        SqlCommand cmdPost = new SqlCommand(sqlPostApplication, conn);
        //        cmdPost.Parameters.AddWithValue("@Name", application.Name);
        //        cmdPost.Parameters.AddWithValue("@Creation_dt", DateTime.Now);

        //        int numRows = cmdPost.ExecuteNonQuery();

        //        if (numRows > 0)
        //        {
        //            //Vou fazer um select para ir buscar o id o creation_dt
        //            SqlCommand cmdGet = new SqlCommand(sqlGetApplication, conn);
        //            cmdGet.Parameters.AddWithValue("@Name", application.Name);

        //            SqlDataReader reader = cmdGet.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                application.Id = (int)reader["Id"];
        //                application.Name = (string)reader["Name"];
        //                application.Creation_dt = (DateTime)reader["Creation_dt"];
        //            }

        //            reader.Close();
        //            conn.Close();

        //            handler.AddApplication(application);
        //            return Content(HttpStatusCode.OK, "Application created successfully", Configuration.Formatters.XmlFormatter);
        //        }
        //        return InternalServerError();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Print("[DEBUG] 'Exception in Post() in ApplicationController' | " + e.Message);

        //        if (conn.State == System.Data.ConnectionState.Open)
        //        {
        //            conn.Close();
        //        }
        //        return InternalServerError();
        //    }
        //}

        //Put method
        [Route("api/somiod/applications/{id}")]
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

                    handler.UpdateApplication(application);
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

        //// DELETE: api/Application/1
        //[HttpDelete]
        //[Route("api/somiod/applications/{name}")]
        //public IHttpActionResult DeleteApplication(string name)
        //{
        //    Application application = GetApplicationByName(name);
        //    if (application != null)
        //    {
        //        GetApplicationByName(name);
        //        return Ok(application);
        //    }
        //    return NotFound();
        //}
        

        private string GenerateUniqueName()
        {
            return "App_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /*
        public List<string> DiscoverApplications()
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
                            List<string> applicationNames = new List<string>();
                            while (reader.Read())
                            {
                                applicationNames.Add((string)reader["Name"]);
                            }
                            return applicationNames;
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
        
        private Application GetApplicationById(int id)
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
        */
        private void UpdateApplication(Application application)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Your implementation to update the application in the database
                    using (SqlCommand cmd = new SqlCommand("UPDATE Applications SET Name = @Name WHERE Id = @Id", connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", application.Name);
                        cmd.Parameters.AddWithValue("@Id", application.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error updating application: {ex.Message}");
                throw;
            }
        }

        private void DeleteApplicationById(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Your implementation to delete the application from the database by ID
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Applications WHERE Id = @Id", connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error deleting application: {ex.Message}");
                throw;
            }
        }
    }

    
}
