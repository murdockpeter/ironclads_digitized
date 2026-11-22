using System;
using System.IO;

namespace Ironclads
{
    // Shared protocol checks used by the multi-client standalone suite.
    public static class NetworkVerification
    {
        public static void Framing()
        {
            byte[] payload=System.Text.Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");using(var stream=new MemoryStream()) {
                DirectTcpTransport.WriteFrame(stream,payload);stream.Position=0;using(var fragmented=new Fragmented(stream))if(System.Text.Encoding.UTF8.GetString(DirectTcpTransport.ReadFrame(fragmented))!="{\"type\":\"ping\"}")throw new Exception("Fragmented frame changed.");
            }
            bool rejected=false;try{DirectTcpTransport.ReadFrame(new MemoryStream(new byte[]{0x7f,0xff,0xff,0xff}));}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("Oversized frame accepted.");
            if(MultiplayerSession.ValidShip(new TablePacket{ship=1,q=10000,r=0,load=.5f})||MultiplayerSession.ValidShip(new TablePacket{ship=1,load=float.NaN})||MultiplayerSession.ValidShip(new TablePacket{ship=1,dobMoved=float.NaN}))throw new Exception("Invalid ship coordinates/load accepted.");
        }
        sealed class Fragmented : Stream
        {
            readonly Stream source;public Fragmented(Stream source){this.source=source;}
            public override int Read(byte[] buffer,int offset,int count)=>source.Read(buffer,offset,Math.Min(2,count));
            public override bool CanRead=>true;public override bool CanSeek=>false;public override bool CanWrite=>false;public override long Length=>source.Length;
            public override long Position {get=>source.Position;set=>throw new NotSupportedException();}
            public override void Flush(){}public override long Seek(long offset,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long value)=>throw new NotSupportedException();public override void Write(byte[] buffer,int offset,int count)=>throw new NotSupportedException();
        }
    }
}
