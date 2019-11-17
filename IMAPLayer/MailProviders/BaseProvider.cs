using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace IMAPLayer.MailProviders
{
    internal abstract class BaseProvider : IDisposable
    {
        private readonly TcpClient tcp;
        private readonly Stream stream;
        private readonly Random rnd = new Random((int)DateTime.Now.ToFileTime());
        private readonly Queue<string> messageQueue = new Queue<string>();
        private readonly Dictionary<string, object> responses = new Dictionary<string, object>();
        private readonly Thread listenerThread;
        private readonly Thread transmitterThread;
        private readonly SortedSet<string> usedTags = new SortedSet<string>();
        private bool firstMessage = false;
        private int ignoredBytes = 0;
        private bool isDisposing = false;
        public BaseProvider(string address, int port, bool UseSSl)
        {
            firstMessage = true;
            tcp = new TcpClient();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    tcp.Connect(address, port);
                    break;
                }
                catch (SocketException)
                {
                    if (i == 9) throw;
                }
            }
            if (UseSSl)
            {
                SslStream s = new SslStream(tcp.GetStream());
                s.AuthenticateAsClient(address);
                stream = s;
            }
            else
            {
                stream = tcp.GetStream();
            }
            listenerThread = new Thread(new ThreadStart(Listener));
            transmitterThread = new Thread(new ThreadStart(Transmitter));
            listenerThread.Start();
            transmitterThread.Start();
        }
        private void Listener()
        {
            List<byte> buffer = new List<byte>();
            StringBuilder builder = new StringBuilder();
            byte[] arr = new byte[1024];
            stream.ReadTimeout = 100;
            while (!isDisposing)
            {
                int bytes;
                while(true)
                {
                    try
                    {
                        bytes = stream.Read(arr, 0, 1024);
                        break;
                    }
                    catch(IOException)
                    {
                        if (isDisposing) return;
                    }
                }
                buffer.AddRange(new Span<byte>(arr, 0, bytes).ToArray());
                string res = Encoding.ASCII.GetString(buffer.ToArray());
                while (res.Contains("\r\n"))
                {
                    bool ignore = (ignoredBytes > 0);
                    if(ignore) ignoredBytes -= buffer.Count;
                    buffer.Clear();
                    int i = res.IndexOf("\r\n") + 2;
                    if (i != res.Length)
                    {
                        byte[] over = Encoding.ASCII.GetBytes(res.Substring(i));
                        buffer.AddRange(over);
                        if(ignore) ignoredBytes += over.Length;
                        res = res.Substring(0, i);
                    }
                    if (firstMessage)
                    {
                        firstMessage = false;
                        builder.Clear();
                        continue;
                    }
                    if (!ignore)
                    {
                        i = res.IndexOf(' ');
                        string tag = res.Substring(0, i);
                        string message = res.Substring(i + 1);
                        builder.Append($"* {message}");
                        if (tag != "*")
                        {
                            lock (responses)
                            {
                                if (responses.ContainsKey(tag))
                                {
                                    object o = responses[tag];
                                    responses[tag] = builder.ToString();
                                    lock(o)
                                    {
                                        Monitor.Pulse(o);
                                    }
                                }
                                builder.Clear();
                            }
                        }
                        else if (message.Contains('(') && message.Contains("{"))
                        {
                            int openIndex = message.IndexOf('{');
                            int closeIndex = message.IndexOf('}');
                            ignoredBytes += int.Parse(message.Substring(openIndex + 1, closeIndex - openIndex - 1)) + 3;
                        }
                    }
                    else
                    {
                        builder.Append(res);
                    }
                    res = Encoding.ASCII.GetString(buffer.ToArray());
                }
            }
        }
        private void Transmitter()
        {
            lock (messageQueue)
            {
                while (!isDisposing)
                {
                    while (messageQueue.Count > 0)
                    {
                        string msg = messageQueue.Dequeue();
                        byte[] buff = Encoding.ASCII.GetBytes(msg);
                        stream.Write(buff, 0, buff.Length);
                    }
                    if (isDisposing) return;
                    Monitor.Wait(messageQueue);
                }
            }
        }
        protected async Task<string[]> Command(string command)
        {
            string[] res = null;
            await Task.Run(() =>
            {
                string tag;
                lock (usedTags)
                {
                    do
                    {
                        tag = rnd.Next((int)1e9).ToString();
                    }
                    while (usedTags.Contains(tag));
                    usedTags.Add(tag);
                }
                object o = new object();
                lock (responses)
                {
                    responses.Add(tag, o);
                }
                lock (messageQueue)
                {
                    messageQueue.Enqueue($"{tag} {command}\r\n");
                    Monitor.Pulse(messageQueue);
                }
                lock (o)
                {
                    Monitor.Wait(o);
                }
                lock (responses)
                {
                    List<string> l = new List<string>(((string)responses[tag]).Split("\r\n"));
                    l.RemoveAt(l.Count - 1);
                    res = l.ToArray();
                    responses.Remove(tag);
                }
                lock (usedTags)
                {
                    usedTags.Remove(tag);
                }
            });
            return res;
        }
        public void Dispose()
        {
            isDisposing = true;
            lock(messageQueue)
            {
                Monitor.Pulse(messageQueue);
            }
            listenerThread.Join();
            transmitterThread.Join();
            tcp.Close();
            stream.Dispose();
        }
        ~BaseProvider()
        {
            Dispose();
        }
    }
}
