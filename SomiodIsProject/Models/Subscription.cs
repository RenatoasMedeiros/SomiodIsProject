using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Middleware.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Creation_dt { get; set; }
        public int Parent { get; set; } // Parent should store the unique id of the parent resource
        public int Event { get; set; } // 1 for creation, 2 for deletion
        public string Endpoint { get; set; }
    }
}