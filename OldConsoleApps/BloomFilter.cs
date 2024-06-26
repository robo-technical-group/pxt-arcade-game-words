using System.Collections;

namespace OldConsoleApps
{
    internal class BloomFilter
    {
        public int n { get; set; }
        public int m { get; set; }
        public int k { get; set; }
        public ulong p { get; set; }
        public double prob { get; set; }
        public ulong[] aValues { get; set; }
        public ulong[] bValues { get; set; }
        protected BitArray bits { get; set; }

        public BloomFilter(int n, ulong p, double prob)
        {
            /*
             *  https://hur.st/bloomfilter/
             *  n = ceil(m / (-k / log(1 - exp(log(p) / k))))
             *  p = pow(1 - exp(-k / (m / n)), k)
             *  m = ceil((n * log(p)) / log(1 / pow(2, log(2))));
             *  k = round((m / n) * log(2));
             */
            this.n = n;
            this.p = p;
            this.prob = prob;
            m = (int)Math.Ceiling(n * Math.Log(prob) / Math.Log(1 / Math.Pow(2, Math.Log(2))));
            if (m % 8 > 0)
            {
                m = m + 8 - m % 8;
            }
            k = (int)Math.Round(m / n * Math.Log(2));
            aValues = new ulong[k];
            bValues = new ulong[k];
            Random r = new Random();
            for (int i = 0; i < k; i++)
            {
                while (aValues[i] == 0)
                {
                    aValues[i] = Convert.ToUInt64(r.NextInt64(Convert.ToInt64(p) - 1));
                }
                bValues[i] = Convert.ToUInt64(r.NextInt64(Convert.ToInt64(p) - 1));
            }
            bits = new(m, false);
        }

        public string filter
        {
            get
            {
                byte[] byteArray = new byte[m / 8];
                bits.CopyTo(byteArray, 0);
                return Convert.ToBase64String(byteArray);
            }
        }

        public void AddWordToFilter(string word)
        {
            ulong value = GetWordValue(word);
            for (int i = 0; i < k; i++)
            {
                bits[GetHashForWordValue(value, i)] = true;
            }
        }

        public bool FindWord(string word)
        {
            ulong value = GetWordValue(word);
            for (int i = 0; i < k; i++)
            {
                if (!GetFilterBit(GetHashForWordValue(value, i)))
                {
                    return false;
                }
            }
            return true;
        }

        protected int GetByte64(string s, int i)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".IndexOf(s[i]);
        }

        protected bool GetFilterBit(int location)
        {
            int blockNum = location / 24;
            int byteNum = (location % 24) / 8;
            int bitNum = location % 8;

            string block = filter.Substring(blockNum * 4, 4);
            byte[] bytes = Convert.FromBase64String(block);
            // https://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte
            return (bytes[byteNum] & (1 << bitNum)) != 0;
            /*
             * Simpler version:
            return bits[location];
             */
        }

        protected int GetHashForWord(string word, int hash)
        {
            // h_a,b(x) = ((ax + b) mod p) mod m
            return GetHashForWordValue(GetWordValue(word), hash);
        }

        protected int GetHashForWordValue(ulong value, int hash)
        {
            UInt128 a = (UInt128)aValues[hash];
            UInt128 b = (UInt128)bValues[hash];
            UInt128 prod = a * value + b;
            return Convert.ToInt32((prod % p % (ulong)m).ToString());
        }

        public static ulong GetWordValue(string word)
        {
            ulong toReturn = 0;
            string ucWord = word.ToUpperInvariant();
            foreach (char c in ucWord)
            {
                toReturn = (toReturn << 5) + (ulong)(c - 'A');
            }
            return Math.Max(toReturn, 1);
        }
    }
}
