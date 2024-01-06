using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows.Forms;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace TestApplication
{
    public partial class Form1 : Form
    {
        RestClient client = new RestClient();
        string[] mStrTopicsInfo = { "lockingReport" };
        MqttClient mosquittoClient = new MqttClient(IPAddress.Parse("127.0.0.1"));

        public Form1()
        {
             // 
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var request = new RestRequest("http://localhost:61552/api/somiod/lock", Method.Get);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                #region Criar Application
                var xml = @"<application>
                           <name>lock</name>
                           <res_type>application</res_type>
                       </application>";

                request = new RestRequest("http://localhost:61552/api/somiod/", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    MessageBox.Show("There was an error. Creating Application");
                #endregion

                #region Criar Container
                //criar container

                xml = @"<container>
                           <name>lockingMechanism</name>
                           <res_type>container</res_type>
                       </container>";

                request = new RestRequest("http://localhost:61552/api/somiod/applications/lock", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    MessageBox.Show("There was an error. Creating Container");

                #endregion
                
                #region Criar Subs

                xml = @"<subscription>
                            <name>lockingReport</name>
                            <event>1</event>
                            <endpoint>127.0.0.1</endpoint>
                            <res_type>subscription</res_type>
                        </subscription>";

                request = new RestRequest("http://localhost:61552/api/somiod/lock/lockingMechanism/subscription", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);
                
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    MessageBox.Show("There was an error. Creating Subscription");
                #endregion
                
                pictureBoxLock.Image = Properties.Resources.Locked;
            }

            request = new RestRequest("http://localhost:61552/api/somiod/lock/lockingMechanism/data/lockedStatus", Method.Get);
            request.AddHeader("somiod-discover", "data");
            response = client.Execute(request);

            var status = response.Content;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(status);

            XmlNode contentNode = xmlDoc.SelectSingleNode("/*[local-name()='Data']/*[local-name()='Content']");

            status = contentNode.InnerText;

            mosquittoClient.Connect(Guid.NewGuid().ToString());
            if (!mosquittoClient.IsConnected)
            {
                MessageBox.Show("Error connecting to message broker...");
                return;
            }

            mosquittoClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

           byte[] qosLevels = { 1 };//QoS
            mosquittoClient.Subscribe(mStrTopicsInfo, qosLevels);

        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string msg = Encoding.UTF8.GetString(e.Message);
            string topic = e.Topic;

            this.Invoke((MethodInvoker)delegate { //CrossThread 

                if (msg.Equals("lock"))
                {
                    pictureBoxLock.Image = Properties.Resources.Locked;
                }
                else
                {
                    pictureBoxLock.Image = Properties.Resources.Unlocked;
                }
                
            });

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mosquittoClient.IsConnected)
            {
                mosquittoClient.Unsubscribe(mStrTopicsInfo); //Put this in a button to see notif!
                mosquittoClient.Disconnect(); //Free process and process's resources
            }
        }
    }
}
