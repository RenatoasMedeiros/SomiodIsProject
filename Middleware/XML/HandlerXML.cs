using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Schema;
using System.Xml;
using Middleware.Models;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Reflection;
using System.Web.Hosting;

namespace Middleware.XML
{
    public class HandlerXML
    {
        private bool isValid = true;
        private string validationMessage;
        public string XmlFileTempPath { get; set; }
        public string XmlFilePathApplications { get; set; }

        public string XmlFilePathContainers { get; set; }
        public string XmlFilePath { get; set; }
        public string XsdFilePathApplications { get; set; }

        public string XsdFilePathContainers { get; set; }
        public HandlerXML()
        {
            XmlFileTempPath = HostingEnvironment.MapPath("~/XML/Files/temp.xml");

            XmlFilePath = HostingEnvironment.MapPath("~/XML/Files/applications.xml");

            XmlFilePathContainers = HostingEnvironment.MapPath("~/XML/Files/containers.xml");

            XsdFilePathApplications = HostingEnvironment.MapPath("~/XML/Schema/application.xsd");

            XsdFilePathContainers = HostingEnvironment.MapPath("~/XML/Schema/containers.xsd");

        }

        
        public string ValidationMessage
        {
            get { return validationMessage; }
        }

        #region XML Validations
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

        //Validar se é ficheiroXML , tirado da ficha 4
        public bool ValidateXML(string XmlFile, string XsdFilePath)
        {
            isValid = true;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(XmlFile);
                ValidationEventHandler eventHandler = new ValidationEventHandler(MyValidateMethod);
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

        private void MyValidateMethod(object sender, ValidationEventArgs args)
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

        #region XML Application handler
        // funções para os applications

        #endregion

        #region XML Containers handler
        public bool IsValidContainerSchema(string XML)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += XML;

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


        public void AddContainer(Container container)
        {
            XmlDocument file= new XmlDocument();
            file.Load(XmlFilePath);

            XmlNode xmlContainer = file.CreateElement("container");

            XmlNode nodeAux = file.CreateElement("id");
            nodeAux.InnerText = container.Id.ToString();
            xmlContainer.AppendChild(nodeAux);

            nodeAux = file.CreateElement("creation_dt");
            nodeAux.InnerText = container.Creation_dt.ToString();
            xmlContainer.AppendChild(nodeAux);

            nodeAux = file.CreateElement("name");
            nodeAux.InnerText = container.Name;
            xmlContainer.AppendChild(nodeAux);

            nodeAux = file.CreateElement("parent");
            nodeAux.InnerText = container.Parent.ToString();
            xmlContainer.AppendChild(nodeAux);

            file.SelectSingleNode("//applications/application[id='" + container.Parent + "']").AppendChild(xmlContainer);

            file.Save(XmlFilePath);
        }
        
        public void UpdateContainer(Container container)
        {

            XmlDocument file = new XmlDocument();
            file.Load(XmlFilePath);

            XmlNode node = file.SelectSingleNode("//applications/application[id ='" + container.Parent + "']/container[id ='" + container.Id + "']");
            node.SelectSingleNode("name").InnerText = container.Name;
            file.Save(XmlFilePath);
        }
        
        
        public void DeleteContainer(Container container)
        {

            XmlDocument file = new XmlDocument();
            file.Load(XmlFilePath);

            // Obter No Pai
            XmlNode parentNode = file.SelectSingleNode("//applications/application[id ='" + container.Parent + "']/");
            int numMod = parentNode.ChildNodes.Count;
            // Obter No a eliminar
            XmlNode node = file.SelectSingleNode("//container[id ='" + container.Id + "']");

            // Eliminar node
            node.ParentNode.RemoveChild(node);//testar esta condição , se calhar guardar o valor antigo e comparar 
            if (numMod == 1)
            {
                parentNode.ParentNode.RemoveChild(parentNode);
            }

            file.Save(XmlFilePath);

        }

        #endregion

        #region XML Data handler
        // funções para os datas

        #endregion

        #region XML Subscriptions handler

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
               // subscription.Event = node.SelectSingleNode("event").InnerText;
            }

            if (node.SelectSingleNode("endpoint") != null)
            {
                subscription.Endpoint = node.SelectSingleNode("endpoint").InnerText;
            }

            RefreshTempFile();
            return subscription;
        }

        public void AddSubscription(Subscription subscription)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFilePath);
            XmlNode nodeSubscriptions = doc.SelectSingleNode("//applications/application/containers/container[id='" + subscription.Parent + "']/subscriptions");

            //Inserir tag <containers>
            if (nodeSubscriptions == null)
            {
                nodeSubscriptions = doc.CreateElement("subscriptions");
                doc.SelectSingleNode("//applications/application/containers/container[id='" + subscription.Parent + "']").AppendChild(nodeSubscriptions);
            }

            //Inserir tag <container>
            XmlNode xmlSubscription = doc.CreateElement("subscription");

            XmlNode nodeAux = doc.CreateElement("id");
            nodeAux.InnerText = subscription.Id.ToString();
            xmlSubscription.AppendChild(nodeAux);

            nodeAux = doc.CreateElement("creation_dt");
            nodeAux.InnerText = subscription.Creation_dt.ToString();
            xmlSubscription.AppendChild(nodeAux);

            nodeAux = doc.CreateElement("name");
            nodeAux.InnerText = subscription.Name;
            xmlSubscription.AppendChild(nodeAux);

            nodeAux = doc.CreateElement("parent");
            nodeAux.InnerText = subscription.Parent.ToString();
            xmlSubscription.AppendChild(nodeAux);

            nodeAux = doc.CreateElement("event");
            nodeAux.InnerText = subscription.Event.ToString();
            xmlSubscription.AppendChild(nodeAux);

            nodeAux = doc.CreateElement("endpoint");
            nodeAux.InnerText = subscription.Endpoint.ToString();
            xmlSubscription.AppendChild(nodeAux);

            doc.SelectSingleNode("//applications/application/containers/container[id='" + subscription.Parent + "']/subscriptions").AppendChild(xmlSubscription);

            doc.Save(XmlFilePath);
        }

        public void DeleteSubscription(Subscription subscription)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFilePath);
            XmlNode subs = doc.SelectSingleNode("//containers[id ='" + subscription.Parent + "']/subscriptions");
            int numSubs = subs.ChildNodes.Count;
            XmlNode node = doc.SelectSingleNode("//subscriptions[id ='" + subscription.Id + "']");
            node.ParentNode.RemoveChild(node);
            if (numSubs == 1)
            {
                subs.ParentNode.RemoveChild(subs);
            }
            doc.Save(XmlFilePath);

            Debug.Print("[DEBUG] 'Subscriptions delete with success' | DeleteSubscription() in HandlerXML");
        }

        #endregion

        #region XML handler
        public void RefreshTempFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFileTempPath);

            XmlNode node = doc.SelectSingleNode("//somiod");

            while (node.HasChildNodes)
            {
                node.RemoveChild(node.FirstChild);
            }

            doc.Save(XmlFileTempPath);
        }
        #endregion

    }
}