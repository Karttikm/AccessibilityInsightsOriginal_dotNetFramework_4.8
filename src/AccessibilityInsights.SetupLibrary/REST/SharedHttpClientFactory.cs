// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace AccessibilityInsights.SetupLibrary.REST
{
    /// <summary>
    /// Lightweight shared HttpClient factory that mimics the benefits of IHttpClientFactory:
    /// - reuses HttpClient instances to avoid socket exhaustion
    /// - central place to tune handler/timeout settings
    /// This is intentionally small and does not require DI to integrate into the existing codebase.
    /// </summary>
    internal static class SharedHttpClientFactory
    {
        private static readonly ConcurrentDictionary<string, HttpClient> s_clients = new();

        public static HttpClient CreateClient(string name = "default")
        {
            return s_clients.GetOrAdd(name, _ =>
            {
                var handler = new SocketsHttpHandler
                {
                    // Recycle connections periodically to avoid long-lived stale connections.
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    // Keep defaults for other handler settings; adjust if you need proxies, credentials, etc.
                };

                var client = new HttpClient(handler, disposeHandler: true)
                {
                    Timeout = TimeSpan.FromSeconds(100)
                };

                return client;
            });
        }
    }
}
