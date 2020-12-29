using DeveloperTest.Models;
using CommonPathAndData;

namespace DeveloperTest.Services.EmailServices
{
    public class EmailClientFactory : IEmailClientFactory
    {
        public IEmailClient GetClient(ServerType serverType, EncryptionType encryptionType, string server, int port,string username, string password ) 
        {
            if (serverType == ServerType.IMAP)
            {
                return new ImapClient(username, password, server, port, encryptionType);
            }
            else
            {
                return new Pop3Client(username, password, server, port, encryptionType);
            }
        }
    }
}
