namespace BusinessApp.Data
{
    using System;
    using BusinessApp.Domain;

    public class RequestMetadata<T>
    {
        public RequestMetadata(T request, string username)
        {
            Request = request.NotDefault().Expect(nameof(request));
            Username = username.NotEmpty().Expect(nameof(username));
        }

        public T Request { get; }
        public string Username { get; }
        public DateTimeOffset OccurredUtc { get; } = DateTimeOffset.UtcNow;
    }
}
