
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
        public HandlerXML()
        {
            XmlFileTempPath = HostingEnvironment.MapPath("~/XML/Files/temp.xml");

            XmlFilePath = HostingEnvironment.MapPath("~/XML/Files/applications.xml");

            XsdFilePathApplications = HostingEnvironment.MapPath("~/XML/Schema/application.xsd");

        }


        #region XML Subscriptions handler

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

        #region XML Application handler

        public bool IsValidApplicationSchemaXML(string rawXml)
        {
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(XmlFileTempPath);

            XmlNode node = docTemp.SelectSingleNode("//somiod");

            node.InnerXml += rawXml;

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

        #region Validate XML with XML Schema (xsd)

        public bool ValidateXML(string file, string XsdFilePath)
        {
            isValid = true;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
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

        #region Validate XML String Request

        public bool IsValidStringXML(string xmlStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(xmlStr);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }
        #endregion

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
    }
}