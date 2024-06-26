using System.Collections;

namespace OldConsoleApps
{
    internal class Base64Test
    {
        public static void Run()
        {
            Random r = new();
            BitArray ba = new(48);
            for (int i = 0; i < ba.Length; i++)
            {
                ba[i] = r.Next(2) == 0;
            }
            PrintValues(ba, 8);
            byte[] bytes = new byte[ba.Length / 8];
            ba.CopyTo(bytes, 0);
            string base64rep = Convert.ToBase64String(bytes);
            Console.WriteLine($"Base64 string: {base64rep}");

            /*
            getBit(base64rep, 0);
            Console.WriteLine();
            getBit(base64rep, 24);
            Console.WriteLine();
            */

            for (int i = 0; i < ba.Length; i++)
            {
                bool bit = getBit(base64rep, i);
                if (bit != ba[i])
                {
                    Console.WriteLine($"Location {i} is inaccurate.");
                }
                Console.WriteLine();
            }
        }

        protected static void PrintValues(IEnumerable myList, int myWidth)
        {
            int i = myWidth;
            foreach (Object obj in myList)
            {
                if (i <= 0)
                {
                    i = myWidth;
                    Console.WriteLine();
                }
                i--;
                Console.Write("{0,8}", obj);
            }
            Console.WriteLine();
        }

        protected static void PrintBits(byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    Console.Write((b & (1 << i)) != 0 ? "1" : "0");
                }
                Console.Write(' ');
            }
        }

        protected static bool getBit(string b64string, int loc)
        {
            int blockNum = loc / 24;
            int bytenum = (loc % 24) / 8;
            int bitnum = loc % 8;
            string block = b64string.Substring(blockNum * 4, 4);

            Console.Write($"Location {loc} block {blockNum} byte {bytenum} bit {bitnum} substring {block} ");
            byte[] bytes = Convert.FromBase64String(block);
            /*
            Console.Write("bits ");
            PrintBits(bytes);
            Console.Write(" ");
            */

            return (bytes[bytenum] & (1 << bitnum)) != 0;
        }
    }
}
