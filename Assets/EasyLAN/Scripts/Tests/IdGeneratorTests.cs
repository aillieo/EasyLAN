// -----------------------------------------------------------------------
// <copyright file="IdGeneratorTests.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Tests
{
    using System.Linq;
    using NUnit.Framework;

    public class IdGeneratorTests
    {
        private IdGenerator generator;

        [SetUp]
        public void Setup()
        {
            this.generator = new IdGenerator();
        }

        [Test]
        public void GetIds()
        {
            var ids = Enumerable.Range(0, 100).Select(_ => this.generator.GetId());
            UnityEngine.Debug.Log(string.Join(",", ids));
        }
    }
}
