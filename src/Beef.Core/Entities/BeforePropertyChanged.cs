// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.ComponentModel;

namespace Beef.Entities
{
    /// <summary>
    /// Provides data for the <see cref="EntityBasicBase.BeforePropertyChanged"/> event.
    /// </summary>
    public class BeforePropertyChangedEventArgs : PropertyChangedEventArgs
    {
        private object _newValue;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforePropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="newValue">The new value.</param>
        public BeforePropertyChangedEventArgs(string propertyName, object newValue)
            : base(propertyName)
        {
            _newValue = newValue;
        }

        /// <summary>
        /// Gets the <b>new</b> value.
        /// </summary>
        public object NewValue
        {
            get { return _newValue; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="EntityBasicBase.BeforePropertyChanged"/> event raised when a property is changed on a component.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="BeforePropertyChangedEventArgs"/> that contains the event data.</param>
    public delegate void BeforePropertyChangedEventHandler(object sender, BeforePropertyChangedEventArgs e);
}
