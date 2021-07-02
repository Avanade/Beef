// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Net.Http
{
    /// <summary>
    /// Represents an HTTP multi-part response reader.
    /// </summary>
    public sealed class HttpMultiPartResponseReader : IDisposable
    {
        private static readonly MultiPartReadState[] DataOrEmptyStates = new MultiPartReadState[] { MultiPartReadState.Data, MultiPartReadState.Empty };

        private readonly TextReader _reader;
        private bool _isClosed = false;
        private string? _line = null;
        private MultiPartReadState _state;
        private bool _isEndOfStream = false;
        private bool _isInBatch = true;
        private bool _isInChangeSet = false;
        private bool _isFirstRead = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultiPartResponseReader"/> class.
        /// </summary>
        /// <param name="reader">The underlying <see cref="TextReader"/> to read from.</param>
        public HttpMultiPartResponseReader(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        /// Reads the next <see cref="HttpResponseMessage"/> from the response stream.
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/> where found; otherwise, <c>null</c>.</returns>
        public async Task<HttpResponseMessage?> ReadNextAsync()
        {
            /* Note: Yes I know I used a couple of GOTOs - generally I am not a fan either (bad practice yada yada)!
             * - I debated it for a while but for the purposes of the logic here I am inclined to keep it as it simplified the code - YMMV. */

            if (_isFirstRead)
            {
                _isFirstRead = false;

                // Must start at a boundary.
                if ((await ReadNextLineAsync().ConfigureAwait(false)) != MultiPartReadState.StartOfBatch)
                    throw new InvalidOperationException("The multipart response part must begin with a valid '--batchresponse_' boundary.");
            }

            if (_isClosed)
                throw new InvalidOperationException("The reader has been closed; no further reading is supported.");

            if (_isEndOfStream)
                return null;

            Start:
            var multiPart = await ReadNextPartAsync().ConfigureAwait(false);

            CheckState:
            switch (_state)
            {
                case MultiPartReadState.Data:
                case MultiPartReadState.Empty:
                    throw new InvalidOperationException("The multipart response is malformed.");

                case MultiPartReadState.StartOfBatch:
                    if (_isInChangeSet)
                        throw new InvalidOperationException("The start of a batch boundary is unexpected; response is malformed.");

                    _isInBatch = true;
                    break;

                case MultiPartReadState.StartOfChangeSet:
                    if (!multiPart.IsResponseMessage && !_isInChangeSet)
                    {
                        _isInChangeSet = true;
                        goto Start;
                    }

                    if (multiPart.IsResponseMessage && _isInChangeSet)
                        break;

                    throw new InvalidOperationException("The start of a changeset boundary is unexpected; response is malformed.");

                case MultiPartReadState.EndOfChangeSet:
                    if (!_isInChangeSet)
                        throw new InvalidOperationException("The end of a changeset boundary is unexpected; response is malformed.");

                    _isInChangeSet = false;
                    await ReadNextLineAsync(true).ConfigureAwait(false);
                    goto CheckState;

                case MultiPartReadState.EndOfBatch:
                    if (_isInChangeSet || !_isInBatch)
                        throw new InvalidOperationException("The end of a batch boundary is unexpected; response is malformed.");

                    _isInBatch = false;
                    await ReadNextLineAsync(true).ConfigureAwait(false);
                    goto CheckState;

                case MultiPartReadState.EndOfStream:
                    if (_isInBatch)
                        throw new InvalidOperationException("The end of a the stream encountered without a corresponding end of batch; response is malformed.");

                    _isEndOfStream = true;
                    break;
            }

            return multiPart.CreateResponseMessage();
        }

        /// <summary>
        /// Reads and parses the next response part.
        /// </summary>
        private async Task<MultiPart> ReadNextPartAsync()
        {
            var multiPart = new MultiPart();

            // Response headers.
            while (CheckValidReadStates(await ReadNextLineAsync().ConfigureAwait(false), true, DataOrEmptyStates) == MultiPartReadState.Data)
            {
                if (!TryParseHeader(_line!, out KeyValuePair<string, string[]> header))
                    throw new InvalidOperationException("The multipart response header is malformed.");

                multiPart.ResponseHeaders.Add(header.Key, header.Value);
            }

            // Check headers to determine if multipart changeset.
            if (multiPart.ResponseHeaders.ContainsKey("Content-Type"))
            {
                var val = multiPart.ResponseHeaders["Content-Type"];
                if (val.Length > 0 && val[0] == "multipart/mixed")
                {
                    var isInvalidMultiPart = true;
                    if (val.Length > 1)
                    {
                        var nvp = SplitNameValuePair(val[1]);
                        if (nvp.Item1 == "boundary" && nvp.Item2.StartsWith("changesetresponse_", StringComparison.OrdinalIgnoreCase))
                            isInvalidMultiPart = false;
                    }

                    if (isInvalidMultiPart)
                        throw new InvalidOperationException("The multipart response has a Content-Type of multipart/mixed with an unexpected boundary.");

                    await ReadNextLineAsync(true).ConfigureAwait(false);
                    return multiPart;
                }
            }

            // Response status.
            CheckValidReadStates(await ReadNextLineAsync().ConfigureAwait(false), true, MultiPartReadState.Data);
            if (!TryParseStatusCode(_line!, out HttpStatusCode statusCode))
                throw new InvalidOperationException("Unexpected HTTP status code; response is malformed.");

            multiPart.StatusCode = statusCode;

            // Content headers.
            while (CheckValidReadStates(await ReadNextLineAsync().ConfigureAwait(false), true, DataOrEmptyStates) == MultiPartReadState.Data)
            {
                if (!TryParseHeader(_line!, out KeyValuePair<string, string[]> header))
                    throw new InvalidOperationException("The multipart response header is malformed.");

                multiPart.ContentHeaders.Add(header.Key, header.Value);
            }

            // Content.
            while (CheckValidReadStates(await ReadNextLineAsync().ConfigureAwait(false), false, MultiPartReadState.EndOfStream) == MultiPartReadState.Data)
            {
                multiPart.Content.AppendLine(_line);
            }

            // Move to non-empty state.
            if (_state == MultiPartReadState.Empty)
                await ReadNextLineAsync(true).ConfigureAwait(false);

            multiPart.IsResponseMessage = true;
            return multiPart;
        }

        /// <summary>
        /// Reads the next line and determines state.
        /// </summary>
        private async Task<MultiPartReadState> ReadNextLineAsync(bool ignoreEmpty = false)
        {
            return _state = await ReadNextLineInternalAsync(ignoreEmpty).ConfigureAwait(false);
        }

        /// <summary>
        /// Actually reads the next line and determines state.
        /// </summary>
        private async Task<MultiPartReadState> ReadNextLineInternalAsync(bool ignoreEmpty)
        {
            while (true)
            {
                _line = await _reader.ReadLineAsync().ConfigureAwait(false);
                if (_line == null)
                    return MultiPartReadState.EndOfStream;
                else if (_line.Length == 0)
                {
                    if (ignoreEmpty)
                        continue;
                    else
                        return MultiPartReadState.Empty;
                }
                else if (_line.StartsWith("--batchresponse_", StringComparison.OrdinalIgnoreCase))
                    return _line.EndsWith("--", StringComparison.OrdinalIgnoreCase) ? MultiPartReadState.EndOfBatch : MultiPartReadState.StartOfBatch;
                else if (_line.StartsWith("--changesetresponse_", StringComparison.OrdinalIgnoreCase))
                    return _line.EndsWith("--", StringComparison.OrdinalIgnoreCase) ? MultiPartReadState.EndOfChangeSet : MultiPartReadState.StartOfChangeSet;
                else
                    return MultiPartReadState.Data;
            }
        }

        /// <summary>
        /// Checks that the read state is valid.
        /// </summary>
        private static MultiPartReadState CheckValidReadStates(MultiPartReadState state, bool statesAreValid, params MultiPartReadState[] states)
        {
            if (states == null || states.Length == 0)
                return state;

            if (states.Contains(state))
                return statesAreValid ? state : throw new InvalidOperationException("Unexpected stream data; response is malformed.");
            else
                return !statesAreValid ? state : throw new InvalidOperationException("Unexpected stream data; response is malformed.");
        }

        /// <summary>
        /// Parses a header string into appropriate parts.
        /// </summary>
        private static bool TryParseHeader(string text, out KeyValuePair<string, string[]> header)
        {
            var i = text.IndexOf(": ", StringComparison.OrdinalIgnoreCase);
            if (i < 0)
            {
                header = new KeyValuePair<string, string[]>();
                return false;
            }

            var key = text.Substring(0, i);
            var val = text.Substring(i + 2);

            if (string.IsNullOrEmpty(val))
                header = new KeyValuePair<string, string[]>(key, Array.Empty<string>());
            else
                header = new KeyValuePair<string, string[]>(key, val.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return true;
        }

        /// <summary>
        /// Parses the status code.
        /// </summary>
        private static bool TryParseStatusCode(string text, out HttpStatusCode statusCode)
        {
            statusCode = HttpStatusCode.BadRequest;
            var parts = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return false;

            if (parts[0] != "HTTP/1.1")
                return false;

            return Enum.TryParse<HttpStatusCode>(parts[1], out statusCode);
        }

        /// <summary>
        /// Splits a name=value pair.
        /// </summary>
        private static Tuple<string, string> SplitNameValuePair(string text)
        {
            var parts = text.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return new Tuple<string, string>(text, string.Empty);
            else
                return new Tuple<string, string>(parts[0], parts[1]);
        }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        public void Close()
        {
            _isClosed = true;
        }

        /// <summary>
        /// Disposes of all resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #region Internal 

        /// <summary>
        /// Provides the read state.
        /// </summary>
        private enum MultiPartReadState
        {
            StartOfBatch,
            StartOfChangeSet,
            Empty,
            Data,
            EndOfChangeSet,
            EndOfBatch,
            EndOfStream
        }

        /// <summary>
        /// Multi-part interim data bag.
        /// </summary>
        private class MultiPart
        {
            public Dictionary<string, string[]> ResponseHeaders = new();
            public bool IsResponseMessage;
            public HttpStatusCode StatusCode = HttpStatusCode.BadRequest;
            public Dictionary<string, string[]> ContentHeaders = new();
            public StringBuilder Content = new();

            /// <summary>
            /// Create the corresponding <see cref="HttpResponseMessage"/>.
            /// </summary>
            /// <returns></returns>
            public HttpResponseMessage CreateResponseMessage()
            {
                var response = new HttpResponseMessage(StatusCode);
                foreach (var h in ResponseHeaders)
                {
                    response.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                if (Content.Length > 0)
                {
                    var sc = new StringContent(Content.ToString());
                    foreach (var h in ContentHeaders)
                    {
                        sc.Headers.TryAddWithoutValidation(h.Key, h.Value);
                    }

                    response.Content = sc;
                }

                return response;
            }
        }

        #endregion
    }
}