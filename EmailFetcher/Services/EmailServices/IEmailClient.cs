using System;
using System.Threading.Tasks;
using DeveloperTest.Models;
using CommonPathAndData;
using System.Collections.Generic;
using Limilabs.Client.IMAP;

namespace DeveloperTest.Services.EmailServices
{
    public interface IEmailClient
    {
        bool IsBusy { get; set; }
        string Username { get; }
        EncryptionType EncryptionType { get; }
        ServerType ServerType { get; }

        Task<string[]> GetAllMessageByUID();

        Task<List<string>> GetUIDsOfAllMessage();
        Task<EmailEnvelope> GetHeaderByUID(string messageId);
        Task<EmailBody> GetBodyByUID(string messageId);
        Task CloseConnection();

        void SetIdleStatus();
        Task<List<string>> GetUIDsOfAllUnseenMessage();

        void FreeClient();
    }
}
