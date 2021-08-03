// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Text;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a <see cref="MessageItem"/> collection.
    /// </summary>

    [System.Diagnostics.DebuggerStepThrough]
    public class MessageItemCollection : EntityBaseCollection<MessageItem>
    {
        /// <summary>
        /// Adds a new <see cref="MessageItem"/> for a specified <see cref="MessageType"/> and text.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem Add(MessageType type, LText text)
        {
            MessageItem item = MessageItem.CreateMessage(type, text);
            this.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a new <see cref="MessageItem"/> for a specified <see cref="MessageType"/>, text format and additional values included in the text.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem Add(MessageType type, LText format, params object[] values)
        {
            MessageItem item = MessageItem.CreateMessage(type, format, values);
            this.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a new <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/>, <see cref="MessageType"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem Add(string? property, MessageType type, LText text)
        {
            MessageItem item = MessageItem.CreateMessage(property, type, text);
            this.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a new <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/>, <see cref="MessageType"/>, text format and additional values included in the text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem Add(string? property, MessageType type, LText format, params object?[] values)
        {
            MessageItem item = MessageItem.CreateMessage(property, type, format, values);
            this.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> for a specified text.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddError(LText text)
        {
            return Add(MessageType.Error, text);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> for a specified text format and additional values included in the text.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddError(LText format, params object[] values)
        {
            return Add(MessageType.Error, format, values);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddError(string property, LText text)
        {
            return Add(property, MessageType.Error, text);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Error"/> <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/>, text format and and additional values included in the text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddError(string property, LText format, params object[] values)
        {
            return Add(property, MessageType.Error, format, values);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Warning"/> <see cref="MessageItem"/> for a specified text.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddWarning(LText text)
        {
            return Add(MessageType.Warning, text);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Warning"/> <see cref="MessageItem"/> for a specified text format and additional values included in the text.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddWarning(LText format, params object[] values)
        {
            return Add(MessageType.Warning, format, values);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Warning"/> <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddWarning(string property, LText text)
        {
            return Add(property, MessageType.Warning, text);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Warning"/> <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/>, text format and and additional values included in the text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddWarning(string property, LText format, params object[] values)
        {
            return Add(property, MessageType.Warning, format, values);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Info"/> <see cref="MessageItem"/> for a specified text.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddInfo(LText text)
        {
            return Add(MessageType.Info, text);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Info"/> <see cref="MessageItem"/> for a specified text format and additional values included in the text.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddInfo(LText format, params object[] values)
        {
            return Add(MessageType.Info, format, values);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Info"/> <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/> and text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="text">The message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddInfo(string property, LText text)
        {
            return Add(property, MessageType.Info, text);
        }

        /// <summary>
        /// Adds a new <see cref="MessageType.Info"/> <see cref="MessageItem"/> for the specified <see cref="MessageItem.Property"/>, text format and and additional values included in the text.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>A <see cref="MessageItem"/>.</returns>
        public MessageItem AddInfo(string property, LText format, params object[] values)
        {
            return Add(property, MessageType.Info, format, values);
        }

        /// <summary>
        /// Gets a new <see cref="MessageItemCollection"/> for a selected <see cref="MessageType"/>.
        /// </summary>
        /// <param name="type">Message validatorType.</param>
        /// <returns>A new <see cref="MessageItemCollection"/>.</returns>
        public MessageItemCollection GetMessagesForType(MessageType type)
        {
            MessageItemCollection msgs = new();

            foreach (MessageItem item in this)
            {
                if (item.Type == type)
                    msgs.Add(item);
            }

            return msgs;
        }

        /// <summary>
        /// Gets a new <see cref="MessageItemCollection"/> for a selected <see cref="MessageType"/> and <see cref="MessageItem.Property"/>.
        /// </summary>
        /// <param name="type">Message validatorType.</param>
        /// <param name="property">The name of the property that the message relates to.</param>
        /// <returns>A new <see cref="MessageItemCollection"/>.</returns>
        public MessageItemCollection GetMessagesForType(MessageType type, string property)
        {
            MessageItemCollection msgs = new();

            foreach (MessageItem item in this)
            {
                if (item.Type == type && item.Property == property)
                    msgs.Add(item);
            }

            return msgs;
        }

        /// <summary>
        /// Gets a new <see cref="MessageItemCollection"/> for a selected <see cref="MessageItem.Property"/>.
        /// </summary>
        /// <param name="property">The name of the property that the message relates to.</param>
        /// <returns>A new <see cref="MessageItemCollection"/>.</returns>
        public MessageItemCollection GetMessagesForProperty(string property)
        {
            MessageItemCollection msgs = new();

            foreach (MessageItem item in this)
            {
                if (item.Property == property)
                    msgs.Add(item);
            }

            return msgs;
        }

        /// <summary>
        /// Determines whether a message exists for a <see cref="MessageItem.Property"/> <see cref="MessageType.Error"/>.
        /// </summary>
        /// <param name="property">The name of the property that the message relates to.</param>
        /// <returns><c>true</c> if a message exists; otherwise, <c>false</c>.</returns>
        public bool ContainsError(string property)
        {
            return ContainsType(MessageType.Error, property);
        }

        /// <summary>
		/// Determines whether a message exists for a selected <see cref="MessageType"/>.
		/// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
		/// <returns><c>true</c> if a message exists; otherwise, <c>false</c>.</returns>
		public bool ContainsType(MessageType type)
        {
            foreach (MessageItem item in this)
            {
                if (item.Type == type)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a message exists for a selected <see cref="MessageType"/> and <see cref="MessageItem.Property"/>.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="property">The name of the property that the message relates to.</param>
        /// <returns><c>true</c> if a message exists; otherwise, <c>false</c>.</returns>
        public bool ContainsType(MessageType type, string property)
        {
            foreach (MessageItem item in this)
            {
                if (item.Type == type && item.Property == property)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a message exists for a selected <see cref="MessageItem.Property"/>.
        /// </summary>
        /// <param name="property">The name of the property that the message relates to.</param>
        /// <returns><c>true</c> if a message exists; otherwise, <c>false</c>.</returns>
        public bool ContainsProperty(string property)
        {
            foreach (MessageItem item in this)
            {
                if (item.Property == property)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="MessageItemCollection"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="MessageItemCollection"/>.</returns>
        public override object Clone()
        {
            MessageItemCollection clone = new();
            foreach (MessageItem item in this)
                clone.Add((MessageItem)item.Clone());

            return clone;
        }

        /// <summary>
        /// Outputs the list of messages as a <see cref="string"/>.
        /// </summary>
        /// <returns>The list of messages as a <see cref="string"/>.</returns>
        public override string ToString()
        {
            if (Count == 0)
                return new LText("None.");

            var sb = new StringBuilder();
            foreach (var item in this)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append($"{item.Type}: {item.Text}");
                if (!string.IsNullOrEmpty(item.Property))
                    sb.Append($" [{item.Property}]");
            }

            return sb.ToString();
        }
    }
}