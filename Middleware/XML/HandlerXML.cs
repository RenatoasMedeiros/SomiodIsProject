
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Schema;
using System.Xml;
using Middleware.Models;
using System.Diagnostics;
using System.Web.Hosting;


namespace Middleware.XML
{
    public class HandlerXML
    {
        private bool isValid = true;

        private string validationMessage;
        public string XmlFileTempPath { get; set; }
        public string XmlFilePath { get; set; }
        public string XsdFilePathApplications { get; set; }
        public string XsdFilePathContainers { get; set; }
        public string XsdFilePathData { get; set; }
        public string XsdFilePathSubscriptions { get; set; }
        public string XsdFilePathSomiod { get; set; }

        public HandlerXML()
        {
            XmlFileTempPath = HostingEnvironment.MapPath("~/XML/Files/temp.xml");

            XmlFilePath = HostingEnvironment.MapPath("~/XML/Files/applications.xml");

            XsdFilePathApplications = HostingEnvironment.MapPath("~/XML/Schema/application.xsd");

            XsdFilePathContainers = HostingEnvironment.MapPath("~/XML/Schema/containers.xsd");

            XsdFilePathData = HostingEnvironment.MapPath("~/XML/Schema/data.xsd");

            XsdFilePathSubscriptions = HostingEnvironment.MapPath("~/XML/Schema/Subscription.xsd");
        }



        #region XML Application handler
        // funções para os applications
        public bool IsValidApplicationSchemaXML(string rawXml)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += rawXml;

            XmlNode resType = docTemp.SelectSingleNode("//res_type");

            if (resType.InnerText != "application")
                return false;

            docTemp.LastChild.LastChild.LastChild.RemoveChild(resType);

            docTemp.Save(XmlFileTempPath);

            if (ValidateXML(XmlFileTempPath, XsdFilePathApplications))
            {
                return true;
            }

            Debug.Print("[DEBUG] 'Invalid Schema in XML' | IsValidApplicationsSchemaXML() in HandlerXML");
            RefreshTempFile();
            return false;
        }

        public string DealRequestApplication()
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            string appName = docTemp.SelectSingleNode("//somiod/application/name").InnerText;

            Debug.Print("[DEBUG] 'App name: " + appName + "' | DealRequestApplication() in HandlerXML");
            RefreshTempFile();

            return appName;
        }

        public void AddApplication(Application application)
        {
            XmlDocument docDefinitive = new XmlDocument();
            docDefinitive.Load(XmlFilePath);

            XmlNode node = docDefinitive.CreateElement("application");

            XmlNode nodeAux = docDefinitive.CreateElement("id");
            nodeAux.InnerText = application.Id.ToString();
            node.AppendChild(nodeAux);

            nodeAux = docDefinitive.CreateElement("creation_dt");
            nodeAux.InnerText = application.Creation_dt.ToString();
            node.AppendChild(nodeAux);

            nodeAux = docDefinitive.CreateElement("name");
            nodeAux.InnerText = application.Name;
            node.AppendChild(nodeAux);

            docDefinitive.SelectSingleNode("//applications").AppendChild(node);

            docDefinitive.Save(XmlFilePath);
            Debug.Print("[DEBUG] 'Applications inserted with success' | AddApplication() in HandlerXML");
        }

        public void UpdateApplication(Application application)
        {
            XmlDocument docDefinitive = new XmlDocument();
            docDefinitive.Load(XmlFilePath);

            XmlNode node = docDefinitive.SelectSingleNode($"//applications/application[id ='{application.Id}']");

            if (node != null)
            {
                node.SelectSingleNode("name").InnerText = application.Name;

                docDefinitive.Save(XmlFilePath);

                Debug.Print("[DEBUG] 'Applications update with success' | UpdateApplication() in HandlerXML");
            }
            else
            {
                Debug.Print("[DEBUG] 'Application not found' | UpdateApplication() in HandlerXML");
            }
        }

        public void DeleteApplication(Application application)
        {
            XmlDocument docDefinitive = new XmlDocument();
            docDefinitive.Load(XmlFilePath);

            XmlNode node = docDefinitive.SelectSingleNode($"//applications/application[id ='{application.Id}']");

            if (node != null)
            {
                docDefinitive.DocumentElement.RemoveChild(node);

                docDefinitive.Save(XmlFilePath);

                Debug.Print("[DEBUG] 'Applications delete with success' | DeleteApplication() in HandlerXML");
            }
            else
            {
                Debug.Print("[DEBUG] 'Application not found' | DeleteApplication() in HandlerXML");
            }
        }


        #endregion

        #region XML Containers handler
        public bool ValidateContainerSchema(string XML)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += XML;

            XmlNode resType = docTemp.SelectSingleNode("//res_type");

            if(resType.InnerText != "container")
                return false;

            docTemp.LastChild.FirstChild.RemoveChild(resType);

            docTemp.Save(XmlFileTempPath);

            if (ValidateXML(XmlFileTempPath, XsdFilePathContainers))
            {
                return true;
            }

            RefreshTempFile();
            return false;
        }

        public string ContainerRequest()
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            string container = docTemp.SelectSingleNode("//somiod/container/name").InnerText;

            RefreshTempFile();

            return container;
        }

        #endregion

        #region XML Data handler

        // Analisa e extrai dados relevantes para processos subsequentes
        public Data DataRequest()
        {
            Data data = new Data();
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod/data");

            if (node != null)
            {
                if (node.SelectSingleNode("name") != null)
                {
                    data.Name = node.SelectSingleNode("name").InnerText;
                }

                if (node.SelectSingleNode("content") != null)
                {
                    data.Content = node.SelectSingleNode("content").InnerText;
                }
            }

            RefreshTempFile();
            return data;
        }

        // Valida o Schema do Data
        public bool ValidateDataSchemaXML(string XML)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);
            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += XML;

            XmlNode resType = docTemp.LastChild.LastChild.SelectSingleNode("//res_type");

            if (resType.InnerText != "data")
                return false;

            // Obtém o nó pai do nó "res_type"
            XmlNode parentNode = resType.ParentNode;

            // Remove o nó "res_type" do seu pai
            if (parentNode != null)
            {
                parentNode.RemoveChild(resType);

                docTemp.Save(XmlFileTempPath);

                if (ValidateXML(XmlFileTempPath, XsdFilePathData))
                {
                    return true;
                }
            }

            docTemp.Save(XmlFileTempPath);

            if (ValidateXML(XmlFileTempPath, XsdFilePathData))
            {
                return true;
            }

            Debug.Print("[DEBUG] 'Invalid Schema in XML' | ValidateDataSchemaXML() in HandlerXML");
            RefreshTempFile();
            return false;
        }

        #endregion

        #region XML Subscriptions handler

        public bool ValidateSubscriptionsSchemaXML(string rawXml)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);
            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += rawXml;

            XmlNode resType = docTemp.SelectSingleNode("//res_type");

            if (resType.InnerText != "subscription")
                return false;

            docTemp.LastChild.FirstChild.RemoveChild(resType);

            docTemp.Save(XmlFileTempPath);

            // If valid Schema in XML 
            if (ValidateXML(XmlFileTempPath, XsdFilePathSubscriptions))
            {
                return true;
            }

            Debug.Print("[DEBUG] 'Invalid Schema in XML' | IsValidSubscriptionsSchemaXML() in HandlerXML");
            RefreshTempFile();
            return false;
        }

        // Faz tratamento dos dados do request retorna uma nova subscription 
        public Subscription SubscriptionRequest()
        {
            XmlDocument doc = new XmlDocument();
            Subscription subscription = new Subscription();
            doc.Load(XmlFileTempPath);

            XmlNode node = doc.SelectSingleNode("//somiod/subscription");
            if (node.SelectSingleNode("name") != null)
            {
                subscription.Name = node.SelectSingleNode("name").InnerText;
            }

            if (node.SelectSingleNode("event") != null)
            {
                subscription.Event = node.SelectSingleNode("event").InnerText;
            }

            if (node.SelectSingleNode("endpoint") != null)
            {
                subscription.Endpoint = node.SelectSingleNode("endpoint").InnerText;
            }

            RefreshTempFile();
            return subscription;
        }

        #endregion

        #region Compare XML with XML Schema (xsd)

        public bool ValidateXML(string file, string XsdFilePath)
        {
            isValid = true;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
                ValidationEventHandler eventHandler = new ValidationEventHandler(EventErrorValidation);
                doc.Schemas.Add(file, XsdFilePath);
                doc.Validate(eventHandler);
            }
            catch (XmlException ex)
            {
                isValid = false;
                validationMessage = string.Format("ERROR: {0}", ex.ToString());
            }
            return isValid;
        }

        // \
        private void EventErrorValidation(object sender, ValidationEventArgs args)
        {
            isValid = false;
            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    validationMessage = string.Format("ERROR: {0}", args.Message);
                    break;
                case XmlSeverityType.Warning:
                    validationMessage = string.Format("WARNING: {0}", args.Message);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region XML handler

        public bool IsValidXML(string input)
        {
            // verifica primeiro se a string não vem vazia.
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(input);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void RefreshTempFile()
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod");

            while (node.HasChildNodes)
            {
                node.RemoveChild(node.FirstChild);
            }

            docTemp.Save(XmlFileTempPath);
        }

        #endregion

    }
}