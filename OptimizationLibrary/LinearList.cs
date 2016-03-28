using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    /// <summary>
    /// 片方向連結リストクラス
    /// </summary>
    class LinearList<T>
    {
        /// <summary>
        /// 連結リストのセル
        /// </summary>
        private class Cell
        {
            public T value;
            public Cell next;

            public Cell(T value, Cell next)
            {
                this.value = value;
                this.next = next;
            }
        }

        private Cell head;

        public LinearList()
        {
            this.head = null;
        }

        /// <summary>
        /// リストに新しい要素を追加
        /// </summary>
        public void Add(T value)
        {
            this.head = new Cell(value, head);
        }

        /// <summary>
        /// 列挙子を取得
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (Cell c = this.head; c != null; c = c.next)
            {
                yield return c.value;
            }
        }
    }

    class ForeachSample
    {
        static void Main()
        {
            LinearList<int> list = new LinearList<int>();

            for (int i = 0; i < 10; ++i)
            {
                list.Add(i * (i + 1) / 2);
            }

            foreach (int s in list)
            {
                Console.Write(s + " ");
            }
        }
    }
}
