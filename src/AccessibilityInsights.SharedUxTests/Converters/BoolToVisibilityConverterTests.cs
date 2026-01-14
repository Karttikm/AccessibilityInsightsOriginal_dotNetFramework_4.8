// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.SharedUx.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace AccessibilityInsights.SharedUxTests.Converters
{
    [TestClass]
    public class BoolToVisibilityConverterTests
    {
        [TestMethod]
        public void Convert_InputIsNull_ThrowsArgumentNullException()
        {
            BoolToVisibilityConverter converter = new BoolToVisibilityConverter();
            Assert.ThrowsExactly<ArgumentNullException>(() => converter.Convert(null, typeof(Visibility), null, null));
        }

        [TestMethod]
        public void BoolToVisibilityConverter_ConvertTrue()
        {
            BoolToVisibilityConverter converter = new BoolToVisibilityConverter();
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
        }

        [TestMethod]
        public void BoolToVisibilityConverter_ConvertFalse()
        {
            BoolToVisibilityConverter converter = new BoolToVisibilityConverter();
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, null));
        }

        [TestMethod]
        public void BoolToVisibilityConverter_ConvertBackVisible()
        {
            BoolToVisibilityConverter converter = new BoolToVisibilityConverter();
            Assert.IsTrue((bool?)converter.ConvertBack(Visibility.Visible, typeof(bool), null, null));
        }
    }
}
