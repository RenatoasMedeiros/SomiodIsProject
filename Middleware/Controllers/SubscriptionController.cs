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


namespace Middleware.Controllers
{
    public class SubscriptionController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        #region Gets

        //Get all subscriptions.
        [HttpGet]
        [Route("api/somiod/subscriptions")]
        public IEnumerable<Subscription> GetAllSubscriptions()
        {
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
                return subscriptions;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] ERROR CONNECTING TO Database");
                }
                return subscriptions;
            }
            
        }
        #endregion

        #region CRUDS

        // create new subscription.
        [HttpPost]
        [Route("api/somiod/{appName}/{containerName}")]
        public IHttpActionResult Post([FromBody] string appName, [FromBody] string containerName, HttpRequestMessage request)
        {
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
            if (!handler.IsValidSubscriptionsSchemaXML(rawXml))
            {
                // TODO: verificação do isValidDataSchemaXMl... 

                Debug.Print("[DEBUG] 'Invalid Schema in XML' | Post() in SubscriptionController");
                return Content(HttpStatusCode.BadRequest, "Invalid Schema in XML", Configuration.Formatters.XmlFormatter);
            }


            // Criação da nova subscription após as verificações XML serem feitas
            Subscription subscription = new Subscription();
            subscription = handler.SubscriptionRequest();
            subscription.Parent = containerId;

            Debug.Print("[DEBUG] 'Event: " + subscription.Event + " ' | Post() in SubscriptionController");

            // verificação do tipo de evento - creation ou deletion
            if (!subscription.Event.ToLower().Equals("creation")
                && !subscription.Event.ToLower().Equals("deletion"))
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

            // verificar previamente na base de dados se já existe um subscription com o mesmo evento.
            string sqlVerifySubscriptionInContainer = "SELECT COUNT(*) FROM Subscriptions WHERE Parent = @ParentId AND UPPER(Name) = UPPER(@Name) AND (UPPER(Event) = UPPER(@Event)";
            string sqlPostSubscription = "INSERT INTO Subscriptions(Name, Creation_dt, Parent, Event, Endpoint) OUTPUT INSERTED.Id VALUES (@Name, @Creation_dt, @Parent, @Event, @Endpoint)";

            try
            {
                conn.Open();

                SqlCommand cmdSubscriptionName = new SqlCommand(sqlVerifySubscriptionInContainer, conn);
                cmdSubscriptionName.Parameters.AddWithValue("@ParentId", containerId);
                cmdSubscriptionName.Parameters.AddWithValue("@Name", subscription.Name);
                cmdSubscriptionName.Parameters.AddWithValue("@Event", subscription.Event);
                cmdSubscriptionName.Parameters.AddWithValue("@Both", "both");

                if ((int)cmdSubscriptionName.ExecuteScalar() > 0)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] 'This subscription name already exists in this module with the same event (" + subscription.Event + ")' | Post() in SubscriptionController");
                    return Content(HttpStatusCode.BadRequest, "This subscription name already exists in this module with the same event or exists with event 'both'", Configuration.Formatters.XmlFormatter);
                }

                SqlCommand cmdPost = new SqlCommand(sqlPostSubscription, conn);
                cmdPost.Parameters.AddWithValue("@Name", subscription.Name);
                cmdPost.Parameters.AddWithValue("@Creation_dt", DateTime.Now);
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


        //Delete method
        [Route("api/somiod/subscriptions/{id}")]
        public IHttpActionResult Delete(int id)
        {
            HandlerXML handler = new HandlerXML();
            string sqlGetSubscription = "SELECT * FROM Subscriptions WHERE Id = @Id";
            string sql = "DELETE FROM Subscriptions WHERE Id = @Id";
            Subscription subscription = new Subscription();

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand cmdGetSubscription = new SqlCommand(sqlGetSubscription, conn);
                cmdGetSubscription.Parameters.AddWithValue("@Id", id);
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
                cmd.Parameters.AddWithValue("@Id", id);

                int numRows = cmd.ExecuteNonQuery();
                conn.Close();

                if (numRows > 0)
                {
                    return Content(HttpStatusCode.OK, "Subscription delete successfully", Configuration.Formatters.XmlFormatter);
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
