using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace akka.pipeto
{
    public class MessageProcessor : ReceiveActor
    {
        private readonly Random _random;
        private int GetRandomDelay() => _random.Next(1000, 2000);
        
        public MessageProcessor()
        {
            _random = new Random();
            
            Receive<Message>(message => HandleMessage(message));
            ReceiveAsync<MessageOne>(message => HandleMessageOne(message).PipeTo(Sender));
            Receive<MessageTwo>(message => HandleMessageTwoV3(message));
            ReceiveAsync<MessageThree>(message => HandleMessageThree(message).PipeTo(Sender));
        }

        private void HandleMessage(Message message)
        {
            Console.WriteLine("Received a message with text: {0}", message.Text);
        }

        private async Task<IMessageResponse> HandleMessageOne(MessageOne message)
        {
            Console.WriteLine($"{DateTime.Now:O}: Before process MessageOne: {message.Text}");
            await Task.Delay(GetRandomDelay());
            Console.WriteLine($"{DateTime.Now:O}: After process MessageOne: {message.Text}");
            return new MessageProcessed();
        }
        
        private void HandleMessageTwo(MessageTwo message)
        {
            Console.WriteLine($"{DateTime.Now:O}: Before process MessageTwo: {message.Text}");
            var result = Task.Delay(GetRandomDelay())
                .ContinueWith(task =>
                {
                    Console.WriteLine($"{DateTime.Now:O}: After process MessageTwo: {message.Text}");
                    return new MessageProcessed();
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            
            result.PipeTo(Sender);
        }
        
        private void HandleMessageTwoV2(MessageTwo message)
        {
            Console.WriteLine($"{DateTime.Now:O}: Before process MessageTwo: {message.Text}");
            var result = Task.Delay(GetRandomDelay())
                .ContinueWith(task =>
                {
                    Console.WriteLine($"{DateTime.Now:O}: In process MessageTwo: {message.Text}");
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)
                .ContinueWith(task => Task.Delay(GetRandomDelay()))
                .ContinueWith(task =>
                {
                    Console.WriteLine($"{DateTime.Now:O}: In process MessageTwo: {message.Text}");
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)
                .ContinueWith(task => Task.Delay(GetRandomDelay()))
                .ContinueWith(task =>
                {
                    Console.WriteLine($"{DateTime.Now:O}: After process MessageTwo: {message.Text}");
                    return new MessageProcessed();
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            
            result.PipeTo(Sender);
        }
        
        private void HandleMessageTwoV3(MessageTwo message)
        {
            Console.WriteLine($"{DateTime.Now:O}: Start process MessageTwo: {message.Text}");
            var task = Task.Run(async () =>
            {
                Console.WriteLine($"{DateTime.Now:O}: Before process MessageTwo: {message.Text}");

                await Task.Delay(GetRandomDelay());

                Console.WriteLine($"{DateTime.Now:O}: In process MessageTwo: {message.Text}");
 
                await Task.Delay(GetRandomDelay());
 
                Console.WriteLine($"{DateTime.Now:O}: After process MessageTwo: {message.Text}");
 
                return new MessageProcessed();
            }).PipeTo(Self);
            
            task.PipeTo(Sender);
        }
        
        private Task HandleMessageThree(MessageThree message)
        {
            Console.WriteLine($"{DateTime.Now:O}: Before process MessageThree: {message.Text}");
            var result = Task.Delay(GetRandomDelay()).ContinueWith(task =>
            {
                Console.WriteLine($"{DateTime.Now:O}: After process MessageThree: {message.Text}");
                return new MessageProcessed();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            
            return result;
        }
    }
}