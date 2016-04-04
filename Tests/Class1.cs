using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using akka_word_counter;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Tests
{
    public class LineProcessorTest : TestKit
    {
        [Fact]
        public void test_empty_line_should_return_cero_worlds()
        {
            var lineProcessorActor = ActorOfAsTestActorRef<LineProcessor>();

            lineProcessorActor.Tell("");

            var result = ExpectMsg<int>();

            result.Should().Be(0);
        }

        [Fact]
        public void test_one_word_line_should_return_one()
        {
            var lineProcessorActor = ActorOfAsTestActorRef<LineProcessor>();

            lineProcessorActor.Tell("asdfaf");

            var result = ExpectMsg<int>();

            result.Should().Be(1);
        }
    }

    public class StubLineProcessor : ReceiveActor
    {
        public StubLineProcessor()
        {
            Receive<string>(s => Sender.Tell(2));
        }
    }

    public class TextProcessorTest : TestKit
    {
        [Fact]
        public void test_empty_text_should_return_cero()
        {
            var textProcessorActor = ActorOfAsTestActorRef<TextProcessor>(Props.Empty);

            textProcessorActor.Tell("");

            var result = ExpectMsg<int>();
            result.Should().Be(0);
        }

        [Fact]
        public void test_two_lines_text_with_two_words_each_should_return_four()
        {
            TestProbe probe = CreateTestProbe();

            probe.SetAutoPilot(new DelegateAutoPilot((sender, message) =>
            {
                sender.Tell(2);
                return AutoPilot.KeepRunning;
            }));

            IActorRefFactory factory = Substitute.For<IActorRefFactory>();
            
            factory.ActorOf<LineProcessor>()
                .Returns(probe.Ref);

            var textProcessorActor = ActorOfAsTestActorRef<TextProcessor>(
                () => new TextProcessor(factory));

            textProcessorActor.Tell("one two\n tree four");
            
            textProcessorActor.UnderlyingActor.WordCount.Should().Be(4);
        }
    }
}
