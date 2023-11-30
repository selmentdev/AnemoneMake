// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemone.Base.Tests;

[TestClass]
public class ArrayExtensionTests
{
    [TestMethod]
    public void IsUnique()
    {
        var self = new[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(self.IsUnique());
    }

    [TestMethod]
    public void IsUnique_Duplicates()
    {
        var self = new[] { 1, 2, 3, 3, 4, 5 };
        Assert.IsFalse(self.IsUnique());
    }

    [TestMethod]
    public void IsUnique_Sorted()
    {
        var self = new[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(self.IsSortedUnique());
    }

    [TestMethod]
    public void IsUnique_Sorted_Duplicates()
    {
        var self = new[] { 1, 2, 3, 3, 4, 5, 5 };
        Assert.IsFalse(self.IsSortedUnique());
    }

    private class TestItem
    {
        public int Value { get; set; }
    }

    [TestMethod]
    public void IsUnique_Sorted_Duplicates_With_Selector()
    {
        var self = new[] {
            new TestItem { Value = 1 },
            new TestItem { Value = 2 },
            new TestItem { Value = 3 },
            new TestItem { Value = 3 },
            new TestItem { Value = 4 },
            new TestItem { Value = 5 },
            new TestItem { Value = 5 } };

        Assert.IsFalse(self.IsSortedUnique(item => item.Value));
    }

    [TestMethod]
    public void IsUnique_Sorted_With_Selector()
    {
        var self = new[] {
            new TestItem { Value = 1 },
            new TestItem { Value = 2 },
            new TestItem { Value = 3 },
            new TestItem { Value = 4 },
            new TestItem { Value = 5 } };

        Assert.IsTrue(self.IsSortedUnique(item => item.Value));
    }

    [TestMethod]
    public void ToCharUpper()
    {
        Assert.AreEqual('9', ArrayExtensions.ToCharUpper(9));
        Assert.AreEqual('A', ArrayExtensions.ToCharUpper(10));
        Assert.AreEqual('B', ArrayExtensions.ToCharUpper(11));
        Assert.AreEqual('C', ArrayExtensions.ToCharUpper(12));
        Assert.AreEqual('D', ArrayExtensions.ToCharUpper(13));
        Assert.AreEqual('E', ArrayExtensions.ToCharUpper(14));
        Assert.AreEqual('F', ArrayExtensions.ToCharUpper(15));
    }

    [TestMethod]
    public void ToHexSimple()
    {
        var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

        Assert.AreEqual("00", bytes.ToHex(0, 1));
        Assert.AreEqual("01", bytes.ToHex(1, 1));
        Assert.AreEqual("02", bytes.ToHex(2, 1));
        Assert.AreEqual("03", bytes.ToHex(3, 1));
        Assert.AreEqual("04", bytes.ToHex(4, 1));
        Assert.AreEqual("05", bytes.ToHex(5, 1));
        Assert.AreEqual("06", bytes.ToHex(6, 1));
        Assert.AreEqual("07", bytes.ToHex(7, 1));
        Assert.AreEqual("08", bytes.ToHex(8, 1));
        Assert.AreEqual("09", bytes.ToHex(9, 1));
        Assert.AreEqual("0A", bytes.ToHex(10, 1));
        Assert.AreEqual("0B", bytes.ToHex(11, 1));
        Assert.AreEqual("0C", bytes.ToHex(12, 1));
        Assert.AreEqual("0D", bytes.ToHex(13, 1));
        Assert.AreEqual("0E", bytes.ToHex(14, 1));
        Assert.AreEqual("0F", bytes.ToHex(15, 1));

        Assert.AreEqual("0001020304050607", bytes.ToHex(0, 8));
        Assert.AreEqual("08090A0B0C0D0E0F", bytes.ToHex(8, 8));

        Assert.AreEqual("000102030405060708090A0B0C0D0E0F", bytes.ToHex(0, 16));
    }

    [TestMethod]
    public void ToHexComplex()
    {
        var bytes = new byte[] { 0xDE, 0xAD, 0xC0, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xBA, 0xBE, 0x21, 0x37, 0x42, 0x69 };

        Assert.AreEqual("DEADC0DEDEADBEEFCAFEBABE21374269", bytes.ToHex(0, 16));
        Assert.AreEqual("DEADC0DEDEADBEEFCAFEBABE21374269", bytes.ToHex(0));
        Assert.AreEqual("DEADC0DEDEADBEEFCAFEBABE21374269", bytes.ToHex());
    }
}
