using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Middleware.Models;
using System.Data.SqlClient;

namespace SomiodIsProject.Controllers
{
    public class ApplicatoinController : ApiController
    {
        // Update the connection string with your actual connection string
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\renat\\Desktop\\IS-Project\\Middleware\\App_Data\\Database1.mdf;Integrated Security=True";

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
        public IHttpActionResult DeleteApplication(int id)
        {
            Application application = GetApplicationById(id);
            if (application != null)
            {
                DeleteApplication(id);
                return Ok(application);
            }
            return NotFound();
        }

    }
}
