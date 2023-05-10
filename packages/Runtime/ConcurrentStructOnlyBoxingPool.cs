using Katuusagi.Pool.Utils;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool
{
    public static class ConcurrentStructOnlyBoxingPool<T>
        where T : struct
    {
        public readonly ref struct GetHandler
        {
            private readonly object _obj;

            public GetHandler(object obj)
            {
                _obj = obj;
            }

            public void Dispose()
            {
                Return(_obj);
            }
        }

        static ConcurrentStructOnlyBoxingPool()
        {
#if !DISABLE_BOXING_POOL
            object dummy = default(T);
            Unsafe.As<object, Box>(ref dummy);
#endif
        }

        public static void MakeCache(int minCount)
        {
#if !DISABLE_BOXING_POOL
            StaticTypeConcurrentStack<T>.MakeCache(minCount);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GetHandler Get(in T value, out object result)
        {
            result = Get(value);
            return new GetHandler(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Get(in T value)
        {
#if DISABLE_BOXING_POOL
            return value;
#else
            if (!StaticTypeConcurrentStack<T>.TryPop(out var result))
            {
                return value;
            }

            Unsafe.As<object, Box>(ref result).Value = value;
            return result;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
#if !DISABLE_BOXING_POOL
            if (!(value is T))
            {
                return;
            }

            StaticTypeConcurrentStack<T>.Push(value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            if (!(value is T))
            {
                result = default;
                return false;
            }

#if DISABLE_BOXING_POOL
            result = (T)value;
#else
            result = Unsafe.As<object, Box>(ref value).Value;
#endif
            return true;
        }

        private class Box
        {
            public T Value;
        }
    }

    public static class ConcurrentStructOnlyBoxingPool<T, TBase>
        where T : struct, TBase
    {
        public static void MakeCache(int count)
        {
            ConcurrentStructOnlyBoxingPool<T>.MakeCache(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConcurrentStructOnlyBoxingPool<T>.GetHandler Get(in T value, out TBase result)
        {
            result = Get(value);
            return new ConcurrentStructOnlyBoxingPool<T>.GetHandler(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBase Get(in T value)
        {
            return (TBase)ConcurrentStructOnlyBoxingPool<T>.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
            ConcurrentStructOnlyBoxingPool<T>.Return(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            return ConcurrentStructOnlyBoxingPool<T>.Unbox(value, out result);
        }
    }
}
