// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper.Converters;
using System;
using System.Collections.Generic;

namespace Beef.Data.OData.UnitTest.Entities
{
    public class Person
    {
        public static string JsonData = @"{ ""IdX"": 1, ""NameX"": ""Angela"", ""BirthdayX"": ""1970-02-05T00:00:00"", ""SalaryX"": 100000.0, ""AddressX"": { ""StreetNumberX"": 123, ""StreetNameX"": ""Petherick"" }, ""AddressesX"": [ { ""StreetNumberX"": 456, ""StreetNameX"": ""Simpsons"" }, { ""StreetNumberX"": 789, ""StreetNameX"": ""Park"" } ], ""NicknamesX"": [ ""Bro"", ""Mate"" ], ""IsDeceasedX"": ""Yes"" }";

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Birthday { get; set; }

        public decimal Salary { get; set; }

        public Address Address { get; set; }

        public List<Address> Addresses { get; set; }

        public string[] Nicknames { get; set; }

        public bool IsDeceased { get; set; }

        public static Person Create()
        {
            return new Person
            {
                Id = 1,
                Name = "Angela",
                Birthday = new DateTime(1970, 02, 05),
                Salary = 100000m,
                Address = new Address { StreetNumber = 123, StreetName = "Petherick" },
                Addresses = new List<Address>( new Address[] { new Address { StreetNumber = 456, StreetName = "Simpsons" }, new Address { StreetNumber = 789, StreetName = "Park" } } ),
                Nicknames = new string[] { "Bro", "Mate" },
                IsDeceased = true
            };
        }
    }

    public class Person2 : Person
    {
        public Person2(int id, string name, DateTime birthday, decimal salary, Address address, List<Address> addresses, string[] nicknames, bool isDeceased)
        {
            Id = id;
            Name = name;
            Birthday = birthday;
            Salary = salary;
            Address = address;
            Addresses = addresses;
            Nicknames = nicknames;
            IsDeceased = isDeceased;
        }
    }

    public class Address
    {
        public int StreetNumber { get; set; }

        public string StreetName { get; set; }
    }

    public class PersonMapper : ODataMapper<Person>
    {
        public PersonMapper() : base("PersonXX")
        {
            Property(x => x.Id, "IdX", Mapper.OperationTypes.AnyExceptCreate);
            Property(x => x.Name, "NameX");
            Property(x => x.Birthday, "BirthdayX");
            Property(x => x.Salary, "SalaryX");
            Property(x => x.Address, "AddressX").SetMapper(new AddressMapper());
            Property(x => x.Addresses, "AddressesX").SetMapper(new AddressMapper());
            Property(x => x.Nicknames, "NicknamesX");
            Property(x => x.IsDeceased, "IsDeceasedX").SetConverter(BooleanToYesNoConverter.Default);
        }
    }

    public class Person2Mapper : ODataMapper<Person2>
    {
        public Person2Mapper() : base("Person2XX")
        {
            InheritPropertiesFrom(new PersonMapper());
        }
    }

    public class AddressMapper : ODataMapper<Address>
    {
        public AddressMapper() : base("AddressXX")
        {
            Property(x => x.StreetNumber, "StreetNumberX");
            Property(x => x.StreetName, "StreetNameX");
        }
    }
}
