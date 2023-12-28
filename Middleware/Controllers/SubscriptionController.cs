using Middleware.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


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

                // construir a query para procurar o AppName.
                SqlCommand cmdApp = new SqlCommand(sqlApplication, conn);
                cmdApp.Parameters.AddWithValue("@AppName", appName);

                // executar a query e guardar o AppId se for encontrado um appName igual ao desejado.
                SqlDataReader readerApp = cmdApp.ExecuteReader();
                while (readerApp.Read())
                {
                    appId = (int)readerApp["Id"];
                }

                readerApp.Close();

                // Se o appId se mantiver em '-1', então não foi encontrada nenhuma appName.
                if (appId == -1)
                {
                    conn.Close();
                    Debug.Print("[DEBUG] 'Applications does not exist' | Post() in SubscriptionController");
                    return Content(HttpStatusCode.BadRequest, "Application does not exist", Configuration.Formatters.XmlFormatter);
                }

                // Fazer verificação do containerName recebido.
                SqlCommand cmdMod = new SqlCommand(sqlContainer, conn);
                cmdMod.Parameters.AddWithValue("@containerName", containerName);
                cmdMod.Parameters.AddWithValue("@AppId", appId);

                SqlDataReader readerMod = cmdMod.ExecuteReader();
                while (readerMod.Read())
                {
                    containerId = (int)readerMod["Id"];
                }

                readerMod.Close();
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

        #endregion
    }
}
