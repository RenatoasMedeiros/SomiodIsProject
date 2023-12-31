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

            XmlFilePathApplications = HostingEnvironment.MapPath("~/XML/Files/applications.xml");

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

            // If valid Schema in XML 
            if (ValidateXML(XmlFileTempPath, XsdFilePathContainers))
            {
                return true;
            }

            Debug.Print("[DEBUG] 'Invalid Schema in XML' | IsValidApplicationsSchemaXML() in HandlerXML");
            RefreshTempFile();
            return false;
        }

        public string ContainerRequest()
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            string container = docTemp.SelectSingleNode("//somiod/container/name").InnerText;

            Debug.Print("[DEBUG] 'Container name: " + container + "' | ContainerRequest() in HandlerXML");
            RefreshTempFile();

            return container;
        }

/*
        // --> Terceiro a ser chamado na application
        public void AddModule(Module module)
        {
            XmlDocument docDefinitive = new XmlDocument();
            docDefinitive.Load(XmlFilePath);
            XmlNode nodeApplication = docDefinitive.SelectSingleNode("//applications/application[id='" + module.Parent + "']/modules");

            //Inserir tag <modules>
            if (nodeApplication == null)
            {
                nodeApplication = docDefinitive.CreateElement("modules");
                docDefinitive.SelectSingleNode("//applications/application[id='" + module.Parent + "']").AppendChild(nodeApplication);
            }

            //Inserir tag <module>
            XmlNode xmlModule = docDefinitive.CreateElement("module");

            XmlNode nodeAux = docDefinitive.CreateElement("id");
            nodeAux.InnerText = module.Id.ToString();
            xmlModule.AppendChild(nodeAux);

            nodeAux = docDefinitive.CreateElement("creation_dt");
            nodeAux.InnerText = module.Creation_dt.ToString();
            xmlModule.AppendChild(nodeAux);

            nodeAux = docDefinitive.CreateElement("name");
            nodeAux.InnerText = module.Name;
            xmlModule.AppendChild(nodeAux);

            nodeAux = docDefinitive.CreateElement("parent");
            nodeAux.InnerText = module.Parent.ToString();
            xmlModule.AppendChild(nodeAux);

            docDefinitive.SelectSingleNode("//applications/application[id='" + module.Parent + "']/modules").AppendChild(xmlModule);

            docDefinitive.Save(XmlFilePath);
        }

        public void UpdateModule(Module module)
        {

            XmlDocument docDefinitive = new XmlDocument();
            docDefinitive.Load(XmlFilePath);

            XmlNode node = docDefinitive.SelectSingleNode("//application[id ='" + module.Parent + "']/modules/module[id ='" + module.Id + "']");

            node.SelectSingleNode("name").InnerText = module.Name;

            docDefinitive.Save(XmlFilePath);
            Debug.Print("[DEBUG] 'Module update with success' | UpdateModule() in HandlerXML");

        }

        public void DeleteModule(Module module)
        {

            XmlDocument docDefinitive = new XmlDocument();
            docDefinitive.Load(XmlFilePath);

            // Obter No Pai
            XmlNode nodeDad = docDefinitive.SelectSingleNode("//application[id ='" + module.Parent + "']/modules");
            int numMod = nodeDad.ChildNodes.Count;
            // Obter No a eliminar
            XmlNode node = docDefinitive.SelectSingleNode("//module[id ='" + module.Id + "']");

            // Eliminar node
            node.ParentNode.RemoveChild(node);
            if (numMod == 1)
            {
                nodeDad.ParentNode.RemoveChild(nodeDad);
            }

            docDefinitive.Save(XmlFilePath);
            Debug.Print("[DEBUG] 'Module delete with success' | DeleteModule() in HandlerXML");

        }
*/


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