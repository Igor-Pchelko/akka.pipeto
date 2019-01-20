namespace akka.pipeto
{
    public interface IMessage
    {
        string Text { get; }
    }
    
    public class Message : IMessage
    {
        public string Text { get; }

        public Message(string text)
        {
            Text = text;
        }
    }

    public class MessageOne : IMessage
    {
        public string Text { get; }

        public MessageOne(string text)
        {
            Text = text;
        }
    }

    
    public class MessageTwo : IMessage
    {
        public string Text { get; }

        public MessageTwo(string text) 
        {
            Text = text;
        }
    }

    public class MessageThree : IMessage
    {
        public string Text { get; }

        public MessageThree(string text) 
        {
            Text = text;
        }
    }
    
    public interface IMessageResponse
    {
    }
    
    public class MessageProcessed : IMessageResponse
    {
    }
}