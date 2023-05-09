using Katuusagi.Pool.Utils;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool
{
    public static class StructOnlyBoxingPool<T>
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

        static StructOnlyBoxingPool()
        {
            object dummy = default(T);
            Unsafe.As<object, Box>(ref dummy);
        }

        public static void MakeCache(int minCount)
        {
            StaticTypeStack<T>.MakeCache(minCount);
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
            if (!StaticTypeStack<T>.TryPop(out var result))
            {
                return value;
            }

            Unsafe.As<object, Box>(ref result).Value = value;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
            if (!(value is T))
            {
                return;
            }

            StaticTypeStack<T>.Push(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            if (!(value is T))
            {
                result = default;
                return false;
            }

            result = Unsafe.As<object, Box>(ref value).Value;
            return true;
        }

        private class Box
        {
            public T Value;
        }
    }

    public static class StructOnlyBoxingPool<T, TBase>
        where T : struct, TBase
    {
        public static void MakeCache(int count)
        {
            StructOnlyBoxingPool<T>.MakeCache(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructOnlyBoxingPool<T>.GetHandler Get(in T value, out TBase result)
        {
            result = Get(value);
            return new StructOnlyBoxingPool<T>.GetHandler(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBase Get(in T value)
        {
            return (TBase)StructOnlyBoxingPool<T>.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
            StructOnlyBoxingPool<T>.Return(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            return StructOnlyBoxingPool<T>.Unbox(value, out result);
        }
    }
}
