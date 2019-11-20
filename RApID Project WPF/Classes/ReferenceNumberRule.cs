using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RApID_Project_WPF.Classes
{
    public class ReferenceNumberRule : ValidationRule
    {
        public ReferenceNumberRule() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value.ToString();

            if (string.IsNullOrWhiteSpace(input)) return ValidationResult.ValidResult;

            Regex pattern = new Regex("^[A-Z]{1}[0-9]{1,4}$", RegexOptions.Compiled);

            if (pattern.IsMatch(input))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, $"Please enter a captial letter [A-Z] then up to 4 digits (0-9).");
        }
    }
}
