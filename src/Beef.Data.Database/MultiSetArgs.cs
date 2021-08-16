// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper;
using Beef.RefData;
using System;
using System.Collections.Generic;

namespace Beef.Data.Database
{
    /// <summary>
    /// Enables the <b>Database</b> multi-set arguments
    /// </summary>
    public interface IMultiSetArgs
    {
        /// <summary>
        /// Gets the minimum number of rows allowed.
        /// </summary>
        int MinRows { get; }

        /// <summary>
        /// Gets the maximum number of rows allowed.
        /// </summary>
        int? MaxRows { get; }

        /// <summary>
        /// Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).
        /// </summary>
        bool StopOnNull { get; }

        /// <summary>
        /// Gets the predicate that when returns <c>true</c> indicates to stop further query result set processing.
        /// </summary>
        Func<DatabaseRecord, bool>? StopOnPredicate { get; }

        /// <summary>
        /// Gets the single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.
        /// </summary>
        OperationTypes OperationType { get; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        void DatasetRecord(DatabaseRecord dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        void InvokeResult();
    }

    /// <summary>
    /// Enables the <b>Database</b> multi-set arguments with a <see cref="Mapper"/>.
    /// </summary>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public interface IMultiSetArgs<TItem> : IMultiSetArgs
        where TItem : class, new()
    {
        /// <summary>
        /// Gets the <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        IDatabaseMapper<TItem> Mapper { get; }
    }

    /// <summary>
    /// Provides the base <b>Database</b> multi-set arguments when expecting a single item/record only.
    /// </summary>
    public abstract class MultiSetSingleArgs : IMultiSetArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetSingleArgs"/> class.
        /// </summary>
        /// <param name="isMandatory">Indicates whether the value is mandatory; defaults to <c>true</c>.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        /// <param name="stopOnPredicate">The predicate that when returns <c>true</c> indicates to stop further query result set processing.</param>
        protected MultiSetSingleArgs(bool isMandatory = true, bool stopOnNull = false, Func<DatabaseRecord, bool>? stopOnPredicate = null)
        {
            IsMandatory = isMandatory;
            StopOnNull = stopOnNull;
            StopOnPredicate = stopOnPredicate;
        }

        /// <summary>
        /// Indicates whether the value is mandatory; i.e. a corresponding record must be read.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.
        /// </summary>
        public OperationTypes OperationType { get; set; } = OperationTypes.Get;

        /// <summary>
        /// Gets or sets the minimum number of rows allowed.
        /// </summary>
        public int MinRows => IsMandatory ? 1 : 0;

        /// <summary>
        /// Gets or sets the maximum number of rows allowed.
        /// </summary>
        public int? MaxRows => 1;

        /// <summary>
        /// Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).
        /// </summary>
        public bool StopOnNull { get; set; }

        /// <summary>
        /// Gets the predicate that when returns <c>true</c> indicates to stop further query result set processing.
        /// </summary>
        public Func<DatabaseRecord, bool>? StopOnPredicate { get; set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public abstract void DatasetRecord(DatabaseRecord dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public virtual void InvokeResult() { }
    }

    /// <summary>
    /// Provides the <b>Database</b> multi-set arguments when expecting a single item/record only.
    /// </summary>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public class MultiSetSingleArgs<TItem> : MultiSetSingleArgs, IMultiSetArgs<TItem>
        where TItem : class, new()
    {
        private TItem? _value;
        private readonly Action<TItem> _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetSingleArgs{TItem}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.</param>
        /// <param name="result">The action that will be invoked with the result of the set.</param>
        /// <param name="isMandatory">Indicates whether the value is mandatory; defaults to <c>true</c>.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        /// <param name="stopOnPredicate">The predicate that when returns <c>true</c> indicates to stop further query result set processing.</param>
        public MultiSetSingleArgs(IDatabaseMapper<TItem> mapper, Action<TItem> result, bool isMandatory = true, bool stopOnNull = false, Func<DatabaseRecord, bool>? stopOnPredicate = null)
            : base(isMandatory, stopOnNull, stopOnPredicate)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            _result = Check.NotNull(result, nameof(result));
        }

        /// <summary>
        /// Gets the <see cref="DatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        public IDatabaseMapper<TItem> Mapper { get; private set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public override void DatasetRecord(DatabaseRecord dr)
        {
            _value = Mapper.MapFromDb(dr, OperationType, null!);
        }

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public override void InvokeResult()
        {
            if (_value != null)
                _result(_value);
        }
    }

    /// <summary>
    /// Provides the base <b>Database</b> multi-set arguments when expecting a collection of items/records.
    /// </summary>
    public abstract class MultiSetCollArgs : IMultiSetArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetCollArgs"/> class.
        /// </summary>
        /// <param name="minRows">The minimum number of rows allowed.</param>
        /// <param name="maxRows">The maximum number of rows allowed.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        /// <param name="stopOnPredicate">The predicate that when returns <c>true</c> indicates to stop further query result set processing.</param>
        protected MultiSetCollArgs(int minRows = 0, int? maxRows = null, bool stopOnNull = false, Func<DatabaseRecord, bool>? stopOnPredicate = null)
        {
            Check.IsTrue(!maxRows.HasValue || minRows <= maxRows.Value, nameof(maxRows), "Max Rows is less than Min Rows.");
            MinRows = minRows;
            MaxRows = maxRows;
            StopOnNull = stopOnNull;
            StopOnPredicate = stopOnPredicate;
        }

        /// <summary>
        /// Gets or sets the single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.
        /// </summary>
        public OperationTypes OperationType { get; set; } = OperationTypes.Get;

        /// <summary>
        /// Gets or sets the minimum number of rows allowed.
        /// </summary>
        public int MinRows { get; private set; }

        /// <summary>
        /// Gets or sets the maximum number of rows allowed.
        /// </summary>
        public int? MaxRows { get; private set; }

        /// <summary>
        /// Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).
        /// </summary>
        public bool StopOnNull { get; set; }

        /// <summary>
        /// Gets the predicate that when returns <c>true</c> indicates to stop further query result set processing.
        /// </summary>
        public Func<DatabaseRecord, bool>? StopOnPredicate { get; set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public abstract void DatasetRecord(DatabaseRecord dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public virtual void InvokeResult() { }
    }

    /// <summary>
    /// Provides the <b>Database</b> multi-set arguments when expecting a collection of items/records.
    /// </summary>
    /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public class MultiSetCollArgs<TColl, TItem> : MultiSetCollArgs, IMultiSetArgs<TItem>
        where TItem : class, new()
        where TColl : class, ICollection<TItem>, new()
    {
        private TColl? _coll;
        private readonly Action<TColl> _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetCollArgs{TColl, TItem}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.</param>
        /// <param name="result">The action that will be invoked with the result of the set.</param>
        /// <param name="minRows">The minimum number of rows allowed.</param>
        /// <param name="maxRows">The maximum number of rows allowed.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        /// <param name="stopOnPredicate">The predicate that when returns <c>true</c> indicates to stop further query result set processing.</param>
        public MultiSetCollArgs(IDatabaseMapper<TItem> mapper, Action<TColl> result, int minRows = 0, int? maxRows = null, bool stopOnNull = false, Func<DatabaseRecord, bool>? stopOnPredicate = null)
            : base(minRows, maxRows, stopOnNull, stopOnPredicate)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            _result = Check.NotNull(result, nameof(result));
        }

        /// <summary>
        /// Gets the <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        public IDatabaseMapper<TItem> Mapper { get; private set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public override void DatasetRecord(DatabaseRecord dr)
        {
            if (dr == null)
                throw new ArgumentNullException(nameof(dr));

            if (_coll == null)
                _coll = new TColl();

            var item = Mapper.MapFromDb(dr, OperationType, null!);
            if (item != null)
                _coll.Add(item);
        }

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public override void InvokeResult()
        {
            if (_coll != null)
                _result(_coll);
        }
    }

    /// <summary>
    /// Provides the <b>Database</b> multi-set arguments when expecting a collection of <see cref="ReferenceDataSidListBase"/> items.
    /// </summary>
    /// <typeparam name="TSid">The <b>Serialization Identifier</b> (SID) <see cref="Type"/>; supports only: <see cref="String"/>, <see cref="Int32"/> and <see cref="Guid"/>.</typeparam>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> item <see cref="Type"/>.</typeparam>
    public class MultiSetCollReferenceDataSidArgs<TItem, TSid> : MultiSetCollArgs, IMultiSetArgs
        where TItem : ReferenceDataBase, new()
    {
        private readonly string _columnName;
        private List<TItem>? _coll;
        private readonly Action<IEnumerable<TItem>> _result;
        private readonly ReferenceDataIdTypeCode _idTypeCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetCollReferenceDataSidArgs{TItem, TSid}"/> class.
        /// </summary>
        /// <param name="columnName">The column name for the reference data identifier.</param>
        /// <param name="result">The action that will be invoked with the result of the set.</param>
        /// <param name="minRows">The minimum number of rows allowed.</param>
        /// <param name="maxRows">The maximum number of rows allowed.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        /// <param name="stopOnPredicate">The predicate that when returns <c>true</c> indicates to stop further query result set processing.</param>
        public MultiSetCollReferenceDataSidArgs(string columnName, Action<IEnumerable<TItem>> result, int minRows = 0, int? maxRows = null, bool stopOnNull = false, Func<DatabaseRecord, bool>? stopOnPredicate = null)
            : base(minRows, maxRows, stopOnNull, stopOnPredicate)
        {
            _columnName = Check.NotEmpty(columnName, nameof(columnName));
            _result = Check.NotNull(result, nameof(result));

            _idTypeCode = ReferenceDataBase.GetIdTypeCode(typeof(TItem));
        }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public override void DatasetRecord(DatabaseRecord dr)
        {
            if (dr == null)
                throw new ArgumentNullException(nameof(dr));

            if (_coll == null)
                _coll = new List<TItem>();

            switch (_idTypeCode)
            {
                case ReferenceDataIdTypeCode.Int32:
                    _coll.Add((TItem)ReferenceDataManager.Current[typeof(TItem)].GetById(dr.GetValue<int>(_columnName))!);
                    break;

                case ReferenceDataIdTypeCode.Int64:
                    _coll.Add((TItem)ReferenceDataManager.Current[typeof(TItem)].GetById(dr.GetValue<long>(_columnName))!);
                    break;

                case ReferenceDataIdTypeCode.Guid:
                    _coll.Add((TItem)ReferenceDataManager.Current[typeof(TItem)].GetById(dr.GetValue<Guid>(_columnName))!);
                    break;

                case ReferenceDataIdTypeCode.String:
                    _coll.Add((TItem)ReferenceDataManager.Current[typeof(TItem)].GetById(dr.GetValue<string>(_columnName))!);
                    break;
            }
        }

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public override void InvokeResult()
        {
            if (_coll != null)
                _result(_coll);
        }
    }
}