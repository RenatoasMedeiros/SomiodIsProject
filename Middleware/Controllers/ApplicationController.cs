﻿using System;
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
        string connectionString = Properties.Settings.Default.ConnStr;


        // GET: api/Application
        [HttpGet]
        [Route("api/somiod/applications")]
        public IHttpActionResult GetApplications()
        {
            List<string> applicationNames = DiscoverApplications();
            return Ok(applicationNames);
        }

        // GET: api/Application/1
        [HttpGet]
        [Route("api/somiod/applications/{name}")]
        public IHttpActionResult GetApplication(string name)
        {
            Application application = GetApplicationByName(name);
            if (application != null)
            {
                return Ok(application);
            }
            return NotFound();
        }

        // POST: api/Application
        [HttpPost]
        [Route("api/somiod/applications")]
        public IHttpActionResult PostApplication(Application application)
        {
            try
            {
                if (ModelState.IsValid)
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

                        return CreatedAtRoute("DefaultApi", new { id = application.Id }, application);
                    }
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error adding application: {ex.Message}");
                throw;
            }
        }

        //we need to change this to name
        // PUT: api/somiod/1
        [HttpPut]
        [Route("api/somiod/applications/{name}")]
        public IHttpActionResult PutApplication(string name, Application application)
        {
            if (name != application.Name)
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
        [Route("api/somiod/applications/{name}")]
        public IHttpActionResult DeleteApplication(string name)
        {
            Application application = GetApplicationByName(name);
            if (application != null)
            {
                GetApplicationByName(name);
                return Ok(application);
            }
            return NotFound();
        }
        

        private string GenerateUniqueName()
        {
            return "App_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }


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
