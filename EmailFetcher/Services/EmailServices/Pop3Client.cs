using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonPathAndData;
using DeveloperTest.Models;
using Limilabs.Client.POP3;
using Limilabs.Mail;

namespace DeveloperTest.Services.EmailServices
{
    internal class Pop3Client : BaseEmailClient, IEmailClient
    {
        private readonly Pop3 _pop3 = new Pop3();

        public Pop3Client(string username, string password, string server, int port, EncryptionType encryptionType) 
            : base(username, encryptionType)
        {
            _pop3 = new Pop3();

            if (encryptionType == EncryptionType.SSLTLS)
            {
                _pop3.ConnectSSL(server, port);
            }
            else
            {
                _pop3.Connect(server, port);

                if (encryptionType == EncryptionType.StartTLS)
                {
                    _pop3.StartTLS();
                }
            }

            _pop3.Login(username, password);
        }

        public ServerType ServerType { get; } = ServerType.POP3;

        public Task CloseConnection()
        {
            _pop3.Close();

            return Task.FromResult(0);
        }

        public Task<string[]> GetAllMessageByUID()
        {
            var emailIds = _pop3.GetAll().ToArray();

            FreeClient();

            return Task.FromResult(emailIds);
        }

        public Task<List<string>> GetUIDsOfAllMessage()
        {
            var emailIds = _pop3.GetAll().ToList();
            return Task.FromResult(emailIds);
        }

        public void SetIdleStatus()
        {
           // throw new NotImplementedException("");
        }

        public Task<EmailEnvelope> GetHeaderByUID(string messageId)
        {
            var builder = new MailBuilder();
            var headers = _pop3.GetHeadersByUID(messageId);
            var email = builder.CreateFromEml(headers);

            FreeClient();

            return Task.FromResult(new EmailEnvelope()
            {
                From = email.From.Select(x => x.Address).ToArray(),
                Subject = email.Subject,
                Date = email.Date
            });
        }

        public Task<EmailBody> GetBodyByUID(string messageId)
        {
            var builder = new MailBuilder();
            var message = _pop3.GetMessageByUID(messageId);
            var email = builder.CreateFromEml(message);
            var attachments = email?.Attachments?
                .Select(x => new EmailAttachment() { Id = x.ContentId, Name = x.SafeFileName }).ToArray();

            FreeClient();

            return Task.FromResult(new EmailBody
            {
                Text = email.Text,
                Html = email.Html,
                Attachments = attachments
            });
        }

        public Task<List<string>> GetUIDsOfAllUnseenMessage()
        {
            throw new NotImplementedException();
        }
    }
}
