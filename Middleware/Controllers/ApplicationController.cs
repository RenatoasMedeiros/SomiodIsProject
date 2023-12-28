using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Middleware.Models;
using System.Data.SqlClient;

namespace Middleware.Controllers
{
    public class ApplicationController : ApiController
    {
        // Update the connection string with your actual connection string
        string connectionString = Properties.Settings.Default.ConnStr;

        // GET: api/Application
        public IHttpActionResult GetApplications()
        {
            List<string> applicationNames = DiscoverApplications();
            return Ok(applicationNames);
        }

        // GET: api/Application/1
        public IHttpActionResult GetApplication(int id)
        {
            Application application = GetApplicationById(id);
            if (application != null)
            {
                return Ok(application);
            }
            return NotFound();
        }

        // POST: api/Application
        [HttpPost]
        public IHttpActionResult PostApplication(Application application)
        {
            if (ModelState.IsValid)
            {
                AddApplication(application);
                return CreatedAtRoute("DefaultApi", new { id = application.Id }, application);
            }
            return BadRequest(ModelState);
        }

        // PUT: api/Application/1
        [HttpPut]
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
        public IHttpActionResult DeleteApplication(int id)
        {
            Application application = GetApplicationById(id);
            if (application != null)
            {
                DeleteApplicationById(id);
                return Ok(application);
            }
            return NotFound();
        }

        [HttpGet]
        private void AddApplication(Application application)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the application has a name, generate a unique name if not provided
                    if (string.IsNullOrWhiteSpace(application.Name))
                    {
                        application.Name = GenerateUniqueName();
                    }

                    // Your implementation to add the application to the database
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Applications (Name, Creation_dt) VALUES (@Name, @Creation_dt); SELECT SCOPE_IDENTITY();", connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", application.Name);
                        cmd.Parameters.AddWithValue("@Creation_dt", application.Creation_dt);

                        // ExecuteScalar returns the identity (Id) of the newly added record
                        application.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error adding application: {ex.Message}");
                throw;
            }
        }

        private string GenerateUniqueName()
        {
            return "App_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }


        private List<string> DiscoverApplications()
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
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Id = @Id", connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Application
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Creation_dt = (DateTime)reader["Creation_dt"]
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

        [HttpGet]
        [Route("api/somiod/discover")]
        public IHttpActionResult DiscoverResources()
        {
            try
            {
                // Check if the somiod-discover header is present
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader != null && discoverHeader.Contains("application"))
                {
                    // Discover applications
                    List<string> applicationNames = DiscoverApplications();
                    return Ok(applicationNames);
                }
                else if (discoverHeader != null && discoverHeader.Contains("container"))
                {
                    // Discover containers
                    List<string> containerNames = DiscoverContainers();
                    return Ok(containerNames);
                }
                // Add similar logic for other resource types (data, subscription)

                // Default case: Invalid or missing somiod-discover header
                return BadRequest("Invalid or missing somiod-discover header.");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error discovering resources: {ex.Message}");
                return InternalServerError();
            }
        }

        //we need to replace this after we got the ContainersController
        private List<string> DiscoverContainers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Your implementation to return a list of container names
                    using (SqlCommand cmd = new SqlCommand("SELECT Name FROM Containers", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> containerNames = new List<string>();
                            while (reader.Read())
                            {
                                containerNames.Add((string)reader["Name"]);
                            }
                            return containerNames;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error discovering containers: {ex.Message}");
                throw;
            }
        }

    }
}
