using System;

namespace DsmSuite.DsmViewer.Model.Exceptions
{
    public class DsmException : ApplicationException
    {
        public DsmException()
        {
        }
        public DsmException(string message)
            : base(message)
        {
        }

        public DsmException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
