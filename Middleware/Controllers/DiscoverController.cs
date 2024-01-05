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
