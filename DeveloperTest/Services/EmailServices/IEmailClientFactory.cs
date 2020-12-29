using System;
using DeveloperTest.Models;
using CommonPathAndData;

namespace DeveloperTest.Services.EmailServices
{
    public interface IEmailClientFactory
    {
        IEmailClient GetClient(ServerType serverType, EncryptionType encryptionType, string server, int port, string username, string password);
    }
}
