using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using uPLibrary.Networking.M2Mqtt;

namespace Middleware.Models
{
    public class Mqtt
    {
        public MqttClient mClient { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }

        public void connectToEndpoint(string endpoint)
        {
            mClient = new MqttClient(endpoint);
            mClient.Connect(Guid.NewGuid().ToString());
            Debug.Print("Connected to endpoint: " + endpoint);
        }
    }
}