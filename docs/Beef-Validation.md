# Validation

A validation capability has been provided to enable a consistent approach to validations that is deeply integrated into the overall framework. The framework provides the key (common) validations, whilst also being extensible to allow additional validations to be added as required.

<br/>

## Composition

At the core a `Validator` can contain one or more **Rules** (provides a specific value validation), which can be further conditionally controlled by zero or more **Clauses** (provides a means to check whether a validation should occur).

<br/>

### Rules

All rules must inherit from	`PropertyRuleBase` which enables the following key capabilities:

Capability | Description
-|-
`Name` | Gets the underlying property/value name.
`Text` | Gets/sets the friendly text name used in validation messages.
`DependsOn()` | Adds a `DependsOnClause` (supports zero or more).
`When()` | Adds a `WhenClause` (supports zero or more).
`WhenOperation()` | Adds a `WhenClause` that states that the `ExecutionContext.Current.OperationType` is equal to the specified value.
`WhenNotOperation()` | Adds a `WhenClause` that states that the `ExecutionContext.Current.OperationType` is not equal to the specified value.

The following represent the available rules:

Rule | Description 
-|-
`BetweenRule` | Provides a comparision validation between a specified from and to value.
`CollectionRule` | Provides collection (`IEnumerable`) validation including `MinCount`, `MaxCount`, per item validation `CollectionRuleItem` and duplicate checking. 
`CommonRule` | Provides for integrating a common validation against a specified property.
`ComparePropertyRule` | Provides a comparision validation against another property within the same entity; also confirms other property has no errors prior to comparison.
`CompareValueRule` | Provides a comparision validation against a specified value. 
`CustomRule` | Provides a custom validation against a specified property.
`DecimalRule` | Represents a numeric rule that validates `DecimalPlaces` (fractional-part length) and `MaxDigits` (being the sum of the integer-part and fractional-part lengths). 
`DictionaryRule` | Provides dictionary (`IDictionary`) validation including `MinCount`, `MaxCount` and per item validation `DictionaryRuleItem`. 
`DuplicateRule` | Provides validation where the rule predicate must return `false` to not be considered a duplicate. 
`EntityRule` | Provides entity validation. 
`ExistsRule` | Provides validation where the rule predicate must return `true` or a value to verify it exists.
`ImmutableRule` | Provides validation where the rule predicate must return `true` to be considered valid (has not been modified).
`MandatoryRule` | Provides mandatory validation; determined as mandatory when it contains its default value.
`MustRule` | Provides validation where the rule predicate must return `true` to be considered valid.
`NumericRule` | Represents a numeric rule to validate whether negatives are allowed.
`OverrideRule` | Provides the ability to override the property value.
`ReferenceDataCodeRule` | Provides validation for a `ReferenceDataBase.Code`; validates that it exists and that the corresponding `ReferenceDataBase.IsValid`.
`ReferenceDataRule` | Provides validation for a `ReferenceDataBase`; validates that the `ReferenceDataBase.IsValid`.
`ReferenceDataSidListRule` | Provides validation for a `ReferenceDataSidListBase` including `MinCount`, `MaxCount`, per item `ReferenceDataBase.IsValid` and whether to `AllowDuplicates`.
`StringRule` | Provides `string` validation including `MinLength`, `MaxLength` and `Regex`.
`WildcardRule` | Provides `string` `Wildcard` validation.

<br/>

### Clauses

The following represent the available clauses:

Clause | Description 
-|-
`DependsOnClause` | Represents a depends on test clause; in that another specified property of the entity must have a non-default value to continue.
`WhenClause` | Represents a when test clause; in that the condition must be `true` to continue.

<br/>

### Extension Methods

The **rules** are generally not instantiated directly, but accessed via pre-defined extension methods to provide a more simplified, natural, experience using fluent-style (method-chaining) approach to development.

The following represent the available extension methods:

Extension method | Description | Underlying rule
-|-|-
`AreValid()` | Adds a *reference data list* validation. | `ReferenceDataSidListRule`
`Between()` | Adds a *between comparision* validation. | `BetweenRule`
`Collection()` | Adds a *collection* validation. | `CollectionRule`
`CompareProperty()` | Adds a *property comparison* validation. | `ComparePropertyRule`
`CompareValue()` | Adds a *value comparison* validation. | `CompareValueRule`
`Currency()` | Adds a *currency* validation for a `decimal` using a `NumberFormatInfo`. | `DecimalRule`
`Custom()` | Adds a *custom* validation. | `CustomRule`
`Default()` | Adds a property value override where the current value is the default for the `Type`. | `OverrideRule`
`Dictionary()` | Adds a *dictionary* validation. | `DictionaryRule`
`Duplicate()` | Adds a *duplicate* validation. | `DuplicateRule`
`Entity()` | Adds an *entity* validation. | `EntityValidationRule`
`EntityCollection()` | Adds an *entity collection* validation. | `EntityCollectionValidationRule`
`Exists()` | Adds an *exists* validation. | `ExistsRule`
`Immutable()` | Adds an *immutable* validation. | `ImmutableRule`
`IsValid()` | Adds a *reference data* validation. | `ReferenceDataRule`
`Mandatory()` | Adds a *mandatory* validation. | `MandatoryRule`
`Must()` | Adds a *must* validation. | `MustRule`
`Numeric()` | Adds a *numeric* validation. | `NumericRule` or `DecimalRule`
`Override` | Adds a property value override. | `OverrideRule`
`RefDataCode` | Adds a *reference data code* validation. | `ReferenceDataCodeRule`
`String()` | Adds a `string` validation. | `StringRule`
`Wildcard()` | Adds a `string` *wildcard* validation. | `WildcardRule`

Additional extension methods included are as follows:

Extension method | Description
-|-
`Text()` | Updates the rule friendly name text used in validation messages.
`Common()` | Provides for integrating a common validation against a specified property.
`Validate()` | Enables (sets up) validation for a value.

<br/>

### Error messages

All error messages are managed as an embedded resources accessible via the `ValidatorStrings` class; as follows:

Property | Format string
-|-
`AllowNegativesFormat` | {0} must not be negative.
`BetweenInclusiveFormat` | {0} must be between {2} and {3}.
`BetweenExclusiveFormat` | {0} must be between {2} and {3} (exclusive).
`CollectionNullItemFormat` | {0} contains one or more items that are not specified.
`CompareEqualFormat` | {0} must be equal to {2}.
`CompareGreaterThanEqualFormat` | {0} must be greater than or equal to {2}.
`CompareGreaterThanFormat` | {0} must be greater than {2}.
`CompareLessThanEqualFormat` | {0} must be less than or equal to {2}.
`CompareLessThanFormat` | {0} must be less than {2}.
`CompareNotEqualFormat` | {0} must not be equal to {2}.
`DecimalPlacesFormat` | {0} exceeds the maximum specified number of decimal places ({2}).
`DependsOnFormat` | {0} is required where {2} has a value.
`DictionaryNullKeyFormat` | {0} contains one or more keys that are not specified.
`DictionaryNullValueFormat` | {0} contains one or more values that are not specified.
`DuplicateFormat` | {0} already exists and would result in a duplicate.
`DuplicateValue2Format` | {0} contains duplicates; {2} value specified more than once.
`DuplicateValueFormat` | {0} contains duplicates; {2} value '{3}' specified more than once.
`ExistsFormat` | {0} is not found; a valid value is required.
`ImmutableFormat` | {0} is not allowed to change; please reset value.
`InvalidFormat` | {0} is invalid.
`MandatoryFormat` | {0} is required.
`MaxCountFormat` | {0} must not exceed {2} item(s).
`MaxDigitsFormat` | {0} must not exceed {2} digits in total.
`MaxLengthFormat` | {0} must not exceed {2} characters in length.
`MaxValueFormat` | {0} is greater than the maximum allowed value of {2}.
`MinCountFormat` | {0} must have at least {2} item(s).
`MinLengthFormat` | {0} must be at least {2} characters in length.
`MinValueFormat` | {0} is less than the minimum allowed value of {2}.
`MustFormat` | {0} is invalid.
`RegexFormat` | {0} is invalid.
`WildcardFormat` | {0} contains invalid or non-supported wildcard selection.

The validation framework passes the friendly text name as `{0}`, and the validating value as `{1}` for inclusion in the final message output. Higher numbered format strings are applicable to the specific validator rule consuming.

<br/>

## Usage

There are multiple means to leverage the validation framework.

<br/>

### Entity-based validator class

The primary means for an entity-based validator is to inherit from the `Validator` class. The instance should be instantiated once (and cached) where possible as the underlying property expressions can be a relatively expensive (performance) operation.

Additionally, the `OnValidate` method can be overridden to add more complex and/or cross-property validations as required.

Each property for the entity is configured using the `Property` method and a corresponding property expression. The property expression is advantageous as the friendly text name can be inferred (in order specified):
- Use the `DisplayAttribute(Name="Product Code")` value; will be: "Product Code";
- Use the property name `string CustomerNumber { get; set; }` formatted as Sentence Case; will be "Customer Number".
- The resulting text from above is then passed through the text localization (`LText`) resource string replacement.

An example is as follows:

``` csharp
public class PersonValidator : Validator<Person>
{
    public PersonValidator()
    {
        Property(x => x.Name).Mandatory().String(maxLength: 50);
        Property(x => x.Birthday).CompareValue(CompareOperator.LessThanEqual, DateTime.Now, "today");
    }

    protected override Task OnValidateAsync(ValidationContext<Test> context)
    {
        // Check that Amount property has not had an error already; then validate and error.
        context.Check(x = x.Amount, (val) => val <= 100, "{0} must be greater than 100.");

        return Task.CompletedTask;
    }
}

var person = new Person { Name = "Freddie", Birthday = new DateTime(1946, 09, 05), Amount = 150 };

// Validate the value.
var result = await new PersonValidator().ValidateAsync(person);
```

<br/>

### Entity-based inline validator

The secondary means for an entity-based validator is to define and execute inline. The `HasProperty()` is used to create a property, with a corresponding action to enable validation configuration.

An example is as follows:

``` csharp
var person = new Person { Name = "Freddie", Birthday = new DateTime(1946, 09, 05);

// Create an entity-based validator on the fly.
var result = await Validator.Create<Test>()
    .HasProperty(x => x.Name, p => p.Mandatory().String(maxLength: 50))
    .HasProperty(x => x.Birthdar, p => p.CompareValue(CompareOperator.LessThanEqual, DateTime.Now, "today"))
    .ValidateAsync(person);
```

<br/>


### Value-based validator

Values, both entity and non-entity, can be validated directly. Examples are as follows:

``` csharp
var person = new Person { Name = "Freddie", Birthday = new DateTime(1946, 09, 05);

// Validate an entity value; being the Person class.
var pv = new PersonValidator();
await person.Validate().Entity(pv).RunAsync();

// Validate a value (e.g. a string, int, DateTime, etc.) without an entity-based validator.
await person.Name.Validate().Mandatory().String(maxLength: 10).RunAsync(throwOnError: true);
```

<br/>

### Validation chaining

As demonstrated in the prior examples the validation supports fluent-style (method-chaining) for the underlying **rules** and **clauses**.

When the validation is executed the **rules** will be invoked in the order in which they are specified, and conditionally invoked where succeeding **clauses** (optional) are specified for a rule (to the right of). A validation may have zero or more clauses before the first rule, then a rule with zero or more succeeding clauses, followed by zero or more rules, etc.

The following is a property that will only perform any succeeding rules once the `DependsOn` clause results in `true`; otherwise, no rules will be executed. Where `true` then the `CompareProperty` will be executed:

``` csharp
Property(x => x.DateTo).DependsOn(x => x.DateFrom).CompareProperty(CompareOperator.GreaterThanEqual, x => x.DateFrom);
```

The following is a property that will always execute the `Mandatory` rule, and only the `CompareProperty` rule where the `DependsOn` clause results in `true`:

``` csharp
Property(x => x.DateTo).Mandatory.CompareProperty(CompareOperator.GreaterThanEqual, x => x.DateFrom).DependsOn(x => x.DateFrom);
```

<br/>

### Common validations

To support reusablility of property validations a `CommonValidator` is used to enable. This allows for the validation logic to be defined once, and reused (shared) across multiple validations. This validator also enables validation to be configured for non-entities (e.g. intrinisic types).

An example is as follows:

``` csharp
var cv = CommonValidator<string> _cv = Validator.CreateCommon<string>(v => v.String(5).Must(x => x.Value != "XXXXX"));

var v = Validator.Create<TestData>()
    .HasProperty(x => x.Text, p => p.Mandatory().Common(cv));
```

<br/>

### Examples

The following represents a number of additional examples demonstrating property validation scenarios:

``` csharp
// The integer is mandatory, must be positive, and has a max value of 999, and must have a value greater of 10.
Property(x => x.Integer).Mandatory().Numeric(allowNegatives: false, maxDigits: 3).CompareValue(CompareOperator.GreaterThan, 10);

// The decimal should be treated as a positive currency with default decimal places (NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits).
Property(x => x.Amount).Currency(allowNegatives: true);

// The decimal must be positive, with a max value of 999.999, and max three decimal places (max digits includes decimal places).
Property(x => x.Amount2).Numeric(allowNegatives: false, maxDigits: 6, decimalPlaces: 3);

// The Date From must be greater than Now; with specified text "today" to include in error message.
Property(x => x.DateFrom).CompareValue(CompareOperator.GreaterThan, DateTime.Now, "today");

// The Date To must be greater than the Date From where the Date From (DependsOn) has a value 
// (also DependsOn will not validate where the dependent field has previously failed).
Property(x => x.DateTo).DependsOn(x => x.DateFrom).CompareProperty(CompareOperator.GreaterThanEqual, x => x.DateFrom);

// When can be used to conditionalise a previous rule; so Name is mandatory only when the Integer value is 50; also, max length is 50.
Property(x => x.Name).Mandatory().When(x => x.Integer == 50).String(maxLength: 50);

// Must can used for more complex logic, as in the condition 'must' be true otherwise the value is considered invalid.
Property(x => x.Amount).Must(x => x.Integer > 10);

// The phone number will be validated against the defined regex. 
Property(x => x.PhoneNo).String(new Regex(@"\+0\d{9}|\+0[1-9]\d{12}|0[1-9]\d{8}|00[1-9]\d{9}|00[1-9]\d{13}"));

// The Gender (which is a Reference Data entity) is mandatory and must be considered valid.
Property(x => x.Gender).Mandatory().IsValid();

// Check the sub entity exists (mandatory) and is valid (using defined validator).
Property(x => x.SubTest).Mandatory().Entity().With(test2Validator);

// Check the sub entity collection (exists), has 1-4 items in the collection, and each is valid (using defined validator).
Property(x => x.SubTesters).Mandatory().Collection(minCount: 1, maxCount: 4, item: CollectionRuleItem.Create<Test2>(test2Validator));
```

The following demonstrates the mixing of both entity-based options:

``` csharp
public class PersonValidator : Validator<Person>
{
    private static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
        .HasProperty(x => x.Street, p => p.Mandatory().String(50))
        .HasProperty(x => x.City, p => p.Mandatory().String(50));

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonValidator"/>.
    /// </summary>
    public PersonValidator()
    {
        Property(x => x.FirstName).Mandatory().String(50);
        Property(x => x.LastName).Mandatory().String(50);
        Property(x => x.Gender).Mandatory().IsValid();
        Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, DateTime.Now, "Today");
        Property(x => x.Address).Entity().With(_addressValidator);
    }
}
```

<br/>

## Advanced

There are additional features that enable more advanced / complex validation scenarios.

<br/>

### Conditional rule set

The `RuleSet` represents a conditional validation rule for an entity, in that it groups one or more **rules** together that only get invoked where a specified condition results in **true**.

The following demonstrates `RuleSet` usage for an *Entity-based validator class*:

``` csharp
public class TestItemValidator : Validator<TestItem>
{
    public TestItemValidator()
    {
        RuleSet(x => x.Value.Code == "A", () => 
        {
            Property(x => x.Text).Mandatory().Must(x => x.Text == "A");
        });

        RuleSet(x => x.Value.Code == "B", () =>
        {
            Property(x => x.Text).Mandatory().Must(x => x.Text == "B");
        });
    }
}
```

The following demonstrates `HasRuleSet` usage for an `Entity-based inline validator`:

``` csharp
var v = Validator.Create<TestItem>()
    .HasRuleSet(x => x.Value.Code == "A", y =>
    {
        y.Property(x => x.Text).Mandatory().Must(x => x.Text == "A");
    })
    .HasRuleSet(x => x.Value.Code == "B", (y) =>
    {
        y.Property(x => x.Text).Mandatory().Must(x => x.Text == "B");
    });
```

<br/>

### Include/inherit validators

Where entities leverage inheritence, having the corresponding validators include the base (parent) classes validations rules can be advantageous (versus codifying the rules multiple times). The `IncludeBase` method enables a base validator to be included within another validator's rule set.

The following is an example of using the `IncludeBase` method:

``` csharp
var r = Validator.Create<TestData>()
    .IncludeBase(testDataBaseValidator)
    .HasProperty(x => x.CountB, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10))
    .Validate(new TestData { CountB = 0 });
```

<br/>

### Consolidating multiple validators

The `MultiValidator` enables the validation of multiple values there is a need to consolidate the results into a single set of `Messages` (and/or `ValidationException`).

An example is as follows:

``` csharp
var result = await MultiValidator.Create()
    .Add(person.Validate(nameof(value)).Mandatory().Entity(personValidator))
    .Add(other.Validate(nameof(other)).Mandatory().Entity(otherValidator))
    .RunAsync();
```