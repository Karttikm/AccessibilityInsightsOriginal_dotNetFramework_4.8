// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AccessibilityInsights.SetupLibrary.REST
{
    /// <summary>
    /// HttpClient-backed helper that exposes the resolved ResponseUri and a task-based download API.
    /// Uses SharedHttpClientFactory.CreateClient() by default so all callers share pooled HttpClient instances.
    /// </summary>
    internal sealed class InterceptingWebClient : IDisposable
    {
        private readonly HttpClient _client;
        private readonly bool _ownsClient;

        public InterceptingWebClient()
            : this(SharedHttpClientFactory.CreateClient(), ownsClient: false)
        {
        }

        public InterceptingWebClient(HttpClient client, bool ownsClient = false)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _ownsClient = ownsClient;
        }

        public Uri ResponseUri { get; private set; }

        /// <summary>
        /// Download the resource as bytes. Returns downloaded bytes and sets ResponseUri to the final URI (after redirects).
        /// Cancellation token may be supplied to cancel the request.
        /// </summary>
        public async Task<byte[]> DownloadDataAsync(Uri address, CancellationToken cancellationToken = default)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            using HttpResponseMessage resp = await _client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            ResponseUri = resp.RequestMessage?.RequestUri;
            return await resp.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_ownsClient)
            {
                _client.Dispose();
            }
        }
    }
}
