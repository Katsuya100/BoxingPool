using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Katuusagi.Pool.Utils
{
    internal static class ThreadStaticTypeStack<T>
    {
        [ThreadStatic]
        private static Stack<object> _stack;

        public static void MakeCache(int minCount)
        {
            if (_stack == null)
            {
                _stack = new Stack<object>();
            }
            for (int i = 0; i < minCount - _stack.Count; ++i)
            {
                _stack.Push(default(T));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryPop(out object result)
        {
            if (_stack == null)
            {
                _stack = new Stack<object>();
                MakeCache(32);
            }
            return _stack.TryPop(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(object value)
        {
            if (_stack == null)
            {
                _stack = new Stack<object>();
                MakeCache(32);
            }
            _stack.Push(value);
        }
    }
}
