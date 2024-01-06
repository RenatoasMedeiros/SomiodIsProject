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
        RestClient client = new RestClient();

        
        public Form1()
        {
             // 
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var request = new RestRequest("http://localhost:61552/api/somiod/applications/lock", Method.Get);
            request.AddHeader("somiod-discover", "application");

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)// ???
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
                    MessageBox.Show("There was an error. Shits Crazy");
                #endregion

                #region Criar Container
                //criar container

                xml = @"<container>
                           <name>lockingMechanism</name>
                           <res_type>container</res_type>
                       </container>";

                request = new RestRequest("http://localhost:61552/api/somiod/applications/lock/containers", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    MessageBox.Show("There was an error. Shits Crazy");

                #endregion
                
                #region Criar Data
                xml = @"<data>
                        <name>lockingStatus</name>
                        <res_type>data</res_type>
                    </data>";

                request = new RestRequest("http://localhost:61552/api/somiod/applications/lock/containers/lockingMechanism/data", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);
                
                response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    MessageBox.Show("There was an error. Shits Crazy");

                //criar subs
                #endregion

                #region Criar Subs

                xml = @"<subs>
                        <name>lockingReport</name>
                        <res_type>subs</res_type>
                    </subs>";

                request = new RestRequest("http://localhost:61552/api/somiod/applications/lock/containers/lockingMechanism/subs", Method.Post)
                {
                    RequestFormat = DataFormat.Xml
                };

                request.AddParameter("application/xml", xml, ParameterType.RequestBody);

                response = client.Execute(request);
                
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    MessageBox.Show("There was an error. Shits Crazy");
                #endregion
                pictureBoxLock.Image = Properties.Resources.Locked;
            }

            request = new RestRequest("http://localhost:61552/api/somiod/applications/lock/containers/lockingMechanism/data/lockedStatus", Method.Get);
            request.AddHeader("somiod-discover", "data");
            response = client.Execute(request);

            if (response.Content != "") { }

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
