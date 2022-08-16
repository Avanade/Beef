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
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace My.Hr.Test.Validators
{
    [TestFixture]
    public class PerformanceReviewValidatorTest
    {
        private Action<IServiceCollection>? _testSetup;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var rd = new Mock<IReferenceDataData>();
            rd.Setup(x => x.PerformanceOutcomeGetAllAsync()).ReturnsAsync(new PerformanceOutcomeCollection { new PerformanceOutcome { Id = Guid.NewGuid(), Code = "ME" } });

            var em = new Mock<IEmployeeManager>();
            em.Setup(x => x.GetAsync(404.ToGuid())).ReturnsAsync((Employee)null!);
            em.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new Employee { Id = 1.ToGuid(), StartDate = DateTime.Now.AddYears(-1) });
            em.Setup(x => x.GetAsync(2.ToGuid())).ReturnsAsync(new Employee { Id = 2.ToGuid(), StartDate = DateTime.Now.AddYears(-1), Termination = new TerminationDetail { Date = DateTime.Now.AddMonths(-1) } });

            var prm = new Mock<IPerformanceReviewManager>();
            prm.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new PerformanceReview { Id = 1.ToGuid(), EmployeeId = 2.ToGuid() });

            _testSetup = sc => sc
                .AddValidationTextProvider()
                .AddValidators<PerformanceReviewValidator>()
                .AddJsonSerializer()
                .AddReferenceDataOrchestrator(sp => new ReferenceDataOrchestrator(sp).Register())
                .AddGeneratedReferenceDataManagerServices()
                .AddGeneratedReferenceDataDataSvcServices()
                .AddScoped(_ => rd.Object)
                .AddScoped(_ => em.Object)
                .AddScoped(_ => prm.Object);
        }

        [Test]
        public void A110_Validate_Initial()
        {
            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "Employee is required.",
                    "Date is required.",
                    "Outcome is required.",
                    "Reviewer is required.")
                .Run<IValidator<PerformanceReview>, PerformanceReview>(new PerformanceReview());
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

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors(
                    "Date must be less than or equal to today.",
                    "Outcome is invalid.",
                    "Employee is not found; a valid value is required.",
                    "Reviewer must not exceed 256 characters in length.",
                    "Notes must not exceed 4000 characters in length.")
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
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

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors("Date must not be prior to the Employee starting.")
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
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

            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .ExpectErrors("Date must not be after the Employee has terminated.")
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
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
            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .OperationType(CoreEx.OperationType.Update)
                .ExpectErrorType(CoreEx.Abstractions.ErrorType.NotFoundError)
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
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
            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .OperationType(CoreEx.OperationType.Update)
                .ExpectErrors("Employee is not allowed to change; please reset value.")
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
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
            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .OperationType(CoreEx.OperationType.Create)
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
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
            using var test = ValidationTester.Create();

            test.ConfigureServices(_testSetup!)
                .OperationType(CoreEx.OperationType.Update)
                .Run<IValidator<PerformanceReview>, PerformanceReview>(pr);
        }
    }
}