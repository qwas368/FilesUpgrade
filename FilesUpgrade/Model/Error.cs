
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesUpgrade.Model
{
    public struct Error
    {
        public readonly string Message;
        public readonly Option<Exception> Exception;

        Error(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public static Error FromString(string message) =>
            new Error(message, null);

        public static Error FromException(Exception ex) =>
            new Error(ex.Message, ex);
    }
}
