using System;

namespace SignalR.ScaleOut
{
    public class WireMessage
    {
        public string SignalKey { get; set; }
        public object Value { get; set; }
        public long Id { get; set; }
        public DateTime Created { get; set; }

        public Message ToMessage()
        {
            return new Message(SignalKey, Id, Value, Created);
        }
    }
}