using Company.AppName.Business.Entities;

namespace Company.AppName.Test.Validators;

[TestFixture]
public class PersonValidatorTest
{
    private Action<IServiceCollection>? _testSetup;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var rd = new Mock<IReferenceDataData>();
#if (!implement_mysql)
        rd.Setup(x => x.GenderGetAllAsync()).ReturnsAsync(new GenderCollection { new Gender { Id = Guid.NewGuid(), Code = "F" } });
#endif
#if (implement_mysql)
        rd.Setup(x => x.GenderGetAllAsync()).ReturnsAsync(new GenderCollection { new Gender { Id = 1, Code = "F" } });
#endif

        _testSetup = sc => sc
            .AddValidationTextProvider()
            .AddValidators<PersonValidator>()
            .AddJsonSerializer()
            .AddReferenceDataOrchestrator(sp => new ReferenceDataOrchestrator(sp).Register())
            .AddGeneratedReferenceDataManagerServices()
            .AddGeneratedReferenceDataDataSvcServices()
            .AddScoped(_ => rd.Object);
    }

    [Test]
    public async Task A110_Validation_Empty()
    {
        using var test = GenericTester.Create();

        await test
            .ConfigureServices(_testSetup!)
            .ExpectErrors(
                "First Name is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.")
            .Validation().WithAsync<PersonValidator, Person>(new Person());
    }

    [Test]
    public async Task A120_Validation_Invalid()
    {
        var p = new Person
        {
            FirstName = 'x'.ToLongString(),
            LastName = 'x'.ToLongString(),
            Gender = "X",
            Birthday = DateTime.UtcNow.AddDays(1)
        };

        using var test = GenericTester.Create();

        await test
            .ConfigureServices(_testSetup!)
            .ExpectErrors(
                "First Name must not exceed 100 characters in length.",
                "Last Name must not exceed 100 characters in length.",
                "Gender is invalid.",
                "Birthday must be less than or equal to Today.")
            .Validation().WithAsync<PersonValidator, Person>(p);
    }

    [Test]
    public async Task A130_Validation_OK()
    {
        var p = new Person
        {
            FirstName = "Sam",
            LastName = "Reilly",
            Gender = "F",
            Birthday = DateTime.UtcNow.AddYears(-18)
        };

        using var test = GenericTester.Create();

        await test
            .ConfigureServices(_testSetup!)
            .ExpectSuccess()
            .Validation().WithAsync<PersonValidator, Person>(p);
    }
}