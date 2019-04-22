// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using System;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class PersonValidator : Validator<Person, PersonValidator>
    {
        public PersonValidator()
        {
            Property(x => x.FirstName).Mandatory().String(30);
            Property(x => x.LastName).Mandatory().String(30);
            Property(x => x.Birthday).Mandatory().Must(x => x.Birthday.AddYears(18) <= DateTime.Now, "{0} is invalid; age must be greater than 18.");
            Property(x => x.Nickname).String(30);
        }
    }
}
