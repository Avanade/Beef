// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Beef.Data.OData
{
    /// <summary>
    /// Enables the <b>OData</b> arguments capabilities.
    /// </summary>
    public interface IODataArgs
    {
        /// <summary>
        /// Overrides the default HTTP Method for the request.
        /// </summary>
        HttpMethod OverrideHttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IODataMapper"/>.
        /// </summary>
        IODataMapper Mapper { get; set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult Paging { get; }

        /// <summary>
        /// Indicates whether there are any additional <see cref="Headers"/>.
        /// </summary>
        bool HasHeaders { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        bool NullOnNotFoundResponse { get; }

        /// <summary>
        /// Gets or sets the <see cref="ODataIfMatch"/> condition.
        /// </summary>
        ODataIfMatch IfMatch { get; set; }

        /// <summary>
        /// Gets the Headers <see cref="NameValueCollection"/> to be added to the OData request.
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Gets or sets the <see cref="HttpRequestMessage"/>.
        /// </summary>
        HttpRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpResponseMessage"/>.
        /// </summary>
        HttpResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// Gets the <b>OData</b> query statement.
        /// </summary>
        /// <returns>The <b>OData</b> query statement.</returns>
        string GetODataQuery();
    }

    /// <summary>
    /// Provides the base <b>OData</b> arguments capabilities.
    /// </summary>
    public class ODataArgs : IODataArgs
    {
        private NameValueCollection _headers;

        /// <summary>
        /// Creates a new instance of the <see cref="ODataArgs"/> class.
        /// </summary>
        /// <returns>The <see cref="ODataArgs"/>.</returns>
        public static ODataArgs Create()
        {
            return new ODataArgs();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ODataArgs"/> class overridding the <see cref="HttpMethod"/>.
        /// </summary>
        /// <param name="overrideHttpMethod">The <see cref="HttpMethod"/>.</param>
        /// <returns>The <see cref="ODataArgs"/>.</returns>
        public static ODataArgs Create(HttpMethod overrideHttpMethod)
        {
            return new ODataArgs { OverrideHttpMethod = overrideHttpMethod };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ODataArgs"/> class overridding the <see cref="IfMatch"/>.
        /// </summary>
        /// <param name="ifMatch">The <see cref="HttpMethod"/>.</param>
        /// <returns>The <see cref="ODataArgs"/>.</returns>
        public static ODataArgs Create(ODataIfMatch ifMatch)
        {
            return new ODataArgs { IfMatch = ifMatch };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs"/> class.
        /// </summary>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public ODataArgs(PagingArgs paging) : this(new PagingResult(paging ?? throw new ArgumentNullException(nameof(paging)))) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs"/> class.
        /// </summary>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public ODataArgs(PagingResult paging = null)
        {
            Paging = paging;
        }

        /// <summary>
        /// Overrides the default <see cref="HttpMethod"/> for the request.
        /// </summary>
        public HttpMethod OverrideHttpMethod { get; set; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        public bool NullOnNotFoundResponse { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataIfMatch"/> condition.
        /// </summary>
        public ODataIfMatch IfMatch { get; set; } = ODataIfMatch.UpdateEtag;

        /// <summary>
        /// Gets or sets the <see cref="IODataMapper"/> (<see cref="ODataAutoMapper"/> is used where no value is specified).
        /// </summary>
        public IODataMapper Mapper { get; set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult Paging { get; private set; }

        /// <summary>
        /// Gets the <see cref="NameValueCollection"/> that contains any additional headers that are to be added to the OData request.
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                if (_headers == null)
                    _headers = new NameValueCollection();

                return _headers;
            }
        }

        /// <summary>
        /// Indicates whether there are any additional <see cref="Headers"/>.
        /// </summary>
        public bool HasHeaders { get => _headers != null && _headers.Count > 0; }

        /// <summary>
        /// Gets or sets the <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpResponseMessage"/>.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// Gets the <b>OData</b> query statement.
        /// </summary>
        /// <returns>The <b>OData</b> query statement.</returns>
        public virtual string GetODataQuery() => $"$select={Mapper.GetODataFieldNamesQuery()}";
    }

    /// <summary>
    /// Provides the typed-mapper <b>OData</b> arguments capabilities.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    public class ODataArgs<T> : ODataArgs where T : class, new()
    {
        private List<Tuple<IODataMapper, string>> _expandList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs{T}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IODataMapper{TSrce}"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public ODataArgs(IODataMapper<T> mapper, PagingArgs paging) : base(paging)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IODataMapper{TSrce}"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public ODataArgs(IODataMapper<T> mapper, PagingResult paging = null) : base(paging)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Chains one or more OData <b>$expand</b> query options to include the related entity(s); only applicable for a <see cref="ODataBase.GetAsync{T}(ODataArgs, IComparable[])"/>.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to the underlying property that references the related entity.</param>
        /// <param name="odataExpandName">Specifies the OData expand name (will default to configured OData entity name where <c>null</c>).</param>
        /// <returns>The <see cref="ODataArgs{T}"/> (enables fluent).</returns>
        public ODataArgs<T> Expand<TProperty>(Expression<Func<T, TProperty>> propertyExpression, string odataExpandName = null) where TProperty : class
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
                throw new InvalidOperationException("Only Member access expressions are supported.");

            var me = (MemberExpression)propertyExpression.Body;
            var map = Mapper.GetBySrcePropertyName(me.Member.Name);
            if (map == null || map.Mapper == null)
                throw new ArgumentException($"Expand Property '{me.Member.Name}' must have ODataPropertyMapper configuration with a corresponding complex type Mapper to enable.");

            if (_expandList == null)
                _expandList = new List<Tuple<IODataMapper, string>>();

            _expandList.Add(new Tuple<IODataMapper, string>((IODataMapper)map.Mapper, string.IsNullOrEmpty(odataExpandName) ? map.DestPropertyName : odataExpandName));
            return this;
        }

        /// <summary>
        /// Indicates whether one or more <see cref="Expand{TProperty}(Expression{Func{T, TProperty}}, string)"/> entities have been selected.
        /// </summary>
        public bool HasExpand => _expandList != null;

        /// <summary>
        /// Gets the <b>OData</b> query statement.
        /// </summary>
        /// <returns>The <b>OData</b> query statement.</returns>
        public override string GetODataQuery()
        {
            if (!HasExpand)
                return base.GetODataQuery();

            var sb = new StringBuilder(base.GetODataQuery());
            foreach (var map in _expandList)
            {
                sb.Append($"&$expand={map.Item2}($select={map.Item1.GetODataFieldNamesQuery()})");
            }

            return sb.ToString();
        }
    }
}
