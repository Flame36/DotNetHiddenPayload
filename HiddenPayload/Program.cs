using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HiddenPayload
{
    internal class Program
    {
        static FileStream ownData;
        const string HEADER = "haspayload";
        const int CHUNK = 1024;

        static void Main(string[] args)
        {
            ownData = File.OpenRead(Environment.ProcessPath);

            byte[] payload = GetPayload(ownData);
            if (payload is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No payload found");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[#] Payload: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Encoding.UTF8.GetString(payload));
                Console.ResetColor();
            }


            Console.Write("Enter new payload: ");
            string newPayload = Console.ReadLine();

            Console.Write("Enter file name of copy: ");
            string fileName = Console.ReadLine();

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(newPayload));

            File.Copy(Environment.ProcessPath, fileName, true);
            SetPayload(File.Open(fileName, FileMode.Open), ms);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] Successfully wrote payload");
            Console.ResetColor();
        }

        static byte[] GetPayload(Stream data)
        {
            ReverseStream reverseStream = new ReverseStream(data);

            byte[] buffer = Encoding.UTF8.GetBytes(HEADER);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (reverseStream.ReadByte() != buffer[i])
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Header mismatch");
                    Console.ResetColor();

                    return null;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] Header matches");
            Console.ResetColor();

            buffer = new byte[8];
            reverseStream.Read(buffer, 0, 8);

            long payloadLength = BitConverter.ToInt64(buffer);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[#] Payload Length: {payloadLength}");
            Console.ResetColor();

            byte[] payload = new byte[payloadLength];

            buffer = new byte[CHUNK];
            int toWrite = CHUNK;
            for (int i = 0; i < payloadLength; i += CHUNK)
            {
                reverseStream.Read(buffer, 0, buffer.Length);
                Array.Copy(buffer, 0, payload, i * CHUNK, Math.Min(CHUNK, payloadLength - i * CHUNK));
            }

            return payload;
        }

        static void SetPayload(Stream data, Stream payload)
        {
            long payloadLength = 0;
            long fullPayloadLength = 0;
            byte[] header = Encoding.UTF8.GetBytes(HEADER);
            byte[] buffer = Encoding.UTF8.GetBytes(HEADER);

            bool noPayload = TryGetPayloadLength(data, out payloadLength);

            fullPayloadLength = payloadLength + header.Length + 8; //Adding header + length size to raw payload
            if (noPayload)
                fullPayloadLength = 0;

            data.SetLength(data.Length - fullPayloadLength + payload.Length + header.Length + 8);

            ReverseStream reverseStream = new ReverseStream(data);

            reverseStream.Write(header, 0, header.Length);
            reverseStream.Write(BitConverter.GetBytes(payload.Length), 0, 8);

            buffer = new byte[CHUNK];
            int readAmount = 0;
            for (int i = 0; i < payload.Length; i += CHUNK)
            {
                readAmount = payload.Read(buffer, 0, buffer.Length);
                reverseStream.Write(buffer, 0, readAmount);
            }
        }

        static bool TryGetPayloadLength(Stream data, out long payloadLength)
        {
            try
            {
                ReverseStream reverseStream = new ReverseStream(data);

                byte[] header = Encoding.UTF8.GetBytes(HEADER);
                byte[] buffer = Encoding.UTF8.GetBytes(HEADER);

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (reverseStream.ReadByte() != buffer[i])
                    {
                        payloadLength = 0;
                        return false;
                    }
                }

                buffer = new byte[8];
                reverseStream.Read(buffer, 0, 8);

                payloadLength = BitConverter.ToInt64(buffer);
                return true;
            }
            catch
            {
                payloadLength = 0;
                return false;
            }
        }
    }
}