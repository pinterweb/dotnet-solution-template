namespace BusinessApp.Data
{
    using System;
    using BusinessApp.Domain;

    public class Metadata
    {
        protected Metadata()
        {}

        public Metadata(string dataSetName, MetadataId id, string username, MetadataType type)
        {
            Id = id.NotNull().Expect(nameof(id));
            Username = username.NotEmpty().Expect(nameof(username));
            DataSetName = dataSetName.NotEmpty().Expect(nameof(dataSetName));
            OccurredUtc = DateTimeOffset.UtcNow;
            TypeName = type.ToString();
        }

        public MetadataId Id { get; }
        public string Username { get; }
        public string DataSetName { get; }
        public string TypeName { get; }
        public DateTimeOffset OccurredUtc { get; }
    }

    public class Metadata<T> : Metadata
        where T : class
    {
        private Metadata()
        {}

        public Metadata(MetadataId id, string username, MetadataType type, T data)
            :base (data?.ToString(), id, username, type)
        {
            Data = data.NotDefault().Expect(nameof(data));
        }

        public T Data { get; }
    }
}
