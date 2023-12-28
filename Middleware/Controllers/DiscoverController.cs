using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.SqlClient;
using System.Web.Http;

namespace Middleware.Controllers
{
    public class DiscoverController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        ApplicationController applicationController = new ApplicationController();


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
                    List<string> applicationNames = applicationController.DiscoverApplications();
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

        // Add similar methods for other resource types (application, data, subscription)
    }
}
