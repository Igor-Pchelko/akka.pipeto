using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace akka.pipeto
{
    class Program
    {        
        static async Task Main(string[] args)
        {
            var system = ActorSystem.Create("ActorSystem");
            var actor = system.ActorOf<MessageProcessor>("actor");

            //actor.Tell(new Message("This is first message!"));
            
            Console.WriteLine("Tell MessageOne");
            actor.Tell(new MessageOne("1"));
            actor.Tell(new MessageOne("2"));
            actor.Tell(new MessageOne("3")); 
            
            Console.ReadLine();

            Console.WriteLine("Tell MessageTwo");
            actor.Tell(new MessageTwo("10"));
            actor.Tell(new MessageTwo("20"));
            actor.Tell(new MessageTwo("30"));

            Console.ReadLine();
            
            Console.WriteLine("Ask MessageTwo");
            await actor.Ask(new MessageTwo("10"));
            await actor.Ask(new MessageTwo("20"));
            await actor.Ask(new MessageTwo("30"));

            Console.ReadLine();
            
            Console.WriteLine("Tell MessageThree");
            actor.Tell(new MessageThree("100"));
            actor.Tell(new MessageThree("200"));
            actor.Tell(new MessageThree("300"));

            Console.ReadLine();

            Console.WriteLine("Complete!");
        }
    }
}
