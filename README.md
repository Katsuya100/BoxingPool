# BoxingPool
[日本語](README_ja.md)

## Summary
This library "BoxingPool" provides extremely lightweight Boxing.
Zero-allocation Boxing is achieved by pooling Boxing objects in advance and reusing them when needed.  

## System Requirements
|  Environment  |  Version  |
| ---- | ---- |
| Unity | 2021.3.15f1, 2022.2.0f1 |
| .Net | 4.x, Standard 2.1 |

## Performance
### Measurement code on the editor
[Test Code.](packages/Tests/Runtime/BoxingPoolPerformanceTest.cs) 

#### Result
|  Process  |  Time  |
| ---- | ---- | ---- |
| Boxing_Legacy | 34.47214 ms |
| Boxing_Pool | 17.515775 ms |
| Boxing_StructOnlyPool | 17.239465 ms |
| Boxing_ConcurrentPool | 17.70384 ms |
| Boxing_ConcurrentStructOnlyPool | 15.53006 ms |
Using BoxingPool, the performance improvement is about 2x.  
Also, allocation has been reduced to zero, and memory performance has been improved.  
*Concurrent-type Pools will have allocations when returned.  

### Measurement code on the runtime
```.cs
private readonly ref struct Measure
{
    private readonly string _label;
    private readonly StringBuilder _builder;
    private readonly float _time;

    public Measure(string label, StringBuilder builder)
    {
        _label = label;
        _builder = builder;
        _time = (Time.realtimeSinceStartup * 1000);
    }

    public void Dispose()
    {
        _builder.AppendLine($"{_label}: {(Time.realtimeSinceStartup * 1000) - _time} ms");
    }
}
：
var log = new StringBuilder();
Big big = default(Big);

using (new Measure("Boxing_Legacy", log))
{
    for (int i = 0; i < 5000; ++i)
    {
        big = new Big()
        {
            value = i,
        };
        object o = big;
        Method(o);
    }
}

using (new Measure("Boxing_Pool", log))
{
    for (int i = 0; i < 5000; ++i)
    {
        big = new Big()
        {
            value = i,
        };
        object o = BoxingPool<Big>.Get(big);
        Method(o);
        BoxingPool<Big>.Return(o);
    }
}
```
#### Result
|  Process  |  Mono  |  IL2CPP  |
| ---- | ---- | ---- | ---- |
| Boxing_Legacy | 57.6889 ms | 35.61328 ms |
| Boxing_Pool | 8.582088 ms | 2.003906 ms |
| Boxing_StructOnlyPool | 8.450382 ms | 1.962891 ms |
| Boxing_ConcurrentPool | 9.024681 ms | 3.015625 ms |
| Boxing_ConcurrentStructOnlyPool | 8.934082 ms | 2.90625 ms |

We saw a performance improvement of about 17x.  

## How to install
### Installing BoxingPool
1. Open [Window > Package Manager].
2. click [+ > Add package from git url...].
3. Type `https://github.com/Katsuya100/BoxingPool.git?path=packages` and click [Add].

#### If it doesn't work
The above method may not work well in environments where git is not installed.  
Download the appropriate version of `com.katuusagi.boxingpool.tgz` from [Releases](https://github.com/Katsuya100/BoxingPool/releases), and then [Package Manager > + > Add package from tarball...] Use [Package Manager > + > Add package from tarball...] to install the package.

#### If it still doesn't work
Download the appropriate version of `Katuusagi.BoxingPool.unitypackage` from [Releases](https://github.com/Katsuya100/BoxingPool/releases) and Import it into your project from [Assets > Import Package > Custom Package].

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
Therefore, passing the Generic argument as follows works fine.
```.cs
void Method<T>(T v)
{
    // If the T-type is struct type, boxing costs are reduced.
    // If the T type is a class type, it is cast.
    var o = BoxingPool<T>.Get(v);
    :
}
```

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
Specifically, allocation occurs at Return.  
Plan to improve this in later updates.  

### Cache Creation
You can create a cache in advance by calling the `MakeCache` function.  
Creating a cache in advance allows you to determine the cache size and suppress allocation on the first run.  
```.cs
BoxingPool<int>.MakeCache(32);
```

### Disable BoxingPool
BoxingPool can be disabled to isolate the problem in the event of a defect.  
To disable it, define the following symbols.  
```
DISABLE_BOXING_POOL
```
After disabling, the API will still be valid, but normal Boxing will occur without the Pool.  

## Reasons for high performance
By nature, storing a Boxed object does not allow you to rewrite the structure instance inside.  
If you try to rewrite it normally, it will be reboxed and changed to another instance.  
However, this library rewrites instances with the IL instruction to achieve reuse.  

Also, Pool can be retrieved at high speed using `Static Type Caching`.  
Although allocation of cache construction is performed at the first access, this allocation can be reduced to zero if the cache is created in advance.  

The `MethodImpl` attribute is set to `AggressiveInline`, so you can also expect optimization by inline expansion at build time.

The above techniques provide overwhelming performance compared to conventional Boxing.  
