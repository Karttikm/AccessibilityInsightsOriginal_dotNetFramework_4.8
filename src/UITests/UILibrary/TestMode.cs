// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.SharedUx.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.UILibrary
{
    public class TestMode
    {
        public AutomatedChecks AutomatedChecks { get; }
        public ResultsInUIATree ResultsInUIATree { get; }
        public TestMode(WindowsDriver session)
        {
            AutomatedChecks = new AutomatedChecks(session);
            ResultsInUIATree = new ResultsInUIATree(session);
        }
    }

    public class AutomatedChecks
    {
        readonly WindowsDriver Session;
        public AutomatedChecks(WindowsDriver session)
        {
            Session = session;
        }

        public void ViewInUIATree() => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.AutomatedChecksUIATreeButton)).Click();

        public void GoToAutomatedChecksElementDetails(int element)
        {
            var resultsGrid = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.AutomatedChecksResultsListView));
            var results = resultsGrid.FindElements(MobileBy.ClassName("ListViewItem"));

            results[element].FindElement(MobileBy.ClassName("Button")).Click();
        }

        public void ValidateAutomatedChecks(int? nonFrameworkErrorCount, int? frameworkErrorCount)
        {
            ValidateResultCountForSet(AutomationIDs.AutomatedChecksResultsListView, AutomationIDs.AutomatedChecksExpandAllButton, nonFrameworkErrorCount);
            ValidateResultCountForSet(AutomationIDs.AutomatedChecksFrameworkResultsListView, AutomationIDs.AutomatedChecksFrameworkExpandAllButton, frameworkErrorCount);
            var resultsText = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.AutomatedChecksResultsTextBlock)).Text;
            var resultTextCount = int.Parse(resultsText.Split()[0]);

            int expectedTotalResultCount = (nonFrameworkErrorCount ?? 0) + (frameworkErrorCount ?? 0);
            Assert.AreEqual(expectedTotalResultCount, resultTextCount);
        }

        private void ValidateResultCountForSet(string selector, string expandAllSelector, int? expectedErrorCount)
        {
            if (expectedErrorCount.HasValue)
            {
                var resultsGrid = Session.FindElement(MobileBy.AccessibilityId(selector));
                Session.FindElement(MobileBy.AccessibilityId(expandAllSelector)).Click();
                var results = resultsGrid.FindElements(MobileBy.ClassName("ListViewItem"));
                Assert.HasCount((int)expectedErrorCount, results);
            }
            else
            {
                Assert.Throws<WebDriverException>(() => Session.FindElement(MobileBy.AccessibilityId(selector)));
                Assert.Throws<WebDriverException>(() => Session.FindElement(MobileBy.AccessibilityId(expandAllSelector)));
            }
        }
    }

    public class ResultsInUIATree
    {
        readonly WindowsDriver Session;
        public ResultsInUIATree(WindowsDriver session)
        {
            Session = session;
        }
        public void BackToAutomatedChecks() => Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.MainWinBreadCrumbTwoButton)).Click();

        public void SwitchToResultsTab()
        {
            var tree = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.HierarchyControlUIATreeView));
            var nodes = tree.FindElements(MobileBy.ClassName("TreeViewItem"));
            var patterns = GetPatternsNodes(AutomationIDs.SnapshotModeControl, nodes);

            patterns.Last().SendKeys(Keys.Control + Keys.Tab);
        }

        public void SwitchToDetailsTab()
        {
            var resultsList = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.ScannerResultsListView));
            var resultsAll = resultsList.FindElements(MobileBy.ClassName("ListViewItem"));

            resultsAll.First().SendKeys(Keys.Control + Keys.Tab);
        }

        public void SelectElementInTree(int element)
        {
            var tree = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.HierarchyControlUIATreeView));
            var nodes = tree.FindElements(MobileBy.ClassName("TreeViewItem"));
            nodes[element].SendKeys(Keys.Enter);
        }

        public void ValidateDetails(string firstPattern, string firstProperty, int patternCount, int propCount)
        {
            var snapshotModeControl = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.SnapshotModeControl));
            var props = snapshotModeControl.FindElements(MobileBy.ClassName("ListViewItem"));
            props[0].Click();
            var tree = snapshotModeControl.FindElement(MobileBy.AccessibilityId(AutomationIDs.HierarchyControlUIATreeView));
            var nodes = tree.FindElements(MobileBy.ClassName("TreeViewItem"));
            var patterns = GetPatternsNodes(AutomationIDs.SnapshotModeControl, nodes);

            Assert.AreEqual(firstPattern, patterns.First().Text);
            Assert.AreEqual(firstProperty, props[0].Text);
            Assert.AreEqual(patternCount, patterns.Count());
            Assert.HasCount(propCount, props);
        }

        public void ValidateTree(string firstNodeText, int nodeCount)
        {
            var tree = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.HierarchyControlUIATreeView));
            var nodes = tree.FindElements(MobileBy.ClassName("TreeViewItem"));

            Assert.HasCount(nodeCount, nodes);

            // We're seeing the Text property here return different values on different versions of .NET framework.
            // As such, we only check for Contains (not Equals) here to make the test more flexible.
            Assert.Contains(firstNodeText, nodes.First().Text);
        }

        public void ValidateResults(int nonExpandedNonFrameworkResultsCount, int expandedNonFrameworkResultsCount,
            int nonExpandedFrameworkResultsCount, int expandedFrameworkResultsCount)
        {
            ValidateCurrentResultCount(AutomationIDs.ScannerResultsListView, nonExpandedNonFrameworkResultsCount);
            ValidateCurrentResultCount(AutomationIDs.ScannerResultsFrameworkResultsListView, nonExpandedFrameworkResultsCount);

            if (expandedNonFrameworkResultsCount > 0 || expandedFrameworkResultsCount > 0)
            {
                Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.ScannerResultsShowAllButton)).Click();
                var fixFollowTb = Session.FindElement(MobileBy.AccessibilityId(AutomationIDs.ScannerResultsFixFollowingTextBox));
                Assert.IsFalse(string.IsNullOrEmpty(fixFollowTb.Text));

                ValidateCurrentResultCount(AutomationIDs.ScannerResultsListView, expandedNonFrameworkResultsCount);
                ValidateCurrentResultCount(AutomationIDs.ScannerResultsFrameworkResultsListView, expandedFrameworkResultsCount);
            }
        }

        private void ValidateResultListIsCollapsed(string automationId)
        {
            Assert.Throws<WebDriverException>(() => Session.FindElement(MobileBy.AccessibilityId(automationId)));
        }

        private void ValidateCurrentResultCount(string automationId, int expectedResultsCount)
        {
            if (expectedResultsCount > 0)
            {
                var resultsList = Session.FindElement(MobileBy.AccessibilityId(automationId));
                var results = resultsList.FindElements(MobileBy.ClassName("ListViewItem"));
                Assert.HasCount(expectedResultsCount, results);
            }
            else
            {
                ValidateResultListIsCollapsed(automationId);
            }
        }

        private IEnumerable<AppiumElement> GetPatternsNodes(string parentId, ReadOnlyCollection<AppiumElement> nonPatternNodes)
        {
            var parent = Session.FindElement(MobileBy.AccessibilityId(parentId));
            var allnodes = parent.FindElements(MobileBy.ClassName("TreeViewItem"));
            var patterns = allnodes.Except(nonPatternNodes);

            foreach (var pattern in patterns)
            {
                pattern.SendKeys(Keys.Right);
            }

            allnodes = parent.FindElements(MobileBy.ClassName("TreeViewItem"));
            return allnodes.Except(nonPatternNodes);
        }
    }
}
