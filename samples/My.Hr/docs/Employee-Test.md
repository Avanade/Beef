# Step 3 - Employee Test

This will walk through the process of creating the required end-to-end intra-domain integration tests to validate the employee CRUD APIs.

The [`Beef.Test.NUnit`](../../../tools/Beef.Test.NUnit/README.md) provides the integration testing capabilities that will be leveraged. The underlying documentation describes these capabilities and the approach in greater detail.

<br/>

## Project structure

The overall `My.Hr.Test` solution was created with the following; the existing `PersonTest.cs` and `PersonValidatorTest.cs` should be removed (deleted).

```
└── Apis
  └── FixtureSetup.cs           <- leave; contains key logic to set up database
  └── PersonTest.cs             <- remove
└── Data
  └── Data.yaml                 <- leave; will replace contents
└── Validators
  └── PersonValidatorTest.cs    <- remove
```

<br/>

## Data population

For the end-to-end testing to function data must first be populated into the database; noting that the Reference Data configured and created earlier will be included automatically. Foundationally, the `My.Hr.Database` is leveraged to create and set up the database, as well as populate it with data.

For the purposes of testing the APIs implemented so far, a set of employees and related data is required. The YAML defines the schema, table(s) and colums(s) with the required column data values. Replace the existing `Data.yaml` with the following.

``` yaml
Hr:
  - Employee:
    - { EmployeeId: 1, Email: w.jones@org.com, FirstName: Wendy, LastName: Jones, GenderCode: F, Birthday: 1985-03-18, StartDate: 2000-12-11, PhoneNo: (425) 612 8113 }
    - { EmployeeId: 2, Email: b.smith@org.com,  FirstName: Brian, LastName: Smith, GenderCode: M, Birthday: 1994-11-07, StartDate: 2013-08-06, TerminationDate: 2015-04-08, TerminationReasonCode: RE, PhoneNo: (429) 120 0098 }
    - { EmployeeId: 3, Email: r.Browne@org.com,  FirstName: Rachael, LastName: Browne, GenderCode: F, Birthday: 1972-06-28, StartDate: 2019-11-06, PhoneNo: (421) 783 2343 }
    - { EmployeeId: 4, Email: w.smither@org.com,  FirstName: Waylon, LastName: Smithers, GenderCode: M, Birthday: 1952-02-21, StartDate: 2001-01-22, PhoneNo: (428) 893 2793, AddressJson: '{ "street1": "8365 851 PL NE", "city": "Redmond", "state": "WA", "postCode": "98052" }' }
  - EmergencyContact:
    - { EmergencyContactId: 201, EmployeeId: 2, FirstName: Garth, LastName: Smith, PhoneNo: (443) 678 1827, RelationshipTypeCode: PAR }
    - { EmergencyContactId: 202, EmployeeId: 2, FirstName: Sarah, LastName: Smith, PhoneNo: (443) 234 3837, RelationshipTypeCode: PAR }
    - { EmergencyContactId: 401, EmployeeId: 4, FirstName: Michael, LastName: Manners, PhoneNo: (234) 297 9834, RelationshipTypeCode: FRD }
```

<br/>

## Employee API test

For the purposes of this sample, copy the contents of [`EmployeeTest.cs`](../My.Hr.Test/Apis/EmployeeTest.cs) and paste into an equivalent (new) `EmployeeTest.cs`. Comment out the regions `GetByArgs` and `Termination` as these capabilities have not been implemented yet.

Review and execute the tests and ensure they all pass as expected.

</br>

## Employee Validator test

This is more of a pure unit test; in that all data repository access is mocked out. This allows these to execute faster without database set up requirements, but will need the likes of reference data, and other, mocked as required. The sample demonstrates how these validators can be easily and thoroughly tested.

For the purposes of this sample, copy the contents of [`EmployeeValidatorTest.cs`](../My.Hr.Test/Validators/EmployeeValidatorTest.cs) and paste into an equiavlent (new) `Validators/EmployeeValidatorTest.cs`.

Review and execute the tests and ensure they all pass as expected.

</br>

## Conclusion

At this stage we now have a set of functioning and tested performance review APIs. All the desired functionality is now complete. These are now essentially ready for deployment; obviously, before doing so security would need to be integrated into the solution.

Next we will implement the [employee search](./Employee-Search.md) endpoint.

