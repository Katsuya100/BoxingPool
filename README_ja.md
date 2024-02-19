# BoxingPool
## 概要
本ライブラリ「BoxingPool」は極めて軽量なBox化を提供します。  
Box化オブジェクトを事前にPoolしておき、必要なときに再利用することで  
ゼロアロケーションなBox化を実現します。  

## 動作確認環境
|  環境  |  バージョン  |
| ---- | ---- |
| Unity | 2021.3.15f1, 2022.2.0f1 |
| .Net | 4.x, Standard 2.1 |

## 性能
### エディタ上の計測コード
[テストコード](packages/Tests/Runtime/BoxingPoolPerformanceTest.cs) 

#### 結果
|  実行処理  |  処理時間  |
| ---- | ---- |
| Boxing_Legacy | 31.004235 ms |
| Boxing_Pool | 15.1678 ms |
| Boxing_StructOnlyPool | 15.358 ms |
| Boxing_ConcurrentPool | 15.4086 ms |
| Boxing_ConcurrentStructOnlyPool | 15.39855 ms |
| Boxing_ThreadStaticPool | 15.19975 ms |
| Boxing_ThreadStaticStructOnlyPool | 15.3461 ms |

BoxingPoolを使うことで2倍程度のパフォーマンス改善が見られます。  
またアロケーションもゼロになり、メモリパフォーマンスも向上しています。  
※Concurrent系Poolでは返却時にアロケーションが発生します。

### ビルド後の計測コード
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
#### 結果
|  実行処理  |  Mono  |  IL2CPP  |
| ---- | ---- | ---- |
| Boxing_Legacy | 52.86654 ms | 41.43652 ms |
| Boxing_Pool | 9.189175 ms | 2.425781 ms |
| Boxing_StructOnlyPool | 9.142063 ms | 2.452148 ms |
| Boxing_ConcurrentPool | 9.591019 ms | 3.321289 ms |
| Boxing_ConcurrentStructOnlyPool | 9.610249 ms | 3.245117 ms |
| Boxing_ThreadStaticPool | 9.15694 ms | 2.49707 ms |
| Boxing_ThreadStaticStructOnlyPool | 9.292259 ms | 2.520508 ms |

17倍程度のパフォーマンス改善が見られました。  

## インストール方法
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
そのため、以下のようにGeneric引数を渡しても正常に動作します。  
```.cs
void Method<T>(T v)
{
    // T型がstruct型であればBoxingコストが削減される。
    // T型がclass型であればキャストされる。
    var o = BoxingPool<T>.Get(v);
    :
}
```

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
IComparable o = BoxingPool<int, IComparable>.Get(100);
```

### マルチスレッドに対応したい場合
マルチスレッド環境で使用したい場合は  
`ConcurrentBoxingPool`や`ConcurrentStructOnlyBoxingPool`を使用してください。  
```.cs
var o = ConcurrentBoxingPool<GameObject>.Get(gameObject);
```
Concurrentシリーズは他のBoxingPoolと異なり、固有のPoolを持っています。  
これにより、マルチスレッド環境でも使用することが可能です。  
しかし、BoxingPoolに比べて性能面での課題があります。  
具体的にはReturn時にアロケーションが発生します。  
後のアップデートで改善していく予定です。  

#### ThreadStaticなプール
`ThreadStaticBoxingPool`や`ThreadStaticStructOnlyBoxingPool`を使用することで  
パフォーマンスを落とさずにマルチスレッド対応が可能です。  
```.cs
var o = ThreadStaticBoxingPool<GameObject>.Get(gameObject);
```
Thread毎に異なるプールを用いるため、Concurrentシリーズに比べてメモリ消費量が多くなる可能性があります。
また、Getしたobjectを異なるスレッドで返却しないように注意してください。  
返却は正常に完了しますが取得したプールと異なるプールに返却されてしまいます。  

### キャッシュの作成
`MakeCache`関数を呼び出すことで事前にキャッシュを作成することができます。  
事前にキャッシュを作成しておけばキャッシュサイズを決められる他、初回実行時のアロケーションを抑制することができます。  
```.cs
BoxingPool<int>.MakeCache(32);
```

### BoxingPool無効化
不具合が発生した際に問題の切り分けのためにBoxingPoolを無効化できます。  
無効化したい場合は、以下のシンボルを定義してください。  
```
DISABLE_BOXING_POOL
```
無効化後もAPIは有効ですが、Poolを介さず通常のBox化が発生します。  

## 高速な理由
本来Box化オブジェクトを保管しても中にある構造体インスタンスを書き換えることはできません。  
普通に書き換えようとすれば再Box化が行われるため、別のインスタンスに変わってしまいます。  
しかし、本ライブラリではIL命令でインスタンスを書き換え、再利用を実現しています。  

また、Poolは`Static Type Caching`を用いて高速に取得できます。  
初回アクセス時にキャッシュ構築のアロケーションが走りますが、事前にキャッシュを作っておけばこのアロケーションもゼロになります。  

`MethodImpl`属性で`AggressiveInline`を設定しているため、ビルド時のインライン展開による最適化も期待できます。

以上のテクニックにより従来のBox化に比べ圧倒的なパフォーマンスを実現しています。  
