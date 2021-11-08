using System;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Exception to throw when data partially succeeded and was not rolled back
    /// </summary>
    public class PartialException : BusinessAppException
    {
        public PartialException(Exception e, object data)
            : base(e.Message, e)
        {
            Data.Add("Data", data);
        }
    }
}
