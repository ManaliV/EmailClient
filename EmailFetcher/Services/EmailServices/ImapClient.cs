using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonPathAndData;
using DeveloperTest.Models;
using Limilabs.Client.IMAP;

namespace DeveloperTest.Services.EmailServices
{
    internal class ImapClient : BaseEmailClient, IEmailClient
    {
        private readonly Imap _imap = new Imap();
        public ImapClient(string username, string password, string server, int port, EncryptionType encryptionType)
            : base(username, encryptionType)
        {
            _imap = new Imap();

            if (encryptionType == EncryptionType.SSLTLS)
            {
                _imap.ConnectSSL(server, port);
            }
            else 
            { 
                _imap.Connect(server, port);

                if (encryptionType == EncryptionType.StartTLS)
                {
                    _imap.StartTLS();
                }
            }

            try
            {
                _imap.Login(username, password);
            }
            catch
            {
                _imap.Close();
                throw;
            }
            _imap.SelectInbox();
        }

        public ServerType ServerType { get; } = ServerType.IMAP;

        public Task CloseConnection()
        {
            _imap.Close();

            return Task.FromResult(0);
        }

        public Task<string[]> GetAllMessageByUID()
        {
            var messageIds = _imap.Search(Flag.All);

            FreeClient();

            return Task.FromResult(messageIds.Select(x => x.ToString()).ToArray());
        }

        public Task<List<string>> GetUIDsOfAllMessage()
        {
            var messageIds = _imap.Search(Flag.All);
            return Task.FromResult(messageIds.Select(x => x.ToString()).ToList());
        }

        public Task<EmailEnvelope> GetHeaderByUID(string messageId)
        {
            var info = _imap.GetMessageInfoByUID(long.Parse(messageId));
            
            FreeClient();

            return Task.FromResult(new EmailEnvelope()
            {
                Id = messageId,
                From = info.Envelope.From.Select(x => x.Address).ToArray(),
                Subject = info.Envelope.Subject,
                Date = info.Envelope.Date
            }); ;
        }

        public Task<EmailBody> GetBodyByUID(string messageId)
        {
            var structure = _imap.GetBodyStructureByUID(long.Parse(messageId));

            var attachments = structure?.Attachments?
                .Select(x => new EmailAttachment() { Id = x.UID?.ToString(), Name = x.SafeFileName }).ToArray();
            var text = structure.Text != null ? _imap.GetTextByUID(structure.Text) : null;
            var html = structure.Html != null ? _imap.GetTextByUID(structure.Html) : null;

            FreeClient();

            return Task.FromResult(new EmailBody
            {
                Text = text,
                Html = html,
                Attachments = attachments
            });
        }

        public void SetIdleStatus()
        {
            if(_imap!=null)
            _imap.Idle();            
        }

        public Task<List<string>> GetUIDsOfAllUnseenMessage()
        {
            var messageIds = _imap.Search(Flag.Unseen);
            return Task.FromResult(messageIds.Select(x => x.ToString()).ToList());
        }
    }
}
