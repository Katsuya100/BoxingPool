using Katuusagi.Pool.Utils;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool
{
    public static class ThreadStaticStructOnlyBoxingPool<T>
        where T : struct
    {
        public readonly struct ReadOnlyHandler
        {
            private readonly object _obj;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlyHandler(object obj)
            {
                _obj = obj;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Return(_obj);
            }
        }

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

            public static implicit operator ReadOnlyHandler(GetHandler obj)
            {
                return new ReadOnlyHandler(obj._obj);
            }
        }

        public static void MakeCache(int minCount)
        {
#if !DISABLE_BOXING_POOL
            ThreadStaticTypeStack<T>.MakeCache(minCount);
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
            if (!ThreadStaticTypeStack<T>.TryPop(out var result))
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
            if (!(value is T))
            {
                return;
            }

            ThreadStaticTypeStack<T>.Push(value);
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
            result = BoxingUtils.Unbox<T>(value);
#endif
            return true;
        }
    }

    public static class ThreadStaticStructOnlyBoxingPool<T, TBase>
        where T : struct, TBase
    {
        public static void MakeCache(int count)
        {
            ThreadStaticStructOnlyBoxingPool<T>.MakeCache(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ThreadStaticStructOnlyBoxingPool<T>.GetHandler Get(in T value, out TBase result)
        {
            result = Get(value);
            return new ThreadStaticStructOnlyBoxingPool<T>.GetHandler(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBase Get(in T value)
        {
            return (TBase)ThreadStaticStructOnlyBoxingPool<T>.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
            ThreadStaticStructOnlyBoxingPool<T>.Return(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            return ThreadStaticStructOnlyBoxingPool<T>.Unbox(value, out result);
        }
    }
}
