using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ironclads
{
    // Only byte/string I/O runs on background threads; Unity state stays on the main thread.
    public sealed class DirectTcpTransport : IDisposable
    {
        public const int MaxFrame=65536;
        public readonly struct Notice
        {
            public readonly int peer; public readonly string kind,text;
            public Notice(int peer,string kind,string text=""){this.peer=peer;this.kind=kind;this.text=text;}
        }
        readonly ConcurrentQueue<Notice> incoming=new ConcurrentQueue<Notice>();
        readonly object gate=new object();
        volatile bool running=true;
        TcpListener listener;TcpClient connecting;Peer current;
        int serial,queued;
        public bool Overflowed {get;private set;}
        public int Port {get;private set;}
        public bool Poll(out Notice notice){if(incoming.TryDequeue(out notice)){Interlocked.Decrement(ref queued);return true;}return false;}
        bool Post(Notice notice)
        {
            if(!running)return false;if(Interlocked.Increment(ref queued)>256){Interlocked.Decrement(ref queued);Overflowed=true;return false;}
            incoming.Enqueue(notice);return true;
        }
        static void Background(ThreadStart action){new Thread(action){IsBackground=true,Name="Ironclads direct TCP"}.Start();}
        public void Listen(int port,bool loopbackOnly)
        {
            listener=new TcpListener(loopbackOnly?IPAddress.Loopback:IPAddress.Any,port);listener.Start(2);Port=((IPEndPoint)listener.LocalEndpoint).Port;
            Background(()=>{while(running)try{var socket=listener.AcceptTcpClient();Attach(socket);}catch{if(running)Post(new Notice(0,"error","TCP listener stopped."));break;}});
        }
        public void Connect(string address,int port)
        {
            Background(()=>{
                TcpClient socket=null;
                try {
                    socket=new TcpClient(AddressFamily.InterNetwork);lock(gate){if(!running){socket.Close();return;}connecting=socket;}
                    var attempt=socket.BeginConnect(IPAddress.Parse(address),port,null,null);
                    using(attempt.AsyncWaitHandle){if(!attempt.AsyncWaitHandle.WaitOne(5000))throw new TimeoutException();socket.EndConnect(attempt);}
                    lock(gate)connecting=null;Attach(socket);
                }catch{socket?.Close();if(running)Post(new Notice(0,"error","Could not connect. Check the IP address, TCP port and firewall."));}
            });
        }
        void Attach(TcpClient socket)
        {
            lock(gate) {
                if(!running||current!=null){socket.Close();return;}
                socket.NoDelay=true;socket.SendTimeout=5000;current=new Peer(this,socket,++serial);
                Post(new Notice(current.id,"connected"));current.Start();
            }
        }
        public bool Send(int peer,string message)
        {
            lock(gate)return current!=null&&current.id==peer&&current.Send(message);
        }
        public void Drop(int peer){lock(gate)if(current!=null&&current.id==peer)current.Close();}
        void Gone(Peer peer)
        {
            lock(gate){if(current==peer)current=null;}
            Post(new Notice(peer.id,"closed","Connection closed. The last table position is retained."));
        }
        public static byte[] ReadFrame(Stream stream)
        {
            var header=new byte[4];ReadExact(stream,header);
            int size=(header[0]<<24)|(header[1]<<16)|(header[2]<<8)|header[3];
            if(size<=0||size>MaxFrame)throw new InvalidDataException("Invalid TCP frame length.");
            var bytes=new byte[size];ReadExact(stream,bytes);return bytes;
        }
        static void ReadExact(Stream stream,byte[] bytes)
        {
            int offset=0;while(offset<bytes.Length){int n=stream.Read(bytes,offset,bytes.Length-offset);if(n==0)throw new EndOfStreamException();offset+=n;}
        }
        public static void WriteFrame(Stream stream,byte[] bytes)
        {
            if(bytes.Length<=0||bytes.Length>MaxFrame)throw new InvalidDataException("Invalid TCP frame length.");
            int size=bytes.Length;stream.Write(new[]{(byte)(size>>24),(byte)(size>>16),(byte)(size>>8),(byte)size},0,4);stream.Write(bytes,0,size);
        }
        public void Dispose()
        {
            running=false;listener?.Stop();lock(gate){connecting?.Close();current?.Close();current=null;}
        }
        sealed class Peer
        {
            public readonly int id;
            readonly DirectTcpTransport owner;readonly TcpClient socket;readonly NetworkStream stream;
            readonly BlockingCollection<byte[]> outgoing=new BlockingCollection<byte[]>(128);
            int closed;
            public Peer(DirectTcpTransport owner,TcpClient socket,int id){this.owner=owner;this.socket=socket;this.id=id;stream=socket.GetStream();}
            public bool Send(string text)
            {
                byte[] bytes=Encoding.UTF8.GetBytes(text);if(bytes.Length>MaxFrame)return false;
                try{if(outgoing.TryAdd(bytes))return true;}catch(InvalidOperationException){}
                Close();return false;
            }
            public void Start()
            {
                Background(()=>{try{while(owner.running&&closed==0){byte[] bytes=ReadFrame(stream);if(!owner.Post(new Notice(id,"message",new UTF8Encoding(false,true).GetString(bytes))))break;}}catch{}finally{Close();}});
                Background(()=>{try{foreach(var bytes in outgoing.GetConsumingEnumerable())WriteFrame(stream,bytes);}catch{}finally{Close();}});
            }
            public void Close(){if(Interlocked.Exchange(ref closed,1)!=0)return;outgoing.CompleteAdding();socket.Close();owner.Gone(this);}
        }
    }
}
