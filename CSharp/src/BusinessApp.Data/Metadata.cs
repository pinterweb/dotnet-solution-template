namespace BusinessApp.Data
{
    using System;
    using BusinessApp.Domain;

    public class Metadata
    {
#nullable disable
        protected Metadata()
        {}
#nullable restore

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
#nullable disable
        private Metadata()
        {}
#nullable restore

        public Metadata(MetadataId id, string username, MetadataType type, T data)
            :base (
                data.Expect(nameof(data))
                    .ToString()
                    .Expect("data ToString() must return a value for the DataSetName"),
                id, username, type)
        {
            Data = data.NotDefault().Expect(nameof(data))!;
        }

        public T Data { get; }
    }
}
