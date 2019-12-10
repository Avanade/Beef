// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Beef.Entities
{
    /// <summary>
    /// Represents the base <b>Entity</b> class.
    /// </summary>
    public abstract class EntityBase : EntityBasicBase, IEditableObject, ICloneable, ICopyFrom, ICleanUp, IUniqueKey, IChangeTrackingLogging
    {
        private object _editCopy;

        #region Static

        /// <summary>
        /// Performs a <see cref="ICleanUp.CleanUp"/> on the <paramref name="values"/>.
        /// </summary>
        /// <param name="values">List of values to <see cref="ICleanUp.CleanUp"/>.</param>
        /// <remarks>Only cleans up values that implement <see cref="ICleanUp"/>; otherwise, they will remain unchanged.</remarks>
        public static void CleanUp(params object[] values)
        {
            if (values == null)
                return;

            foreach (object o in values)
            {
                if (o is ICleanUp value)
                    value.CleanUp();
            }
        }

        #endregion

        /// <summary>
        /// Performs a deep copy from another object updating this instance.
        /// </summary>
        /// <param name="from">The object to copy from.</param>
        public virtual void CopyFrom(object from)
        {
            ValidateCopyFromType<object>(from);
            CopyFrom((EntityBase)from);
        }

#pragma warning disable CA1801, CA1822, IDE0060 // Mark members as static; this is intended as an instance method.
        /// <summary>
        /// Performs a deep copy from another object updating this instance.
        /// </summary>
        /// <param name="from">The object to copy from.</param>
        public void CopyFrom(EntityBase from) { }
#pragma warning restore CA1801, CA1822, IDE0060

        /// <summary>
        /// Validates the <see cref="CopyFrom(object)"/> <see cref="Type"/> is valid.
        /// </summary>
        /// <typeparam name="T">The expected <see cref="Type"/>.</typeparam>
        /// <param name="from">The object to copy from.</param>
        /// <returns>The <paramref name="from"/> value casted to <typeparamref name="T"/>.</returns>
        protected static T ValidateCopyFromType<T>(object from) where T : class
        {
            Check.NotNull(from, nameof(from));

            if (!(from is T val))
                throw new ArgumentException($"Cannot copy from Type '{from?.GetType().FullName}' as it is incompatible.", nameof(from));

            return val;
        }

        /// <summary>
        /// Copies (<see cref="ICopyFrom"/>) or clones (<see cref="ICloneable"/>) the <paramref name="from"/> value.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="from">The from value.</param>
        /// <param name="to">The to value (required to support a <see cref="ICopyFrom.CopyFrom(object)"/>).</param>
        /// <returns>The resulting to value.</returns>
        /// <remarks>A <see cref="ICopyFrom.CopyFrom(object)"/> will be attempted first where supported, then a <see cref="ICloneable.Clone"/>; otherwise, a <see cref="InvalidOperationException"/> will be thrown.
        /// <i>Note:</i> <see cref="ICopyFrom"/> is not supported for collections.</remarks>
        /// <exception cref="InvalidOperationException">Thrown where neither <see cref="ICopyFrom"/>) or <see cref="ICloneable"/> are supported.</exception>
        protected static T CopyOrClone<T>(T from, T to) where T : class
        {
            if (from == null)
                return null;

            if (to == default && from is ICloneable c)
                return (T)c.Clone();
            else if (to is ICopyFrom cf)
            {
                cf.CopyFrom(from);
                return to;
            }
            else if (from is ICloneable c2)
                return (T)c2.Clone();

            throw new ArgumentException("The Type of the value must support ICopyFrom and/or ICloneable (minimum).", nameof(from));
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="EntityBase"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="EntityBase"/>.</returns>
        public abstract object Clone();

        #region IEditableObject

        /// <summary>
        /// Begins an edit on an entity.
        /// </summary>
        public void BeginEdit()
        {
            // Exit where already in edit mode.
            if (_editCopy != null)
                return;

            _editCopy = this.Clone();
        }

        /// <summary>
        /// Discards the entity changes since the last <see cref="BeginEdit"/>.
        /// </summary>
        /// <remarks>Resets the entity state to unchanged (see <see cref="AcceptChanges"/>) after the changes have been discarded.</remarks>
        public void CancelEdit()
        {
            if (_editCopy != null)
                CopyFrom(_editCopy);

            AcceptChanges();
        }

        /// <summary>
        /// Ends and commits the entity changes since the last <see cref="BeginEdit"/>.
        /// </summary>
        /// <remarks>Resets the entity state to unchanged (see <see cref="AcceptChanges"/>).</remarks>
        public void EndEdit()
        {
            if (_editCopy != null)
                AcceptChanges();
        }

        #endregion

        /// <summary>
        /// Resets the entity state to unchanged by accepting the changes (resets <see cref="ChangeTracking"/>).
        /// </summary>
        /// <remarks>Ends and commits the entity changes (see <see cref="EndEdit"/>).</remarks>
        public override void AcceptChanges()
        {
            base.AcceptChanges();
            _editCopy = null;
            ChangeTracking = null;
        }

        #region ICleanup

        /// <summary>
        /// Performs a clean-up of the <see cref="EntityBase"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public virtual void CleanUp()
        {
        }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        public abstract bool IsInitial { get; }

        #endregion

        #region IUniqueKey

        /// <summary>
        /// Indicates whether the <see cref="Object"/> has a <see cref="UniqueKey"/> value.
        /// </summary>
        public virtual bool HasUniqueKey
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the <see cref="UniqueKey"/>.
        /// </summary>
        public virtual UniqueKey UniqueKey
        {
            get { return UniqueKey.Empty; }
        }

#pragma warning disable CA1819 // Properties should not return arrays; by-design, acceptable usage for DTO's and is OK as changes cannot have a side-effect.
        /// <summary>
        /// Gets the list of property names that represent the unique key.
        /// </summary>
        public virtual string[] UniqueKeyProperties => Array.Empty<string>();
#pragma warning restore CA1819

        #endregion

        #region IChangeTracking

        /// <summary>
        /// Determines that until <see cref="AcceptChanges"/> is invoked property changes are to be logged (see <see cref="ChangeTracking"/>).
        /// </summary>
        public virtual void TrackChanges()
        {
            if (ChangeTracking == null)
                ChangeTracking = new StringCollection();
        }

        /// <summary>
        /// Listens to the <see cref="OnPropertyChanged"/> to perform <see cref="ChangeTracking"/>.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (ChangeTracking != null && !ChangeTracking.Contains(propertyName))
                ChangeTracking.Add(propertyName);
        }

        /// <summary>
        /// Lists the properties (names of) that have been changed (note that this property is not JSON serialized).
        /// </summary>
        [JsonIgnore()]
        public StringCollection ChangeTracking { get; private set; }

        /// <summary>
        /// Indicates whether entity is currently <see cref="ChangeTracking"/>; <see cref="TrackChanges"/> and <see cref="IChangeTracking.AcceptChanges"/>.
        /// </summary>
        [JsonIgnore()]
        public bool IsChangeTracking => ChangeTracking != null;

        #endregion
    }
}