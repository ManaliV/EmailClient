using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using CommonPathAndData;
using GalaSoft.MvvmLight.Command;
using CommonServiceLocator;
using System.Collections.ObjectModel;
using System.Linq;
//using System.Web.Script.Serialization;
using DeveloperTest.Models;
using DeveloperTest.Managers;
using DeveloperTest.Services.Helpers;
using System.Collections.Generic;
using System.Threading;

namespace DeveloperTest.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        
        private ServerType _selectedServerType        = ServerType.IMAP;
        private EncryptionType _selectedEncryptionType= EncryptionType.Unencrypted;

        private ushort _portNumber    = ConstantValues.EncryptedIMAPPort;
        private string _serverName    = ConstantValues.IMAPGmailServer;
        private string _bodyHtml;
        private bool _enableControl;
        private string _buttonContent;
        private EmailEnvelopeItem _selectedEmail;

        private IUser _user;
        private readonly IEmailManager _emailManager;
        private readonly IDialogService _dialog;
        private bool _isConnected = false;

        CancellationTokenSource stopToken;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
           _dialog       = ServiceLocator.Current.GetInstance<IDialogService>();
           _user         = ServiceLocator.Current.GetInstance<IUser>();
           _emailManager = ServiceLocator.Current.GetInstance<IEmailManager>();

            _enableControl = true;
            _buttonContent = ConstantValues.StartButtonText;

            StartMailFetching = new RelayCommand(FetchAllMail);

        }

        #region Properties
        public IUser CurrentUser
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }
        public ServerType SelectedServerType
        {
            get { return _selectedServerType; }
            set { Set(ref _selectedServerType, value); }
        }

        public EncryptionType SelectedEncryptionType
        {
            get { return _selectedEncryptionType; }
            set { Set(ref _selectedEncryptionType, value); }
        }

        public ushort PortNumber
        {
            get { return _portNumber; }
            set { Set(ref _portNumber, value); }
        }

        public string ServerName
        {
            get { return _serverName; }
            set { Set(ref _serverName, value); }
        }
        
        public string BodyHtml
        {
            get { return _bodyHtml; }
            set { Set(ref _bodyHtml, value); }
        }

        public string ButtonContent
        {
            get { return _buttonContent; }
            set { Set(ref _buttonContent, value); }
        }

        public bool EnableControl
        {
            get { return _enableControl; }
            set { Set(ref _enableControl, value); }
        }

        public ObservableCollection<EmailEnvelopeItem> Emails { get; } =
            new ObservableCollection<EmailEnvelopeItem>();

        public EmailEnvelopeItem SelectedEmail
        {
            get { return _selectedEmail; }
            set { Set(ref _selectedEmail, value); }
        }

        public RelayCommand StartMailFetching { get; set; }
       

        private RelayCommand _showEmailBody;
        public RelayCommand ShowEmailBody
        {
            get
            {
                return _showEmailBody ?? (_showEmailBody = new RelayCommand(
                    () => Task.Run(DisplayEmailContent)
                    
                ));
            }
        }

        #endregion

        private void OnEmailReceived(EmailModel email)
        {
            App.Current.Dispatcher.Invoke((Action)delegate 
            {
                Emails.Add(new EmailEnvelopeItem()
                {
                    Id      = email.Id,
                    From    = email.Header.From[0],
                    Subject = email.Header.Subject,
                    Date    = email.Header.Date
                });

            });
           
        }


        private async Task DisplayEmailContent()
        {
            EmailModel model=null;
            List<EmailModel>allEmails= _emailManager.GetAllDownloadedEmails();
            if(allEmails != null && SelectedEmail!=null)
            {
                model=allEmails.FirstOrDefault(x => x.Id == SelectedEmail.Id);
            }
            
            if(model!=null)
            {
                if (model.Body != null)
                    BodyHtml = model.Body.Html;
                else
                {
                    var body= await _emailManager.DownloadEmailBody(SelectedEmail.Id);                    
                    BodyHtml = body;
                }
                    
            }
            
        }

        private void CancelEmailFetching()
        {
            stopToken.Cancel();
            try
            {
                _emailManager.CloseAllConnections();
            }
            catch
            {

            }
        }

        private async Task FetchRealTimeEmail()
        {
            try
            {
                while (true)
                {
                    var emails = await _emailManager.ReadRealTimeEmails();

                    if (emails != null)
                    {
                        foreach (var email in emails)
                        {
                            OnEmailReceived(email);
                        }
                    }
                }

            }
            catch (Exception)
            {
                _dialog.ShowMessage(ConstantValues.UnknowErrorDescription, ConstantValues.UnknowErrorTitle);
            }
        }

        private async Task FetchAllInboxEmail()
        {
            if (_isConnected)
            {                
                CancelEmailFetching();
                EnableControl = true;
                ButtonContent = ConstantValues.StartButtonText;
                _isConnected  = false;
                BodyHtml = ConstantValues.AboutBlank;
 
            }                
            else
            {
                _isConnected = true;
                ButtonContent = ConstantValues.StopButtonText;
                EnableControl = false;               
  
                try
                {
                    await _emailManager.Initialize(CurrentUser.Username, CurrentUser.Password, ServerName, PortNumber, SelectedServerType, SelectedEncryptionType);
                    int itterations=_emailManager.GetNumberOfIterations();
                    for (int index = 0,emailIndex=0; index < itterations; ++index)
                    {
                        var emails = await _emailManager.ReadAllMailsParallely();

                        if (emails != null && _isConnected)
                        {
                            while(emailIndex<emails.Count)
                            {
                                OnEmailReceived(emails[emailIndex]);
                                ++emailIndex;
                            }
                            
                        }
                    }
                    await _emailManager.KeepOnlyTwoConnectionAlive();
                }                
                catch (Limilabs.Client.IMAP.ImapResponseException)
                {
                    if(_isConnected)
                    {
                        _dialog.ShowMessage(ConstantValues.LoginFailedDescription, ConstantValues.LoginFailedTitle);
                        ButtonContent = ConstantValues.StartButtonText;
                        EnableControl = true;
                        _isConnected = false;
                        await _emailManager.CloseAllConnections();
                    }
                    
                }
                catch (Limilabs.Client.ServerException)
                {
                    if(_isConnected)
                    {
                        _dialog.ShowMessage(ConstantValues.ConnectionErrorDescription, ConstantValues.ConnectionErrorTitle);
                        ButtonContent = ConstantValues.StartButtonText;
                        EnableControl = true;
                        _isConnected = false;
                        await _emailManager.CloseAllConnections();
                    }
                    
                }               
                catch (Exception)
                {
                    if(_isConnected)
                    {
                        _dialog.ShowMessage(ConstantValues.UnknowErrorDescription, ConstantValues.UnknowErrorTitle);
                        ButtonContent = ConstantValues.StartButtonText;
                        EnableControl = true;
                        _isConnected = false;
                        await _emailManager.CloseAllConnections();
                    }
                    
                }
               
            }

        }

        private void FetchAllMail()
        {
            Emails.Clear();
            stopToken = new CancellationTokenSource();           
            
            Task mailFetcherTask= Task.Run(()=> FetchAllInboxEmail(), stopToken.Token);

            mailFetcherTask.ContinueWith(async (RealTimeMailChecking) => await FetchRealTimeEmail(),
                                                                               stopToken.Token);                                  
        }   

    }
}