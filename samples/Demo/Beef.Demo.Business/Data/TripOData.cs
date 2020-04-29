using Beef.Data.OData;
using System;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Trip</b> OData endpoint.
    /// </summary>
    public class TripOData : OData<TripOData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestOData"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI string.</param>
        public TripOData(Uri baseUri) : base(baseUri) => IsPagingGetCountSupported = false;
    }
}