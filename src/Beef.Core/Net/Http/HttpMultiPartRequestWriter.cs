// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Net.Http
{
    /// <summary>
    /// Represents an HTTP multi-part request writer.
    /// </summary>
    public sealed class HttpMultiPartRequestWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private Guid _batchId;
        private Guid _changeSetId;
        private int _changeSetCount = 1;
        private bool _isClosed = false;
        private readonly bool _isChangeSet = false;
        private bool _isHeaderOutput = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultiPartRequestWriter"/> class.
        /// </summary>
        /// <param name="writer">The underlying <see cref="TextReader"/> to read from.</param>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="isChangeSet">Indicates whether the batch is a change set (an atomic unit of work).</param>
        public HttpMultiPartRequestWriter(TextWriter writer, Guid batchId, bool isChangeSet = false)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _batchId = batchId;
            _isChangeSet = isChangeSet;
        }

        /// <summary>
        /// Writes the <see cref="HttpRequestMessage"/> to the request stream.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        public async Task WriteAsync(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!_isHeaderOutput)
            {
                _isHeaderOutput = true;
                if (_isChangeSet)
                {
                    _changeSetId = Guid.NewGuid();
                    await _writer.WriteLineAsync($"--batch_{_batchId.ToString()}").ConfigureAwait(false);
                    await _writer.WriteLineAsync($"Content-Type: multipart/mixed; boundary=changeset_{_changeSetId}").ConfigureAwait(false);
                    await _writer.WriteLineAsync().ConfigureAwait(false);
                }
            }

            if (_isChangeSet)
                await _writer.WriteLineAsync($"--changeset_{_changeSetId}").ConfigureAwait(false);
            else
                await _writer.WriteLineAsync($"--batch_{_batchId}").ConfigureAwait(false);

            await _writer.WriteLineAsync("Content-Type: application/http").ConfigureAwait(false);
            await _writer.WriteLineAsync("Content-Transfer-Encoding: binary").ConfigureAwait(false);
            if (_isChangeSet)
                await _writer.WriteLineAsync($"Content-ID: {_changeSetCount++}").ConfigureAwait(false);

            await _writer.WriteLineAsync().ConfigureAwait(false);
            await _writer.WriteLineAsync($"{request.Method.Method} {request.RequestUri.ToString()} HTTP/1.1").ConfigureAwait(false);

            foreach (var h in request.Headers)
            {
                foreach (var v in h.Value)
                {
                    await _writer.WriteLineAsync($"{h.Key}: {v}").ConfigureAwait(false);
                }
            }

            if (request.Content != null)
            {
                foreach (var h in request.Content.Headers)
                {
                    foreach (var v in h.Value)
                    {
                        await _writer.WriteLineAsync($"{h.Key}: {v}").ConfigureAwait(false);
                    }
                }

                await _writer.WriteLineAsync().ConfigureAwait(false);
                await _writer.WriteLineAsync(await request.Content.ReadAsStringAsync().ConfigureAwait(false)).ConfigureAwait(false);
                await _writer.WriteLineAsync().ConfigureAwait(false);
            }

            await _writer.WriteLineAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Closes the reader after finalizing the batch (and optional changeset).
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task CloseAsync()
        {
            if (!_isClosed)
            {
                _isClosed = true;
                if (_isChangeSet)
                    await _writer.WriteLineAsync($"--changeset_{_changeSetId}--").ConfigureAwait(false);

                await _writer.WriteLineAsync($"--batch_{_batchId}--").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Disposes of all resources.
        /// </summary>
        /// <remarks>This will not invoke the <see cref="CloseAsync"/>; this must be called explicitly.</remarks>
        public void Dispose() { }
    }
}
