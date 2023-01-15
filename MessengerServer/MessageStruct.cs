namespace Messenger
{
    public struct Message
    {
        public string userName;
        public string data;
        public Message(string name, string msg)
        {
            userName = name;
            data = msg;
        }
    }
}
