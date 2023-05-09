# BoxingPool
## Summary
This library, BoxingPool, provides extremely lightweight Boxing.  
Zero-allocation Boxing is achieved by pooling Boxing objects in advance and reusing them when needed.  

## Performance
### Measurement code on the editor
```.cs
private static void Run()
{
    Big big = default(Big);
    Profiler.BeginSample("boxing legacy");
    for (int i = 0; i < 100000; ++i)
    {
        big = new Big()
        {
            value = i,
        };
        object o = big;
        Method(o);
        Method(o);
    }
    Profiler.EndSample();

    Profiler.BeginSample("boxing pool");
    for (int i = 0; i < 100000; ++i)
    {
        big = new Big()
        {
            value = i,
        };
        object o = BoxingPool<Big>.Get(big);
        Method(o);
        Method(o);
        BoxingPool<Big>.Return(o);
    }
    Profiler.EndSample();
}

[MethodImpl(MethodImplOptions.NoInlining)]
private static bool Method(object o)
{
    return o is Big;
}

[StructLayout(LayoutKind.Explicit)]
private struct Big
{
    [FieldOffset(9992)]
    public int value;
}
```
#### Result
![image](https://github.com/Katsuya100/BoxingPool/assets/33303650/b00c15fd-7b9e-4e27-88d2-09cf91f929ec)  
Using BoxingPool, the performance improvement is about 2x.  
Allocation has also been reduced to zero and memory performance has improved.  

### Measurement code on the runtime
```.cs
Big big = default(Big);
var start = Time.realtimeSinceStartup;
for (int i = 0; i < 100000; ++i)
{
    big = new Big()
    {
        value = i,
    };
    object o = big;
    Method(o);
    Method(o);
}
var s1 = Time.realtimeSinceStartup - start;

start = Time.realtimeSinceStartup;
for (int i = 0; i < 100000; ++i)
{
    big = new Big()
    {
        value = i,
    };
    object o = BoxingPool<Big>.Get(big);
    Method(o);
    Method(o);
    BoxingPool<Big>.Return(o);
}
var s2 = Time.realtimeSinceStartup - start;
```
#### Result
- Mono  
```
legacy:1.092957 sec
pool:0.1122379 sec
```

- IL2CPP  
```
legacy:0.8230033 sec
pool:0.08929324 sec
```

We saw a performance improvement of about 10 times.  

## How to Use
### Normal usage
BoxingPool usage with the following notation.  
When using BoxingPool, please return objects as much as possible.  
If not, the cache in the Pool will be reduced, which may lead to performance degradation.  
```.cs
object o = BoxingPool<int>.Get(100);
Debug.Log(o);
BoxingPool<int>.Return(o);
```

If it is troublesome to return, the following notation is also valid.  
```.cs
using(BoxingPool<int>.Get(100, out object o))
{
    Debug.Log(o);
}
```

If a Pool of type Class is performed as follows, no cache is built and normal casting is performed.  
```.cs
var o = BoxingPool<GameObject>.Get(gameObject);
```
Therefore, passing Generic arguments works fine.  

### If it is clear that struct is to be used
If the type is definitely struct, `StructOnlyBoxingPool` can be used for better theoretical performance.  
```.cs
object o = StructOnlyBoxingPool<int>.Get(100);
Debug.Log(o);
StructBoxingPool<int>.Return(o);
```

### Boxing to base classes other than object
Boxing to base classes other than object can be realized with the following notation.  
```.cs
object o = BoxingPool<int, IComparable>.Get(100);
```

### If you want to support multi-threading
If you want to use it in a multi-threaded environment Use `ConcurrentBoxingPool` or `CurrentStructOnlyBoxingPool`.  
```.cs
var o = ConcurrentBoxingPool<GameObject>.Get(gameObject);
```
Unlike other BoxingPools, the Concurrent series has a unique Pool.  
This allows it to be used in multi-threaded environments.  
However, there are some performance issues compared to BoxingPool.  
We plan to improve this in later updates.  

### Cache Creation
You can create a cache in advance by calling the `MakeCache` function.  
Creating a cache in advance allows you to determine the cache size and suppress allocation on the first run.  
```.cs
BoxingPool<int>.MakeCache(32);
```

## Reasons for high performance
By nature, storing a Boxed object does not allow you to rewrite the structure instance inside.  
If you try to rewrite it normally, it will be reboxed and changed to another instance.  
However, this library uses `Unsafe` to rewrite a structure wrapped in an object type from memory to achieve reuse.  

Pools can also be retrieved quickly using `Static Type Caching`.  
Although allocation of cache construction is performed at the first access, this allocation can be eliminated if the cache is created in advance.  

The `MethodImpl` attribute is set to `AggressiveInline`, so you can also expect optimization by inline expansion at build time.

The above techniques provide overwhelming performance compared to conventional Boxing.  

Translated with www.DeepL.com/Translator (free version)
