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

using System.Numerics;

namespace BaseX
{
    public static class StaticRadixCoders
    {
        public class Uint8 : RadixCoder<byte>
        {
            public Uint8(int b) : base(b)
            {
                CheckBaseMax(BASE_MAX_U8);
            }

            protected override byte Mask { get { return 0xff; } }
        }

        public class Uint16 : RadixCoder<ushort>
        {
            public Uint16(int b) : base(b)
            {
                CheckBaseMax(BASE_MAX_U16);
            }

            protected override ushort Mask { get { return 0xffff; } }
        }
    }

    public abstract class RadixCoder
    {
        public static readonly int BASE_MIN = 2;
        public static readonly int BASE_MAX_U8 = 0x100;
        public static readonly int BASE_MAX_U16 = 0x10000;
        public static readonly double LOG_BYTE = Math.Log(BASE_MAX_U8);

        protected readonly int b; // base
        protected readonly double encodeFactor;
        protected readonly double decodeFactor;

        public RadixCoder(int b)
        {
            if (b < BASE_MIN)
            {
                throw new ArgumentOutOfRangeException(nameof(b), $"Base must be >= {BASE_MIN}");
            }
            this.b = b;
            double logBase = Math.Log(b);
            encodeFactor = LOG_BYTE / logBase;
            decodeFactor = logBase / LOG_BYTE;
        }

        public int Base
        {
            get { return b; }
        }

        public int HashCode
        {
            get { return b; }
        }
        protected void CheckBaseMax(int max)
        {
            if (b > max)
            {
                throw new ArgumentOutOfRangeException(nameof(max), $"Base must be <= {max}.");
            }
        }

        protected void CheckDigitBase(int digit)
        {
            if (digit >= b)
            {
                throw new ArgumentOutOfRangeException(nameof(digit), $"Digit must be < {b}.");
            }
        }

        protected static int LeadingZeros<T>(T[] a) where T : IBinaryInteger<T>
        {
            int zc = 0;
            while (zc < a.Length && a[zc] == T.Zero)
            {
                zc++;
            }
            return zc;
        }

        protected static T[] Drop<T>(T[] a, int start) where T : IBinaryInteger<T>
        {
            return start == 0 ? a : a.TakeLast(a.Length - start).ToArray();
        }

        protected static int CeilMultiply(int n, double f)
        {
            return (int)Math.Ceiling(n * f);
        }
    }

    public abstract class RadixCoder<N>(int b) : RadixCoder(b) where N : IBinaryInteger<N>
    {
        protected abstract N Mask
        {
            get;
        }

        public byte[] Decode(N[] n)
        {
            int zeroCount = LeadingZeros(n);
            if (zeroCount == n.Length)
            {
                return new byte[n.Length];
            }
            int capacity = zeroCount + CeilMultiply(n.Length - zeroCount, decodeFactor);
            byte[] dst = new byte[capacity];
            int j = capacity - 2;
            for (int i = zeroCount; i < n.Length; i++)
            {
                int carry = int.CreateTruncating(n[i] & Mask);
                CheckDigitBase(carry);
                for (int k = capacity - 1; k > j; k--)
                {
                    carry += (dst[k] & 0xff) * b;
                    dst[k] = (byte)carry;
                    carry >>>= 8;
                }
                while (carry > 0)
                {
                    dst[j--] = (byte)carry;
                    carry >>>= 8;
                }
            }
            return Drop(dst, j - zeroCount + 1);
        }

        public N[] Encode(byte[] bytes)
        {
            int zeroCount = LeadingZeros(bytes);
            if (zeroCount == bytes.Length)
            {
                return new N[bytes.Length];
            }
            int capacity = zeroCount + CeilMultiply(bytes.Length - zeroCount, encodeFactor);
            N[] dst = new N[capacity];
            int j = capacity - 2;
            for (int i = zeroCount; i < bytes.Length; i++)
            {
                int carry = bytes[i] & 0xff;
                for (int k = capacity - 1; k > j; k--)
                {
                    carry += int.CreateTruncating(dst[k] & Mask) << 8;
                    dst[k] = N.CreateTruncating(carry % b);
                    carry /= b;
                }
                while (carry > 0)
                {
                    dst[j--] = N.CreateTruncating(carry % b);
                    carry /= b;
                }
            }
            return Drop(dst, j - zeroCount + 1);
        }
    }
}
