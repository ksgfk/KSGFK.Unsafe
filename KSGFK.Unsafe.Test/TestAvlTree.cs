using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace KSGFK.Unsafe.Test
{
    public partial class TestAvlTree
    {
        [Test]
        public void Test2()
        {
            const int cnt = 1000000;
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

            for (int i = 0; i < 1000; i++)
            {
                tree.Remove(i);
            }
            Assert.True(tree.Count == tree.Count());
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

    //https://www.luogu.com.cn/record/62482724
    //只是个示例，全部测试在上面网址
    public partial class TestAvlTree
    {

        public const string Question =
            "10\n" +
            "1 106465\n" +
            "4 1\n" +
            "1 317721\n" +
            "1 460929\n" +
            "1 644985\n" +
            "1 84185\n" +
            "1 89851\n" +
            "6 81968\n" +
            "1 492737\n" +
            "5 493598";
        public const string Answare =
            "106465\n" +
            "84185\n" +
            "492737\n";

        [Test]
        public void TestQuestion()
        {
            Console.SetIn(new StringReader(Question));
            StringBuilder sb = new StringBuilder();
            Console.SetOut(new StringWriter(sb));
            AvlDictionary<int, int> tree = new AvlDictionary<int, int>();
            int j = int.Parse(Console.ReadLine());
            for (int i = 0; i < j; i++)
            {
                string r = Console.ReadLine();
                int value;
                int key;
                switch (r[0])
                {
                    case '1':
                        {
                            int va = int.Parse(r.Substring(2));
                            AvlTreeNode<KeyValuePair<int, int>> nod = tree.FindNode(va);
                            if (nod.HasValue)
                            {
                                tree.SetValue(nod, nod.Value.Value + 1);
                            }
                            else
                            {
                                tree.Add(va, 1);
                            }
                            break;
                        }
                    case '2':
                        {
                            AvlTreeNode<KeyValuePair<int, int>> cnt = tree.FindNode(int.Parse(r.Substring(2)));
                            if (cnt.Value.Value == 1)
                            {
                                tree.Remove(cnt);
                            }
                            else
                            {
                                tree.SetValue(cnt, cnt.Value.Value - 1);
                            }
                            break;
                        }
                    case '3':
                        {
                            int v = int.Parse(r.Substring(2));
                            int rnk = 1;
                            foreach (KeyValuePair<int, int> item in tree)
                            {
                                item.Deconstruct(out value, out key);
                                int p = value;
                                int c = key;
                                if (p == v)
                                {
                                    break;
                                }
                                rnk += c;
                            }
                            Console.WriteLine(rnk);
                            break;
                        }
                    case '4':
                        {
                            int a = int.Parse(r.Substring(2));
                            int nnk = 0;
                            int res = int.MinValue;
                            bool isFind = false;
                            foreach (KeyValuePair<int, int> item2 in tree)
                            {
                                item2.Deconstruct(out key, out value);
                                int p2 = key;
                                int c2 = value;
                                res = p2;
                                nnk += c2;
                                if (nnk >= a)
                                {
                                    Console.WriteLine(res);
                                    isFind = true;
                                    break;
                                }
                            }
                            if (!isFind)
                            {
                                Console.WriteLine(res);
                            }
                            break;
                        }
                    case '5':
                        {
                            int no = int.Parse(r.Substring(2));
                            int val = tree.NearestUpper(no, int.MinValue).Key;
                            Console.WriteLine(val);
                            break;
                        }
                    case '6':
                        {
                            int now = int.Parse(r.Substring(2));
                            int val = tree.NearestLower(now, int.MaxValue).Key;
                            Console.WriteLine(val);
                            break;
                        }
                }
            }
            var result = sb.ToString().Replace("\r\n", "\n");
            Assert.True(Answare == result);
        }
    }
}