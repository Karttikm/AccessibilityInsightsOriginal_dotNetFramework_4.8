// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Extensions.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AccessibilityInsights.Extensions.TelemetryTests
{
    [TestClass]
    public class TelemetryClientWrapperUnitTests
    {
        [TestMethod]
        [Timeout(1000, CooperativeCancellation = true)]
        public void Ctor_ClientIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new TelemetryClientWrapper(null));
        }

        [TestMethod]
        [Timeout(1000, CooperativeCancellation = true)]
        public void Ctor_ClientIsNotNull_DoesNotThrow()
        {
            var telemetryConfig = new TelemetryConfiguration();
            var client = new TelemetryClient(telemetryConfig);
            new TelemetryClientWrapper(client);
        }

        [TestMethod]
        [Timeout(1000, CooperativeCancellation = true)]
        public void TrackEvent_DoesNotThrow()
        {
            var telemetryConfig = new TelemetryConfiguration();
            var client = new TelemetryClient(telemetryConfig);
            ITelemetryClientWrapper wrapper = new TelemetryClientWrapper(client);

            wrapper.TrackEvent(new EventTelemetry());
        }

        [TestMethod]
        [Timeout(1000, CooperativeCancellation = true)]
        public void TrackException_DoesNotThrow()
        {
            var telemetryConfig = new TelemetryConfiguration();
            var client = new TelemetryClient(telemetryConfig);
            ITelemetryClientWrapper wrapper = new TelemetryClientWrapper(client);

            wrapper.TrackException(new InvalidOperationException(), new Dictionary<string, string>());
        }

        [TestMethod]
        [Timeout(1000, CooperativeCancellation = true)]
        public void FlushAndShutDown_DoesNotThrow()
        {
            var telemetryConfig = new TelemetryConfiguration();
            var client = new TelemetryClient(telemetryConfig);
            ITelemetryClientWrapper wrapper = new TelemetryClientWrapper(client);

            wrapper.FlushAndShutDown();
        }
    }
}
