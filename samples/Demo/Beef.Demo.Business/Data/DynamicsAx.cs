using Beef.Data.OData;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Dynamics 365 Operations (AX)</b> OData endpoint.
    /// </summary>
    public class DynamicsAx : OData<DynamicsAx>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsAx"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI string.</param>
        public DynamicsAx(string baseUri) : base(baseUri) { }
    }
}
