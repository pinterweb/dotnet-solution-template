namespace BusinessApp.App.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;

    public class FileLoggerTests : IClassFixture<LogFileFixture>
    {
        private readonly FileLogger sut;
        private readonly LogFileFixture fixture;
        private readonly ILogEntryFormatter formatter;
        private readonly IFileProperties file;

        public FileLoggerTests(LogFileFixture fixture = null)
        {
            this.fixture = fixture;
            formatter = A.Fake<ILogEntryFormatter>();
            file = A.Fake<IFileProperties>();

            sut = new FileLogger(file, formatter);
        }

        public class Constructor : FileLoggerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<ILogEntryFormatter>() },
                new object[] { A.Dummy<IFileProperties>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IFileProperties p, ILogEntryFormatter f)
            {
                /* Arrange */
                void shouldThrow() => new FileLogger(p, f);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Log : FileLoggerTests, IDisposable, IClassFixture<LogFileFixture>
        {
            private string childDirectory;

            public Log(LogFileFixture fixture)
                : base(fixture)
            {
                childDirectory = "./testdata";
            }

            [Fact]
            public void Log_NewDirectory_CreatesIt()
            {
                /* Arrange */
                fixture.FilePath = $"{childDirectory}/log-file-test.log";
                A.CallTo(() => file.Name).Returns(fixture.FilePath);

                /* Act */
                var ex = Record.Exception(() => sut.Log(A.Dummy<LogEntry>()));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public void Log_WithLogEntry_LogsMessage()
            {
                /* Arrange */
                var entry = A.Dummy<LogEntry>();
                A.CallTo(() => formatter.Format(entry)).Returns("foobar");
                A.CallTo(() => file.Name).Returns(fixture.FilePath);

                /* Act */
                sut.Log(entry);

                /* Assert */
                Assert.Equal(
                    "foobar",
                    File.ReadAllText(fixture.FilePath)
                );
            }

            public void Dispose()
            {
                if (Directory.Exists(childDirectory))
                {
                    Directory.Delete(childDirectory, true);
                }
            }
        }
    }
}
