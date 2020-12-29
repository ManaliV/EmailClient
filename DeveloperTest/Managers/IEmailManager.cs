using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommonPathAndData;
using DeveloperTest.Models;

namespace DeveloperTest.Managers
{
    public interface IEmailManager
    {
        Task Initialize(string username, string password, string server, int port, ServerType serverType, EncryptionType encryptionType);
        int GetNumberOfIterations();
        Task<List<EmailModel>> ReadAllMailsParallely();
        
        Task<List<EmailModel>>ReadRealTimeEmails();
        Task KeepOnlyTwoConnectionAlive();
        Task CloseAllConnections();

        List<EmailModel> GetAllDownloadedEmails();
        Task<string> DownloadEmailBody(string emailUID);
      
    }
}
