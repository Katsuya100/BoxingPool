# BoxingPool
## 概要
本ライブラリはBoxingPoolは極めて軽量なBox化を提供します。  
Box化オブジェクトを事前にPoolしておき、必要なときに再利用することで  
ゼロアロケーションなBox化を実現します。  

## 性能
### エディタ上の計測コード
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
#### 結果
![image](https://github.com/Katsuya100/BoxingPool/assets/33303650/b00c15fd-7b9e-4e27-88d2-09cf91f929ec)  
BoxingPoolを使うことで2倍程度のパフォーマンス改善が見られます。  
またアロケーションもゼロになり、メモリパフォーマンスも向上しています。  

### ビルド後の計測コード
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
#### 結果
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

10倍程度のパフォーマンス改善が見られました。  

## インストール方法
### Unsafeのインストール
1. [NuGet Package Explorer](https://apps.microsoft.com/store/detail/nuget-package-explorer/9WZDNCRDMDM3?hl=ja-jp&gl=jp)などを使い[Unsafe](https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/)のパッケージをダウンロードする。
2. `System.Runtime.CompilerServices.Unsafe.dll`をPlugins以下に設置する。

### BoxingPoolのインストール
1. [Window > Package Manager]を開く。
2. [+ > Add package from git url...]をクリックする。
3. `https://github.com/Katsuya100/BoxingPool.git?path=packages`と入力し[Add]をクリックする。

#### うまくいかない場合
上記方法は、gitがインストールされていない環境ではうまく動作しない場合があります。
[Releases](https://github.com/Katsuya100/BoxingPool/releases)から該当のバージョンの`com.katuusagi.boxingpool.tgz`をダウンロードし
[Package Manager > + > Add package from tarball...]を使ってインストールしてください。

#### それでもうまくいかない場合
[Releases](https://github.com/Katsuya100/BoxingPool/releases)から該当のバージョンの`Katuusagi.BoxingPool.unitypackage`をダウンロードし
[Assets > Import Package > Custom Package]からプロジェクトにインポートしてください。

## 使い方
### 通常の使用法
以下の記法でBoxingPoolを使用できます。  
使う際はなるべくobjectを返却してください。  
返却されない場合はPool内のキャッシュが減り、パフォーマンス低下に繋がる可能性があります。  
```.cs
object o = BoxingPool<int>.Get(100);
Debug.Log(o);
BoxingPool<int>.Return(o);
```

もし、返却が面倒な場合は下記の記法も有効です。  
```.cs
using(BoxingPool<int>.Get(100, out object o))
{
    Debug.Log(o);
}
```

以下のようにClass型のPoolが行われる場合、キャッシュが構築されず通常のキャストが実行されます。  
```.cs
var o = BoxingPool<GameObject>.Get(gameObject);
```
そのため、Generic引数を渡しても正常に動作します。  

### structを使うことが明確な場合
型が確実にstructの場合は`StructOnlyBoxingPool`を用いると理論上のパフォーマンスが向上します。  
```.cs
object o = StructOnlyBoxingPool<int>.Get(100);
Debug.Log(o);
StructBoxingPool<int>.Return(o);
```

### object以外の基底クラスへのBox化
以下の記法でobject以外の基底クラスへのBoxingを実現できます。  
```.cs
object o = BoxingPool<int, IComparable>.Get(100);
```

### マルチスレッドに対応したい場合
マルチスレッド環境で使用したい場合は  
`ConcurrentBoxingPool`や`CuncurrentStructOnlyBoxingPool`を使用してください。  
```.cs
var o = ConcurrentBoxingPool<GameObject>.Get(gameObject);
```
Concurrentシリーズは他のBoxingPoolと異なり、固有のPoolを持っています。  
これにより、マルチスレッド環境でも使用することが可能です。  
しかし、BoxingPoolに比べて性能面での課題があります。  
後のアップデートで改善していく予定です。  

### キャッシュの作成
`MakeCache`関数を呼び出すことで事前にキャッシュを作成することができます。  
事前にキャッシュを作成しておけばキャッシュサイズを決められる他、初回実行時のアロケーションを抑制することができます。  
```.cs
BoxingPool<int>.MakeCache(32);
```

## 高速な理由
本来Box化オブジェクトを保管しても中にある構造体インスタンスを書き換えることはできません。  
普通に書き換えようとすれば再Box化が行われるため、別のインスタンスに変わってしまいます。  
しかし、本ライブラリでは`Unsafe`を用いobject型でラップされた構造体をメモリ上から書き換えることで再利用を実現しています。  

また、Poolは`Static Type Caching`を用いて高速に取得できます。  
初回アクセス時にキャッシュ構築のアロケーションが走りますが、事前にキャッシュを作っておけばこのアロケーションもゼロになります。  

`MethodImpl`属性で`AggressiveInline`を設定しているため、ビルド時のインライン展開による最適化も期待できます。

以上のテクニックにより従来のBox化に比べ圧倒的なパフォーマンスを実現しています。  
