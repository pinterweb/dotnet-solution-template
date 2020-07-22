namespace BusinessApp.App.IntegrationTest
{
    using System;
    using System.IO;

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
