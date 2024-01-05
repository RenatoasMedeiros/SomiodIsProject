﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;
using Middleware.Models;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Middleware.XML;
using Swashbuckle.Swagger;

namespace Middleware.Controllers
{
    public class DataController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        #region GET Data

        [HttpGet]
        [Route("api/somiod/{application}/data")]
        public HttpResponseMessage GetAllData(string application)
        {
            List<Data> data = new List<Data>();
            string sql = "SELECT d.* FROM Data d " +
                 "JOIN Containers c ON d.parent = c.id " +
                 "JOIN Applications a ON c.parent = a.id " +
                 "WHERE a.name = @Application";

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Application", application);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Data p = new Data
                    {
                        Name = (string)reader["name"],
                    };
                    data.Add(p);
                }
                reader.Close();
                conn.Close();

                var response = Request.CreateResponse(HttpStatusCode.OK, data);
                response.Content = new ObjectContent<List<Data>>(data, new System.Net.Http.Formatting.XmlMediaTypeFormatter());

                return response;

            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception on Get() method in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                var errorResponse = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred while fetching data.");
                return errorResponse;
            }
        }
        #endregion

        #region GET Data by Name

        [HttpGet]
        [Route("api/somiod/{application}/{container}/{dataName}/data")]
        public HttpResponseMessage GetDatabyName(string dataName)
        {
            Data data = new Data();
            string sql = "SELECT * FROM Data WHERE name = @DataName";

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DataName", dataName);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        data.Id = (int)reader["id"];
                        data.Name = (string)reader["name"];
                        data.Content = (string)reader["content"];
                        data.Creation_dt = (DateTime)reader["creation_dt"];
                        data.Parent = (int)reader["parent"];

                    }
                    reader.Close();
                    conn.Close();

                    var response = Request.CreateResponse(HttpStatusCode.OK, data);
                    response.Content = new ObjectContent<Data>(data, new System.Net.Http.Formatting.XmlMediaTypeFormatter());

                    return response;
                }
                else
                {
                    reader.Close();
                    conn.Close();
                    var errorResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "An error occurred while fetching data.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception in Get() in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                var errorResponse = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred while fetching data.");
                return errorResponse;
            }

        }

        #endregion

        #region POST Data

        [HttpPost]
        [Route("api/somiod/{application}/{container}/data")]
        public IHttpActionResult Post(string application, string container, HttpRequestMessage requestNewData)
        {

            #region Verificar XML

            HandlerXML handler = new HandlerXML();

            string requestXML = requestNewData.Content.ReadAsStringAsync().Result
                .Replace(System.Environment.NewLine, String.Empty);
            Debug.Print(requestXML);

            if (requestXML == null)
            {
                Debug.Print("newData:" + requestNewData.Content.ReadAsStringAsync().Result + " is null");
                return BadRequest("Invalid new data. The request body cannot be empty!");

            }

            if (!handler.IsValidXML(requestXML))
            {
                Debug.Print("[DEBUG] 'String is not XML' | Post() in ContainerController");
                return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
            }

            if (!handler.ValidateDataSchemaXML(requestXML))
            {
                Debug.Print("[DEBUG] 'Invalid Schema in XML' | Post() in ContainerController");
                return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
            }

            #endregion

            Data newData = handler.DataRequest();

            int containerId = -1;

            #region Descobrir containerId
            string sqlSelectContainerId = "SELECT c.id FROM Containers c WHERE c.name = @container";

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlSelectContainerId, conn);

                cmd.Parameters.AddWithValue("@container", container);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        containerId = (int)reader["id"];
                    }
                    reader.Close();
                }
                conn.Close();

                if (containerId == -1)
                    return NotFound();

            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception in Post() in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return NotFound();
            }
            #endregion

            newData.Parent = containerId;

            //newData.Parent = containerId;

            // Para fazer o MQTT:

            // SELECT subscription baseado no containerId

            // erro se não houver subscriptions

            // Para cada subscription existente faz um publish MQTT da data

            // Ao fazer o POST, logo de seguida e verificar a resposta OK ou Unternal Server 

            string sqlInsert = "INSERT INTO Data (name, content, creation_dt, parent) VALUES (@name, @content, GETDATE(), @parent)";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlInsert, conn);

                cmd.Parameters.AddWithValue("@name", newData.Name);
                cmd.Parameters.AddWithValue("@content", newData.Content);
                cmd.Parameters.AddWithValue("@parent", newData.Parent);

                int rowsAffected = cmd.ExecuteNonQuery();
                conn.Close();

                if (rowsAffected > 0)
                {

                    return Ok();
                }

                return InternalServerError();
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception on Post() method in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }
        #endregion


        #region PUT Data

        [HttpPut]
        [Route("api/somiod/{application}/{container}/data/{data}")]
        public IHttpActionResult Put(HttpRequestMessage requestDataToEdit, string data)
        {

            if (requestDataToEdit.Content == null)
            {
                return BadRequest("Invalid data. The request body cannot be empty.");
            }

            SqlConnection conn = null;
            try
            {

                #region VerificarXML

                HandlerXML handler = new HandlerXML();

                string requestXML = requestDataToEdit.Content.ReadAsStringAsync().Result
                    .Replace(System.Environment.NewLine, String.Empty);

                if (!handler.IsValidXML(requestXML))
                    return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);

                if (!handler.ValidateDataSchemaXML(requestXML))
                    return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);

                #endregion

                #region Verificar se a data existe
                int dataId = -1;
                int dataParent = -1;
                string queryString = "SELECT Id, Parent FROM Data WHERE name = @dataName";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@dataName", data);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                dataId = (int)reader["Id"];
                                dataParent = (int)reader["Parent"];
                            }
                            reader.Close();
                        }

                        if (dataId == -1)
                            return NotFound();

                    }
                    catch (Exception ex)
                    {
                        Debug.Print("[DEBUG] 'Exception on Put() method in DataController' | " + ex.Message);

                        return InternalServerError();
                    }
                }
                #endregion

                Data dataToEdit = handler.DataRequest();

                string sql = "UPDATE Data SET name = @Name, content = @Content WHERE id = @dataId";

                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Name", dataToEdit.Name);
                cmd.Parameters.AddWithValue("@Content", dataToEdit.Content);
                cmd.Parameters.AddWithValue("dataId", dataId);

                int rowsAffected = cmd.ExecuteNonQuery();
                conn.Close();

                if (rowsAffected > 0)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception on Put() method in DataController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError(); // Return Internal Server Error if an exception occurred
            }
        }
        #endregion

        #region DELETE Data
        //Delete data
        [HttpDelete]
        [Route("api/somiod/{application}/{container}/data/{dataName}")]
        public IHttpActionResult Delete(HttpRequestMessage request, string dataName)
        {

            try
            {
                #region Verificar se a data existe
                int dataId = -1;
                int dataParent = -1;
                string sql = "SELECT Id, Parent FROM Data WHERE name = @dataName";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@dataName", dataName);
                    try
                    {
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                dataId = (int)reader["Id"];
                                dataParent = (int)reader["Parent"];
                            }
                            reader.Close();
                        }

                        if (dataId == -1)
                            return NotFound();

                    }
                    catch (Exception ex)
                    {
                        Debug.Print("[DEBUG] 'Exception on Delete() method in DataController' | " + ex.Message);

                        return InternalServerError();
                    }
                }
                #endregion

                sql = "DELETE FROM Data WHERE Id = @Id";
                //verificar se container existe
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Id", dataId);
                    try
                    {
                        command.Connection.Open();
                        int rows = command.ExecuteNonQuery();
                        if (rows < 0)
                            return InternalServerError();

                    }
                    catch (Exception ex)
                    {
                        Debug.Print("[DEBUG] 'Exception on Delete() method in DataController' | " + ex.Message);

                        return InternalServerError();
                    }
                }

                return Content(HttpStatusCode.OK, "Data Deleted Succefully", Configuration.Formatters.XmlFormatter);
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception on Delete() method in DataController' | " + ex.Message);
                return InternalServerError();
            }
        }
        #endregion

    }
}
