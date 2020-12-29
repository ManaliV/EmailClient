
namespace DeveloperTest.Models
{
    public class EmailModel
    {
        public string Id { get; set; }
        public EmailEnvelope Header { get; set; }
        public EmailBody Body { get; set; }
    }
}
