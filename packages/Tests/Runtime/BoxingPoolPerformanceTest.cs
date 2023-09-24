using NUnit.Framework;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace Katuusagi.Pool.Tests
{
    public class BoxingPoolPerformanceTest
    {
        [SetUp]
        public void Init()
        {
            // 事前にキャッシュを作っておく
            BoxingPool<Big>.MakeCache(32);
            StructOnlyBoxingPool<Big>.MakeCache(32);
            ConcurrentBoxingPool<Big>.MakeCache(32);
            ConcurrentStructOnlyBoxingPool<Big>.MakeCache(32);
        }

        [Test]
        [Performance]
        public void Boxing_Legacy()
        {
            Big big = default(Big);

            int i = 0;
            Measure.Method(() =>
            {
                Profiler.BeginSample("legacy");
                big = new Big()
                {
                    value = i,
                };
                object o = big;
                Method(o);
                ++i;
                Profiler.EndSample();
            })
            .WarmupCount(1)
            .IterationsPerMeasurement(5000)
            .MeasurementCount(20)
            .Run();
        }

        [Test]
        [Performance]
        public void Boxing_Pool()
        {
            Big big = default(Big);

            int i = 0;
            Measure.Method(() =>
            {
                Profiler.BeginSample("pool");
                big = new Big()
                {
                    value = i,
                };
                object o = BoxingPool<Big>.Get(big);
                Method(o);
                BoxingPool<Big>.Return(o);
                ++i;
                Profiler.EndSample();
            })
            .WarmupCount(1)
            .IterationsPerMeasurement(5000)
            .MeasurementCount(20)
            .Run();
        }

        [Test]
        [Performance]
        public void Boxing_StructOnlyPool()
        {
            Big big = default(Big);

            int i = 0;
            Measure.Method(() =>
            {
                Profiler.BeginSample("structonly pool");
                big = new Big()
                {
                    value = i,
                };
                object o = StructOnlyBoxingPool<Big>.Get(big);
                Method(o);
                StructOnlyBoxingPool<Big>.Return(o);
                ++i;
                Profiler.EndSample();
            })
            .WarmupCount(1)
            .IterationsPerMeasurement(5000)
            .MeasurementCount(20)
            .Run();
        }

        [Test]
        [Performance]
        public void Boxing_ConcurrentPool()
        {
            Big big = default(Big);

            int i = 0;
            Measure.Method(() =>
            {
                Profiler.BeginSample("concurrent pool");
                big = new Big()
                {
                    value = i,
                };
                object o = ConcurrentBoxingPool<Big>.Get(big);
                Method(o);
                ConcurrentBoxingPool<Big>.Return(o);
                ++i;
                Profiler.EndSample();
            })
            .WarmupCount(1)
            .IterationsPerMeasurement(5000)
            .MeasurementCount(20)
            .Run();
        }

        [Test]
        [Performance]
        public void Boxing_ConcurrentStructOnlyPool()
        {
            Big big = default(Big);

            int i = 0;
            Measure.Method(() =>
            {
                Profiler.BeginSample("concurrent structonly pool");
                big = new Big()
                {
                    value = i,
                };
                object o = ConcurrentStructOnlyBoxingPool<Big>.Get(big);
                Method(o);
                ConcurrentStructOnlyBoxingPool<Big>.Return(o);
                ++i;
                Profiler.EndSample();
            })
            .WarmupCount(1)
            .IterationsPerMeasurement(5000)
            .MeasurementCount(20)
            .Run();
        }

        // allocしなくて最適化されなさそうな関数
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Method(object o)
        {
            return o is Big;
        }

        // クソでか構造体
        [StructLayout(LayoutKind.Explicit)]
        private struct Big
        {
            [FieldOffset(9992)]
            public int value;
        }
    }
}
