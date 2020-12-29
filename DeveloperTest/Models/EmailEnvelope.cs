using System;

namespace DeveloperTest.Models
{
    public class EmailEnvelope
    {
        public string Id { get; set; }
        public string[] From { get; set; }
        public string Subject { get; set; }
        public DateTime? Date { get; set; }
    }
}
