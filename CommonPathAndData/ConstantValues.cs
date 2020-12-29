using System;

namespace CommonPathAndData
{
    public static class ConstantValues
    {
        public const int MaxConnection                  = 5;
        public const int MaxNoOfSimultanousMailRead     = 5;
        public const int MaxReadOrWriteThread           = 2;

        public const ushort NonEncryptedIMAPPort = 143;
        public const ushort EncryptedIMAPPort    = 993;
        public const ushort NonEncryptedPOP3Port = 110;
        public const ushort EncryptedPO3Port     = 995;

        public const string IMAPGmailServer      = "imap.gmail.com";
        public const string POPGmailServer       = "pop.gmail.com";

        public const string RequiredFieldMessage = "Required Field";
        public const string InvalidNumber        = "Enter Number in Range(0-65535)";
        public const string IllegalCharInNumber  =  "Illegal Characters, Please Enter Numeric Value";

        public const string UnknowErrorTitle       = "Unknown error";
        public const string UnknowErrorDescription = "Error! Check your credentials.Please restart process";

        public const string ConnectionErrorTitle       = "Connection error";
        public const string ConnectionErrorDescription = "Error while connecting with Server.";

        public const string LoginFailedTitle           = "Login Failed";
        public const string LoginFailedDescription     = "Error!Please check your credetials";

        public const string StopButtonText         = "Stop";
        public const string StartButtonText        = "Start";

        public const string AboutBlank             = "<!DOCTYPE html><html >< body ></body></html>";


    }
}
