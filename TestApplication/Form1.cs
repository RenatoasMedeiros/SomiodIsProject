using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows.Forms;

namespace TestApplication
{
    public partial class Form1 : Form
    {
        RestClient client = new RestClient("http://localhost:61552/api/somiod");

        
        public Form1()
        {
             // 
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var request = new RestRequest("http://localhost:61552/api/somiod/lock", Method.Get);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("somiod-discover", "application");

            var response = client.Execute(request);

            if (response.Content != "lock")
            {
                var xml = @"<application>
                           <name>lock</name>
                           <res_type>application</res_type>
                       </application>";

                request = new RestRequest("http://localhost:61552/api/somiod/lock", Method.Post)
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
    }
}
