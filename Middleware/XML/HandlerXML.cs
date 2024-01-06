
using Middleware.Models;
using System;
using System.Diagnostics;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Schema;



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

            XsdFilePathApplications = HostingEnvironment.MapPath("~/XML/Schema/application.xsd");

            XsdFilePathData = HostingEnvironment.MapPath("~/XML/Schema/data.xsd");

            XsdFilePathSubscriptions = HostingEnvironment.MapPath("~/XML/Schema/subscription.xsd");
        }


        #region XML Application handler

        // Funções para as applications

        public bool IsValidApplicationSchemaXML(string rawXml)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod");
            node.InnerXml += rawXml;

            docTemp.Save(XmlFileTempPath);

            if (ValidateXML(XmlFileTempPath, XsdFilePathApplications)) {
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

        #endregion

        #region XML Containers handler
        // funções para os containers

        #endregion

        #region XML Data handler
        // funções para os datas

        #endregion

        #region XML Subscriptions handler


        public bool ValidateSubscriptionsSchemaXML(string rawXml)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);
            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += rawXml;

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


        #endregion

        #region Compare XML with XML Schema (xsd)

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

        // Valida o Schema do Data
        public bool ValidateDataSchemaXML(string rawXml)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);
            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += rawXml;

            docTemp.Save(XmlFileTempPath);

            if (ValidateXML(XmlFileTempPath, XsdFilePathData))
            {
                return true;
            }

            Debug.Print("[DEBUG] 'Invalid Schema in XML' | ValidateDataSchemaXML() in HandlerXML");
            RefreshTempFile();
            return false;
        }

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

        #endregion

        //#region XML Subscriptions handler

        //// Faz tratamento dos dados do request retorna uma nova subscription 
        //public Subscription SubscriptionRequest()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    Subscription subscription = new Subscription();
        //    doc.Load(XmlFileTempPath);

        //    XmlNode node = doc.SelectSingleNode("//somiod/subscription");
        //    if (node.SelectSingleNode("name") != null)
        //    {
        //        subscription.Name = node.SelectSingleNode("name").InnerText;
        //    }

        //    if (node.SelectSingleNode("event") != null)
        //    {
        //        subscription.Event = node.SelectSingleNode("event").InnerText;
        //    }

        //    if (node.SelectSingleNode("endpoint") != null)
        //    {
        //        subscription.Endpoint = node.SelectSingleNode("endpoint").InnerText;
        //    }

        //    RefreshTempFile();
        //    return subscription;
        //}

        //#endregion

        #region Compare XML with XML Schema (xsd)

        // Valida o XML em relação ao XSD Schema
        public bool ValidateXML(string file, string XsdFilePath)
        {
            isValid = true;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
                ValidationEventHandler eventHandler = new ValidationEventHandler(EventErrorValidation);
                doc.Schemas.Add(null, XsdFilePath);
                doc.Validate(eventHandler);
            }
            catch (XmlException ex)
            {
                isValid = false;
                validationMessage = string.Format("ERROR: {0}", ex.ToString());
            }
            return isValid;
        }

        // Evento para lidar com os erros e avisos durante a validação XML ^
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

        // Limpa qualquer ficheiro XML temporário
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