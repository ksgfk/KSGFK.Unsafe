using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestAvlTree
    {
        // https://www.luogu.com.cn/problem/P3369
        // 操作4有一个点莫名其妙的错误
        // Rank哪里写炸了（（
        // public void Test()
        // {
        //     var fs = new FileStream(@"C:\Users\ksgfk\Desktop\out.txt", FileMode.Create, FileAccess.Write);
        //     var sw = new StreamWriter(fs);
        //     Console.SetOut(sw);
        //     var tree = new AvlDictionary<int, int>();
        //     var n = int.Parse(Console.ReadLine());
        //     var line = 1;
        //     for (var i = 0; i < n; i++)
        //     {
        //         var r = Console.ReadLine();
        //         if (line == 1390)
        //         {
        //         }
        //
        //         switch (r[0])
        //         {
        //             case '1':
        //                 var va = int.Parse(r.Substring(2));
        //                 var nod = tree.FindNode(va);
        //                 if (nod.HasValue)
        //                 {
        //                     tree.SetValue(nod, nod.Value.Value + 1);
        //                 }
        //                 else
        //                 {
        //                     tree.Add(va, 1);
        //                 }
        //
        //                 break;
        //             case '2':
        //                 var cnt = tree.FindNode(int.Parse(r.Substring(2)));
        //                 if (cnt.Value.Value == 1)
        //                 {
        //                     tree.Remove(cnt);
        //                 }
        //                 else
        //                 {
        //                     tree.SetValue(cnt, cnt.Value.Value - 1);
        //                 }
        //
        //                 break;
        //             case '3':
        //                 var v = int.Parse(r.Substring(2));
        //                 var rnk = 1;
        //                 foreach (var (p, c) in tree)
        //                 {
        //                     if (p == v) break;
        //                     rnk += c;
        //                 }
        //
        //                 Console.WriteLine(rnk);
        //                 line++;
        //                 break;
        //             case '4':
        //                 var a = int.Parse(r.Substring(2));
        //                 var nnk = 1;
        //                 var res = int.MinValue;
        //                 foreach (var (p, c) in tree)
        //                 {
        //                     var t = c;
        //                     while (t > 0 && nnk < a)
        //                     {
        //                         nnk++;
        //                         t--;
        //                     }
        //
        //                     res = p;
        //                     if (nnk >= a) break;
        //                 }
        //
        //                 var next = tree.NextNode(tree.FindNode(res));
        //                 if (next.HasValue)
        //                 {
        //                     Console.WriteLine(next.Value.Key);
        //                 }
        //                 else
        //                 {
        //                     Console.WriteLine(res);
        //                 }
        //
        //                 line++;
        //                 break;
        //             case '5':
        //                 var no = int.Parse(r.Substring(2));
        //                 var noww = tree.NearestUpper(no, int.MinValue);
        //                 Console.WriteLine(noww.Key);
        //                 line++;
        //                 break;
        //             case '6':
        //                 var now = int.Parse(r.Substring(2));
        //                 var nowww = tree.NearestLower(now, int.MaxValue);
        //                 Console.WriteLine(nowww.Key);
        //                 line++;
        //                 break;
        //         }
        //     }
        //
        //     sw.Close();
        //     fs.Close();
        // }

        [Test]
        public void Test2()
        {
            var s = new SortedDictionary<int, float>();

            const int cnt = 10000000;
            var raw = Enumerable.Range(1, cnt).ToArray();
            var datas = raw.ToArray();
            var rand = new Random();
            for (var i = datas.Length - 1; i > 0; i--)
            {
                var value = datas[i];
                var randomIndex = rand.Next(0, i);
                datas[i] = datas[randomIndex];
                datas[randomIndex] = value;
            }

            var tree = new AvlTree<int>();
            foreach (var i in datas)
            {
                tree.Add(i);
            }

            Assert.True(tree.Min == 1);
            Assert.True(tree.Max == cnt);
            Assert.True(tree.Count == raw.Length);

            var ind = 0;
            foreach (var i in tree)
            {
                Assert.True(tree.Contains(i));
                Assert.True(raw[ind] == i);
                ind++;
            }

            Assert.False(tree.Contains(0));
            Assert.False(tree.Contains(int.MinValue));

            foreach (var i in tree)
            {
                var node = tree.FindNode(i);
                Assert.True(node.HasValue);
                var prev = tree.PreviousNode(node);
                var next = tree.NextNode(node);
                if (i == 1)
                {
                    Assert.False(prev.HasValue);
                }
                else
                {
                    Assert.True(prev.HasValue);
                }

                if (i == cnt)
                {
                    Assert.False(next.HasValue);
                }
                else
                {
                    Assert.True(next.HasValue);
                }

                var nearNext = tree.NearestNext(i, int.MaxValue);
                var nearPrev = tree.NearestPrevious(i, int.MinValue);
                if (i == cnt)
                {
                    Assert.True(nearNext == int.MaxValue);
                }
                else
                {
                    Assert.True(nearNext == i + 1);
                }

                if (i == 1)
                {
                    Assert.True(nearPrev == int.MinValue);
                }
                else
                {
                    Assert.True(nearPrev == i - 1);
                }
            }

            Assert.True(tree.MaxHeight == tree.RecordHeight);
            Console.WriteLine(tree.MaxHeight);
            Console.WriteLine(Log2(cnt));

            tree.CheckHeight();
        }

        private static int Log2(int value)
        {
            var result = 0;
            while (value > 0)
            {
                result++;
                value >>= 1;
            }

            return result;
        }
    }
}