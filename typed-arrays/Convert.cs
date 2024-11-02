namespace typed_arrays
{
    public static class Convert
    {
        public static IList<int> PackI8(int n) { return [n & 0xff,]; }
        public static int UnpackI8(IList<int> bytes) { return AsSigned(bytes[0], 8); }

        public static IList<int> PackU8(int n) { return [n & 0xff,]; }
        public static int UnpackU8(IList<int> bytes) { return AsUnsigned(bytes[0], 8); }

        public static IList<int> PackU8Clamped(int n) { return [(n < 0 ? 0 : n > 0xff ? 0xff : n & 0xff),]; }

        public static IList<int> PackI16(int n) { return [n & 0xff, (n >> 8) & 0xff,]; }
        public static int UnpackI16(IList<int> bytes) { return AsSigned(bytes[1] << 8 | bytes[0], 16); }

        public static IList<int> PackU16(int n) { return [n & 0xff, (n >> 8) & 0xff,]; }
        public static int UnpackU16(IList<int> bytes) { return AsUnsigned(bytes[1] << 8 | bytes[0], 16); }

        public static IList<int> PackI32(int n) { return [n & 0xff, (n >> 8) & 0xff, (n >> 16) & 0xff, (n >> 24) & 0xff,]; }
        public static int UnpackI32(IList<int> bytes) { return AsSigned(bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0], 32); }

        public static IList<int> PackU32(int n) { return [n & 0xff, (n >> 8) & 0xff, (n >> 16) & 0xff, (n >> 24) & 0xff,]; }
        public static int UnpackU32(IList<int> bytes) { return AsUnsigned(bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0], 32); }

        private static int AsSigned(int value, int bits)
        {
            int s = 32 - bits;
            return (value << s) >> s;
        }

        private static int AsUnsigned(int value, int bits)
        {
            int s = 32 - bits;
            return (value << s) >>> s;
        }
    }
}
