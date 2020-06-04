namespace BusinessApp.App.UnitTest
{
    using System.ComponentModel.DataAnnotations;

    public class DummyCommand { }

    public class DataAnnotatedCommandStub
    {
        [StringLength(10)]
        public string Foo { get; set; }

        [Required]
        public string Bar { get; set; } = "lorem";
    }

}
