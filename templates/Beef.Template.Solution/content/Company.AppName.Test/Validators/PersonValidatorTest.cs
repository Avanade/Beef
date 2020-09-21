using Beef.Test.NUnit;
using Company.AppName.Business.Validation;
using Company.AppName.Common.Agents;
using Company.AppName.Common.Entities;
using Moq;
using NUnit.Framework;
using System;

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
        public void A110_Validation_Empty()
        {
            ValidationTester.Test()
                .ExpectMessages(
                    "First Name is required.",
                    "Last Name is required.",
                    "Gender is required.",
                    "Birthday is required.")
                .Run(() => PersonValidator.Default.Validate(new Person()));
        }

        [Test, TestSetUp]
        public void A120_Validation_Invalid()
        {
            var p = new Person
            {
                FirstName = 'x'.ToLongString(),
                LastName = 'x'.ToLongString(),
                GenderSid = "X",
                Birthday = DateTime.UtcNow.AddDays(1)
            };

            ValidationTester.Test()
                .AddScopedService(_referenceData)
                .ExpectMessages(
                    "First Name must not exceed 100 characters in length.",
                    "Last Name must not exceed 100 characters in length.",
                    "Gender is invalid.",
                    "Birthday must be less than or equal to Today.")
                .Run(() => PersonValidator.Default.Validate(p));
        }

        [Test, TestSetUp]
        public void A130_Validation_OK()
        {
            var p = new Person
            {
                FirstName = "Sam",
                LastName = "Reilly",
                GenderSid = "F",
                Birthday = DateTime.UtcNow.AddYears(-18)
            };

            ValidationTester.Test()
                .AddScopedService(_referenceData)
                .Run(() => PersonValidator.Default.Validate(p));
        }
    }
}