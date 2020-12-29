using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Models
{
    public class EmailEnvelopeItem
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public DateTime? Date { get; set; }
    }
}
