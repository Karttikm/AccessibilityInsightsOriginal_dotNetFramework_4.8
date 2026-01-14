// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.SharedUx.Properties;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UITests.UILibrary
{
    public class GettingStarted
    {
        readonly WindowsDriver Session;

        public GettingStarted(WindowsDriver session)
        {
            Session = session;
        }

        public void DismissTelemetry() => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.TelemetryDialogExitButton)).Click();
        public void DismissStartupPage() => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.StartUpModeExitButton)).Click();
    }
}
