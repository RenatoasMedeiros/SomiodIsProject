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
        [Route("api/somiod/subscriptions")]
        public IEnumerable<Subscription> GetAll()
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
                        subscription.Name = (string)reader["Name"];
                        subscription.Creation_dt = (DateTime)reader["Creation_dt"];
                        subscription.Parent = (int)reader["Parent"];
                        subscription.Event = (string)reader["Event"];
                        subscription.Endpoint = (string)reader["Endpoint"];
                    };

                    subscriptions.Add(subscription);
                }

                reader.Close();
                conn.Close();

            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return subscriptions;
        }

        #endregion

        #region CRUDS

        // create new subscription.
        [HttpPost]
        [Route("api/somiod/{appName}/{containerName}")]
        public IHttpActionResult Post([FromBody] string appName, [FromBody] string containerName)
        {
            // Verificar se application ou container estão guardados na BD.
            string sqlApplication = "SELECT Id FROM Applications WHERE UPPER(Name) = UPPER(@AppName)";
            string sqlContainer = "SELECT Id, Name FROM Containers WHERE UPPER(Name) = UPPER(@ContainerName) and Parent = @AppId";

            // init dos Ids.
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
                    appId = (int)readerApp["Id"];
                }

                readerApp.Close(); // para a procura

                // Se o appId se mantiver em '-1', então não foi encontrada nenhuma aplicação.
                if (appId == -1)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] 'Applications does not exist' | Post() in SubscriptionController");
                    return Content(HttpStatusCode.BadRequest, "Application does not exist", Configuration.Formatters.XmlFormatter);
                }

                // Fazer verificação do containerName recebido.
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

                // msg de erro
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

            // verificar se o XML do request é valido


            return null;
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
                    handler.DeleteSubscription(subscription);
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
