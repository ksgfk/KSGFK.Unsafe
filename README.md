# KSGFK.Unsafe
和名字一样，不安全的库

储存其他工程里积累下来的代码的repo

本体目标框架是.NET Standard 2.1

Benchmark和Test是.NET 6

## Feature

* 高性能AVL树
* Heap/PriorityQueue
* Octree八叉树[WIP]
* Quadtree四叉树[WIP]
* 简单封装Span上的Stack和Queue结构

## TODO

* 让八叉树支持范围查询（懒了，有没有大佬pull一个）
* 让四叉树支持射线检测
* ~~（可能会有SAH BVH吧）~~

## QA

#### Q：.NET 6有PriorityQueue，为啥还要自己写一个？

A：我咋知道优先队列会在.NET6加入标准库嘛...上传这个repo的时候.Net5还没出来呢...非要说有没有用的话，PriorityQueue里面有static函数可以直接在数组上建堆。

#### Q：Span上封装Stack和Queue怎么用？有啥用？

A：好问题。

```c#
SpanQueue<int> q = new SpanQueue<int>(stackalloc int[128]);
```

关于有啥用嘛...没啥用。除了极少数情况，比如队列、栈模拟递归的时候为了追求0 GC可能会用用。还有爆栈风险（非要极限性能，GC关掉一段时间再开不是更好...）

不过呢它还能接收void*作为储存空间。说不定可以用来当native容器呢（不可扩容就是了...

#### Q：AVL有个p用？标准库都用RBT作为Set的数据结构

A：说的好，建议跑下KSGFK.Unsafe.Benchmark

库里的AVL实现参考~~CV~~了[avlmini](https://github.com/skywind3000/avlmini)，插入删除节点时并没有一直回溯到根节点，而是当左右子节点平衡后就停下来

**一般** 情况下（需要查询的数据有部分不在树中），AVL更浅的深度，查询速度更快。

**极端** 情况下（需要查询的数据全部都在树中），AVL查询速度反而会慢一些

基本无论什么情况，AVL插入都会比RBT慢。

（删除节点还没测完，互有胜负）

