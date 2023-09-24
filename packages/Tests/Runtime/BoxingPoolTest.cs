using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Katuusagi.Pool.Tests
{
    public class BoxingPoolTest
    {
        [Test]
        public void Boxing()
        {
            using (BoxingPool<int>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }

            using (BoxingPool<int, IComparable>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }

            var type = typeof(BoxingPoolTest);
            using (BoxingPool<Type>.Get(type, out var v))
            {
                Assert.AreEqual(v, type);
            }

            using (BoxingPool<Type, IReflect>.Get(type, out var v))
            {
                Assert.AreEqual(v, type);
            }
        }

        [Test]
        public void StructOnlyBoxing()
        {
            using (StructOnlyBoxingPool<int>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }

            using (StructOnlyBoxingPool<int, IComparable>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }
        }

        [Test]
        public void ConcurrentBoxing()
        {
            using (ConcurrentBoxingPool<int>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }

            using (ConcurrentBoxingPool<int, IComparable>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }

            var type = typeof(BoxingPoolTest);
            using (ConcurrentBoxingPool<Type>.Get(type, out var v))
            {
                Assert.AreEqual(v, type);
            }

            using (ConcurrentBoxingPool<Type, IReflect>.Get(type, out var v))
            {
                Assert.AreEqual(v, type);
            }
        }

        [Test]
        public void ConcurrentStructOnlyBoxing()
        {
            using (ConcurrentStructOnlyBoxingPool<int>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }

            using (ConcurrentStructOnlyBoxingPool<int, IComparable>.Get(10, out var v))
            {
                Assert.AreEqual(v, 10);
            }
        }

        [Test]
        public void Parallel_()
        {
            var wait = new SpinWait();
            var result = Parallel.For(0, 10000, (i) =>
            {
                using (ConcurrentBoxingPool<int>.Get(i, out var v))
                {
                    Assert.AreEqual(v, i);
                }
            });

            while (!result.IsCompleted)
            {
                wait.SpinOnce();
            }

            result = Parallel.For(0, 10000, (i) =>
            {
                using (ConcurrentStructOnlyBoxingPool<int>.Get(i, out var v))
                {
                    Assert.AreEqual(v, i);
                }
            });

            while (!result.IsCompleted)
            {
                wait.SpinOnce();
            }
        }
    }
}
