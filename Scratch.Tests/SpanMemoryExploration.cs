using System;
using NUnit.Framework;

namespace Scratch.Tests
{

    [TestFixture]
    public class SpanMemoryExploration
    {
        [Test]
        public void METHOD()
        {
            int[] ints = {1, 2, 3, 4, 5};
            Span<int> spanInts = ints;
            Memory<int> memoryInts = ints;

        }
    }
}