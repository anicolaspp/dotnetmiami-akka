using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Util.Internal;

namespace akka_word_counter_complex
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("map-reduce");

            var main = system.ActorOf(config =>
            {
                config.Receive<string>((file, c) =>
                {
                    var receiver = c.ActorOf<FileReceirver>();

                    receiver.Tell(file);
                });
            });

            main.Tell(@"C:\data\vuldat.txt");

            Console.ReadLine();
        }
    }

    class Item
    {
        public int Value { get; set; }

        public bool IsLocked { get; set; }
    }

    class ProcessorActor : ReceiveActor
    {
        public ProcessorActor()
        {
            Receive<Item>(item =>
            {
                
            });
        }
    }

    class FileReceirver : ReceiveActor
    {
        public FileReceirver()
        {
            Receive<string>(fileName => 
            {
                using (var stream = new StreamReader(new FileStream(fileName, FileMode.Open)))
                {
                    var aggregator = Context.ActorOf(LinearAggregator.Prop($"{fileName}.out.txt"));
                    int lineCount = 0;

                    while (!stream.EndOfStream)
                    {
                        lineCount++;

                        string line = stream.ReadLine();

                        var collector = Context.ActorOf(LinearCollector.Prop(aggregator));

                        collector.Tell(line);
                    }

                    aggregator.Tell(lineCount);
                } 
            });
        }
    }

  


    class LinearAggregator : ReceiveActor
    {
        private readonly string _outputLocation;
        private readonly Dictionary<string, int> _wordGlobalCounter = new Dictionary<string, int>();
        private int _total = 0;
        private int _received = 0;
        private readonly long _startingPoint;

        public static Props Prop(string outputLocation)
        {
            return Props.Create(() => new LinearAggregator(outputLocation));
        }

        public LinearAggregator(string outputLocation)
        {
            _outputLocation = outputLocation;
            _startingPoint = DateTime.Now.Millisecond;

            Receive<IEnumerable<Pair>>(pairs =>
            {
                foreach (var pair in pairs)
                {
                    if (_wordGlobalCounter.ContainsKey(pair.Word))
                    {
                        _wordGlobalCounter[pair.Word] += pair.Count;
                    }
                    else
                    {
                        _wordGlobalCounter.Add(pair.Word, pair.Count);
                    }
                }

                if (DidIFinish)
                {
                    OutputValues();
                }
            });

            Receive<int>(totalCount =>
            {
                this._total = totalCount;

                if (DidIFinish)
                {
                    OutputValues();
                }
            });
        }

        private bool DidIFinish => ++_received == _total;

        private void OutputValues()
        {
            using (var outputStream = new StreamWriter(new FileStream(_outputLocation, FileMode.Create)))
            {
                outputStream.WriteLine($"compute time: {DateTime.Now.Millisecond - _startingPoint} milliseconds");

                _wordGlobalCounter
                    .OrderByDescending(p => p.Value)
                    .ForEach(i => outputStream.WriteLine($"word: {i.Key}, count: {i.Value}"));
            }
        }
    }

    public class LinearCollector : ReceiveActor
    {
        public static Props Prop(IActorRef aggreagator)
        {
            return Props.Create(() => new LinearCollector(aggreagator));
        }

        public LinearCollector(IActorRef aggregator)
        {
            Receive<string>(line =>
            {
                var words = line.Split(' ').Select(w => new Pair(w, 1));

                aggregator.Tell(words);
            });
        }
    }

    class Pair
    {
        public string Word { get; private set; }
        public int Count { get; private set; }

        public Pair(string word, int count)
        {
            Word = word;
            Count = count;
        }
    }
}
