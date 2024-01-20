// -----------------------------------------------------------------------
// <copyright file="ByteArrayPoolTests.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Tests
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ByteArrayPoolTests
    {
        [Test]
        public void Get_IsThreadSafe()
        {
            var byteArrayPool = new ByteArrayPool();
            var length = 16;

            Parallel.For(0, 1000, i =>
            {
                var result = byteArrayPool.Get(length);
                Assert.AreEqual(length, result.Length);
            });
        }

        [Test]
        public void Recycle_IsThreadSafe()
        {
            var byteArrayPool = new ByteArrayPool();
            var length = 16;

            Parallel.For(0, 1000, i =>
            {
                var array = byteArrayPool.Get(length);
                byteArrayPool.Recycle(array);
            });
        }

        [Test]
        public void Get_DoesNotReturnSameArray()
        {
            var byteArrayPool = new ByteArrayPool();
            var length = 16;
            var arraysBag = new ConcurrentBag<byte[]>();

            Parallel.For(0, 1000, i =>
            {
                var array = byteArrayPool.Get(length);
                arraysBag.Add(array);
            });

            Assert.AreEqual(1000, arraysBag.Distinct().Count());
        }

        [Test]
        public void GetAndRecycle_IsThreadSafe()
        {
            var byteArrayPool = new ByteArrayPool();
            var length = 16;
            var arraysBag = new ConcurrentBag<byte[]>();

            Parallel.For(0, 1000, i =>
            {
                var array = byteArrayPool.Get(length);
                arraysBag.Add(array);

                array[0] = 1;

                byteArrayPool.Recycle(array);
            });
        }
    }
}
