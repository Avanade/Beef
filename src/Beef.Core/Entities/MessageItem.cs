// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a <see cref="MessageItem"/>.
    /// </summary>
    [DebuggerDisplay("Type = {Type}, Text = {Text}, Property = {Property}")]
    [JsonObject(MemberSerialization.OptIn)]
    public class MessageItem : EntityBase
	{
		private MessageType _type;
		private string _text;
		private string _property;
        private object _tag;

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
                Text = string.Format(format, values)
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageItem"/> with the specified <see cref="Property"/>, <see cref="MessageType"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateMessage(string property, MessageType type, LText text)
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
        public static MessageItem CreateMessage(string property, MessageType type, LText format, params object[] values)
        {
            return new MessageItem
            {
                Property = property,
                Type = type,
                Text = string.Format(format, values)
            };
        }

        /// <summary>
        /// Creates a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> with the specified <see cref="Property"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public static MessageItem CreateErrorMessage(string property, LText text)
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
        public static MessageItem CreateErrorMessage(string property, LText format, params object[] values)
        {
            return new MessageItem
            {
                Property = property,
                Type = MessageType.Error,
                Text = string.Format(format, values)
            };
        }

        #endregion

        #region Properties

		/// <summary>
		/// Gets the message severity validatorType.
		/// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public MessageType Type
		{
			get { return _type; }
            set { SetValue<MessageType>(ref _type, value, false, false, TypeProperty); }
		}

		/// <summary>
		/// Gets or sets the message text.
		/// </summary>
        [JsonProperty("text")]
        public string Text
		{
			get { return _text; }
			set { SetValue<string>(ref _text, value, false, false, TextProperty); }
		}

		/// <summary>
		/// Gets or sets the name of the property that the message relates to.
		/// </summary>
        [JsonProperty("property", NullValueHandling = NullValueHandling.Ignore)]
        public string Property
		{
			get { return _property; }
			set { SetValue<string>(ref _property, value, false, false, PropertyProperty); }
        }

        /// <summary>
        /// Gets or sets an optional user tag associated with the message.
        /// </summary>
        /// <remarks>Note: This property is not serialized/deserialized.</remarks>
        public object Tag
        {
            get { return _tag; }
            set { SetValue<object>(ref _tag, value, false, false, TagProperty); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copies from another <see cref="MessageItem"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="MessageItem"/> to copy from.</param>
        public void CopyFrom(MessageItem from)
        {
            if (from == null)
                throw new ArgumentNullException("from");

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
            MessageItem clone = new MessageItem();
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Performs a clean-up of the <see cref="MessageItem"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Type = Cleaner.Clean<MessageType>(Type);
            Text = Cleaner.Clean<string>(Text);
            Property = Cleaner.Clean<string>(Property);
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
        public override string ToString()
		{
			return Text;
        }

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

        #region PropertyNames

        /// <summary>
        /// Represents the <see cref="Type"/> property name.
        /// </summary>
        public const string TypeProperty = "Type";

        /// <summary>
        /// Represents the <see cref="Text"/> property name.
        /// </summary>
        public const string TextProperty = "Text";

        /// <summary>
        /// Represents the <see cref="Property"/> property name.
        /// </summary>
        public const string PropertyProperty = "Property";

        /// <summary>
        /// Represents the <see cref="Tag"/> property name.
        /// </summary>
        public const string TagProperty = "Tag";

        #endregion
    }
}