// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Anemone.Base.Tests;

[TestClass]
public class GuidExtensionsTests
{
    [TestMethod]
    public void EmptyGuidString()
    {
        var guid = GuidExtensions.FromString("");
        Assert.AreEqual(guid, new Guid("42c4b0e3-fc98-141c-9afb-f4c8996fb924"));
    }

    [TestMethod]
    public void GuidString()
    {
        var guid = GuidExtensions.FromString("hello world");
        Assert.AreEqual(guid, new Guid("b9274db9-4d93-083e-a52e-52d7da7dabfa"));
    }
}
