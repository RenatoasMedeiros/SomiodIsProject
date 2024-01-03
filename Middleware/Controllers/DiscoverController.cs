using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.SqlClient;
using System.Web.Http;
using Middleware.Models;

namespace Middleware.Controllers
{
    public class DiscoverController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        ApplicationController applicationController = new ApplicationController();
        ContainerController containerController = new ContainerController();


        [HttpGet]
        [Route("api/somiod/applications/{application}")]
        public IHttpActionResult DiscoverResources()
        {
            try
            {
                // Check if the somiod-discover header is present
                var discoverHeader = Request.Headers.GetValues("somiod-discover");

                if (discoverHeader != null && discoverHeader.Contains("application"))
                {
                    // Discover applications
                    //List<string> applicationNames = applicationController.DiscoverApplications();
                    return Ok();//(applicationNames);
                }
                else if (discoverHeader != null && discoverHeader.Contains("container"))
                {
                    // Discover containers
                    return Ok(containerController.DiscoverContainers());
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
    }
}
