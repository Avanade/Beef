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
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class EntityBase : EntityBasicBase, IEditableObject, ICloneable, ICopyFrom, ICleanUp, IChangeTrackingLogging
    {
        private object? _editCopy;

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

            if (from is not T val)
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
        protected static T? CopyOrClone<T>(T? from, T? to) where T : class
        {
            if (from == default)
                return default!;

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

        /// <summary>
        /// Determines whether the specified <paramref name="obj"/> is equal to the current instance by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        /// <remarks>At the <see cref="EntityBase"/> level no properties are checked therefore assumed always equals; this method is intended to be overridden.</remarks>
        public override bool Equals(object obj) => obj != null;

        /// <summary>
        /// Returns a hash code for the <see cref="EntityBase"/> (always returns the same value regardless; inheritors should override).
        /// </summary>
        /// <returns>A hash code for the <see cref="EntityBase"/>.</returns>
        public override int GetHashCode() => 0;

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
        public virtual void CleanUp() { }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        public abstract bool IsInitial { get; }

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
        public StringCollection? ChangeTracking { get; private set; }

        /// <summary>
        /// Indicates whether entity is currently <see cref="ChangeTracking"/>; <see cref="TrackChanges"/> and <see cref="IChangeTracking.AcceptChanges"/>.
        /// </summary>
        [JsonIgnore()]
        public bool IsChangeTracking => ChangeTracking != null;

        #endregion
    }
}