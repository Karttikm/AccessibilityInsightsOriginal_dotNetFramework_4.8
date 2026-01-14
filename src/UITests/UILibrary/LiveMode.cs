// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.SharedUx.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Appium;

namespace UITests.UILibrary
{
    public class LiveMode
    {
        readonly WindowsDriver Session;

        // These AutomationIDs came from inspecting the open file dialog with ai-win.
        // The assumption is that they are the same across machines--if they aren't,
        // we'll need a more robust way to navigate the dialog.
        const string OpenFileFolderTextBoxAutomationID = "41477";
        const string OpenFileFileTextBoxAutomationID = "1148";
        const string OpenFileAllLocationsElementName = "All locations";

        public string SelectedElementText => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.InspectTabsElementTextBlock)).Text;

        public LiveMode(WindowsDriver session)
        {
            Session = session;
        }

        public void OpenFile(string folder, string fileName)
        {
            Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.MainWinLoadButton)).Click();
            Session.FindElement(MobileBy.Name(OpenFileAllLocationsElementName)).Click();

            var folderTextbox = Session.FindElement(MobileBy.AccessibilityId(OpenFileFolderTextBoxAutomationID));
            folderTextbox.SendKeys(folder + Keys.Enter);

            var fileTextbox = Session.FindElement(MobileBy.AccessibilityId(OpenFileFileTextBoxAutomationID));
            fileTextbox.SendKeys(fileName + Keys.Enter);
        }

        public void TogglePause() => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.MainWindow)).SendKeys(Keys.Shift + Keys.F5);

        public void RunTests() => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.HierarchyControlTestElementButton)).Click();
    }
}
