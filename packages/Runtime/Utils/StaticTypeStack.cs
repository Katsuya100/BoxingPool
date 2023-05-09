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

            _stack = new();
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
            return _stack.TryPop(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(object value)
        {
            _stack.Push(value);
        }
    }
}
