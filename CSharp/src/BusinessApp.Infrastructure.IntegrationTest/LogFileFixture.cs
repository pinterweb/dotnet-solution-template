using System;
using System.IO;

namespace BusinessApp.Infrastructure.IntegrationTest
{
    public class LogFileFixture : IDisposable
    {
        public string FilePath { get; set; } = "./log-file-test.log";

        public void Dispose()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
