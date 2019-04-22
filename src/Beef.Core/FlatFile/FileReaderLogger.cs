// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Entities;
using System;
using System.Linq;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the passed logging <b>data</b> (passed in the <see cref="Logger"/> shortcut methods ending in <b>2</b>).
    /// </summary>
    public class FileReaderLoggerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileReaderLoggerData"/> class.
        /// </summary>
        /// <param name="operationResult">The <see cref="FileOperationResult"/>.</param>
        /// <param name="fileRecord">The corresponding <see cref="T:FileRecord"/> to the logged message.</param>
        /// <param name="messageItem">The specific <see cref="T:MessageItem"/> from the <see cref="FileRecord"/> being logged.</param>
        public FileReaderLoggerData(FileOperationResult operationResult, FileRecord fileRecord = null, MessageItem messageItem = null)
        {
            OperationResult = operationResult ?? throw new ArgumentNullException(nameof(operationResult));
            FileRecord = fileRecord;
            MessageItem = messageItem;
        }

        /// <summary>
        /// Gets the <see cref="FileOperationResult"/>.
        /// </summary>
        public FileOperationResult OperationResult { get; private set; }

        /// <summary>
        /// Gets the corresponding <see cref="T:FileRecord"/> to the logged message (where applicable).
        /// </summary>
        public FileRecord FileRecord { get; private set; }

        /// <summary>
        /// Gets the specific <see cref="T:MessageItem"/> from the <see cref="FileRecord"/> being logged (where applicable).
        /// </summary>
        public MessageItem MessageItem { get; private set; }
    }

    /// <summary>
    /// Provides a standardised implementation of a <see cref="FileReaderBase"/> logger; designed for attaching the <see cref="LogFileOperation"/> to the
    /// <see cref="FileReaderBase.RecordRead"/> event. Intercepts the <see cref="FileOperationEventArgs"/> and writes applicable log messages based on the outcomes of the read
    /// including passing a <see cref="FileReaderLoggerData"/> as the logging <b>data</b> (passed in the <see cref="Logger"/> shortcut methods ending in <b>2</b>).
    /// </summary>
    public class FileReaderLogger
    {
        /// <summary>
        /// Gets the <b>default</b> instance.
        /// </summary>
        public static FileReaderLogger Default { get; } = new FileReaderLogger();

        /// <summary>
        /// Gets or sets the valid <see cref="FileContentStatus.Header"/> message.
        /// </summary>
        /// <remarks>A <c>null</c> indicates that a message is not to be logged.</remarks>
        public string HeaderValidMessage { get; set; } = "Header record encountered and validated successfully.";

        /// <summary>
        /// Gets or sets the invalid <see cref="FileContentStatus.Header"/> message.
        /// </summary>
        /// <remarks>A <c>null</c> indicates that a message is not to be logged.</remarks>
        public string HeaderInvalidMessage { get; set; } = "Header record encountered and validated with error(s).";

        /// <summary>
        /// Gets or sets the valid <see cref="FileContentStatus.Trailer"/> message.
        /// </summary>
        /// <remarks>A <c>null</c> indicates that a message is not to be logged.</remarks>
        public string TrailerValidMessage { get; set; } = "Trailer record encountered and validated successfully.";

        /// <summary>
        /// Gets or sets the invalid <see cref="FileContentStatus.Trailer"/> message.
        /// </summary>
        /// <remarks>A <c>null</c> indicates that a message is not to be logged.</remarks>
        public string TrailerInvalidMessage { get; set; } = "Trailer record encountered and validated with error(s).";

        /// <summary>
        /// Gets or sets the valid <see cref="FileContentStatus.Content"/> message. 
        /// </summary>
        /// <remarks>A <c>null</c> indicates that a message is not to be logged.</remarks>
        public string ContentValidMessage { get; set; } = null;

        /// <summary>
        /// Gets or sets the invalid <see cref="FileContentStatus.Content"/> message.
        /// </summary>
        public string ContentInvalidMessage { get; set; } = null;

        /// <summary>
        /// Gets or sets the <see cref="FileContentStatus.EndOfFile"/> message.
        /// </summary>
        /// <remarks>A <c>null</c> indicates that a message is not to be logged. The <see cref="FileOperationResult.TotalLines"/> is automatically passed to the string formatter to
        /// allow inclusion within message.</remarks>
        public string EndOfFileMessage { get; set; } = "Total number of records: {0}";

        /// <summary>
        /// Uses the <see cref="FileReaderBase.RecordRead"/> event to write standardised log messages.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileOperationEventArgs"/>.</param>
        public void LogFileOperation(object sender, FileOperationEventArgs e)
        {
            var or = e.OperationResult;

            switch (or.Status)
            {
                case FileContentStatus.Header:
                    if (or.HasErrors)
                    {
                        if (HeaderInvalidMessage != null)
                            Logger.Default.Warning2(new FileReaderLoggerData(or), HeaderInvalidMessage);
                    }
                    else
                    {
                        if (HeaderValidMessage != null)
                            Logger.Default.Info2(new FileReaderLoggerData(or), HeaderValidMessage);
                    }

                    break;

                case FileContentStatus.Trailer:
                    if (or.HasErrors)
                    {
                        if (TrailerInvalidMessage != null)
                            Logger.Default.Warning2(new FileReaderLoggerData(or), TrailerInvalidMessage);
                    }
                    else
                    {
                        if (TrailerValidMessage != null)
                            Logger.Default.Info2(new FileReaderLoggerData(or), TrailerValidMessage);
                    }

                    break;

                case FileContentStatus.Content:
                    if (or.HasErrors)
                    {
                        if (ContentInvalidMessage != null)
                            Logger.Default.Warning2(new FileReaderLoggerData(or), ContentInvalidMessage);
                    }
                    else
                    {
                        if (ContentValidMessage != null)
                            Logger.Default.Info2(new FileReaderLoggerData(or), ContentValidMessage);
                    }

                    break;

                case FileContentStatus.EndOfFile:
                    if (EndOfFileMessage != null)
                        Logger.Default.Info2(new FileReaderLoggerData(or), string.Format(EndOfFileMessage, e.OperationResult.TotalLines));

                    break;
            }

            // Log any/all corresponding messages.
            if (or.Records != null && or.Records.Length > 0)
            {
                foreach (var rec in or.Records.Where(x => x.HasMessages))
                {
                    foreach (var msg in rec.Messages)
                    {
                        switch (msg.Type)
                        {
                            case MessageType.Info:
                                Logger.Default.Info2(new FileReaderLoggerData(or, rec, msg), msg.Text);
                                break;

                            case MessageType.Warning:
                                Logger.Default.Warning2(new FileReaderLoggerData(or, rec, msg), msg.Text);
                                break;

                            case MessageType.Error:
                                Logger.Default.Error2(new FileReaderLoggerData(or, rec, msg), msg.Text);
                                break;
                        }
                    }
                }
            }
        }
    }
}
