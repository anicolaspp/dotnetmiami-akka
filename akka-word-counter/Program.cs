using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;

namespace akka_word_counter
{

   class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("system");

            IActorRef main = system.ActorOf(config =>
            {
                config.Receive<string>((s, c) =>
                {
                    var actor = c.ActorOf(Props.Create<TextProcessor>(() => new TextProcessor(system)));

                    actor.Tell(s);
                });

                config.Receive<int>((count, context) =>
                {
                    Console.WriteLine(count);
                });
            });

            for (int i = 0; i < 1000; i++)
            {
                var str = RandomString(100);

                main.Tell(str);

                Thread.Sleep(1);
            }


            Console.ReadLine();
        }

        static Random r = new Random();

        static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();

           
            char c;

            for (int i = 0; i < size; i++)
            {
                if (i%10 == 0)
                {
                    builder.Append(' ');
                }
                else
                {
                    builder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(26*r.NextDouble() + 65))));
                }

              
            }

            return builder.ToString()
                .ToUpper()
                .Replace('A', ' ')
                .Replace('B', ' ')
                .Replace('M', ' ');
        }
    }

    public class TextProcessor : ReceiveActor
    {
        private readonly IActorRefFactory _maker;
        private readonly Props _props;

        private int _lines;
        private int _linesProcessed;
        private int _total;

        public int WordCount => _total;


        public TextProcessor(IActorRefFactory maker)
        {
            _maker = maker;

            Receive<string>(text =>
            {
                string[] lines = text.Split('\n');
                _lines = lines.Length;

                foreach (var line in lines)
                {
                    var child = _maker.ActorOf<LineProcessor>();

                    child.Tell(line);
                }
            });

            Receive<int>(count =>
            {
              //  Context.Stop(Sender); this is not actually required

                _linesProcessed++;
                _total += count;

                if (_linesProcessed == _lines)
                {
                    Context.Parent.Tell(_total);
                }
            });
        }
    }


    public class LineProcessor : ReceiveActor
    {
        public LineProcessor()
        {
            Receive<string>(line =>
            {
                if (string.IsNullOrEmpty(line))
                {
                    Sender.Tell(0);
                }
                else
                {
                    string[] pieces = line.Split(' ');
                    Sender.Tell(pieces.Length);
                }
            });
        }
    }
}

