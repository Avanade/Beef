// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Core.UnitTest.Core
{
    [TestFixture]
    public class WildcardTest
    {
        #region Determine

        [Test]
        public void Parse_1_NoneOrEqual()
        {
            var wc = Wildcard.BothAll;
            Check(WildcardSelection.None, null, wc.Parse(null));
            Check(WildcardSelection.None, null, wc.Parse(string.Empty));
            Check(WildcardSelection.None, null, wc.Parse(" "));
            Check(WildcardSelection.Equal, "X", wc.Parse("X"));
            Check(WildcardSelection.Equal, "XX", wc.Parse("XX"));
            Check(WildcardSelection.Equal, "XXX", wc.Parse("XXX"));
            Check(WildcardSelection.Equal, "XXX", wc.Parse(" XXX "));
        }

        [Test]
        public void Parse_2_Single()
        {
            var wc = Wildcard.BothAll;
            Check(WildcardSelection.Single | WildcardSelection.MultiWildcard, "*", wc.Parse("*"));
            Check(WildcardSelection.Single | WildcardSelection.MultiWildcard, "*", wc.Parse("**"));
            Check(WildcardSelection.Single | WildcardSelection.MultiWildcard, "*", wc.Parse("***"));
            Check(WildcardSelection.Single | WildcardSelection.SingleWildcard, "?", wc.Parse("?"));
            Check(WildcardSelection.StartsWith | WildcardSelection.EndsWith | WildcardSelection.SingleWildcard | WildcardSelection.AdjacentWildcards, "??", wc.Parse("??"));
            Check(WildcardSelection.StartsWith | WildcardSelection.EndsWith | WildcardSelection.Embedded | WildcardSelection.SingleWildcard | WildcardSelection.AdjacentWildcards, "???", wc.Parse("???"));
        }

        [Test]
        public void Parse_3_StartsAndEndsWithOrContains()
        {
            var wc = Wildcard.BothAll;
            Check(WildcardSelection.EndsWith | WildcardSelection.MultiWildcard, "*X", wc.Parse("*X"));
            Check(WildcardSelection.EndsWith | WildcardSelection.SingleWildcard, "?X", wc.Parse("?X"));
            Check(WildcardSelection.EndsWith | WildcardSelection.SingleWildcard, "?XX", wc.Parse("?XX"));
            Check(WildcardSelection.StartsWith | WildcardSelection.MultiWildcard, "X*", wc.Parse("X*"));
            Check(WildcardSelection.StartsWith | WildcardSelection.SingleWildcard, "X?", wc.Parse("X?"));
            Check(WildcardSelection.StartsWith | WildcardSelection.SingleWildcard, "XX?", wc.Parse("XX?"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Contains, "*X*", wc.Parse("*X*"));
            Check(WildcardSelection.SingleWildcard | WildcardSelection.Contains, "?X?", wc.Parse("?X?"));
        }

        [Test]
        public void Parse_4_EmbeddedOrContains()
        {
            var wc = Wildcard.BothAll;
            Check(WildcardSelection.Embedded | WildcardSelection.MultiWildcard, "X*X", wc.Parse("X*X"));
            Check(WildcardSelection.Embedded | WildcardSelection.MultiWildcard, "XX*XX", wc.Parse("XX*XX"));
            Check(WildcardSelection.Embedded | WildcardSelection.SingleWildcard, "XX?XX", wc.Parse("XX?XX"));
            Check(WildcardSelection.Embedded | WildcardSelection.SingleWildcard, "XX?XX", wc.Parse("XX?XX"));
            Check(WildcardSelection.Embedded | WildcardSelection.MultiWildcard, "X*X*XX", wc.Parse("X*X*XX"));
            Check(WildcardSelection.Embedded | WildcardSelection.MultiWildcard, "X*XX", wc.Parse("X**XX"));
            Check(WildcardSelection.Embedded | WildcardSelection.MultiWildcard | WildcardSelection.StartsWith, "XX*XX*", wc.Parse("XX*XX*"));

            Check(WildcardSelection.Contains | WildcardSelection.MultiWildcard, "*X*", wc.Parse("*X*"));
            Check(WildcardSelection.Contains | WildcardSelection.MultiWildcard | WildcardSelection.SingleWildcard, "*X?", wc.Parse("*X?"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Contains, "*X*", wc.Parse("**X*"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Contains, "*X*", wc.Parse("*X**"));
        }

        [Test]
        public void Parse_5_InvalidCharacters()
        {
            var wc = new Wildcard(WildcardSelection.BothAll, singleWildcard: '_', charactersNotAllowed: new char[] { '?' });
            Check(WildcardSelection.SingleWildcard | WildcardSelection.InvalidCharacter | WildcardSelection.StartsWith, "X?_", wc.Parse("X?_"));
        }

        [Test]
        public void Parse_6_SpaceTreatment()
        {
            var wc = new Wildcard(WildcardSelection.MultiAll, spaceTreatment: WildcardSpaceTreatment.Compress);
            Check(WildcardSelection.Equal, "X X", wc.Parse("X X")); 
            Check(WildcardSelection.Equal, "X X", wc.Parse("X  X"));
            Check(WildcardSelection.Equal, "X X X", wc.Parse("X  X  X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.EndsWith, "*X X", wc.Parse("*X X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.StartsWith, "X X*", wc.Parse("X  X*"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded, "X X* X", wc.Parse("X  X*  X"));

            wc = new Wildcard(WildcardSelection.MultiAll, spaceTreatment: WildcardSpaceTreatment.MultiWildcardAlways);
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded, "X*X", wc.Parse("X X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded, "X*X", wc.Parse("X  X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded, "X*X*X", wc.Parse("X  X  X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded | WildcardSelection.EndsWith, "*X*X", wc.Parse("*X X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded | WildcardSelection.StartsWith, "X*X*", wc.Parse("X  X*"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded, "X*X*X", wc.Parse("X  X*  X"));

            wc = new Wildcard(WildcardSelection.MultiAll, spaceTreatment: WildcardSpaceTreatment.MultiWildcardWhenOthers);
            Check(WildcardSelection.Equal, "X X", wc.Parse("X X"));
            Check(WildcardSelection.Equal, "X X", wc.Parse("X  X"));
            Check(WildcardSelection.Equal, "X X X", wc.Parse("X  X  X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded | WildcardSelection.EndsWith, "*X*X", wc.Parse("*X X"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded | WildcardSelection.StartsWith, "X*X*", wc.Parse("X  X*"));
            Check(WildcardSelection.MultiWildcard | WildcardSelection.Embedded, "X*X*X", wc.Parse("X  X*  X"));
        }

        private void Check(WildcardSelection selection, string text, WildcardResult result)
        {
            Assert.AreEqual(selection, result.Selection);
            Assert.AreEqual(text, result.Text);
        }

        #endregion

        #region Validate

        [Test]
        public void Validate_1_Default()
        {
            var wc = Wildcard.BothAll;
            Assert.IsTrue(wc.Validate(null));
            Assert.IsTrue(wc.Validate(string.Empty));
            Assert.IsTrue(wc.Validate("X"));
            Assert.IsTrue(wc.Validate("*"));
            Assert.IsTrue(wc.Validate("?"));
            Assert.IsTrue(wc.Validate("XX"));
            Assert.IsTrue(wc.Validate("*X"));
            Assert.IsTrue(wc.Validate("?X"));
            Assert.IsTrue(wc.Validate("X*"));
            Assert.IsTrue(wc.Validate("X?"));
            Assert.IsTrue(wc.Validate("XXX"));
            Assert.IsTrue(wc.Validate("X*X"));
            Assert.IsTrue(wc.Validate("X?X"));
            Assert.IsTrue(wc.Validate("*?*"));
            Assert.IsTrue(wc.Validate("*X*"));
        }

        [Test]
        public void Validate_2_CharactersNotAllowed()
        {
            var wc = new Wildcard(WildcardSelection.BothAll, multiWildcard: '%', singleWildcard: '_', charactersNotAllowed: new char[] { '*', '?' });
            Assert.IsTrue(wc.Validate(null));
            Assert.IsTrue(wc.Validate(string.Empty));
            Assert.IsTrue(wc.Validate("X"));
            Assert.IsFalse(wc.Validate("*"));
            Assert.IsFalse(wc.Validate("?"));
            Assert.IsTrue(wc.Validate("XX"));
            Assert.IsFalse(wc.Validate("*X"));
            Assert.IsFalse(wc.Validate("?X"));
            Assert.IsFalse(wc.Validate("X*"));
            Assert.IsFalse(wc.Validate("X?"));
            Assert.IsTrue(wc.Validate("XXX"));
            Assert.IsFalse(wc.Validate("X*X"));
            Assert.IsFalse(wc.Validate("X?X"));
            Assert.IsFalse(wc.Validate("*?*"));
            Assert.IsFalse(wc.Validate("*X*"));
        }

        [Test]
        public void Validate_3_EndWildcardOnly()
        {
            var wc = new Wildcard(WildcardSelection.EndsWith | WildcardSelection.MultiWildcard | WildcardSelection.SingleWildcard, singleWildcard: Wildcard.SingleWildcardCharacter);
            Assert.IsFalse(wc.Validate(null));
            Assert.IsFalse(wc.Validate(string.Empty));
            Assert.IsFalse(wc.Validate("X"));
            Assert.IsFalse(wc.Validate("*"));
            Assert.IsFalse(wc.Validate("?"));
            Assert.IsFalse(wc.Validate("XX"));
            Assert.IsTrue(wc.Validate("*X"));
            Assert.IsTrue(wc.Validate("?X"));
            Assert.IsFalse(wc.Validate("X*"));
            Assert.IsFalse(wc.Validate("X?"));
            Assert.IsFalse(wc.Validate("XXX"));
            Assert.IsFalse(wc.Validate("X*X"));
            Assert.IsFalse(wc.Validate("X?X"));
            Assert.IsFalse(wc.Validate("*?*"));
            Assert.IsFalse(wc.Validate("*X*"));
        }

        [Test]
        public void Validate_4_StartWildcardOnly()
        {
            var wc = new Wildcard(WildcardSelection.StartsWith | WildcardSelection.MultiWildcard | WildcardSelection.SingleWildcard, singleWildcard: Wildcard.SingleWildcardCharacter);
            Assert.IsFalse(wc.Validate(null));
            Assert.IsFalse(wc.Validate(string.Empty));
            Assert.IsFalse(wc.Validate("X"));
            Assert.IsFalse(wc.Validate("*"));
            Assert.IsFalse(wc.Validate("?"));
            Assert.IsFalse(wc.Validate("XX"));
            Assert.IsFalse(wc.Validate("*X"));
            Assert.IsFalse(wc.Validate("?X"));
            Assert.IsTrue(wc.Validate("X*"));
            Assert.IsTrue(wc.Validate("X?"));
            Assert.IsFalse(wc.Validate("XXX"));
            Assert.IsFalse(wc.Validate("X*X"));
            Assert.IsFalse(wc.Validate("X?X"));
            Assert.IsFalse(wc.Validate("*?*"));
            Assert.IsFalse(wc.Validate("*X*"));
        }

        [Test]
        public void Validate_5_EmbeddedWildcardOnly()
        {
            var wc = new Wildcard(WildcardSelection.Embedded | WildcardSelection.MultiWildcard | WildcardSelection.SingleWildcard, singleWildcard: Wildcard.SingleWildcardCharacter);
            Assert.IsFalse(wc.Validate(null));
            Assert.IsFalse(wc.Validate(string.Empty));
            Assert.IsFalse(wc.Validate("X"));
            Assert.IsFalse(wc.Validate("*"));
            Assert.IsFalse(wc.Validate("?"));
            Assert.IsFalse(wc.Validate("XX"));
            Assert.IsFalse(wc.Validate("*X"));
            Assert.IsFalse(wc.Validate("?X"));
            Assert.IsFalse(wc.Validate("X*"));
            Assert.IsFalse(wc.Validate("X?"));
            Assert.IsFalse(wc.Validate("XXX"));
            Assert.IsTrue(wc.Validate("X*X"));
            Assert.IsTrue(wc.Validate("X?X"));
            Assert.IsFalse(wc.Validate("*?*"));
            Assert.IsFalse(wc.Validate("*X*"));
        }

        [Test]
        public void Validate_6_SingleOrMultiWildcard()
        {
            var wc = new Wildcard(WildcardSelection.Embedded | WildcardSelection.MultiWildcard, singleWildcard: Wildcard.SingleWildcardCharacter);
            Assert.IsTrue(wc.Validate("X*X"));
            Assert.IsFalse(wc.Validate("X?X"));

            wc = new Wildcard(WildcardSelection.Embedded | WildcardSelection.SingleWildcard, singleWildcard: Wildcard.SingleWildcardCharacter);
            Assert.IsFalse(wc.Validate("X*X"));
            Assert.IsTrue(wc.Validate("X?X"));
        }

        [Test]
        public void Validate_7_NoneAndEqual()
        {
            var wc = new Wildcard(WildcardSelection.None | WildcardSelection.Equal, singleWildcard: Wildcard.SingleWildcardCharacter);
            Assert.IsTrue(wc.Validate(null));
            Assert.IsTrue(wc.Validate(string.Empty));
            Assert.IsTrue(wc.Validate("X"));
            Assert.IsFalse(wc.Validate("*"));
            Assert.IsFalse(wc.Validate("?"));
            Assert.IsTrue(wc.Validate("XX"));
            Assert.IsFalse(wc.Validate("*X"));
            Assert.IsFalse(wc.Validate("?X"));
            Assert.IsFalse(wc.Validate("X*"));
            Assert.IsFalse(wc.Validate("X?"));
            Assert.IsTrue(wc.Validate("XXX"));
            Assert.IsFalse(wc.Validate("X*X"));
            Assert.IsFalse(wc.Validate("X?X"));
            Assert.IsFalse(wc.Validate("*?*"));
            Assert.IsFalse(wc.Validate("*X*"));
        }

        #endregion

        #region WhereWildcard

        private class Person
        {
            public string First { get; set; }
            public string Last { get; set; }
        }

        private List<Person> GetPeople()
        {
            return new List<Person>
            {
                new Person { First = "Amy", Last = "Johnson" },
                new Person { First = "Jenny", Last = "Smith" },
                new Person { First = "Gerry", Last = "McQuire" },
                new Person { First = "Gary", Last = "Lawson" },
                new Person { First = "Simon", Last = "Reynolds" },
                new Person { First = "Amanada", Last = "Gray" },
                new Person { First = "B", Last = "P" },
                new Person { First = null, Last = null }
            };
        }

        [Test]
        public void IEnumerableExtensions_WhereWildcard()
        {
            // None (all).
            Assert.AreEqual(8, GetPeople().WhereWildcard(x => x.First, null).Select(x => x.Last).Count());
            Assert.AreEqual(8, GetPeople().WhereWildcard(x => x.First, "").Select(x => x.Last).Count());

            // Equal.
            Assert.AreEqual("Reynolds", GetPeople().WhereWildcard(x => x.First, "SIMON").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().WhereWildcard(x => x.First, "SIMON", ignoreCase: false).SingleOrDefault());

            // Single (all).
            Assert.AreEqual(8, GetPeople().WhereWildcard(x => x.First, "*").Select(x => x.Last).Count());

            // Starts with.
            Assert.AreEqual("Reynolds", GetPeople().WhereWildcard(x => x.First, "SI*").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().WhereWildcard(x => x.First, "SI*", ignoreCase: false).SingleOrDefault());

            // Ends with.
            Assert.AreEqual("Reynolds", GetPeople().WhereWildcard(x => x.First, "*ON").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().WhereWildcard(x => x.First, "*ON", ignoreCase: false).SingleOrDefault());

            // Contains.
            Assert.AreEqual("Reynolds", GetPeople().WhereWildcard(x => x.First, "*IM*").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().WhereWildcard(x => x.First, "*IM*", ignoreCase: false).SingleOrDefault());

            // Regex-based: embedded.
            Assert.AreEqual("Reynolds", GetPeople().WhereWildcard(x => x.First, "S*N").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().WhereWildcard(x => x.First, "S*N", ignoreCase: false).Select(x => x.Last).SingleOrDefault());

            // Regex-based: single-char match.
            Assert.AreEqual("Lawson", GetPeople().WhereWildcard(x => x.First, "G?RY", wildcard: Wildcard.BothAll).Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().WhereWildcard(x => x.First, "G?RY", ignoreCase: false, wildcard: Wildcard.BothAll).Select(x => x.Last).SingleOrDefault());

            // Regex-based: single-char all.
            Assert.IsNull(GetPeople().Where(x => true).WhereWildcard(x => x.First, " ? ").Select(x => x.Last).SingleOrDefault());
            Assert.AreEqual("P", GetPeople().Where(x => true).WhereWildcard(x => x.First, " ? ", wildcard: Wildcard.BothAll).Select(x => x.Last).SingleOrDefault());
        }

        [Test]
        public void IQueryableExtensions_WhereWildcard()
        {
            // None(all).
            Assert.AreEqual(8, GetPeople().AsQueryable().WhereWildcard(x => x.First, null).Select(x => x.Last).Count());
            Assert.AreEqual(8, GetPeople().AsQueryable().WhereWildcard(x => x.First, "").Select(x => x.Last).Count());

            // Equal.
            Assert.AreEqual("Reynolds", GetPeople().AsQueryable().WhereWildcard(x => x.First, "SIMON").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().AsQueryable().WhereWildcard(x => x.First, "SIMON", ignoreCase: false).SingleOrDefault());

            // Single (all).
            Assert.AreEqual(8, GetPeople().AsQueryable().WhereWildcard(x => x.First, "*").Select(x => x.Last).Count());

            // Starts with.
            Assert.AreEqual("Reynolds", GetPeople().AsQueryable().WhereWildcard(x => x.First, "SI*").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().AsQueryable().WhereWildcard(x => x.First, "SI*", ignoreCase: false).SingleOrDefault());

            // Ends with.
            Assert.AreEqual("Reynolds", GetPeople().AsQueryable().WhereWildcard(x => x.First, "*ON").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().AsQueryable().WhereWildcard(x => x.First, "*ON", ignoreCase: false).SingleOrDefault());

            // Contains.
            Assert.AreEqual("Reynolds", GetPeople().AsQueryable().WhereWildcard(x => x.First, "*IM*").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().AsQueryable().WhereWildcard(x => x.First, "*IM*", ignoreCase: false).SingleOrDefault());

            // Embedded.
            ExpectException.Throws<InvalidOperationException>("Wildcard selection text is not supported.",
                () => GetPeople().AsQueryable().WhereWildcard(x => x.First, "S*N").Select(x => x.Last).SingleOrDefault());

            ExpectException.Throws<InvalidOperationException>("Wildcard selection text is not supported.",
                () => GetPeople().AsQueryable().WhereWildcard(x => x.First, "S*N", ignoreCase: false).Select(x => x.Last).SingleOrDefault());

            // Single-char all; '?' is ignored.
            Assert.IsNull(GetPeople().AsQueryable().Where(x => true).WhereWildcard(x => x.First, " ? ").Select(x => x.Last).SingleOrDefault());
            Assert.IsNull(GetPeople().AsQueryable().Where(x => true).WhereWildcard(x => x.First, " ? ", ignoreCase: false).Select(x => x.Last).SingleOrDefault());
        }

        #endregion
    }
}