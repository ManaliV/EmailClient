using System;
using DeveloperTest.Models;
using CommonPathAndData;

namespace DeveloperTest.Services.EmailServices
{
    public abstract class BaseEmailClient
    {
        
        public BaseEmailClient(string username, EncryptionType encryptionType) 
        {
            Username = username;
            EncryptionType = encryptionType;
        }

        public bool IsBusy { get; set; }

        public EncryptionType EncryptionType { get; private set; }

        public string Username { get; private set; }

        public void FreeClient()
        {
            IsBusy = false;            
        }
    }
}
