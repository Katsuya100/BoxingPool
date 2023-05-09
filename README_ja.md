# BoxingPool
## �T�v
�{���C�u������BoxingPool�͋ɂ߂Čy�ʂ�Box����񋟂��܂��B  
Box���I�u�W�F�N�g�����O��Pool���Ă����A�K�v�ȂƂ��ɍė��p���邱�Ƃ�  
�[���A���P�[�V������Box�����������܂��B  

## ���\
### �G�f�B�^��̌v���R�[�h
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
#### ����
![image](https://github.com/Katsuya100/BoxingPool/assets/33303650/b00c15fd-7b9e-4e27-88d2-09cf91f929ec)  
BoxingPool���g�����Ƃ�2�{���x�̃p�t�H�[�}���X���P�������܂��B  
�܂��A���P�[�V�������[���ɂȂ�A�������p�t�H�[�}���X�����サ�Ă��܂��B  

### �r���h��̌v���R�[�h
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
#### ����
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

10�{���x�̃p�t�H�[�}���X���P�������܂����B  

## �g����
### �ʏ�̎g�p�@
�ȉ��̋L�@��BoxingPool���g�p�ł��܂��B  
�g���ۂ͂Ȃ�ׂ�object��ԋp���Ă��������B  
�ԋp����Ȃ��ꍇ��Pool���̃L���b�V��������A�p�t�H�[�}���X�ቺ�Ɍq����\��������܂��B  
```.cs
object o = BoxingPool<int>.Get(100);
Debug.Log(o);
BoxingPool<int>.Return(o);
```

�����A�ԋp���ʓ|�ȏꍇ�͉��L�̋L�@���L���ł��B  
```.cs
using(BoxingPool<int>.Get(100, out object o))
{
    Debug.Log(o);
}
```

�ȉ��̂悤��Class�^��Pool���s����ꍇ�A�L���b�V�����\�z���ꂸ�ʏ�̃L���X�g�����s����܂��B  
```.cs
var o = BoxingPool<GameObject>.Get(gameObject);
```
���̂��߁AGeneric������n���Ă�����ɓ��삵�܂��B  

### struct���g�����Ƃ����m�ȏꍇ
�^���m����struct�̏ꍇ��`StructOnlyBoxingPool`��p����Ɨ��_��̃p�t�H�[�}���X�����サ�܂��B  
```.cs
object o = StructOnlyBoxingPool<int>.Get(100);
Debug.Log(o);
StructBoxingPool<int>.Return(o);
```

### object�ȊO�̊��N���X�ւ�Box��
�ȉ��̋L�@��object�ȊO�̊��N���X�ւ�Boxing�������ł��܂��B  
```.cs
object o = BoxingPool<int, IComparable>.Get(100);
```

### �}���`�X���b�h�ɑΉ��������ꍇ
�}���`�X���b�h���Ŏg�p�������ꍇ��  
`ConcurrentBoxingPool`��`CuncurrentStructOnlyBoxingPool`���g�p���Ă��������B  
```.cs
var o = ConcurrentBoxingPool<GameObject>.Get(gameObject);
```
Concurrent�V���[�Y�͑���BoxingPool�ƈقȂ�A�ŗL��Pool�������Ă��܂��B  
����ɂ��A�}���`�X���b�h���ł��g�p���邱�Ƃ��\�ł��B  
�������ABoxingPool�ɔ�ׂĐ��\�ʂł̉ۑ肪����܂��B  
��̃A�b�v�f�[�g�ŉ��P���Ă����\��ł��B  

### �L���b�V���̍쐬
`MakeCache`�֐����Ăяo�����ƂŎ��O�ɃL���b�V�����쐬���邱�Ƃ��ł��܂��B  
���O�ɃL���b�V�����쐬���Ă����΃L���b�V���T�C�Y�����߂��鑼�A������s���̃A���P�[�V������}�����邱�Ƃ��ł��܂��B  
```.cs
BoxingPool<int>.MakeCache(32);
```

## �����ȗ��R
�{��Box���I�u�W�F�N�g��ۊǂ��Ă����ɂ���\���̃C���X�^���X�����������邱�Ƃ͂ł��܂���B  
���ʂɏ��������悤�Ƃ���΍�Box�����s���邽�߁A�ʂ̃C���X�^���X�ɕς���Ă��܂��܂��B  
�������A�{���C�u�����ł�`Unsafe`��p��object�^�Ń��b�v���ꂽ�\���̂��������ォ�珑�������邱�Ƃōė��p���������Ă��܂��B  

�܂��APool��`Static Type Caching`��p���č����Ɏ擾�ł��܂��B  
����A�N�Z�X���ɃL���b�V���\�z�̃A���P�[�V����������܂����A���O�ɃL���b�V��������Ă����΂��̃A���P�[�V�������[���ɂȂ�܂��B  

`MethodImpl`������`AggressiveInline`��ݒ肵�Ă��邽�߁A�r���h���̃C�����C���W�J�ɂ��œK�������҂ł��܂��B

�ȏ�̃e�N�j�b�N�ɂ��]����Box���ɔ�׈��|�I�ȃp�t�H�[�}���X���������Ă��܂��B  
