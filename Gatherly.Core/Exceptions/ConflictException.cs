namespace Gatherly.Core.Exceptions
{
    /// <summary>
    /// The request conflicts with the current state of the data
    /// (a business rule conflict or an optimistic concurrency
    /// conflict). The API returns 409 Conflict.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
