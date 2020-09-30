namespace BusinessApp.Domain
{
    using System;

    public static class ResultExtensions
    {
        /// <summary>
        /// Helper method to remove the generic okay value. Useful if the error is all
        /// that matters
        /// </summary>
        public static Result<_, E> IgnoreValue<T, E>(this Result<T, E> result)
            where E : IFormattable
        {
            return result.MapOrElse(
                err => Result<_, E>.Error(err),
                ok => Result<_, E>.Ok(new _())
            );
        }
    }
}
