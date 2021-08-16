// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a <see cref="MessageItem"/>.
    /// </summary>
    [DebuggerDisplay("Type = {Type}, Text = {Text}, Property = {Property}")]
    [JsonObject(MemberSerialization.OptIn)]
    [System.Diagnostics.DebuggerStepThrough]
    public class MessageItem : EntityBase, IEquatable<MessageItem>
    {
        private MessageType _type;
        private string? _text;
        private string? _property;
        private object? _tag;

        #region Static

        /// <summary>
        /// Creates a new <see cref="MessageItem"/> with a specified <see cref="MessageType"/> and text.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateMessage(MessageType type, LText text)
        {
            return new MessageItem
            {
                Type = type,
                Text = text
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageItem"/> with a specified <see cref="MessageType"/>, text format and and additional values included in the text.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateMessage(MessageType type, LText format, params object[] values)
        {
            return new MessageItem
            {
                Type = type,
                Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, format, values)
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageItem"/> with the specified <see cref="Property"/>, <see cref="MessageType"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateMessage(string? property, MessageType type, LText text)
        {
            return new MessageItem
            {
                Property = property,
                Type = type,
                Text = text
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageItem"/> with the specified <see cref="Property"/>, <see cref="MessageType"/>, text format and additional values included in the text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateMessage(string? property, MessageType type, LText format, params object?[] values)
        {
            return new MessageItem
            {
                Property = property,
                Type = type,
                Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, format, values)
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> with the specified <see cref="Property"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateErrorMessage(string? property, LText text)
        {
            return new MessageItem
            {
                Property = property,
                Type = MessageType.Error,
                Text = text
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> with the specified <see cref="Property"/>, text format and additional values included in the text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateErrorMessage(string property, LText format, params object?[] values)
        {
            return new MessageItem
            {
                Property = property,
                Type = MessageType.Error,
                Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, format, values)
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the message severity validatorType.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType Type
        {
            get { return _type; }
            set { SetValue<MessageType>(ref _type, value, false, false, nameof(Type)); }
        }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        [JsonProperty("text")]
        public string? Text
        {
            get { return _text; }
            set { SetValue(ref _text, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Text)); }
        }

        /// <summary>
        /// Gets or sets the name of the property that the message relates to.
        /// </summary>
        [JsonProperty("property", NullValueHandling = NullValueHandling.Ignore)]
        public string? Property
        {
            get { return _property; }
            set { SetValue(ref _property, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Property)); }
        }

        /// <summary>
        /// Gets or sets an optional user tag associated with the message.
        /// </summary>
        /// <remarks>Note: This property is not serialized/deserialized.</remarks>
        public object? Tag
        {
            get { return _tag; }
            set { SetValue(ref _tag, value, false, false, nameof(Tag)); }
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj != null && obj is MessageItem val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="MessageItem"/> is equal to the current <see cref="MessageItem"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(MessageItem? value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null)
                return false;

            return base.Equals((object)value)
                && Equals(Type, value.Type)
                && Equals(Text, value.Text)
                && Equals(Property, value.Property)
                && Equals(Tag, value.Tag);
        }

        /// <summary>
        /// Compares two <see cref="MessageItem"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="MessageItem"/> A.</param>
        /// <param name="b"><see cref="MessageItem"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator ==(MessageItem? a, MessageItem? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="MessageItem"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="MessageItem"/> A.</param>
        /// <param name="b"><see cref="MessageItem"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator !=(MessageItem? a, MessageItem? b) => !Equals(a, b);

        /// <summary>
        /// Returns a hash code for the <see cref="MessageItem"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="MessageItem"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Type);
            hash.Add(Text);
            hash.Add(Property);
            hash.Add(Tag);
            return base.GetHashCode() ^ hash.ToHashCode();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copies from another <see cref="MessageItem"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="MessageItem"/> to copy from.</param>
        public void CopyFrom(MessageItem from)
        {
            Check.NotNull(from, nameof(from));
            base.CopyFrom(from);
            Type = from.Type;
            Text = from.Text;
            Property = from.Property;
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="MessageItem"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="MessageItem"/>.</returns>
        public override object Clone()
        {
            MessageItem clone = new();
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Performs a clean-up of the <see cref="MessageItem"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Type = Cleaner.Clean(Type);
            Text = Cleaner.Clean(Text);
            Property = Cleaner.Clean(Property);
        }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial => false;

        /// <summary>
        /// Returns the message <see cref="Text"/>.
        /// </summary>
        /// <returns>The message <see cref="Text"/>.</returns>
        public override string? ToString() => Text;

        /// <summary>
        /// Sets the <see cref="Property"/> and returns <see cref="MessageItem"/> instance to enable fluent-style.
        /// </summary>
        /// <param name="property">The name of the property that the message relates to.</param>
        /// <returns>This <see cref="MessageItem"/> instance.</returns>
        public MessageItem SetProperty(string property)
        {
            Property = property;
            return this;
        }

        #endregion
    }
}