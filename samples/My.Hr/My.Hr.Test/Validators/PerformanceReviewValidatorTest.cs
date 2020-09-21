using Beef.Test.NUnit;
using Moq;
using My.Hr.Business;
using My.Hr.Business.Validation;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using NUnit.Framework;
using System;

namespace My.Hr.Test.Validators
{
    [TestFixture]
    public class PerformanceReviewValidatorTest
    {
        private readonly Mock<IReferenceDataAgent> _referenceData = new Mock<IReferenceDataAgent>();
        private readonly Mock<IEmployeeManager> _employeeManager = new Mock<IEmployeeManager>();
        private readonly Mock<IPerformanceReviewManager> _perfReviewManager = new Mock<IPerformanceReviewManager>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _referenceData.Setup(x => x.PerformanceOutcomeGetAllAsync(null, null)).ReturnsWebApiAgentResultAsync(new PerformanceOutcomeCollection { new PerformanceOutcome { Id = Guid.NewGuid(), Code = "ME" } });

            _employeeManager.Setup(x => x.GetAsync(404.ToGuid())).ReturnsAsync((Employee)null!);
            _employeeManager.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new Employee { Id = 1.ToGuid(), StartDate = DateTime.Now.AddYears(-1) });
            _employeeManager.Setup(x => x.GetAsync(2.ToGuid())).ReturnsAsync(new Employee { Id = 2.ToGuid(), StartDate = DateTime.Now.AddYears(-1), Termination = new TerminationDetail { Date = DateTime.Now.AddMonths(-1) } });

            _perfReviewManager.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new PerformanceReview { Id = 1.ToGuid(), EmployeeId = 2.ToGuid() });
        }

        [Test]
        public void A110_Validate_Initial()
        {
            ValidationTester.Test()
                .ExpectMessages(
                    "Employee is required.",
                    "Date is required.",
                    "Outcome is required.",
                    "Reviewer is required.")
                .Run(() => PerformanceReviewValidator.Default.Validate(new PerformanceReview()));
        }

        [Test]
        public void A120_Validate_BadData()
        {
            var pr = new PerformanceReview
            {
                EmployeeId = 404.ToGuid(),
                Date = DateTime.Now.AddDays(1),
                OutcomeSid = "XX",
                Reviewer = new string('X', 5000),
                Notes = new string('X', 5000)
            };

            ValidationTester.Test()
                .AddScopedService(_referenceData)
                .AddScopedService(_employeeManager)
                .ExpectMessages(
                    "Date must be less than or equal to today.",
                    "Outcome is invalid.",
                    "Employee is not found; a valid value is required.",
                    "Reviewer must not exceed 256 characters in length.",
                    "Notes must not exceed 4000 characters in length.")
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }

        [Test]
        public void A130_Validate_BeforeStarting()
        {
            var pr = new PerformanceReview
            {
                EmployeeId = 1.ToGuid(),
                Date = DateTime.Now.AddYears(-2),
                OutcomeSid = "ME",
                Reviewer = "test@org.com",
                Notes = "Thumbs up!"
            };

            ValidationTester.Test()
                .AddScopedService(_referenceData)
                .AddScopedService(_employeeManager)
                .ExpectMessages("Date must not be prior to the Employee starting.")
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }

        [Test]
        public void A140_Validate_AfterTermination()
        {
            var pr = new PerformanceReview
            {
                EmployeeId = 2.ToGuid(),
                Date = DateTime.Now,
                OutcomeSid = "ME",
                Reviewer = "test@org.com",
                Notes = "Thumbs up!"
            };

            ValidationTester.Test()
                .AddScopedService(_referenceData)
                .AddScopedService(_employeeManager)
                .ExpectMessages("Date must not be after the Employee has terminated.")
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }

        [Test]
        public void A150_Validate_EmployeeNotFound()
        {
            var pr = new PerformanceReview
            {
                Id = 404.ToGuid(),
                EmployeeId = 2.ToGuid(),
                Date = DateTime.Now.AddMonths(-3),
                OutcomeSid = "ME",
                Reviewer = "test@org.com",
                Notes = "Thumbs up!"
            };

            // Need to set the OperationType to Update to exercise logic.
            ValidationTester.Test()
                .OperationType(Beef.OperationType.Update)
                .AddScopedService(_referenceData)
                .AddScopedService(_perfReviewManager)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }

        [Test]
        public void A160_Validate_EmployeeImmutable()
        {
            var pr = new PerformanceReview
            {
                Id = 1.ToGuid(),
                EmployeeId = 1.ToGuid(),
                Date = DateTime.Now.AddMonths(-3),
                OutcomeSid = "ME",
                Reviewer = "test@org.com",
                Notes = "Thumbs up!"
            };

            // Need to set the OperationType to Update to exercise logic.
            ValidationTester.Test()
                .OperationType(Beef.OperationType.Update)
                .AddScopedService(_referenceData)
                .AddScopedService(_perfReviewManager)
                .ExpectMessages("Employee is not allowed to change; please reset value.")
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }

        [Test]
        public void A170_Validate_CreateOK()
        {
            var pr = new PerformanceReview
            {
                EmployeeId = 2.ToGuid(),
                Date = DateTime.Now.AddMonths(-3),
                OutcomeSid = "ME",
                Reviewer = "test@org.com",
                Notes = "Thumbs up!"
            };

            // Need to set the OperationType to Create to exercise logic.
            ValidationTester.Test()
                .OperationType(Beef.OperationType.Create)
                .AddScopedService(_referenceData)
                .AddScopedService(_employeeManager)
                .AddScopedService(_perfReviewManager)
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }

        [Test]
        public void A180_Validate_UpdateOK()
        {
            var pr = new PerformanceReview
            {
                Id = 1.ToGuid(),
                EmployeeId = 2.ToGuid(),
                Date = DateTime.Now.AddMonths(-3),
                OutcomeSid = "ME",
                Reviewer = "test@org.com",
                Notes = "Thumbs up!"
            };

            // Need to set the OperationType to Update to exercise logic.
            ValidationTester.Test()
                .OperationType(Beef.OperationType.Update)
                .AddScopedService(_referenceData)
                .AddScopedService(_employeeManager)
                .AddScopedService(_perfReviewManager)
                .Run(() => PerformanceReviewValidator.Default.Validate(pr));
        }
    }
}