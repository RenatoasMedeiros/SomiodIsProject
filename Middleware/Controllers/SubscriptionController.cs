using Middleware.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Middleware.XML;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace Middleware.Controllers
{
    public class SubscriptionController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        #region Get methods

        #region Get all subscriptions from the database.
        [HttpGet]
        [Route("api/somiod/applications/containers/subscriptions")]
        public HttpResponseMessage GetAllSubscriptions()
        {
            #region Verificar Header
            var discoverHeader = Request.Headers.GetValues("somiod-discover");

            if (discoverHeader == null || !discoverHeader.Contains("subscription"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error - There was an error in the Request");
            }
            #endregion

            // criar lista vazia de subscriptions.
            List<Subscription> subscriptions = new List<Subscription>();
            string sql = "SELECT * FROM Subscriptions ORDER BY Id";
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Subscription subscription = new Subscription();
                    {
                        subscription.Id = (int)reader["Id"];
                        subscription.Name = (string)reader["name"];
                        subscription.Creation_dt = (DateTime)reader["creation_dt"];
                        subscription.Parent = (int)reader["parent"];
                        subscription.Event = (string)reader["event"];
                        subscription.Endpoint = (string)reader["endpoint"];
                    };

                    subscriptions.Add(subscription);
                }
                reader.Close();
                conn.Close();

                var response = Request.CreateResponse(HttpStatusCode.OK);

                using (var writer = new StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(List<Subscription>));
                    serializer.Serialize(writer, subscriptions);
                    var xmlString = writer.ToString();

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlString);

                    // Remover os elementos do XML que não interessam
                    foreach (XmlNode subscriptionNode in xmlDoc.SelectNodes("//Subscription"))
                    {
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Id"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Creation_dt"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Parent"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Event"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Endpoint"));
                    }

                    response.Content = new StringContent(xmlDoc.OuterXml, Encoding.UTF8, "application/xml");
                }

                return response;


            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] ERROR CONNECTING TO Database");
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - There was an error : " + ex);
            }
            


        }

        #endregion

        #region Get all subscriptions from a certain container from a certain application
        [HttpGet]
        [Route("api/somiod/applications/{appName}/containers/{containerName}/subscriptions")]
        public HttpResponseMessage GetAllContainerSubscriptions([FromUri] string appName, [FromUri] string containerName)
        {
            
            #region Verificar Header
            var discoverHeader = Request.Headers.GetValues("somiod-discover");

            if (discoverHeader == null || !discoverHeader.Contains("subscription"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error - There was an error in the Request");
            }
            #endregion

            List<Subscription> subscriptions = new List<Subscription>();
            int appId = -1; // id da application detetada.
            int containerId = -1; // id do container detetado.

            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                // query para verificar se application existe
                string sqlSelectApplication = "SELECT Id FROM Applications WHERE UPPER(Name) = UPPER(@AppName)";
                SqlCommand cmd = new SqlCommand(sqlSelectApplication, connection);
                cmd.Parameters.AddWithValue("@Name", appName);
                try
                {
                    cmd.Connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            appId = (int)reader["Id"];
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - There was an error : " + ex);
                }

                // Se o appId se mantiver em '-1', então não foi encontrada nenhuma aplicação.
                if (appId == -1)
                {
                    connection.Close();
                    Debug.Print("[DEBUG] 'Applications does not exist' | Post() in SubscriptionController");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - Application does not exist.");
                }

                // query para verificar se o container existe
                string sqlSelectContainer = "SELECT Id, Name FROM Containers WHERE UPPER(Name) = UPPER(@ContainerName) and Parent = @AppId";

                // executar a query do container
                SqlCommand cmdContainer = new SqlCommand(sqlSelectContainer, connection);
                cmdContainer.Parameters.AddWithValue("@Name", containerName);
                cmdContainer.Parameters.AddWithValue("@AppId", appId);
                try
                {
                    cmdContainer.Connection.Open();
                    SqlDataReader reader = cmdContainer.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            containerId = (int)reader["Id"];
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - There was an error : " + ex);
                }

                // msg de erro do container
                if (containerId == -1)
                {
                    connection.Close();
                    Debug.Print("[DEBUG] 'Container does not exist in this application' | Post() in SubscriptionController");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - Container does not exist.");
                }

                // query para dar fetch dos subscriptions do container detetado
                string sqlSelectSubscriptions = "SELECT * From Subscriptions WHERE Parent = @ParentId";
                SqlCommand cmdSubscriptions = new SqlCommand(sqlSelectSubscriptions, connection);
                cmdSubscriptions.Parameters.AddWithValue("@ParentId", containerId);


                try
                {
                    SqlDataReader reader = cmdSubscriptions.ExecuteReader();
                    while (reader.Read())
                    {
                        Subscription s = new Subscription
                        {
                            Name = (string)reader["Name"],
                        };
                        subscriptions.Add(s);

                    }
                    reader.Close();
                    connection.Close();

                    var response = Request.CreateResponse(HttpStatusCode.OK);

                    using (var writer = new StringWriter())
                    {
                        var serializer = new XmlSerializer(typeof(List<Container>));
                        serializer.Serialize(writer, subscriptions);
                        var xmlString = writer.ToString();

                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlString);

                        foreach (XmlNode containerNode in xmlDoc.SelectNodes("//Subscription"))
                        {
                            containerNode.RemoveChild(containerNode.SelectSingleNode("Id"));
                            containerNode.RemoveChild(containerNode.SelectSingleNode("Creation_dt"));
                            containerNode.RemoveChild(containerNode.SelectSingleNode("Parent"));
                        }
                        response.Content = new StringContent(xmlDoc.OuterXml, Encoding.UTF8, "application/xml");
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - There was an error : " + ex);
                }
            }

        }

        #endregion

        #region Get all subscriptions from an application.
        [HttpGet]
        [Route("api/somiod/{appName}/subscriptions")]
        public HttpResponseMessage GetAllApplicationSubscriptions([FromUri] string appName)
        {

            #region Verificar Header
            var discoverHeader = Request.Headers.GetValues("somiod-discover");

            if (discoverHeader == null || !discoverHeader.Contains("subscription"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error - There was an error in the Request");
            }
            #endregion

            List<Subscription> subscriptions = new List<Subscription>();
            SqlConnection connection = null;

            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlSelectSubscriptions = @"
                    SELECT s.* 
                    FROM Subscriptions s
                    INNER JOIN Containers c ON s.Parent = c.Id
                    INNER JOIN Applications a ON c.parent = a.Id
                    WHERE a.name = @AppName
                    ORDER BY s.Id";

                    SqlCommand cmdSubscriptions = new SqlCommand(sqlSelectSubscriptions, connection);
                    cmdSubscriptions.Parameters.AddWithValue("@AppName", appName);

                    SqlDataReader reader = cmdSubscriptions.ExecuteReader();
                    while (reader.Read())
                    {
                        Subscription s = new Subscription
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"],
                            Creation_dt = (DateTime)reader["Creation_dt"],
                            Parent = (int)reader["Parent"],
                            Event = (string)reader["Event"],
                            Endpoint = (string)reader["Endpoint"]
                        };
                        subscriptions.Add(s);
                    }
                    reader.Close();
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);

                using (var writer = new StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(List<Subscription>));
                    serializer.Serialize(writer, subscriptions);
                    var xmlString = writer.ToString();

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlString);

                    // Remover os elementos do XML que não interessam
                    foreach (XmlNode subscriptionNode in xmlDoc.SelectNodes("//Subscription"))
                    {
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Id"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Creation_dt"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Parent"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Event"));
                        subscriptionNode.RemoveChild(subscriptionNode.SelectSingleNode("Endpoint"));
                    }

                    response.Content = new StringContent(xmlDoc.OuterXml, Encoding.UTF8, "application/xml");
                }

                return response;
            }
            catch (Exception ex)
            {
                if (connection?.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error - There was an error : " + ex);
            }
        }


        #endregion

        #endregion

        #region POST

        // create new subscription.
        [HttpPost]
        [Route("api/somiod/{appName}/{containerName}/subscription")]
        public IHttpActionResult Post(string appName,  string containerName, HttpRequestMessage request)
        {

            #region Verificar Header
            var discoverHeader = Request.Headers.GetValues("somiod-discover");

            if (discoverHeader == null || !discoverHeader.Contains("subscription"))
            {
                return Content(HttpStatusCode.BadRequest, "Invalid or missing somiod-discover header.", Configuration.Formatters.XmlFormatter);
            }
            #endregion

            // verificar se o XML do request é valido.
            HandlerXML handler = new HandlerXML();

            // serializar a string do request e remover os espaços vazios.
            string rawXml = request.Content.ReadAsStringAsync().Result.Replace(System.Environment.NewLine, String.Empty);

            // verificar se o XML do request é valido.
            if (!handler.IsValidXML(rawXml))
            {
                Debug.Print("[DEBUG] 'String is not XML' | Post() in SubscriptionController");
                return Content(HttpStatusCode.BadRequest, "Request is not XML", Configuration.Formatters.XmlFormatter);
            }


            // Verificar se o xml está de acordo com o schema.
            if (!handler.ValidateSubscriptionsSchemaXML(rawXml))
            {
                Debug.Print("[DEBUG] 'Invalid Schema in XML' | Post() in SubscriptionController");
                return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
            }



            // Verificar se application ou container estão guardados na BD.
            string sqlApplication = "SELECT Id FROM Applications WHERE UPPER(Name) = UPPER(@AppName)";
            string sqlContainer = "SELECT Id, Name FROM Containers WHERE UPPER(Name) = UPPER(@ContainerName) and Parent = @AppId";

            // init dos Ids. (vai dar erro se permanecerem: -1)
            int appId = -1, containerId = -1;
            SqlConnection conn = null;

            try
            {
                // tentar ligar a BD.
                conn = new SqlConnection(connectionString);
                conn.Open();

                // construir a query para procurar o a aplicação.
                SqlCommand cmdApp = new SqlCommand(sqlApplication, conn);
                cmdApp.Parameters.AddWithValue("@AppName", appName);

                // executar a query e guardar o AppId se for encontrado uma aplicação igual ao desejado.
                SqlDataReader readerApp = cmdApp.ExecuteReader();
                while (readerApp.Read())
                {
                    appId = (int)readerApp["Id"]; // O Id vem do SELECT da query.
                }

                readerApp.Close(); // Termina a procura

                // Se o appId se mantiver em '-1', então não foi encontrada nenhuma aplicação.
                if (appId == -1)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] 'Applications does not exist' | Post() in SubscriptionController");
                    return Content(HttpStatusCode.BadRequest, "Application does not exist", Configuration.Formatters.XmlFormatter);
                }

                // Após a verificação da aplicação, fazer verificação do container desta aplicação.
                SqlCommand cmdContainer = new SqlCommand(sqlContainer, conn);
                cmdContainer.Parameters.AddWithValue("@containerName", containerName);
                cmdContainer.Parameters.AddWithValue("@AppId", appId);

                // executa a query e procura ate encontrar um matching container.
                SqlDataReader readerContainer = cmdContainer.ExecuteReader();
                while (readerContainer.Read())
                {
                    containerId = (int)readerContainer["Id"];
                }

                readerContainer.Close(); // para a procura

                // msg de erro do container
                if (containerId == -1)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] 'Container does not exist in this application' | Post() in SubscriptionController");
                    return Content(HttpStatusCode.BadRequest, "Container does not exist in this application", Configuration.Formatters.XmlFormatter);
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Debug.Print("[DEBUG] 'Exception in Post() in SubscriptionController' | " + ex.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }

            // Criação da nova subscription após as verificações XML serem feitas
            Subscription subscription = handler.SubscriptionRequest();
            subscription.Parent = containerId;

            Debug.Print("[DEBUG] 'Event: " + subscription.Event + " ' | Post() in SubscriptionController");

            // verificação do tipo de evento - creation 1 ou deletion 2
            if (!subscription.Event.Equals("1") && !subscription.Event.Equals("2"))
            {
                Debug.Print("[DEBUG] 'Event is invalid' | Post() in SubscriptionController");
                return Content(HttpStatusCode.BadRequest, "Subscription event is invalid", Configuration.Formatters.XmlFormatter);
            }

            // verificação do subscription name & endpoint
            if (String.IsNullOrEmpty(subscription.Name))
            {
                subscription.Name = containerName;
            }

            if (String.IsNullOrEmpty(subscription.Endpoint))
            {
                subscription.Endpoint = "127.0.0.1";
            }

            string sqlPostSubscription = "INSERT INTO Subscriptions(Name, Creation_dt, Parent, Event, Endpoint) OUTPUT INSERTED.Id VALUES (@Name, GETDATE(), @Parent, @Event, @Endpoint)";

            try
            {
                conn.Open();

                SqlCommand cmdPost = new SqlCommand(sqlPostSubscription, conn);
                cmdPost.Parameters.AddWithValue("@Name", subscription.Name);
                cmdPost.Parameters.AddWithValue("@Parent", subscription.Parent);
                cmdPost.Parameters.AddWithValue("@Event", subscription.Event);
                cmdPost.Parameters.AddWithValue("@Endpoint", subscription.Endpoint);

                // Obter o Id diretamente do comando de inserção
                subscription.Id = (int)cmdPost.ExecuteScalar();

                conn.Close();

                if (subscription.Id > 0)
                {
                    return Content(HttpStatusCode.OK, "Subscription added successfully", Configuration.Formatters.XmlFormatter);
                }

                return InternalServerError();
            }
            catch (Exception e)
            {
                Debug.Print("[DEBUG] 'Exception in Post() in SubscriptionController' | " + e.Message);

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }

                return InternalServerError();
            }
        }
        #endregion

        #region DELETE

        //Delete method
        [Route("api/somiod/subscriptions/{name}")]
        public IHttpActionResult Delete(string name)
        {
            HandlerXML handler = new HandlerXML();
            string sqlGetSubscription = "SELECT * FROM Subscriptions WHERE Name = @Name";
            string sql = "DELETE FROM Subscriptions WHERE Name = @Name";
            Subscription subscription = new Subscription();

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmdGetSubscription = new SqlCommand(sqlGetSubscription, conn);
                cmdGetSubscription.Parameters.AddWithValue("@Name", name);
                SqlDataReader reader = cmdGetSubscription.ExecuteReader();
                while (reader.Read())
                {
                    subscription.Id = (int)reader["Id"];
                    subscription.Name = (string)reader["Name"];
                    subscription.Parent = (int)reader["Parent"];
                    subscription.Creation_dt = (DateTime)reader["Creation_dt"];
                    subscription.Event = (string)reader["Event"];
                    subscription.Endpoint = (string)reader["Endpoint"];
                }
                reader.Close();

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);

                // Numero de linhas afetadas.
                int numRows = cmd.ExecuteNonQuery();
                conn.Close();

                if (numRows > 0)
                {
                    return Content(HttpStatusCode.OK, "Subscription deleted successfully", Configuration.Formatters.XmlFormatter);
                }
                return Content(HttpStatusCode.BadRequest, "Subscription does not exist", Configuration.Formatters.XmlFormatter);
            }
            catch (Exception e)
            {
                Debug.Print("[DEBUG] 'Exception in Delete() in SubscriptionController' | " + e.Message);
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return InternalServerError();
            }
        }
        #endregion
    }
}
