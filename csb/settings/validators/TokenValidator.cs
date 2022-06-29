using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace csb.settings.validators
{
    class TokenValidator : IValidator
    {
        public string Message => "Неверный формат токена бота";

        public bool IsValid(string value)
        {
            Regex regex = new Regex(@"[0-9]{9}:[a-zA-Z0-9_-]{35}");
            return regex.IsMatch(value);
        }
    }
}
