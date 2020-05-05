using Beef.Data.OData;
using System;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Test</b> OData endpoint.
    /// </summary>
    public class TestOData : OData<TestOData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestOData"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI string.</param>
        public TestOData(Uri baseUri) : base(baseUri) { }
    }
}