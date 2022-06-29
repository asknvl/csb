using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.settings.validators
{
    public interface IValidator
    {
        string Message { get; }
        bool IsValid(string value);
    }
}
