using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonPathAndData;
using DeveloperTest.Models;
using DeveloperTest.Services.EmailServices;

namespace DeveloperTest.Managers
{

    public class EmailManager : IEmailManager
    {
        private readonly IEmailClientFactory _emailFactory;
        private readonly List<IEmailClient> _clientConnections = new List<IEmailClient>();

        private string _username;
        private string _password;
        private string _server;
        private int _port;
        private ServerType _serverType;
        private EncryptionType _encryptionType;

        private List<string> _emailUIDs;
        private List<EmailModel> _allEmails;
        private List<EmailModel> _unseenEmails;
        private int _startRange = 0;
        private int _countRange = ConstantValues.MaxNoOfSimultanousMailRead;


        Object _clientConnectionCreationLock = new Object();
        ReaderWriterLockSlim _emailListLock = new ReaderWriterLockSlim();

        public EmailManager(IEmailClientFactory emailFactory) 
        {
            _emailFactory = emailFactory;
            _allEmails    = new List<EmailModel>();
            _unseenEmails = new List<EmailModel>();
        }

        public async Task<List<EmailModel>> ReadRealTimeEmails()
        {
            if(_clientConnections!=null)
            {
                var freeEmailClient = _clientConnections.FirstOrDefault(x => !x.IsBusy);
                if (freeEmailClient != null)
                {
                    freeEmailClient.IsBusy = true;

                    freeEmailClient.SetIdleStatus();
                    var unseenUIDs = await freeEmailClient.GetUIDsOfAllUnseenMessage();
                    await ReadRealTimeMessage(freeEmailClient, unseenUIDs);
                }
            }
            
            return _unseenEmails;           
        }

        public async Task<List<EmailModel>> ReadAllMailsParallely()
        {
            List<string> uids = _emailUIDs.GetRange(_startRange,_countRange);
            List<IEmailClient> emailCLients = new List<IEmailClient>();
            List<List<string>> groupOfUIDs = new List<List<string>>();
            groupOfUIDs = SplitUids(uids);

            var taskList = new List<Task<bool>>();
            for (int headerTaskIndex = 0; headerTaskIndex < ConstantValues.MaxReadOrWriteThread; ++headerTaskIndex)
            {
               IEmailClient headerClient = _clientConnections.FirstOrDefault(x => !x.IsBusy);
               headerClient.IsBusy = true;
               emailCLients.Add(headerClient);

               taskList.Add(ReadHeader(headerClient, groupOfUIDs[headerTaskIndex], _allEmails));

            }
            for (int bodyTaskIndex = 0; bodyTaskIndex < ConstantValues.MaxReadOrWriteThread; ++bodyTaskIndex)
            {
                IEmailClient bodyClient = _clientConnections.FirstOrDefault(x => !x.IsBusy);
                bodyClient.IsBusy = true;
                emailCLients.Add(bodyClient);

                taskList.Add(ReadBody(bodyClient, groupOfUIDs[bodyTaskIndex], _allEmails));
            }
           
            await Task.WhenAll(taskList);
            foreach (var client in emailCLients)
            {
               client.IsBusy = false;
            }

            UpdateEmailRangeVariables();

            return _allEmails;
        }
        public async Task Initialize(string username, string password, string server, int port, ServerType serverType, EncryptionType encryptionType)
        {
            InitializeEmailManager(username, password, server, port, serverType, encryptionType);
            CreateAllConnections();
            var connection = _clientConnections.FirstOrDefault(x => !x.IsBusy);
            await GetAllUIDs(connection);
            connection.IsBusy = false;

        }
        public Task KeepOnlyTwoConnectionAlive()
        {

            for(int clientIndex=2;clientIndex<_clientConnections.Count;++clientIndex)
            {
                var client = _clientConnections[clientIndex];
                client.CloseConnection().ConfigureAwait(false);
                _clientConnections.RemoveAt(clientIndex);
            }

            return Task.FromResult(0);
        }

        public Task CloseAllConnections()
        {

            for (int clientIndex = 0; clientIndex < _clientConnections.Count; ++clientIndex)
            {
                var client = _clientConnections[clientIndex];
                client.CloseConnection().ConfigureAwait(false);
                _clientConnections.RemoveAt(clientIndex);
            }

            return Task.FromResult(0);
        }


        public async Task<string> DownloadEmailBody(string emailUID)
        {
            string bodyHTML=null;
            List<string> emailUIDList = new List<string>();
            emailUIDList.Add(emailUID);

            if (_clientConnections != null)
            {
                var freeEmailClient = _clientConnections.FirstOrDefault(x => !x.IsBusy);
                if (freeEmailClient != null)
                {
                    freeEmailClient.IsBusy = true;
                    try
                    {                        
                        await ReadBody(freeEmailClient, emailUIDList, _allEmails);                     
                    }
                    finally
                    {
                        var model = _allEmails.FirstOrDefault(x => x.Id == emailUID);
                        if(model!=null && model.Body!=null)
                        {   
                           bodyHTML = model.Body.Html;
                        }
                        freeEmailClient.IsBusy = false;
                    }
                }

            }
            
            return bodyHTML;
        }

        private void UpdateEmailRangeVariables()
        {
            _startRange += _countRange;

            if ((_startRange + _countRange) > (_emailUIDs.Count - 1))
                _countRange = (_emailUIDs.Count - _startRange - 1);

        }
        private List<List<string>> SplitUids(IList<string> uids)
        {
            List<List<string>> groupOfUIds = new List<List<string>>();
            
            for (int list_index = 0; list_index < ConstantValues.MaxReadOrWriteThread; list_index++)
            {
                List<string> list_of_uids = new List<string>();
                groupOfUIds.Add(list_of_uids);
            }
            
            int list_iterator = 0;
            for (int uid_index = 0; uid_index < uids.Count(); uid_index++)
            {
                groupOfUIds[list_iterator].Add(uids[uid_index]);
                list_iterator++;
                //2 means, 2 task for reading header and 2 for reading body at a time
                if (list_iterator == 2)
                {
                    list_iterator = 0;
                }
            }
            return groupOfUIds;
        }



        private async Task GetAllUIDs(IEmailClient client)
        {
            _emailUIDs = await client.GetUIDsOfAllMessage();
            _emailUIDs.Reverse();
        }

        public List<EmailModel> GetAllDownloadedEmails()
        {
            return _allEmails;
        }

        private async Task<bool> ReadHeader(IEmailClient client,List<string> emailUIDs,List<EmailModel> emailList)
        {

            foreach(var Id in emailUIDs)
            {
                var header = await client.GetHeaderByUID(Id);

                var headerModel = new EmailEnvelope()
                {
                    Id   = Id,
                    Date = header.Date.Value,
                    Subject = header.Subject,
                    From = header.From
                };
                
                _emailListLock.EnterReadLock();
                EmailModel emailModel= emailList.FirstOrDefault(x => x.Id == Id);
                _emailListLock.ExitReadLock();
               
               if (emailModel != null)
                    emailModel.Header = headerModel;
               else
               {
                    emailModel        = new EmailModel();
                    emailModel.Id     = Id;
                    emailModel.Header = header;

                    _emailListLock.EnterWriteLock();
                    emailList.Add(emailModel);
                    _emailListLock.ExitWriteLock();
               }

            }

            return true;

        }

        private async Task<bool> ReadBody(IEmailClient client,List<string> emailUIDs, List<EmailModel> emailList)
        {
            foreach (var Id in emailUIDs)
            {
                _emailListLock.EnterReadLock();
                EmailModel emailModel = emailList.FirstOrDefault(x => x.Id == Id);
                _emailListLock.ExitReadLock();

                if(emailModel!=null && emailModel.Body==null)
                {
                     var body = await client.GetBodyByUID(Id);
                     var bodyModel = new EmailBody()
                     {
                          Text = body.Text,
                          Html = body.Html,
                          //Attachments= body.Attachments.Select(x=>x.Id).ToArray()
                           Attachments = body.Attachments
                     };

                     if (emailModel != null)
                          emailModel.Body = bodyModel;
                     else
                     {
                          emailModel = new EmailModel();
                          emailModel.Id = Id;
                          emailModel.Body = bodyModel;

                          _emailListLock.EnterWriteLock();
                          emailList.Add(emailModel);
                          _emailListLock.ExitWriteLock();
                     }

                   
                }
                
            }
            return true;
        }

        private async Task ReadRealTimeMessage(IEmailClient client, List<string> emailUIDs)
        {
            _unseenEmails.Clear();
            await ReadHeader(client, emailUIDs,_unseenEmails);
            await ReadBody(client, emailUIDs,_unseenEmails);
            _allEmails.AddRange(_unseenEmails);
        }

        private void InitializeEmailManager(string username, string password, string server, int port, ServerType serverType, EncryptionType encryptionType)
        {
            _username = username;
            _password = password;
            _server = server;
            _port = port;
            _serverType = serverType;
            _encryptionType = encryptionType;
        }

        private void CreateAllConnections()
        {
            _clientConnections.Clear();

            Parallel.For(0, ConstantValues.MaxConnection, index =>
            {

                var connection = _emailFactory.GetClient(_serverType, _encryptionType, _server, _port, _username, _password);
                if (connection != null)
                {
                    connection.IsBusy = false;
                    lock (_clientConnectionCreationLock)
                    {
                        _clientConnections.Add(connection);
                    }
                }

            });

        }

        public int GetNumberOfIterations()
        {
            int itterations= _emailUIDs.Count / ConstantValues.MaxNoOfSimultanousMailRead;
            int modulus    = _emailUIDs.Count % ConstantValues.MaxNoOfSimultanousMailRead;
            if (modulus != 0) ++itterations;
            return itterations;
        }
    }
}
