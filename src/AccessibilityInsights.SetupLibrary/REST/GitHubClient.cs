// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AccessibilityInsights.SetupLibrary.REST
{
    /// <summary>
    /// Class that actually makes calls to GitHub. So far, we're just making a simple GET call,
    /// with no GitHub-specific characteristics.
    /// </summary>
    public static class GitHubClient
    {
        private class DownloadState
        {
            public TriState Status { get; set; } = TriState.Unknown;
            public Stream Stream { get; set; }
            public Action<int> ProgressCallback { get; set; }
            public int StreamLength { get; set; }
        }

        /// <summary>
        /// Load the contents of the given Uri into a Stream
        /// </summary>
        /// <returns>Metadata about the stream</returns>
        public static StreamMetadata LoadUriContentsIntoStream(Uri uri, Stream stream, TimeSpan timeout, Action<int> progressCallback)
        {
            // Initialize to the requested URI so we always have a non-null URI to pass downstream.
            Uri responseUri = uri;

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            Stopwatch stopwatch = Stopwatch.StartNew();
            DownloadState state = new DownloadState
            {
                Stream = stream,
                ProgressCallback = progressCallback,
            };

            try
            {
                // Use shared HttpClient from factory to get the benefits of pooled connections.
                HttpClient client = SharedHttpClientFactory.CreateClient();

                // Enforce a cancellation-based timeout
                using CancellationTokenSource cts = new CancellationTokenSource(timeout);

                // Request headers only (so we can stream the response)
                HttpResponseMessage response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                                                     .GetAwaiter()
                                                     .GetResult();

                response.EnsureSuccessStatusCode();

                // If redirected, capture final URI; otherwise keep original
                responseUri = response.RequestMessage?.RequestUri ?? responseUri;

                long? contentLength = response.Content.Headers.ContentLength;

                using Stream responseStream = response.Content.ReadAsStreamAsync(cts.Token).GetAwaiter().GetResult();

                const int bufferSize = 81920;
                byte[] buffer = new byte[bufferSize];
                long totalRead = 0;
                int lastReportedPercent = -1;

                while (true)
                {
                    int read = Task.Run(() => responseStream.ReadAsync(buffer, 0, buffer.Length, cts.Token)).GetAwaiter().GetResult();
                    if (read == 0)
                    {
                        break;
                    }

                    stream.Write(buffer, 0, read);
                    totalRead += read;

                    if (progressCallback != null && contentLength.HasValue && contentLength.Value > 0)
                    {
                        int percent = (int)(totalRead * 100L / contentLength.Value);
                        if (percent != lastReportedPercent)
                        {
                            lastReportedPercent = percent;
                            progressCallback(percent);
                        }
                    }
                }

                // Ensure final 100% progress reported when length known
                progressCallback?.Invoke(100);

                // update state to success and length
                state.Status = TriState.Success;
                state.StreamLength = totalRead > int.MaxValue ? int.MaxValue : (int)totalRead;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (OperationCanceledException)
            {
                state.Status = TriState.Failure;
            }
            catch (Exception)
            {
                state.Status = TriState.Failure;
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                stopwatch.Stop();
            }

            if (state.Status != TriState.Success)
            {
                throw new ArgumentException("Unable to get contents from " + uri.ToString(), nameof(uri));
            }

            return new StreamMetadata(uri, responseUri, state.StreamLength);
        }
    }
}
