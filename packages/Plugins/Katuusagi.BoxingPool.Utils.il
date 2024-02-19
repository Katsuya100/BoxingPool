.assembly Katuusagi.BoxingPool.Utils { }
.assembly extern mscorlib { }

.module Katuusagi.BoxingPool.Utils.dll

.class public auto ansi abstract sealed beforefieldinit Katuusagi.Pool.Utils.BoxingUtils
	extends [mscorlib]System.Object
{
    .method public hidebysig static !!T& Unbox<T>(object obj) cil managed aggressiveinlining
    {
        ldarg.0
        unbox !!T
        ret
    }

}
