namespace BusinessApp.App.UnitTest
{
    using BusinessApp.Domain;
    using Xunit;

    public class KeyComparerTests
    {
        public class EqualsImpl : KeyComparerTests
        {
            [Theory]
            [InlineData(1, 1, "foo", "foo", true)]
            [InlineData(1, 1, "foo", "FOO", true)]
            [InlineData(1, 1, "foo", "bar", false)]
            [InlineData(2, 1, "foo", "foo", false)]
            public void MultipleKeyIds_ChecksAllPropertyValues(int intA, int intB,
                string strA, string strB, bool expectEquals)
            {
                /* Arrange */
                var sut = new KeyComparer<KeyStub>();
                var x = new KeyStub { IntId = intA, StrId = strA, Foo = true };
                var y = new KeyStub { IntId = intB, StrId = strB, Foo = false };

                /* Act */
                var result = sut.Equals(x, y);

                /* Assert */
                Assert.Equal(expectEquals, result);
            }
        }

        private sealed class KeyStub
        {
            [KeyId]
            public int IntId { get; set; }

            [KeyId]
            public string StrId { get; set; }

            public bool Foo { get; set; }
        }
    }
}
