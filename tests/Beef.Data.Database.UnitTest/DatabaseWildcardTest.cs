using NUnit.Framework;

namespace Beef.Data.Database.UnitTest
{
    [TestFixture]
    public class DatabaseWildcardTest
    {
        [Test]
        public void Replace_Support_Both()
        {
            var dw = new DatabaseWildcard(Wildcard.BothAll);

            Assert.AreEqual("%", dw.Replace("*"));
            Assert.AreEqual("_", dw.Replace("?"));
            Assert.AreEqual("%_%", dw.Replace("*?*"));
            Assert.AreEqual("%", dw.Replace("**"));
            Assert.AreEqual("__", dw.Replace("??"));
            Assert.AreEqual("%A%", dw.Replace("*A*"));
            Assert.AreEqual("%A%B%", dw.Replace("*A*B*"));
            Assert.AreEqual("%A_", dw.Replace("*A?"));
            Assert.AreEqual("_A_B%", dw.Replace("?A?B*"));
            Assert.AreEqual("%A B%", dw.Replace("*A B*"));
            Assert.AreEqual("%A  B%", dw.Replace("*A  B*"));
            Assert.AreEqual("%A B", dw.Replace("*A B  "));
            Assert.AreEqual("%A B %", dw.Replace("*A B * "));
            Assert.AreEqual("% A % B %", dw.Replace(" * A * B * "));
            Assert.AreEqual("% A % B %", dw.Replace(" ** A ** B ** "));

            Assert.AreEqual("[%]", dw.Replace("%"));
            Assert.AreEqual("[_]", dw.Replace("_"));
            Assert.AreEqual("[[]", dw.Replace("["));
            Assert.AreEqual("]", dw.Replace("]"));

            Assert.AreEqual("X[%]X", dw.Replace("X%X"));
            Assert.AreEqual("X[_]X", dw.Replace("X_X"));
            Assert.AreEqual("X[[]X", dw.Replace("X[X"));
            Assert.AreEqual("X]X", dw.Replace("X]X"));
        }

        [Test]
        public void Replace_Support_MultiOnly()
        {
            var dw = new DatabaseWildcard(Wildcard.MultiAll);

            Assert.AreEqual("%", dw.Replace("*"));
            Assert.AreEqual("?", dw.Replace("?"));
            Assert.AreEqual("%?%", dw.Replace("*?*"));
            Assert.AreEqual("%", dw.Replace("**"));
            Assert.AreEqual("??", dw.Replace("??"));
            Assert.AreEqual("%A%", dw.Replace("*A*"));
            Assert.AreEqual("%A%B%", dw.Replace("*A*B*"));
            Assert.AreEqual("%A?", dw.Replace("*A?"));
            Assert.AreEqual("?A?B%", dw.Replace("?A?B*"));
            Assert.AreEqual("%A B%", dw.Replace("*A B*"));
            Assert.AreEqual("%A  B%", dw.Replace("*A  B*"));
            Assert.AreEqual("%A B", dw.Replace("*A B  "));
            Assert.AreEqual("%A B %", dw.Replace("*A B * "));
            Assert.AreEqual("% A % B %", dw.Replace(" * A * B * "));
            Assert.AreEqual("% A % B %", dw.Replace(" ** A ** B ** "));

            Assert.AreEqual("[%]", dw.Replace("%"));
            Assert.AreEqual("[_]", dw.Replace("_"));
            Assert.AreEqual("[[]", dw.Replace("["));
            Assert.AreEqual("]", dw.Replace("]"));

            Assert.AreEqual("X[%]X", dw.Replace("X%X"));
            Assert.AreEqual("X[_]X", dw.Replace("X_X"));
            Assert.AreEqual("X[[]X", dw.Replace("X[X"));
            Assert.AreEqual("X]X", dw.Replace("X]X"));
        }
    }
}