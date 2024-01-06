﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;

namespace TestApplicationNumpad
{
    public partial class Form1 : Form
    {
        MqttClient mosquittoClient = new MqttClient("127.0.0.1");
        RestClient client = new RestClient("http://localhost:61552/api/somiod");
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            var request = new RestRequest("http://localhost:61552/api/somiod/numpad", Method.Get);
            request.RequestFormat = DataFormat.Xml;

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var xml = @"<application>
                        <name>numpad</name>
                        <res_type>application</res_type>
                    </application>";

                request = new RestRequest("http://localhost:61552/api/somiod", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                xml = @"<container>
                        <name>numpadMechanism</name>
                        <res_type>container</res_type>
                    </container>";


                request = new RestRequest("http://localhost:61552/api/somiod/numpad/containers", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return;
            }

            mosquittoClient.Connect(Guid.NewGuid().ToString());
            if (!mosquittoClient.IsConnected)
            {
                MessageBox.Show("Não foi possivel ligar ao broker.");
                return;
            }
            MessageBox.Show("Connecteti");


        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "1";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "2";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "3";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "4";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "5";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "6";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "7";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "8";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "9";
        }

        private void button0_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "0";
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxPin.Text = "";
        }

        private void buttonEnter_Click(object sender, EventArgs e)
        {


            string pin = textBoxPin.Text;

        }

        private void labelValidation_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

            if (textBoxPin.Text == "1234")
            {
                var request = new RestRequest("http://localhost:61552/api/somiod/lock/lockingMechanism/data/lockingStatus", Method.Get);
                request.RequestFormat = DataFormat.Xml;
                var response = client.Execute(request);
                var xml = "";

                var status = response.Content;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(status);

                XmlNode contentNode = xmlDoc.SelectSingleNode("/*[local-name()='Data']/*[local-name()='Content']");
                byte[] msg = null;
                if (contentNode.InnerXml == "lock")
                {
                    xml = @"<data>
                                <name>lockingStatus</name>
                                <content>unlock</content>
                                <res_type>data</res_type>
                            </data>";

                     msg = Encoding.UTF8.GetBytes("unlock");
                    labelValidation.Text = "Lock Status : Unlock";
                }
                else
                {
                    xml = @"<data>
                                <name>lockingStatus</name>
                                <content>lock</content>
                                <res_type>data</res_type>
                            </data>";
                    msg = Encoding.UTF8.GetBytes("lock");
                    labelValidation.Text = "Lock Status : Lock";
                }

                request = new RestRequest("http://localhost:61552/api/somiod/lock/lockingMechanism/data", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return;

                
                string topic = "lockingReport";

                if (mosquittoClient.IsConnected)
                {
                    MessageBox.Show("Enviado" + msg);
                    mosquittoClient.Publish(topic, msg);
                }
                
                


            }
            else
            {
                MessageBox.Show("Código inválido! (tenta 1234!)");
                textBoxPin.Text = "";
            }
        }
        private void textBoxPin_TextChanged(object sender, EventArgs e)
        {

        }

      
    }
}