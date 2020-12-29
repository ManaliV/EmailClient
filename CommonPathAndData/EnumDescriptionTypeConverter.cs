using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CommonPathAndData
{
    internal class EnumDescriptionTypeConverter:EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type) { }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType == typeof(string) && value!=null)
            {
                FieldInfo valueFieldInfo = value.GetType().GetField(value.ToString());
                if(valueFieldInfo != null)
                {
                    var customAttributes= (DescriptionAttribute[])valueFieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    return (customAttributes.Length > 0 && !string.IsNullOrEmpty(customAttributes[0].Description))
                           ?customAttributes[0].Description
                           :value.ToString();
                }
                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        
    }
}
