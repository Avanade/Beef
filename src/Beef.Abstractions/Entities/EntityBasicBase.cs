// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Beef.Entities
{
    /// <summary>
    /// Represents the basic <b>Entity</b> class with <see cref="INotifyPropertyChanged"/> support.
    /// </summary>
    /// <remarks>The <see cref="EntityBasicBase"/> is not thread-safe; it does however, place a lock around all <b>set</b> operations to minimise concurrency challenges.</remarks>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class EntityBasicBase : INotifyPropertyChanged
    {
        internal const string ValueIsImmutableMessage = "Value is immutable; cannot be changed once already set to a value.";
        internal const string EntityIsReadOnlyMessage = "Entity is read only; property cannot be changed.";

        private readonly object _lock = new();
        private Dictionary<string, PropertyChangedEventHandler>? _propertyEventHandlers;

        /// <summary>
        /// Gets the corresponding <see cref="RefData.ReferenceDataBase"/> <see cref="RefData.ReferenceDataBase.Text"/> when <see cref="ExecutionContext.IsRefDataTextSerializationEnabled"/>.
        /// </summary>
        /// <param name="refData">The function to get the <see cref="RefData.ReferenceDataBase"/> instance.</param>
        /// <returns>The corresponding <see cref="RefData.ReferenceDataBase.Text"/> where applicable; otherwise, <c>null</c>.</returns>
        public static string? GetRefDataText(Func<RefData.ReferenceDataBase?> refData) 
            => ExecutionContext.HasCurrent && ExecutionContext.Current.IsRefDataTextSerializationEnabled ? refData?.Invoke()?.Text : null;

        /// <summary>
        /// Indicates whether the <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised when a property is set with a value that is the same as the existing; 
        /// unless overridden (see <see cref="NotifyChangesWhenSameValue"/>) for a specific instance. Defaults to <c>false</c> indicating to <b>not</b> notify changes for same.
        /// </summary>
        public static bool ShouldNotifyChangesWhenSameValue { get; set; } = false;

        /// <summary>
        /// Indicates whether the <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised when a property is set with a value that is the same as the existing overriding
        /// the <see cref="ShouldNotifyChangesWhenSameValue"/> for the specific instance. A value of <c>null</c> indicates to use the <see cref="ShouldNotifyChangesWhenSameValue"/> setting.
        /// </summary>
        public bool? NotifyChangesWhenSameValue { get; set; } = null;

        /// <summary>
        /// Occurs before a property value is about to change.
        /// </summary>
        public event BeforePropertyChangedEventHandler? BeforePropertyChanged;

        /// <summary>
        /// Raises the <see cref="BeforePropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> indicates that the property change is to be cancelled; otherwise, <c>false</c>.</returns>
        protected virtual bool OnBeforePropertyChanged(string propertyName, object? newValue)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            if (BeforePropertyChanged != null)
            {
                var e = new BeforePropertyChangedEventArgs(propertyName, newValue);
                BeforePropertyChanged(this, e);
                if (e.Cancel)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Trigger the property(s) changed.
        /// </summary>
        /// <param name="propertyNames">The property names.</param>
        private void TriggerPropertyChanged(params string[] propertyNames)
        {
            IsChanged = true;

            foreach (string propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event (typically overridden with additional logic).
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        protected virtual void OnPropertyChanged(string propertyName) => RaisePropertyChanged(propertyName);

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event only (<see cref="OnPropertyChanged"/>).
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Check.NotNull(propertyName, nameof(propertyName))));

        /// <summary>
        /// Gets a property value (automatically instantiating new where current value is null).
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyValue">The property value to get.</param>
        static protected T GetAutoValue<T>(ref T propertyValue) where T : class, new()
        {
            if (propertyValue == null)
                propertyValue = new T();

            return propertyValue;
        }

        /// <summary>
        /// Sets a property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="bubblePropertyChanged">Indicates whether the value should bubble up property changes versus only recording within the sub-entity itself.</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue<T>(ref T propertyValue, T setValue, bool immutable = false, bool bubblePropertyChanged = false, params string[] propertyNames)
            => SetValue<T>(ref propertyValue, setValue, immutable, bubblePropertyChanged, null!, propertyNames);

        /// <summary>
        /// Sets a property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="bubblePropertyChanged">Indicates whether the value should bubble up property changes versus only recording within the sub-entity itself.</param>
        /// <param name="beforeChange">Function to invoke before changing the value; a result of <c>true</c> indicates that the property change is to be cancelled; otherwise, <c>false</c>.</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue<T>(ref T propertyValue, T setValue, bool immutable = false, bool bubblePropertyChanged = false, Func<T, bool>? beforeChange = null, params string[] propertyNames)
        {
            ValidateSetValuePropertyNames(propertyNames);

            lock (_lock)
            {
                // Check and see if the value has changed or not; exit if being set to same value.
                var isChanged = true;
                T val = Cleaner.Clean(setValue, false);
                if (propertyValue is IComparable<T>)
                {
                    if (Comparer<T>.Default.Compare(val, propertyValue) == 0)
                        isChanged = false;
                }
                else if (Equals(propertyValue, val))
                    isChanged = false;

                if (!isChanged && !RaisePropertyChangedWhenSame)
                    return false;

                // Test is read only.
                if (IsReadOnly)
                    return !isChanged ? false : throw new InvalidOperationException(EntityIsReadOnlyMessage);

                // Test immutability.
                if (immutable && isChanged && Comparer<T>.Default.Compare(propertyValue, default!) != 0)
                    throw new InvalidOperationException(ValueIsImmutableMessage);

                // Handle on before property changed.
                if (beforeChange != null)
                {
                    if (beforeChange.Invoke(val))
                        return false;
                }

                if (OnBeforePropertyChanged(propertyNames[0], val))
                    return false;

                // Determine bubbling and unwire old value.
                INotifyPropertyChanged? npc;
                if (bubblePropertyChanged && propertyValue != null)
                {
                    npc = propertyValue as INotifyPropertyChanged;
                    if (npc != null)
                        npc.PropertyChanged -= GetValue_PropertyChanged(propertyNames);
                }

                // Track changes for the new value where parent (this) is tracking.
                if (this is IChangeTrackingLogging ct && ct.IsChangeTracking && val is IChangeTrackingLogging sct && !sct.IsChangeTracking)
                    sct.TrackChanges();

                // Update the property and trigger the property changed.
                propertyValue = val;
                TriggerPropertyChanged(propertyNames);

                // Determine bubbling and wire up new value.
                if (bubblePropertyChanged && val != null)
                {
                    npc = val as INotifyPropertyChanged;
                    if (npc != null)
                        npc.PropertyChanged += GetValue_PropertyChanged(propertyNames);
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="PropertyChangedEventHandler"/> for the named property.
        /// </summary>
        private PropertyChangedEventHandler GetValue_PropertyChanged(params string[] propertyNames)
        {
            if (_propertyEventHandlers == null)
                _propertyEventHandlers = new Dictionary<string, PropertyChangedEventHandler>();

            if (!_propertyEventHandlers.ContainsKey(propertyNames[0]))
                _propertyEventHandlers.Add(propertyNames[0], (sender, e) => TriggerPropertyChanged(propertyNames));

            return _propertyEventHandlers[propertyNames[0]];
        }

        /// <summary>
        /// Sets a <see cref="string"/> property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="trim">The <see cref="StringTrim"/> (defaults to <see cref="StringTrim.End"/>).</param>
        /// <param name="transform">The <see cref="StringTransform"/> (defaults to <see cref="StringTransform.EmptyToNull"/>).</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue(ref string? propertyValue, string? setValue, bool immutable = false, StringTrim trim = StringTrim.End, StringTransform transform = StringTransform.EmptyToNull, params string[] propertyNames)
            => SetValue(ref propertyValue, setValue, immutable, trim, transform, null!, propertyNames);

        /// <summary>
        /// Sets a <see cref="string"/> property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="trim">The <see cref="StringTrim"/> (defaults to <see cref="StringTrim.UseDefault"/>).</param>
        /// <param name="transform">The <see cref="StringTransform"/> (defaults to <see cref="StringTransform.UseDefault"/>).</param>
        /// <param name="beforeChange">Function to invoke before changing the value; a result of <c>true</c> indicates that the property change is to be cancelled; otherwise, <c>false</c>.</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue(ref string? propertyValue, string? setValue, bool immutable = false, StringTrim trim = StringTrim.UseDefault, StringTransform transform = StringTransform.UseDefault, Func<string?, bool>? beforeChange = null, params string[] propertyNames)
        {
            ValidateSetValuePropertyNames(propertyNames);

            lock (_lock)
            {
                string? val = Cleaner.Clean(setValue, trim, transform);
                var isChanged = val != propertyValue;
                if (!RaisePropertyChangedWhenSame && !isChanged)
                    return false;

                if (IsReadOnly && isChanged)
                    throw new InvalidOperationException(EntityIsReadOnlyMessage);

                if (immutable && isChanged && propertyValue != null)
                    throw new InvalidOperationException(ValueIsImmutableMessage);

                if (beforeChange != null)
                {
                    if (beforeChange.Invoke(setValue))
                        return false;
                }

                if (OnBeforePropertyChanged(propertyNames[0], setValue))
                    return false;

                propertyValue = val!;
                TriggerPropertyChanged(propertyNames);

                return true;
            }
        }

        /// <summary>
        /// Sets a <see cref="DateTime"/> property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="transform">The <see cref="DateTimeTransform"/> to be applied (defaults to <see cref="DateTimeTransform.UseDefault"/>).</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue(ref DateTime propertyValue, DateTime setValue, bool immutable = false, DateTimeTransform transform = DateTimeTransform.UseDefault, params string[] propertyNames)
            => SetValue(ref propertyValue, setValue, immutable, transform, null, propertyNames);

        /// <summary>
        /// Sets a <see cref="DateTime"/> property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="transform">The <see cref="DateTimeTransform"/> to be applied (defaults to <see cref="DateTimeTransform.UseDefault"/>).</param>
        /// <param name="beforeChange">Function to invoke before changing the value; a result of <c>true</c> indicates that the property change is to be cancelled; otherwise, <c>false</c>.</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue(ref DateTime propertyValue, DateTime setValue, bool immutable = false, DateTimeTransform transform = DateTimeTransform.UseDefault, Func<DateTime, bool>? beforeChange = null, params string[] propertyNames)
        {
            ValidateSetValuePropertyNames(propertyNames);

            lock (_lock)
            {
                DateTime val = Cleaner.Clean(setValue, transform);
                var isChanged = val != propertyValue;
                if (!RaisePropertyChangedWhenSame && !isChanged)
                    return false;

                if (IsReadOnly && isChanged)
                    throw new InvalidOperationException(EntityIsReadOnlyMessage);

                if (immutable && isChanged && propertyValue != DateTime.MinValue)
                    throw new InvalidOperationException(ValueIsImmutableMessage);

                if (beforeChange != null)
                {
                    if (beforeChange.Invoke(setValue))
                        return false;
                }

                if (OnBeforePropertyChanged(propertyNames[0], setValue))
                    return false;

                propertyValue = val;
                TriggerPropertyChanged(propertyNames);
                return true;
            }
        }

        /// <summary>
        /// Sets a <see cref="Nullable{DateTime}"/> property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="transform">The <see cref="DateTimeTransform"/> to be applied (defaults to <see cref="DateTimeTransform.UseDefault"/>).</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue(ref DateTime? propertyValue, DateTime? setValue, bool immutable = false, DateTimeTransform transform = DateTimeTransform.UseDefault, params string[] propertyNames)
            => SetValue(ref propertyValue, setValue, immutable, transform, null!, propertyNames);

        /// <summary>
        /// Sets a <see cref="Nullable{DateTime}"/> property value and raises the <see cref="PropertyChanged"/> event where applicable.
        /// </summary>
        /// <param name="propertyValue">The property value to set.</param>
        /// <param name="setValue">The value to set.</param>
        /// <param name="immutable">Indicates whether the value is immutable; can not be changed once set.</param>
        /// <param name="transform">The <see cref="DateTimeTransform"/> to be applied (defaults to <see cref="DateTimeTransform.UseDefault"/>).</param>
        /// <param name="beforeChange">Function to invoke before changing the value; a result of <c>true</c> indicates that the property change is to be cancelled; otherwise, <c>false</c>.</param>
        /// <param name="propertyNames">The names of the properties that changed.</param>
        /// <returns><c>true</c> indicates that the property value changed; otherwise, <c>false</c>.</returns>
        /// <remarks>The first property name specified (see <paramref name="propertyNames"/>) is the primary property; therefore, the only property
        /// where the <see cref="BeforePropertyChanged"/> event is raised. The additional property names allow for the <see cref="PropertyChanged"/> 
        /// event to be raised for other properties where related versus having to raise seperately.</remarks>
        protected bool SetValue(ref DateTime? propertyValue, DateTime? setValue, bool immutable = false, DateTimeTransform transform = DateTimeTransform.UseDefault, Func<DateTime?, bool>? beforeChange = null, params string[] propertyNames)
        {
            ValidateSetValuePropertyNames(propertyNames);

            lock (_lock)
            {
                DateTime? val = Cleaner.Clean(setValue, transform);
                var isChanged = val != propertyValue;
                if (!RaisePropertyChangedWhenSame && !isChanged)
                    return false;

                if (IsReadOnly && isChanged)
                    throw new InvalidOperationException(EntityIsReadOnlyMessage);

                if (immutable && isChanged && propertyValue != null)
                    throw new InvalidOperationException(ValueIsImmutableMessage);

                if (beforeChange != null)
                {
                    if (beforeChange.Invoke(setValue))
                        return false;
                }

                if (OnBeforePropertyChanged(propertyNames[0], setValue))
                    return false;

                propertyValue = val;
                TriggerPropertyChanged(propertyNames);
                return true;
            }
        }

        /// <summary>
        /// Validate the set value property names list.
        /// </summary>
        private static void ValidateSetValuePropertyNames(params string[] propertyNames)
        {
            if (propertyNames.Length == 0)
                throw new ArgumentException("At least one property name must be specified.");
        }

        /// <summary>
        /// Indicates whether to raise the property changed event when same value by reviewing the current settings for <see cref="NotifyChangesWhenSameValue"/>
        /// and <see cref="ShouldNotifyChangesWhenSameValue"/>.
        /// </summary>
        protected bool RaisePropertyChangedWhenSame => NotifyChangesWhenSameValue ?? ShouldNotifyChangesWhenSameValue; 

        /// <summary>
        /// Resets the entity state to unchanged by accepting the changes.
        /// </summary>
        public virtual void AcceptChanges() => IsChanged = false;

        /// <summary>
        /// Indicates whether the entity has changed.
        /// </summary>
        public bool IsChanged { get; private set; }

        /// <summary>
        /// Indicates whether the entity is read only (see <see cref="MakeReadOnly"/>).
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Makes the entity readonly; such that it will no longer support any property changes (see <see cref="IsReadOnly"/>).
        /// </summary>
        public void MakeReadOnly() => IsReadOnly = true;
    }
}