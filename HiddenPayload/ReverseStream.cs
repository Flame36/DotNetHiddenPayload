using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenPayload
{
    internal class ReverseStream
    {
        Stream baseStream;

        public ReverseStream(Stream stream)
        {
            baseStream = stream;
            baseStream.Seek(0, SeekOrigin.End);
        }

        public int ReadByte()
        {
            baseStream.Position--;
            var o = baseStream.ReadByte();
            baseStream.Position--;
            return o;
        }

        public int Read(byte[] buffer, long start, int length)
        {
            byte[] toReverse = new byte[length];

            baseStream.Position -= length + start;
            var o = baseStream.Read(toReverse, 0, length);
            baseStream.Position -= length;

            Array.Copy(toReverse.Reverse().ToArray(), buffer, length);

            return o;
        }

        public void Write(byte[] buffer, long start, int length)
        {
            byte[] cropped = new byte[length];
            Array.Copy(buffer, cropped, length);

            baseStream.Position -= length + start;
            baseStream.Write(cropped.Reverse().ToArray(), 0, length);
            baseStream.Position -= length;
        }
    }
}
