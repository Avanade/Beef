using CoreEx.RefData;
using CoreEx.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using My.Hr.Business;
using My.Hr.Business.Data;
using My.Hr.Business.DataSvc;
using My.Hr.Business.Entities;
using My.Hr.Business.Validation;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace My.Hr.Test.Validators
{
    [TestFixture]
    public class EmployeeValidatorTest
    {
        private Action<IServiceCollection>? _testSetup;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var rd = new Mock<IReferenceDataData>();
            rd.Setup(x => x.GenderGetAllAsync(CancellationToken.None)).ReturnsAsync(new GenderCollection { new Gender { Id = Guid.NewGuid(), Code = "F" } });
            rd.Setup(x => x.USStateGetAllAsync(CancellationToken.None)).ReturnsAsync(new USStateCollection { new USState { Id = Guid.NewGuid(), Code = "WA" } });
            rd.Setup(x => x.RelationshipTypeGetAllAsync(CancellationToken.None)).ReturnsAsync(new RelationshipTypeCollection { new RelationshipType { Id = Guid.NewGuid(), Code = "FR" } });

            var eds = new Mock<IEmployeeDataSvc>();

            _testSetup = sc => sc
                .AddValidationTextProvider()
                .AddValidators<EmployeeValidator>()
                .AddJsonSerializer()
                .AddReferenceDataOrchestrator(sp => new ReferenceDataOrchestrator(sp).Register())
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
            GenderSid = "F",
            Birthday = DateTime.Now.AddYears(-20),
            StartDate = new DateTime(2010, 01, 01),
            PhoneNo = "(425) 333 4444",
        };

        [Test]
        public void A110_Validate_Initial()
        {
            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "First Name is required.",
                    "Email is required.",
                    "Last Name is required.",
                    "Gender is required.",
                    "Birthday is required.",
                    "Start Date is required.",
                    "Phone No is required.")
                .Run<IValidator<Employee>, Employee>(new Employee());
        }

        [Test]
        public void A120_Validate_BadData()
        {
            var e = new Employee
            {
                Email = "xxx",
                FirstName = 'x'.ToLongString(),
                LastName = 'x'.ToLongString(),
                GenderSid = "X",
                Birthday = DateTime.Now.AddYears(10),
                StartDate = new DateTime(1996, 12, 31),
                PhoneNo = "(425) 333 4444"
            };

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "Email is not a valid e-mail address.",
                    "First Name must not exceed 100 characters in length.",
                    "Last Name must not exceed 100 characters in length.",
                    "Gender is invalid.",
                    "Birthday is invalid as the Employee must be at least 18 years of age.",
                    "Start Date must be greater than or equal to January 1, 1999.")
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A130_Validate_Address_Empty()
        {
            var e = CreateValidEmployee();
            e.Address = new Address();

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "Street1 is required.",
                    "City is required.",
                    "State is required.",
                    "Post Code is required.")
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A140_Validate_Address_Invalid()
        {
            var e = CreateValidEmployee();
            e.Address = new Address
            {
                Street1 = "8365 Rode Road",
                City = "Redmond",
                StateSid = "FR",
                PostCode = "XXXXXXXXXX"
            };

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "State is invalid.",
                    "Post Code is invalid.")
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A150_Validate_Address_OK()
        {
            var e = CreateValidEmployee();
            e.Address = new Address
            {
                Street1 = "8365 Rode Road",
                City = "Redmond",
                StateSid = "WA",
                PostCode = "98052"
            };

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A160_Validate_Contacts_Empty()
        {
            var e = CreateValidEmployee();
            e.EmergencyContacts = new EmergencyContactCollection { new EmergencyContact() };

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "First Name is required.",
                    "Last Name is required.",
                    "Phone No is required.",
                    "Relationship is required.")
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A170_Validate_Contacts_Invalid()
        {
            var e = CreateValidEmployee();
            e.EmergencyContacts = new EmergencyContactCollection 
            { 
                new EmergencyContact
                {
                    FirstName = "Brian",
                    LastName = "Bellows",
                    PhoneNo = "425 333 4445",
                    RelationshipSid = "XX"
                }
            };

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors("Relationship is invalid.")
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A180_Validate_Contacts_TooMany()
        {
            var e = CreateValidEmployee();
            e.EmergencyContacts = new EmergencyContactCollection();

            for (int i = 0; i < 6; i++)
            {
                e.EmergencyContacts.Add(new EmergencyContact
                {
                    FirstName = "Brian",
                    LastName = "Bellows",
                    PhoneNo = "425 333 4445",
                    RelationshipSid = "FR"
                });
            }

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors("Emergency Contacts must not exceed 5 item(s).")
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void A190_Validate_UpdateTerminated()
        {
            var e = CreateValidEmployee();
            e.Id = 1.ToGuid();

            var eds = new Mock<IEmployeeDataSvc>();
            eds.Setup(x => x.GetAsync(1.ToGuid(), It.IsAny<CancellationToken>())).ReturnsAsync(new Employee { Termination = new TerminationDetail { Date = DateTime.UtcNow } });

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .MockScoped(eds)
                .ExpectException().Type<CoreEx.ValidationException>("Once an Employee has been Terminated the data can no longer be updated.")
                .OperationType(CoreEx.OperationType.Update)
                .Run<IValidator<Employee>, Employee>(e);
        }

        [Test]
        public void B110_CanDelete_NotFound()
        {
            var eds = new Mock<IEmployeeDataSvc>();
            eds.Setup(x => x.GetAsync(1.ToGuid(), CancellationToken.None)).ReturnsAsync((Employee)null!);

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .MockScoped(eds)
                .ExpectException().Type<CoreEx.NotFoundException>()
                .Run(async () => await EmployeeValidator.CanDelete.ValidateAsync(1.ToGuid()));
        }

        [Test]
        public void B110_CanDelete_Invalid()
        {
            var eds = new Mock<IEmployeeDataSvc>();
            eds.Setup(x => x.GetAsync(1.ToGuid(), CancellationToken.None)).ReturnsAsync(new Employee { StartDate = DateTime.UtcNow.AddDays(-1) });

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .MockScoped(eds)
                .ExpectException().Type<CoreEx.ValidationException>("An employee cannot be deleted after they have started their employment.")
                .Run(() => EmployeeValidator.CanDelete.ValidateAsync(1.ToGuid()).Result);
        }
    }
}