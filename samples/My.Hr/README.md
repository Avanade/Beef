# My.Hr APIs

The purpose of this sample is to demonstrate the usage of _Beef_ within the context of a fictitious Human Resources solution. The main intent is to show how _Beef_ can be used against a relational database (SQL Server) leveraging both direct ADO.NET (with stored procedures) and Entity Framework (EF) where applicable.

Also, it will demonstrate how the data can be shaped differently between the database and the entity to leverage both relational and object-oriented constructs to provide a natural consuming experience from the API that accounts for the [object-relational impedence mismatch](https://en.wikipedia.org/wiki/Object-relational_impedance_mismatch#:~:text=The%20object-relational%20impedance%20mismatch%20is%20a%20set%20of,to%20database%20tables%20defined%20by%20a%20relational%20schema.).

This sample will walkthrough an approach of adding the capabilities in a series of logical steps, versus big-bang (all at once), as this is more typical of how a developer may implement.

</br>

## Scope

Within the sample there will two primary entities exposed:
- **Employee** - being an employee that either is, or was, employed by the ficticous organization.
- **Performance Review** - being a recording of a number of performance reviews for an employee over time.

</br>

### Employee

This will represent an employee within the organization, and house key data such as their name, address, phone number, gender, date of birth, start and termination dates, and a list of up to five emergency contacts.

From an endpoint perspective it will support the following.

Endpoint | Description
-|-
`GET /employees/id` | Get employee by primary identifier.
`POST /employees` | Create a new employee.
`PUT /employees/id` | Update (replace) the existing employee (only where not terminated).
`PATCH /employees/id` | Patch the existing employee (only where not terminated).
`DELETE /employees/id` | Delete an existing employee (only where not started).
`GET /employees` | Gets employee(s) that match the selection criteria (a subset of the fields to be returned, plus support for paging).
`POST /employees/id/terminate` | Updates the employee as terminated (other endpoints do not allow termination).

</br>

### Performance Review

This will respresent a performance review (multiple over time), and house key data such as date, outcome, notes and reviewer.

From an endpoint perspective it will support the following.

Endpoint | Description
-|-
`GET /reviews/id` | Get review by primary identifier.
`POST /employees/id/reviews` | Create a review for a specified employee.
`PUT /reviews/id` | Update (replace) the review.
`PATCH /reviews/id` | Patch the existing review.
`DELETE /reviews/id` | Delete an existing review.
`GET /employee/id/reviews` | Gets all review(s) for the employee (with paging support).

</br>

## Solution skeleton

This solution should be created using the solution [template](../../templates/Beef.Template.Solution/README.md) capability, following the getting started [guide](../../docs/Sample-EntityFramework-GettingStarted.md).

The following four commands should be invoked to create the solution structure. Start in a folder where the solution should reside. To simplify the ongoing copy and paste activities within this sample it is highly recommended that the `My.Hr` naming convention below is used.

```
dotnet new -i beef.template.solution --nuget-source https://api.nuget.org/v3/index.json
mkdir My.Hr
cd My.Hr
dotnet new beef --company My --appname Hr --datasource EntityFramework
```

The following solution structure will have been generated. Open `My.Hr.sln` in Visual Studio.

```
└── My.Hr               # Solution that references all underlying projects
  └── Testing
    └── My.Hr.Test      # Unit and intra-integration tests
  └── Tools
    └── My.Hr.CodeGen   # Entity and Reference Data code generation console
    └── My.Hr.Database  # Database code generation console
  └── My.Hr.Api         # API end-point and operations
  └── My.Hr.Business    # Core business logic components
  └── My.Hr.Common      # Common / shared components
```

_Note:_ Code generation should **not** be performed before updating the corresponding YAML files as described in the next sections. Otherwise, extraneous files will be generated that will then need to be manually removed.

Also, any files that start with `Person` (being the demonstration entity) should be removed (deleted) from their respective projects as they are encountered. This then represents the base-line to build up the solution from.

</br>

## Implementation steps

As described earlier, this sample will walk through the implementation in a number of logical steps:
1. [Employee DB](./docs/Employee-DB.md) - creates the `Employee` database table and related stored procedures.
2. [Employee API](./docs/Employee-Api.md) - creates the `Employee` entities, API and related data access logic.
3. [Employee Test](./docs/Employee-Test.md) - creates the `Employee` end-to-end integration tests to validate the API and database functionality.
4. [Employee Search](./docs/Employee-Search.md) - adds the `Employee` search capability and tests.
5. [Employee Terminate](./docs/Employee-Terminate.md) - adds the `Employee` termination capability and tests.
6. [Employee Performance Review](./docs/Performance-Review.md) - adds the employee `PerformanceReview` capability end-to-end, from the the database, through the APIs and corresponding testing.

<br/>

## Conclusion

The basis of the functional capabilities have been created for our fictitious solution. In the end, the developer should have a reasonable understanding of how to build a relatively complicated back-end (API and database) solution leveraging _Beef_.

The developer should have witnessed that reasonably complicated logic can be built using this _config_ to _code-gen_ to _custom_ approach. Where the _custom_ effort is for the most part focused on the key business value delivery; not the related boilerplate. Plus, with the testing framework, how complex end-to-end intra-domain integration tests can be created to appropriately validate the underlying logic - which can easily be integrated into the developer build-test-release lifecycle.

It is acknowledged that there is a learning curve required for using _Beef_; and in time greater acceleration will be achieved as experience is gained. Please review the extended documentation and provide feedback, questions, defects, etc. via a [issue](https://github.com/Avanade/Beef/issues). Thanks and enjoy :-)