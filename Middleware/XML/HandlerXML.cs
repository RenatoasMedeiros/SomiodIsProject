using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Schema;
using System.Xml;
using Middleware.Models;
using System.Diagnostics;

namespace Middleware.XML
{
    public class HandlerXML
    {
        private bool isValid = true;
        public string XmlFileTempPath { get; set; }
        public string XmlFilePath { get; set; }
        public string XsdFilePathApplications { get; set; }
        public string XsdFilePathContainers { get; set; }
        public string XsdFilePathData { get; set; }
        public string XsdFileSubscriptions { get; set; }
        public string XsdFilePathSomiod { get; set; }

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




        #region XML Application handler
        // funções para os applications

        #endregion

        #region XML Containers handler
        // funções para os containers

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
                subscription.Event = node.SelectSingleNode("event").InnerText;
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