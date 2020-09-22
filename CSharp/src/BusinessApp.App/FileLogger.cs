namespace BusinessApp.App
{
    using System.Diagnostics;
    using System.IO;
    using BusinessApp.Domain;

    /// <summary>
    /// ILogger implementation that writes to a file
    /// </summary>
    public class FileLogger : ILogger
    {
        private static readonly object syncObject = new object();
        private readonly IFileProperties file;
        private readonly ILogEntryFormatter formatter;

        public FileLogger(IFileProperties file, ILogEntryFormatter formatter)
        {
            this.file = Guard.Against.Null(file).Expect(nameof(file));
            this.formatter = Guard.Against.Null(formatter).Expect(nameof(formatter));
        }

        public void Log(LogEntry entry)
        {
            int retries = 4;

            lock (syncObject)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file.Name));

                for (int retry = 0; retry < retries; retry++)
                {
                    try
                    {
                        using (var fs = new FileStream($"{file.Name}",
                            FileMode.Append,
                            FileAccess.Write,
                            FileShare.Read))
                        {
                            using (var sw = new StreamWriter(fs))
                            {
                                sw.Write(formatter.Format(entry));
                                sw.Flush();
                            }
                        }

                        break;
                    }
                    catch (IOException)
                    {
                        if (retry == 3)
                        {
                            Trace.WriteLine(formatter.Format(entry));
                        }
                    }
                }
            }
        }
    }
}
