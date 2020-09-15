# Step 2 - Employee API

This will walk through the process of creating the required APIs, including the related business and data access logic.

The [`Beef.CodeGen.Core`](../../../tools/Beef.CodeGen.Core/README.md) provides the code-generation capabilities that will be leveraged. The underlying documentation describes these capabilities and the code-gen approach in greater detail.

_Note:_ Any time that command line execution is requested, this should be performed from the base `My.Hr.CodeGen` folder.

<br/>

## Clean up existing

The following files were created when the solution was provisioned, these should be removed (deleted):
- `My.Hr.Business/Data/Person.cs`
- `My.Hr.Business/Validation/PersonArgsValidator.cs`
- `My.Hr.Business/Validation/PersonValidator.cs`

<br/>

## Reference Data configuration

The `My.RefData.xml` within `My.Hr.CodeGen` provides the code-gen configuration for the [Reference Data](../../../docs/Reference-Data.md). For the purposes of this sample, this configuration is relatively straighforward.

Each reference data entity is defined, by specifying the name, the Web API route prefix (i.e. its endpoint), that it is to be automatically implemented using Entity Framework, and the name of the corresponding Entity Framework model (which was previously generated from the database; see `My.Hr.Business/Data/EfModel/Generated` folder).

Replace the existing `Entity` XML (keeping the `CodeGeneration` element) with the following.

```
  <Entity Name="Gender" RefDataType="Guid" Collection="true" WebApiRoutePrefix="api/v1/ref/genders" AutoImplement="EntityFramework" EntityFrameworkEntity="EfModel.Gender" />
  <Entity Name="TerminationReason" RefDataType="Guid" Collection="true" WebApiRoutePrefix="api/v1/ref/terminationReasons" AutoImplement="EntityFramework" EntityFrameworkEntity="EfModel.TerminationReason" />
  <Entity Name="RelationshipType" RefDataType="Guid" Collection="true" WebApiRoutePrefix="api/v1/ref/relationshipTypes" AutoImplement="EntityFramework" EntityFrameworkEntity="EfModel.RelationshipType" />
  <Entity Name="USState" RefDataType="Guid" Collection="true" WebApiRoutePrefix="api/v1/ref/usStates" AutoImplement="EntityFramework" EntityFrameworkEntity="EfModel.USState" />
```

<br/>

## Reference Data code-gen

Once the reference data has been configured the code-generation can be performed. Use the following command line to generate. This will generate all of the required layers, from the API controller, through to the database access using Entity Framework. This is all that is required to operationalize the reference data. 

```
dotnet run refdata
```

<br/>

## Employee CRUD

To implement the core `Employee` CRUD operations, the following will need to be performed:
- Code-gen configuration
- Code-gen execution
- Data access logic
- Validation

<br/>

### Code-gen configuration

First up, the entities need to be defined (configured) within the `My.Hr.xml` (`My.Hr.CodeGen` project). The entities that will be created are as follows. Note that we will add some shape (e.g. address) to the data so it is easier (and more logical) to consume and understand, versus mapping directly to the database structure. 

- `EmployeeBase` - this represents the base Employee in that it contains the key properties and will be used as the base for searching as a means to minimise the properties that are available outside of the `Employee` CRUD itself.
- `Employee` - this represents the complete detailed Employee, which inherits from the `EmployeeBase`. All of the key operations for the Employee including the search will be configured/grouped as a logical set here.
- `TerminationDetail` - this represents an employee's termination (being date and reason). By having as a sub-type this enable additional related data to be more easily added under the single `Employee.Termination` property.
- `Address` - this represents the employees' address. By having as a sub-type it makes it easier and more explicit that there is a valid address via the `Employee.Address` property; in that we can validate the full address on the existence of the property itself (i.e. not `null`).
- `EmergencyContact` - this represents the collection of emergency contacts for an employee. 

Replace the existing `Entity` XML (keeping the `CodeGeneration`) with the following. The comments including are intended to describe the usage and why certain attributes have been specified.

``` xml
  <!-- Creating an Employee base with only the subset of fields that we want returned from the GetByArgs.
       - As we will be returning more than one we need the Collection and CollectionResult.
       - Any Text with a handlebars '{{xxx}}' is a shortcut for .NET see comments; e.g. '<see cref="xxx"/>'.
       - ExcludeAll is used so only the entity (not other layers are generated); with the exception of ExcludeData of false where it is a special case to output a DataMapper.
       - Use of DataName is to reference the name of the column where different to the property name iteself.
       - Use of DataAutoGenerated indicates that the data source will automatically generate the value.
       - A Type with RefDataNamespace.* is how to reference a reference data entity (will default RefDataType to 'string').
       - A DateTimeTransform of DateOnly is used to indicate that the DateTime property should only be concerned with the Date component.
       - ETag and ChangeLog are special case and Beef will automatically map between RowVersion and other Audit columns where they exist. 
       - AutoImplement of Database will ensure that the DbMapper is generated; used by the Employee.Get/Create/Update. 
       - By specifying the EntityFrameworkEntity the EfMapper (mapping to defined type will be mapped) will also be generated; used by the Employee.GetByArgs.
       - The Termination property DataEntityFrameworkIgnore is set as this cannot be automatically generated; custom code will need to be developed to handle; used by the Employee.GetByArgs. -->
  <Entity Name="EmployeeBase" Text="{{Employee}} base" Collection="true" CollectionResult="true" ExcludeAll="true" ExcludeData="false" AutoImplement="Database" EntityFrameworkEntity="EfModel.Employee" >
    <Property Name="Id" Type="Guid" Text="{{Employee}} identifier" UniqueKey="true" DataName="EmployeeId" DataAutoGenerated="true" />
    <Property Name="Email" Type="string" Text="Unique {{Employee}} Email" />
    <Property Name="FirstName" Type="string" />
    <Property Name="LastName" Type="string" />
    <Property Name="Gender" Type="RefDataNamespace.Gender" DataName="GenderCode" />
    <Property Name="Birthday" Type="DateTime" DateTimeTransform="DateOnly" />
    <Property Name="StartDate" Type="DateTime" DateTimeTransform="DateOnly" />
    <Property Name="Termination" Type="TerminationDetail" DataDatabaseMapper="TerminationDetailData.DbMapper" DataEntityFrameworkIgnore="true" />
    <Property Name="PhoneNo" Type="string" />
    <Property Name="ETag" ArgumentName="etag" Type="string" />
    <Property Name="ChangeLog" Type="ChangeLog" IsEntity="true" />
  </Entity>

  <!-- Creating an Employee inheriting from EmployeeBase (DataMapper will also inherit).
       - The Id is re-specified, but marked as inherited, as is needed to assist with the operations that reference the UniqueKey.
       - The Validator is specified, which is then used by both the Create and Update operations.
       - The AutoImplement specifies that operations should be auto-implemented using Database (ADO.NET) unless explicitly overridden.
       - The WebApiRoutePrefix is defined, which is in turn extended by each operation. 
       - The EntityFrameworkEntity is required so that the GetByArgs code-gen knows what EfModel is to be used; however, DataEntityFrameworkCustomMapper is also used so that a corresponding EfMapper is not output (not required). -->
  <Entity Name="Employee" Inherits="EmployeeBase" Validator="EmployeeValidator" WebApiRoutePrefix="api/v1/employees" AutoImplement="Database" DatabaseSchema="Hr" DataDatabaseMapperInheritsFrom="EmployeeBaseData.DbMapper">
    <Property Name="Id" Type="Guid" UniqueKey="true" Inherited="true" DataDatabaseIgnore="true" />
    <Property Name="Address" Type="Address" DataConverter="ObjectToJsonConverter{T}" DataName="AddressJson"/>
    <Property Name="EmergencyContacts" Type="EmergencyContactCollection" DataDatabaseIgnore="true" />

    <!-- CRUD operations:
         - Get - Get by unique identifier which it infers from the properties marked as UniqueKey; data access cannot be automatically implemented given complexity.
         - Create/Update/Patch - infers UniqueKey where appropriate; data access cannot be automatically implemented given complexity (Patch is Controller-only, reuses Get and Update to perform). 
         - Delete - infers UniqueKey; can be automatically implemented as it will invoke stored procedure with key and expects no result. -->
    <Operation Name="Get" OperationType="Get" UniqueKey="true" WebApiRoute="{id}" AutoImplement="None" />
    <Operation Name="Create" OperationType="Create" WebApiRoute="" AutoImplement="None" />
    <Operation Name="Update" OperationType="Update" UniqueKey="true" WebApiRoute="{id}" AutoImplement="None" />
    <Operation Name="Patch" OperationType="Patch" UniqueKey="true" WebApiRoute="{id}" />
    <Operation Name="Delete" OperationType="Delete" UniqueKey="true" WebApiRoute="{id}" />
  </Entity>

  <!-- Creating a TerminationDetail with Date and Reason.
       - ExcludeAll is used so only the entity (not other layers are generated); with the exception of ExcludeData of false where it is a special case to output a DataMapper. 
       - By specifying the EntityFrameworkEntity the EfMapper (mapping to defined type will be mapped) will also be generated; used by the Employee.GetByArgs. -->
  <Entity Name="TerminationDetail" ExcludeAll="true" ExcludeData="false" AutoImplement="Database" EntityFrameworkEntity="EfModel.Employee">
    <Property Name="Date" Type="DateTime" DateTimeTransform="DateOnly" DataName="TerminationDate" />
    <Property Name="Reason" Type="RefDataNamespace.TerminationReason" DataName="TerminationReasonCode" />
  </Entity>

  <!-- Creating an Address.
       - ExcludeAll is used so only the entity (not other layers are generated); no ExcludeData required as no mapper is needed as converting to JSON. -->
  <Entity Name="Address" ExcludeAll="true">
    <Property Name="Street1" Type="string" />
    <Property Name="Street2" Type="string" />
    <Property Name="City" Type="string" />
    <Property Name="State" Type="RefDataNamespace.USState" />
    <Property Name="PostCode" Type="string" />
  </Entity>

  <!-- Creating a EmergencyContact and corresponding collection.
       - ExcludeAll is used so only the entity (not other layers are generated); with the exception of ExcludeData of false where it is a special case to output a DataMapper. -->
  <Entity Name="EmergencyContact" Collection="true" ExcludeAll="true" ExcludeData="false" AutoImplement="Database" >
    <Property Name="Id" Type="Guid" UniqueKey="true" DataName="EmergencyContactId" />
    <Property Name="FirstName" Type="string" />
    <Property Name="LastName" Type="string" />
    <Property Name="PhoneNo" Type="string" />
    <Property Name="Relationship" Type="RefDataNamespace.RelationshipType" DataName="RelationshipTypeCode" />
  </Entity>
```

<br/>

### Code-gen execution

Once the entities has been configured the code-generation can be performed. Use the following command line to generate. This will generate all of the required layers, from the API controller, through to the database access using Stored Procedures where configured. 

```
dotnet run entity
```

<br/>

### Data access logic

Within the entity code-gen configuration where auto-implmentation is not possible then the data access logic must be specified explicitly. _Beef_ will still generate the boilerplate/skeleton wrapping logic for the data access to ensure consistency, and will invoke a corresponding `OnImplementation` method to perform (which the developer is required to implement).

This logic must be implemented by the developer in a non-generated `partial` class. A new `EmployeeData.cs` must be created within `My.Hr.Business/Data`; do **not** change any files under the `Generated` folder as these will be overridden during the next code-gen execution.

The following represents the initial implementation. 

``` csharp
using Beef;
using Beef.Data.Database;
using Microsoft.EntityFrameworkCore;
using My.Hr.Common.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace My.Hr.Business.Data
{
    public partial class EmployeeData
    {
        /// <summary>
        /// Executes the 'Get' stored procedure passing the identifier and returns the result.
        /// </summary>
        private Task<Employee?> GetOnImplementationAsync(Guid id) =>
            ExecuteStatement(_db.StoredProcedure("[Hr].[spEmployeeGet]")
                                .Param(DbMapper.Default.GetParamName(nameof(Employee.Id)), id));

        /// <summary>
        /// Executes the 'Create' stored procedure and returns the result.
        /// </summary>
        private Task<Employee> CreateOnImplementationAsync(Employee value) =>
            ExecuteStatement("[Hr].[spEmployeeCreate]", value, Beef.Mapper.OperationTypes.Create);

        /// <summary>
        /// Executes the 'Update' stored procedure and returns the result.
        /// </summary>
        private Task<Employee> UpdateOnImplementationAsync(Employee value) =>
            ExecuteStatement("[Hr].[spEmployeeUpdate]", value, Beef.Mapper.OperationTypes.Update);

        /// <summary>
        /// Executes the stored procedure, passing Employee parameters, and the EmergencyContacts as a table-valued parameter (TVP), the operation type to aid mapping, 
        /// and requests for the result to be reselected.
        /// </summary>
        private Task<Employee> ExecuteStatement(string storedProcedureName, Employee value, Beef.Mapper.OperationTypes operationType)
        {
            var sp = _db.StoredProcedure(storedProcedureName)
                        .Params(p => DbMapper.Default.MapToDb(value, p, operationType))
                        .TableValuedParam("@EmergencyContactList", EmergencyContactData.DbMapper.Default.CreateTableValuedParameter(value.EmergencyContacts!))
                        .ReselectRecordParam();

            return ExecuteStatement(sp)!;
        }

        /// <summary>
        /// Executes the underlying stored procedure and processes the result (used by Get, Create and Update).
        /// </summary>
        private async Task<Employee?> ExecuteStatement(DatabaseCommand db)
        {
            Employee? employee = null;

            // Execute the generated stored procedure, selecting (querying) two sets of data:
            // 1. The selected Employee (single row), the row is not mandatory, and stop (do not goto second set) where null. Use the underlying DbMapper to map between columns and .NET Type.
            // 2. Zero or more EmergencyContact rows. Use EmergencyContactData.DbMapper to map between columns and .NET Type. Update the Employee with result.
            await db.SelectQueryMultiSetAsync(
                new MultiSetSingleArgs<Employee>(DbMapper.Default, r => employee = r, isMandatory: false, stopOnNull: true),
                new MultiSetCollArgs<EmergencyContactCollection, EmergencyContact>(EmergencyContactData.DbMapper.Default, r => employee!.EmergencyContacts = r));

            return employee;
        }
    }
}
```

<br/>

### Validation

The final component that must be implemented by the developer is the validation logic. _Beef_ provides a rich, integrated, [validation framework](../../../docs/Beef-Validation.md) to simpplify and standardize the validation as much as possible. This is also intended to encourage a more thorough approach to validation as the API is considered the primary custodian of the underlying data integrity.

To encourage reuse, _Beef_ has the concept of common validators which allow for standardised validations to be created that are then reusable. Within the `My.Hr.Business/Validation` folder create `CommonValidators.cs` and implement as follows.

```
using Beef.Validation;
using System.ComponentModel.DataAnnotations;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Provides common validator capabilities.
    /// </summary>
    public static class CommonValidators
    {
        private static readonly EmailAddressAttribute _emailValidator = new EmailAddressAttribute();

        /// <summary>
        /// Provides a common person's name validator, ensure max length is 100.
        /// </summary>
        public static CommonValidator<string?> PersonName = CommonValidator.Create<string?>(cv => cv.String(100));

        /// <summary>
        /// Provides a common address's street validator, ensure max length is 100.
        /// </summary>
        public static CommonValidator<string?> Street = CommonValidator.Create<string?>(cv => cv.String(100));

        /// <summary>
        /// Provides a common email validator, ensure max length is 250, is all lowercase, and use validator.
        /// </summary>
        public static CommonValidator<string?> Email = CommonValidator.Create<string?>(cv => cv.String(250).Override(v => v.Value!.ToLowerInvariant()).Must(v => _emailValidator.IsValid(v.Value)));

        /// <summary>
        /// Provides a common phone number validator, just length, but could be regex or other.
        /// </summary>
        public static CommonValidator<string?> PhoneNo = CommonValidator.Create<string?>(cv => cv.String(50));
    }
}
```

The entity code-gen configuration references a `EmployeeValidator` that needs to be implemented. Within the `My.Hr.Business/Validation` folder create `EmployeeValidator.cs` and implement as follows.

``` chsarp
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
    /// Represents a <see cref="Person"/> validator.
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
            Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => DateTime.Now.AddYears(-18), errorText: "Birthday is invalid as the Employee must be at least 18 years of age.");
            Property(x => x.StartDate).Mandatory().CompareValue(CompareOperator.GreaterThanEqual, new DateTime(1999, 01, 01), "January 1, 1999");
            Property(x => x.PhoneNo).Mandatory().Common(CommonValidators.PhoneNo);
            Property(x => x.Address).Entity(_addressValidator);
            Property(x => x.EmergencyContacts).Collection(maxCount: 5, item: new CollectionRuleItem<EmergencyContact>(_emergencyContactValidator).UniqueKeyDuplicateCheck());
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
                    var dataSvc = (IEmployeeDataSvc)ExecutionContext.Current.ServiceProvider!.GetService(typeof(IEmployeeDataSvc));
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
```

</br>

## Conclusion

At this stage we now have a compiling and working API including database access logic for the reference data and key employee CRUD activities. Next we need to perform end-to-end testing to ensure it is functioning as expected.