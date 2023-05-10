using Katuusagi.Pool.Utils;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool
{
    public static class BoxingPool<T>
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

        public static readonly bool IsStruct;

        static BoxingPool()
        {
#if !DISABLE_BOXING_POOL
            var t = typeof(T);
            IsStruct = !t.IsClass && !t.IsInterface;
            if (!IsStruct)
            {
                return;
            }

            object dummy = default(T);
            Unsafe.As<object, Box>(ref dummy);
#endif
        }

        public static void MakeCache(int minCount)
        {
#if !DISABLE_BOXING_POOL
            if (!IsStruct)
            {
                return;
            }

            StaticTypeStack<T>.MakeCache(minCount);
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
                !StaticTypeStack<T>.TryPop(out var result))
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
            if (!IsStruct ||
                !(value is T))
            {
                return;
            }

            StaticTypeStack<T>.Push(value);
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

            result = Unsafe.As<object, Box>(ref value).Value;
#endif
            return true;
        }

        private class Box
        {
            public T Value;
        }
    }

    public static class BoxingPool<T, TBase>
        where T : TBase
    {
        public static void MakeCache(int count)
        {
            BoxingPool<T>.MakeCache(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoxingPool<T>.GetHandler Get(in T value, out TBase result)
        {
            result = Get(value);
            return new BoxingPool<T>.GetHandler(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBase Get(in T value)
        {
            return (TBase)BoxingPool<T>.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(object value)
        {
            BoxingPool<T>.Return(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unbox(object value, out T result)
        {
            return BoxingPool<T>.Unbox(value, out result);
        }
    }
}
