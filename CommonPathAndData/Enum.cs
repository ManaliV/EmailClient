using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CommonPathAndData
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ServerType
    {
        [Description("IMAP")]
        IMAP,
        [Description("POP3")]
        POP3
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EncryptionType
    {
        [Description("Unencrypted")]
        Unencrypted,
        [Description("SSL/TLS")]
        SSLTLS,
        [Description("STARTTLS")]
        StartTLS
    }
}
