using Katuusagi.Pool.Utils;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool
{
    public static class ConcurrentBoxingPool<T>
    {
        public readonly ref struct GetHandler
        {
            private readonly object _obj;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public GetHandler(object obj)
            {
                _obj = obj;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Return(_obj);
            }
        }

        private static readonly bool IsStruct;

        static ConcurrentBoxingPool()
        {
#if !DISABLE_BOXING_POOL
            var t = typeof(T);
            IsStruct = !t.IsClass && !t.IsInterface;
#endif
        }

        public static void MakeCache(int minCount)
        {
#if !DISABLE_BOXING_POOL
            if (!IsStruct)
            {
                return;
            }

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
            if (!IsStruct ||
                !StaticTypeConcurrentStack<T>.TryPop(out var result))
            {
                return value;
            }

            BoxingUtils.Unbox<T>(result) = value;
            return result;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
#if !DISABLE_BOXING_POOL
            if (!IsStruct ||
                !(value is T))
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
            if (!IsStruct)
            {
                result = (T)value;
                return true;
            }

            result = BoxingUtils.Unbox<T>(value);
#endif
            return true;
        }
    }

    public static class ConcurrentBoxingPool<T, TBase>
        where T : TBase
    {
        public static void MakeCache(int count)
        {
            ConcurrentBoxingPool<T>.MakeCache(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConcurrentBoxingPool<T>.GetHandler Get(in T value, out TBase result)
        {
            result = Get(value);
            return new ConcurrentBoxingPool<T>.GetHandler(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBase Get(in T value)
        {
            return (TBase)ConcurrentBoxingPool<T>.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
            ConcurrentBoxingPool<T>.Return(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            return ConcurrentBoxingPool<T>.Unbox(value, out result);
        }
    }
}
