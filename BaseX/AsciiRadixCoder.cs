/**
 * https://github.com/hunterwb/base-x
 * 
 * MIT License
 *
 * Copyright (c) 2018-2019, Hunter WB <hunterwb.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace BaseX
{
    public class AsciiRadixCoder
    {
        protected const string BASE_91_STRING = "!#$%&()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        protected readonly byte[] chars;
        protected readonly byte[] digits;
        protected RadixCoder<byte> byteCoder;

        protected AsciiRadixCoder(string alphabet)
        {
            chars = new byte[alphabet.Length];
            digits = new byte[1 << 7];
            Array.Fill(digits, (byte)0xff);
            for (int i = 0; i < chars.Length; i++)
            {
                byte c = CheckAscii(alphabet[i]);
                chars[i] = c;
                if (digits[c] != 0xff)
                {
                    throw new ArgumentException($"Character {alphabet[i]} is repeated in alphabet.", nameof(alphabet));
                }
                digits[c] = (byte)i;
            }
            byteCoder = new StaticRadixCoders.Uint8(chars.Length);
        }

        public int Base { get { return chars.Length; } }
        public string Alphabet { get { return System.Text.Encoding.ASCII.GetString(chars); } }

        public byte[] Decode(string s)
        {
            byte[] bs = new byte[s.Length];
            for (int i = 0; i < bs.Length; i++)
            {
                byte c = CheckAscii(s[i]);
                byte digit = digits[c];
                if (digit ==  0xff)
                {
                    throw new ArgumentException($"Character {s[i]} is not present in alphabet.", nameof(s));
                }
                bs[i] = digit;
            }
            return byteCoder.Decode(bs);
        }

        public string Encode(byte[] bytes)
        {
            byte[] bs = byteCoder.Encode(bytes);
            for (int i = 0; i < bs.Length; i++)
            {
                bs[i] = chars[bs[i]];
            }
            return System.Text.Encoding.ASCII.GetString(bs);
        }

        public static AsciiRadixCoder Base91Coder()
        {
            return new AsciiRadixCoder(BASE_91_STRING);
        }

        public static AsciiRadixCoder Of(string alphabet)
        {
            return new AsciiRadixCoder(alphabet);
        }

        protected static byte CheckAscii(char c)
        {
            if (c >= 1 << 7)
            {
                throw new ArgumentOutOfRangeException(nameof(c), $"Character {c} is not ASCII.");
            }
            return (byte)c;
        }
    }
}
