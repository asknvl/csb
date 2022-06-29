using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace csb.settings.validators
{
    public class PhoneNumberValidator : IValidator
    {
        public string Message => "Неверный формат номера телефона";

        public bool IsValid(string value)
        {
            Regex regex = new Regex(@"^\+\d{1,3}\d{7}");
            return regex.IsMatch(value);
        }
    }
}
