using System;
using System.Collections.Generic;
using System.Text;

namespace DeveloperTest.Models
{
    public class EmailBody
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
        public EmailAttachment[] Attachments { get; set; }
    }
}
