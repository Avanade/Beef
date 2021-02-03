using Beef.Test.NUnit;
using Beef.Validation;
using Company.AppName.Business;
using Company.AppName.Business.Validation;
using Company.AppName.Common.Agents;
using Company.AppName.Common.Entities;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Company.AppName.Test.Validators
{
    [TestFixture]
    public class PersonValidatorTest
    {
        private readonly Mock<IReferenceDataAgent> _referenceData = new Mock<IReferenceDataAgent>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _referenceData.Setup(x => x.GenderGetAllAsync(null, null)).ReturnsWebApiAgentResultAsync(new GenderCollection { new Gender { Id = Guid.NewGuid(), Code = "F" } });
        }

        [Test, TestSetUp]
        public async Task A110_Validation_Empty()
        {
            await ValidationTester.Test()
                .ConfigureServices(ServiceCollectionsValidationExtension.AddGeneratedValidationServices)
                .ExpectMessages(
                    "First Name is required.",
                    "Last Name is required.",
                    "Gender is required.",
                    "Birthday is required.")
                .CreateAndRunAsync<IValidator<Person>, Person>(new Person());
        }

        [Test, TestSetUp]
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
                .ConfigureServices(ServiceCollectionsValidationExtension.AddGeneratedValidationServices)
                .AddScopedService(_referenceData)
                .ExpectMessages(
                    "First Name must not exceed 100 characters in length.",
                    "Last Name must not exceed 100 characters in length.",
                    "Gender is invalid.",
                    "Birthday must be less than or equal to Today.")
                .CreateAndRunAsync<IValidator<Person>, Person>(p);
        }

        [Test, TestSetUp]
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
                .ConfigureServices(ServiceCollectionsValidationExtension.AddGeneratedValidationServices)
                .AddScopedService(_referenceData)
                .CreateAndRunAsync<IValidator<Person>, Person>(p);
        }
    }
}