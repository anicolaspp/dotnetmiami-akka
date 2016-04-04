using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreads
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = new List<Item>();

            data.ForEach(item => { });

            data.AsParallel().ForAll(item => { }); 

            for (int i = 0; i < 100; i++)
            {
                data.Add(new Item {Value = i, IsLocked = false});
            }

            Thread t1 = new Thread(() =>
            {
                Process(data);
            });

            Thread t2 = new Thread(() =>
            {
                Process(data);
            });

            t1.Start();
            t2.Start();

            Console.ReadLine();
        }

        //Real example of file pulling

        private static object _lock = new object();

        private static void Process(List<Item> data)
        {
            while (true)
            {
                lock (_lock)
                {
                    if (data.Count > 0)
                    {
                        Console.WriteLine($"Processing {data[0].Value} in Thread: {Thread.CurrentThread.ManagedThreadId}");

                        data.RemoveAt(0);
                    }
                }
            }
        }
    }

    
    

    class Item
    {
        public int Value { get; set; }

        public bool IsLocked { get; set; }
    }
}
