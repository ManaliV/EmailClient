using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommonPathAndData;

namespace DeveloperTest.ValidationRules
{
    class UnsignedShortNumberRule:ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var validationResult = new ValidationResult(true, null);

            if (value != null)
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    var regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
                    var parsingOk = !regex.IsMatch(value.ToString());
                    if (!parsingOk)
                    {
                        validationResult = new ValidationResult(false, ConstantValues.IllegalCharInNumber);
                    }
                    else
                    {
                        ushort result;
                        bool converted=ushort.TryParse(value.ToString(), out result);
                        if(!converted)
                            validationResult = new ValidationResult(false, ConstantValues.InvalidNumber);
                    }
                }
            }

            return validationResult;
        }
    }
}
