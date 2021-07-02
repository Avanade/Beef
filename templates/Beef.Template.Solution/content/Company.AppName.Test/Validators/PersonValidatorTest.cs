using Beef.Test.NUnit;
using Beef.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Company.AppName.Business;
using Company.AppName.Business.Data;
using Company.AppName.Business.DataSvc;
using Company.AppName.Business.Entities;
using Company.AppName.Business.Validation;

namespace Company.AppName.Test.Validators
{
    [TestFixture]
    public class PersonValidatorTest
    {
        private Func<IServiceCollection, IServiceCollection>? _testSetup;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var rd = new Mock<IReferenceDataData>();
            rd.Setup(x => x.GenderGetAllAsync()).ReturnsAsync(new GenderCollection { new Gender { Id = Guid.NewGuid(), Code = "F" } });

            _testSetup = sc => sc
                .AddGeneratedValidationServices()
                .AddGeneratedReferenceDataManagerServices()
                .AddGeneratedReferenceDataDataSvcServices()
                .AddScoped(_ => rd.Object);
        }

        [Test]
        public async Task A110_Validation_Empty()
        {
            await ValidationTester.Test()
                .ConfigureServices(_testSetup!)
                .ExpectMessages(
                    "First Name is required.",
                    "Last Name is required.",
                    "Gender is required.",
                    "Birthday is required.")
                .CreateAndRunAsync<IValidator<Person>, Person>(new Person());
        }

        [Test]
        public async Task A120_Validation_Invalid()
        {
            var p = new Person
            {
                FirstName = 'x'.ToLongString(),
                LastName = 'x'.ToLongString(),
                GenderSid = "X",
                Birthday = DateTime.UtcNow.AddDays(1)
            };

            await ValidationTester.Test()
                .ConfigureServices(_testSetup!)
                .ExpectMessages(
                    "First Name must not exceed 100 characters in length.",
                    "Last Name must not exceed 100 characters in length.",
                    "Gender is invalid.",
                    "Birthday must be less than or equal to Today.")
                .CreateAndRunAsync<IValidator<Person>, Person>(p);
        }

        [Test]
        public async Task A130_Validation_OK()
        {
            var p = new Person
            {
                FirstName = "Sam",
                LastName = "Reilly",
                GenderSid = "F",
                Birthday = DateTime.UtcNow.AddYears(-18)
            };

            await ValidationTester.Test()
                .ConfigureServices(_testSetup!)
                .CreateAndRunAsync<IValidator<Person>, Person>(p);
        }
    }
}