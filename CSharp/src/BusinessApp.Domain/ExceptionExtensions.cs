namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;

    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> InnerExceptions(this Exception e)
        {
            while (e?.InnerException != null) yield return e = e.InnerException;
        }
    }
}
