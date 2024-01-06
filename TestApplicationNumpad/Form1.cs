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


namespace TestApplicationNumpad
{
    public partial class Form1 : Form
    {
        RestClient client = new RestClient("http://localhost:61552/api/somiod");
        public Form1()
        {
            InitializeComponent();
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
            var request = new RestRequest("http://localhost:61552/api/somiod/numpad", Method.Get);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("somiod-discover", "application");

            var response = client.Execute(request);

            if (response.Content != "numpad")
            {
                var xml = @"<application>
                           <name>numpad</name>
                           <res_type>application</res_type>
                       </application>";

                request = new RestRequest("http://localhost:61552/api/somiod/numpad", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                pictureBoxLock.Image = Properties.Resources.Unlocked;
            }

            //MessageBox.Show(response.Content.ToString());

            //SE JA EXISTIR CONTINA A USAR A EXISTENTE SENAO POST
            //pedido para ver o estado
            //set da imagem

            //SENAO 
            //POST CRIAR CADEADO 
            //pedido para ver o estado
            //set da imagem
        }

        private void textBoxPin_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
