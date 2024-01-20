// -----------------------------------------------------------------------
// <copyright file="ByteBufferTests.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Tests
{
    using System.Text;
    using NUnit.Framework;

    public class ByteBufferTests
    {
        [Test]
        public void Append_WhenDataFitsInBuffer_ShouldAppendData()
        {
            var data = new byte[] { 1, 2, 3 };

            var buffer = new ByteBuffer(8);
            buffer.Append(data);

            Assert.AreEqual(data.Length, buffer.Length);
        }

        [Test]
        public void Append_WhenDataExceedsBufferCapacity_ShouldExpandBuffer()
        {
            var data = new byte[] { 1, 2, 3, 4, 5 };

            var buffer = new ByteBuffer(4);
            buffer.Append(data);

            Assert.AreEqual(data.Length, buffer.Length);
        }

        [Test]
        public void Prepend_WhenDataFitsInBuffer_ShouldPrependData()
        {
            var data = new byte[] { 1, 2, 3 };

            var buffer = new ByteBuffer(8);
            buffer.Prepend(data);

            Assert.AreEqual(data.Length, buffer.Length);
        }

        [Test]
        public void Prepend_WhenDataExceedsBufferCapacity_ShouldExpandBuffer()
        {
            var data = new byte[] { 1, 2, 3, 4, 5 };

            var buffer = new ByteBuffer(4);
            buffer.Prepend(data);

            Assert.AreEqual(data.Length, buffer.Length);
        }

        [Test]
        public void ToArray_ShouldReturnCopyOfBuffer()
        {
            var data = new byte[] { 1, 2, 3 };

            var buffer = new ByteBuffer(10);
            buffer.Append(data);

            var result = buffer.ToArray();

            CollectionAssert.AreEqual(data, result);
        }

        [Test]
        public void Consume_ShouldReturnConsumedDataAndRemoveFromBuffer()
        {
            var data = new byte[] { 1, 2, 3, 4, 5 };

            var buffer = new ByteBuffer(8);
            buffer.Append(data);

            var result = buffer.Consume(3);

            Assert.AreEqual(3, result.Length);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
            Assert.AreEqual(2, buffer.Length);
        }

        [Test]
        public void Consume_WithDestinationArray_ShouldCopyDataToDestinationArray()
        {
            var data = new byte[] { 1, 2, 3 };

            var buffer = new ByteBuffer(10);
            buffer.Append(data);

            var destination = new byte[5];
            buffer.Consume(3, destination, 1);

            CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 0 }, destination);
        }

        [Test]
        public void Append_String_ShouldAppendStringAsBytes()
        {
            var data = "Hello";
            var expected = Encoding.UTF8.GetBytes(data);

            var buffer = new ByteBuffer(8);
            buffer.Append(data);
            buffer.ConsumeInt();

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [Test]
        public void ConsumeString_ShouldReturnConsumedStringAndRemoveFromBuffer()
        {
            var data = "Hello";

            var buffer = new ByteBuffer(8);
            buffer.Append(data);

            var result = buffer.ConsumeString();

            Assert.AreEqual(data, result);
            Assert.AreEqual(0, buffer.Length);
        }

        [Test]
        public void Clear_ShouldClearBuffer()
        {
            var data = new byte[] { 1, 2, 3 };

            var buffer = new ByteBuffer(10);
            buffer.Append(data);

            buffer.Clear();

            Assert.AreEqual(0, buffer.Length);
        }

        [Test]
        public void Dispose_ShouldClearBuffer()
        {
            var data = new byte[] { 1, 2, 3 };

            var buffer = new ByteBuffer(10);
            buffer.Append(data);

            buffer.Dispose();

            Assert.AreEqual(0, buffer.Length);
        }
    }
}
