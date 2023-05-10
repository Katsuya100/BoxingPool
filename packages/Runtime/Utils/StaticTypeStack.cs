using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool.Utils
{
    internal static class StaticTypeStack<T>
    {
        private static Stack<object> _stack;

        static StaticTypeStack()
        {
            var t = typeof(T);
            if (t.IsClass || t.IsInterface)
            {
                return;
            }

            _stack = new Stack<object>();
            MakeCache(32);
        }

        public static void MakeCache(int minCount)
        {
            for (int i = 0; i < minCount - _stack.Count; ++i)
            {
                _stack.Push(default(T));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryPop(out object result)
        {
#if NET_STANDARD_2_1 || NET_UNITY_4_8
            return _stack.TryPop(out result);
#elif NET_STANDARD_2_0 || NET_4_6
            if (_stack.Count > 0)
            {
                result = _stack.Pop();
                return true;
            }

            result = default;
            return false;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(object value)
        {
            _stack.Push(value);
        }
    }
}
