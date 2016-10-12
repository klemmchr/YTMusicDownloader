namespace YTMusicDownloaderAPI.Model
{
    public class Client
    {
        public string Ip { get; }
        public int Requests { get; set; }

        public Client(string ip)
        {
            Ip = ip;
            Requests = 1;
        }

        public override int GetHashCode()
        {
            return Ip.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var client = obj as Client;
            return client != null && client.Ip == Ip;
        }
    }
}