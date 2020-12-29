using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommonPathAndData;

namespace DeveloperTest.ValidationRules
{
    class RequiredFieldRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool status = value == null || string.IsNullOrEmpty(value.ToString().Trim()) || string.IsNullOrWhiteSpace(value.ToString());


            //return valid result if conversion succeeded
            return status ? new ValidationResult(false, ConstantValues.RequiredFieldMessage)
                          : ValidationResult.ValidResult;
        }
    }
}
