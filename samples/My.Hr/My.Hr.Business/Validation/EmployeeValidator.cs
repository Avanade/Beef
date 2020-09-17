using Beef;
using Beef.Validation;
using Beef.Validation.Rules;
using My.Hr.Business.DataSvc;
using My.Hr.Common.Entities;
using System;
using System.Text.RegularExpressions;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Employee"/> validator.
    /// </summary>
    public class EmployeeValidator : Validator<Employee, EmployeeValidator>
    {
        // Address validator implemented using fluent-style method-chaining.
        private static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
            .HasProperty(x => x.Street1, p => p.Mandatory().Common(CommonValidators.Street))
            .HasProperty(x => x.Street2, p => p.Common(CommonValidators.Street))
            .HasProperty(x => x.City, p => p.Mandatory().String(50))
            .HasProperty(x => x.State, p => p.Mandatory().IsValid())
            .HasProperty(x => x.PostCode, p => p.Mandatory().String(new Regex(@"^\d{5}(?:[-\s]\d{4})?$")));

        // Emergency Contact validator implemented using fluent-style method-chaining.
        public static readonly Validator<EmergencyContact> _emergencyContactValidator = Validator.Create<EmergencyContact>()
            .HasProperty(x => x.FirstName, p => p.Mandatory().Common(CommonValidators.PersonName))
            .HasProperty(x => x.LastName, p => p.Mandatory().Common(CommonValidators.PersonName))
            .HasProperty(x => x.PhoneNo, p => p.Mandatory().Common(CommonValidators.PhoneNo))
            .HasProperty(x => x.Relationship, p => p.Mandatory().IsValid());

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeValidator"/> class.
        /// </summary>
        public EmployeeValidator()
        {
            Property(x => x.Email).Mandatory().Common(CommonValidators.Email);
            Property(x => x.FirstName).Mandatory().Common(CommonValidators.PersonName);
            Property(x => x.LastName).Mandatory().Common(CommonValidators.PersonName);
            Property(x => x.Gender).Mandatory().IsValid();
            Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => DateTime.UtcNow.AddYears(-18), errorText: "Birthday is invalid as the Employee must be at least 18 years of age.");
            Property(x => x.StartDate).Mandatory().CompareValue(CompareOperator.GreaterThanEqual, new DateTime(1999, 01, 01, 0, 0, 0, DateTimeKind.Utc), "January 1, 1999");
            Property(x => x.PhoneNo).Mandatory().Common(CommonValidators.PhoneNo);
            Property(x => x.Address).Entity(_addressValidator);
            Property(x => x.EmergencyContacts).Collection(maxCount: 5, item: new CollectionRuleItem<EmergencyContact>(_emergencyContactValidator));
        }

        /// <summary>
        /// Add further validation logic non-property bound.
        /// </summary>
        protected override void OnValidate(ValidationContext<Employee> context)
        {
            base.OnValidate(context);

            // Ensure that the termination data is always null on an update; unless already terminated then it can no longer be updated.
            switch (ExecutionContext.Current.OperationType)
            {
                case OperationType.Create:
                    context.Value.Termination = null;
                    break;

                case OperationType.Update:
                    var dataSvc = (IEmployeeDataSvc)context.ServiceProvider!.GetService(typeof(IEmployeeDataSvc));
                    var existing = dataSvc.GetAsync(context.Value.Id).GetAwaiter().GetResult();
                    if (existing == null)
                        throw new NotFoundException();

                    if (existing.Termination != null)
                        throw new ValidationException("Once an Employee has been Terminated the data can no longer be updated.");

                    context.Value.Termination = null;
                    break;
            }
        }
    }
}