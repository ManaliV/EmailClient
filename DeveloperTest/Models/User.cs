using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace DeveloperTest.Models
{
    public class User : ObservableObject,IUser
    {
        private string _userName;
        private string _password;
        public string Username 
        {
            get{ return _userName;}
            set{Set(ref _userName, value);}
        }
        public string Password 
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }
    }
}
