using System;

namespace Middleware.Models
{
    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime Creation_dt { get; set; }
        public int Parent { get; set; }
    }
}