namespace OldConsoleApps
{
    internal class SearchTest
    {
        protected const string FILTER_FILE = @"D:\Downloads\filters.txt";
        protected static ulong[] PRIMES = new ulong[] { 0, 0,
                2531, // 2525
                252533, // 252525
                25252529, // 25252525
                2525252567, // 2525252525
                252525252527, // 252525252525
                35548071406777, // 25252525252525
                3155772447700777, // 2525252525252525
                282865737627973957, // 252525252525252525 longest 64-bit value
            };
        protected static ulong[][] A_VALUES = new ulong[][] {
                Array.Empty<ulong>(),
                Array.Empty<ulong>(),
                new ulong[] { 2265, 909, 755, 2411, 29, 1656, 245, 1161, 1718, 1678, }, // 2-letter words
                new ulong[] { 44075, 220558, 17548, 228676, 115498, 234172, 158513, 165010, 173133, 54520, }, // 3-letter words
                new ulong[] { 11046141, 16818029, 2349783, 2801053, 2481415, 7349157, 6565051, 6445156, 17220510, 20833463, }, // 4-letter words
                new ulong[] { 1903654165, 1528968809, 130164034, 1758486665, 117119348, 478672246, 484306301, 922874302, 342776417, 1348639979, }, // 5-letter words
                new ulong[] { 85779670938, 52065269547, 170812959701, 129232084340, 71572018322, 210817314764, 94591598528, 91110000195, 20986719230, 185003569305, }, // 6-letter words
                new ulong[] { 16209272772297, 25319714751275, 14666567201143, 4744591577409, 28853764697327, 34534785865671, 9424941828157, 5665447713840, 14561355779553, 7599334080593, }, // 7-letter words
                new ulong[] { 2368251681159559, 604851349411717, 1447009560609687, 1963916936618673, 2418115083755466, 1936818570008801, 2472516983480115, 398519921141054, 1936184286897537, 1050685507727232, }, // 8-letter words
                new ulong[] { 248498580363664844, 105637179117312313, 68635236568628779, 27851941832863095, 128590657659777182, 237848518251234732, 234233871793794979, 28617387496010700, 213288978936245207, 220991753563499472, }, // 9-letter words
            };
        protected static ulong[][] B_VALUES = new ulong[][] {
                Array.Empty<ulong>(),
                Array.Empty<ulong>(),
                new ulong[] { 1670, 2432, 1676, 455, 905, 568, 2511, 587, 1763, 2351, }, // 2-letter words
                new ulong[] { 137813, 42213, 105946, 190645, 112541, 10673, 199525, 149798, 122969, 126368, }, // 3-letter words
                new ulong[] { 12535173, 21381156, 23072992, 15822356, 21296165, 3110812, 22403538, 18480371, 21912446, 7108147, }, // 4-letter words
                new ulong[] { 414513958, 1106841229, 1158485383, 551111495, 141328704, 2160207423, 748236604, 2755103, 1967041911, 902970574, }, // 5-letter words
                new ulong[] { 223251190061, 176580476655, 64409987979, 228003336127, 250377022535, 246803982843, 57155269950, 47473752210, 99583818832, 175767652228, }, // 6-letter words
                new ulong[] { 28698401292482, 24029225363140, 35214219626449, 6438313040297, 29038220718308, 3227503493143, 33782070092027, 12931333846021, 30433925114339, 11490548941537, }, // 7-letter words
                new ulong[] { 2939409589829047, 1915636773296403, 111808634884945, 2186244293412165, 1647024486196525, 2829255847109254, 2221014760128229, 394710106749103, 1337393720302768, 1901143145725804, }, // 8-letter words
                new ulong[] { 196781003119832536, 69165802024429336, 219231404684141616, 225803497313925769, 194244161384162511, 65438956805134727, 72574387326687827, 191943178012680214, 68628457913546941, 250471536132457182, }, // 9-letter words
            };
        protected static ulong[] M_VALUES = new ulong[] { 0, 0, 1584, 16656, 61800, 136464, 239824, 365368, 455240, 447840, };
        protected static int[] K_VALUES = new int[] { 10, 10, 10, 10, 10, 10, 10, 10, };
        protected static List<string> filters = new List<string>();

        public static void Run()
        {
            foreach (string filter in File.ReadLines(FILTER_FILE))
            {
                filters.Add(filter);
            }

            const string EXIT_TOKEN = "EXIT NOW";
            string input = "";
            while (input != EXIT_TOKEN)
            {
                Console.Write("Enter word to search: ");
                input = Console.ReadLine() ?? "";
                if (input == EXIT_TOKEN)
                {
                    break;
                }
                if (searchFilters(input))
                {
                    Console.WriteLine($"Your word {input} is likely in the database.");
                }
                else
                {
                    Console.WriteLine($"Your word {input} is not in the database.");
                }
            }
        }

        protected static int hash(string word, int k)
        {
            // h_a,b(x) = ((ax + b) mod p) mod m
            UInt128 a = (UInt128)A_VALUES[word.Length][k];
            UInt128 b = (UInt128)B_VALUES[word.Length][k];
            ulong p = PRIMES[word.Length];
            UInt128 prod = a * wordValue(word) + b;
            return Convert.ToInt32((prod % p % (ulong)M_VALUES[word.Length]).ToString());
        }

        protected static bool searchFilter(int hash, int wordLength)
        {
            int blockNum = hash / 24;
            int byteNum = (hash % 24) / 8;
            int bitNum = hash % 8;

            string block = filters[wordLength].Substring(blockNum * 4, 4);
            byte[] bytes = Convert.FromBase64String(block);
            // https://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte
            return (bytes[byteNum] & (1 << bitNum)) != 0;
        }

        protected static bool searchFilters(string word)
        {
            bool toReturn = true;
            for (int i = 0; i < K_VALUES[word.Length]; i++)
            {
                int h = hash(word, i);
                if (!searchFilter(h, word.Length))
                {
                    toReturn = false;
                    break;
                }
            }
            return toReturn;
        }

        protected static ulong wordValue(string word)
        {
            string ucWord = word.ToUpperInvariant();
            string wordValue = "";
            foreach (char c in ucWord)
            {
                wordValue += (c - 'A').ToString("00");
            }
            return Math.Max(Convert.ToUInt64(wordValue), 1);
        }
    }
}
