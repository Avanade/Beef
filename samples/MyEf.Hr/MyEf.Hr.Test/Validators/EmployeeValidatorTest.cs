using MyEf.Hr.Business.Entities;

namespace MyEf.Hr.Test.Validators;

[TestFixture]
public class EmployeeValidatorTest
{
    private Action<IServiceCollection>? _testSetup;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var rd = new Mock<IReferenceDataData>();
        rd.Setup(x => x.GenderGetAllAsync()).ReturnsAsync(new GenderCollection { new Gender { Id = Guid.NewGuid(), Code = "F" } });
        rd.Setup(x => x.USStateGetAllAsync()).ReturnsAsync(new USStateCollection { new USState { Id = Guid.NewGuid(), Code = "WA" } });
        rd.Setup(x => x.RelationshipTypeGetAllAsync()).ReturnsAsync(new RelationshipTypeCollection { new RelationshipType { Id = Guid.NewGuid(), Code = "FR" } });

        var eds = new Mock<IEmployeeDataSvc>();

        _testSetup = sc => sc
            .AddValidationTextProvider()
            .AddValidators<EmployeeValidator>()
            .AddJsonSerializer()
            .AddReferenceDataOrchestrator()
            .AddGeneratedReferenceDataManagerServices()
            .AddGeneratedReferenceDataDataSvcServices()
            .AddScoped(_ => rd.Object)
            .AddScoped(_ => eds.Object);
    }

    private static Employee CreateValidEmployee() => new()
    {
        Email = "sarah.smith@org.com",
        FirstName = "Sarah",
        LastName = "Smith",
        Gender = "F",
        Birthday = DateTime.Now.AddYears(-20),
        StartDate = new DateTime(2010, 01, 01),
        PhoneNo = "(425) 333 4444",
    };

    [Test]
    public void A110_Validate_Initial()
    {
        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors(
                "First Name is required.",
                "Email is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.",
                "Start Date is required.",
                "Phone No is required.")
            .Validation().With<EmployeeValidator, Employee>(new Employee());
    }

    [Test]
    public void A120_Validate_BadData()
    {
        var e = new Employee
        {
            Email = "xxx",
            FirstName = 'x'.ToLongString(),
            LastName = 'x'.ToLongString(),
            Gender = "X",
            Birthday = DateTime.Now.AddYears(-17),
            StartDate = new DateTime(1996, 12, 31),
            PhoneNo = "(425) 333 4444"
        };

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors(
                "Email is not a valid e-mail address.",
                "First Name must not exceed 100 characters in length.",
                "Last Name must not exceed 100 characters in length.",
                "Gender is invalid.",
                "Birthday is invalid as the Employee must be at least 18 years of age.",
                "Start Date must be greater than or equal to January 1, 1999.")
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A130_Validate_Address_Empty()
    {
        var e = CreateValidEmployee();
        e.Address = new Address();

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors(
                "Street1 is required.",
                "City is required.",
                "State is required.",
                "Post Code is required.")
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A140_Validate_Address_Invalid()
    {
        var e = CreateValidEmployee();
        e.Address = new Address
        {
            Street1 = "8365 Rode Road",
            City = "Redmond",
            State = "FR",
            PostCode = "XXXXXXXXXX"
        };

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors(
                "State is invalid.",
                "Post Code is invalid.")
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A150_Validate_Address_OK()
    {
        var e = CreateValidEmployee();
        e.Address = new Address
        {
            Street1 = "8365 Rode Road",
            City = "Redmond",
            State = "WA",
            PostCode = "98052"
        };

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectSuccess()
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A160_Validate_Contacts_Empty()
    {
        var e = CreateValidEmployee();
        e.EmergencyContacts = [new EmergencyContact()];

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors(
                "First Name is required.",
                "Last Name is required.",
                "Phone No is required.",
                "Relationship is required.")
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A170_Validate_Contacts_Invalid()
    {
        var e = CreateValidEmployee();
        e.EmergencyContacts =
        [
            new EmergencyContact
            {
                FirstName = "Brian",
                LastName = "Bellows",
                PhoneNo = "425 333 4445",
                Relationship = "XX"
            }
        ];

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors("Relationship is invalid.")
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A180_Validate_Contacts_TooMany()
    {
        var e = CreateValidEmployee();
        e.EmergencyContacts = [];

        for (int i = 0; i < 6; i++)
        {
            e.EmergencyContacts.Add(new EmergencyContact
            {
                FirstName = "Brian",
                LastName = "Bellows",
                PhoneNo = "425 333 4445",
                Relationship = "FR"
            });
        }

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .ExpectErrors("Emergency Contacts must not exceed 5 item(s).")
            .Validation().With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void A190_Validate_UpdateTerminated()
    {
        var e = CreateValidEmployee();
        e.Id = 1.ToGuid();

        var eds = new Mock<IEmployeeDataSvc>();
        eds.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new Employee { Termination = new TerminationDetail { Date = DateTime.UtcNow } });

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .MockScoped(eds)
            .ExpectException().Type<ValidationException>("Once an Employee has been Terminated the data can no longer be updated.")
            .Validation(OperationType.Update).With<EmployeeValidator, Employee>(e);
    }

    [Test]
    public void B110_CanDelete_NotFound()
    {
        var eds = new Mock<IEmployeeDataSvc>();
        eds.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync((Employee?)null!);

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .MockScoped(eds)
            .ExpectException().Type<NotFoundException>()
            .Validation().With(async () => await EmployeeValidator.CanDelete.ValidateAsync(1.ToGuid()));
    }

    [Test]
    public void B110_CanDelete_Invalid()
    {
        var eds = new Mock<IEmployeeDataSvc>();
        eds.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new Employee { StartDate = DateTime.UtcNow.AddDays(-1) });

        using var test = GenericTester.Create();

        test.ConfigureServices(_testSetup!)
            .MockScoped(eds)
            .ExpectException().Type<ValidationException>("An employee cannot be deleted after they have started their employment.")
            .Validation().With(() => EmployeeValidator.CanDelete.ValidateAsync(1.ToGuid()).Result);
    }
}