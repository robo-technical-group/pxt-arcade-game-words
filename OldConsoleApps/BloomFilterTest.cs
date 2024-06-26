using System.Text;
using System.Text.Json;

namespace OldConsoleApps
{
    internal class BloomFilterTest
    {
        const string WORD_FILE = @"D:\T\GW20P\GW20P.txt";
        const string OUT_FILE = @"D:\Downloads\test.ts";
        const string JSON_FILE = @"D:\Downloads\filters.json";

        protected static StreamWriter tsStream = new(OUT_FILE);
        protected static StreamWriter filterStream = new(JSON_FILE);

        // First, count the word sizes.
        protected static List<int> wordLengths = new()
        {
            0, // Index zero will be total word count.
            0, // There are no one-letter words.
            0, // Initialize the two-letter word counter.
        };

        public static async Task Run()
        {
            /**
             * Using Game Words 2020 Provisional
             * https://www.tylerhosting.com/gamewords/
             * Game Words 2020 (provisional) © Dana Bell
             * 
             * Because the Game Words list is sorted alphabetically by word length,
             * we can process the file sequentially without loading words into memory.
             */

            /*
            string maxWord = "Z";
            while (maxWord.Length < 21)
            {
                Console.Write(maxWord);
                Console.Write($" length = {maxWord.Length} value = ");
                Console.WriteLine(BloomFilter.GetWordValue(maxWord));
                maxWord += 'Z';
            }
            Environment.Exit(0);

            Z length = 1 value = 25
            ZZ length = 2 value = 825
            ZZZ length = 3 value = 26425
            ZZZZ length = 4 value = 845625
            ZZZZZ length = 5 value = 27060025
            ZZZZZZ length = 6 value = 865920825
            ZZZZZZZ length = 7 value = 27709466425
            ZZZZZZZZ length = 8 value = 886702925625
            ZZZZZZZZZ length = 9 value = 28374493620025
            ZZZZZZZZZZ length = 10 value = 907983795840825
            ZZZZZZZZZZZ length = 11 value = 29055481466906425
            ZZZZZZZZZZZZ length = 12 value = 929775406941005625
            ZZZZZZZZZZZZZ length = 13 value = 11306068948402628409
            ZZZZZZZZZZZZZZ length = 14 value = 11306068948402628409
            ZZZZZZZZZZZZZZZ length = 15 value = 11306068948402628409
            ZZZZZZZZZZZZZZZZ length = 16 value = 11306068948402628409
            ZZZZZZZZZZZZZZZZZ length = 17 value = 11306068948402628409
            ZZZZZZZZZZZZZZZZZZ length = 18 value = 11306068948402628409
            ZZZZZZZZZZZZZZZZZZZ length = 19 value = 11306068948402628409
            ZZZZZZZZZZZZZZZZZZZZ length = 20 value = 11306068948402628409
            */

            int currWordLength = 2;

            foreach (string word in File.ReadLines(WORD_FILE))
            {
                if (word.Length > 0)
                {
                    if (word.Length != currWordLength)
                    {
                        currWordLength = word.Length;
                        wordLengths.Add(0);
                    }
                    wordLengths[0]++;
                    wordLengths[currWordLength]++;
                }
            }

            // Now, create the Bloom filters.
            const double P = 0.001;
            ulong[] PRIMES = new ulong[] { 0, 0,
                827, // ZZ length = 2 value = 825
                26431, // ZZZ length = 3 value = 26425
                845653, // ZZZZ length = 4 value = 845625
                27060037, // ZZZZZ length = 5 value = 27060025
                865920827, // ZZZZZZ length = 6 value = 865920825
                27709466437, // ZZZZZZZ length = 7 value = 27709466425
                934034459887, // ZZZZZZZZ length = 8 value = 886702925625
                3435346099469, // ZZZZZZZZZ length = 9 value = 28374493620025
                951238329756307, // ZZZZZZZZZZ length = 10 value = 907983795840825
                32919169907653669, // ZZZZZZZZZZZ length = 11 value = 29055481466906425
                964105869742568687, // ZZZZZZZZZZZZ length = 12 value = 929775406941005625 longest 64-bit value
            };

            BloomFilter[] filters = new BloomFilter[PRIMES.Length];
            currWordLength = 1;
            BloomFilter bf = filters[0];
            // JsonSerializerOptions jso = new() { WriteIndented = true };
            int wordsProcessed = 0;

            foreach (string word in File.ReadLines(WORD_FILE))
            {
                if (word.Length != currWordLength)
                {
                    if (currWordLength == 1)
                    {
                        await WriteTypescriptHeader(tsStream);
                        await WriteJsonHeader(filterStream);
                    }
                    else
                    {
                        // Send current values to the output stream.
                        Console.WriteLine($"Writing {currWordLength}-letter word filter to disk.");
                        // await tsStream.WriteAsync(JsonSerializer.Serialize(bf, jso));
                        await WriteTypescriptFilter(tsStream, bf, currWordLength);
                        await filterStream.WriteAsync(JsonSerializer.Serialize(bf));
                        await filterStream.WriteLineAsync(',');
                        filters[currWordLength] = bf;
                    }

                    if (word.Length >= PRIMES.Length)
                    {
                        break;
                    }
                    bf = new BloomFilter(wordLengths[word.Length], PRIMES[word.Length], P);
                    currWordLength = word.Length;
                }

                bf.AddWordToFilter(word);
                wordsProcessed++;
            }

            await WriteTypescriptFooter(tsStream);
            await WriteJsonFooter(filterStream);
            tsStream.Close();
            filterStream.Close();
            Console.WriteLine($"Words written to file: {wordsProcessed}");
            Console.WriteLine("Done!");

            const string EXIT_TOKEN = "EXIT NOW";
            Console.Write("Enter ");
            Console.Write(EXIT_TOKEN);
            Console.WriteLine(" to exit.");
            string input = "";
            while (input != EXIT_TOKEN)
            {
                Console.Write("Enter word to search: ");
                input = Console.ReadLine() ?? EXIT_TOKEN;
                if (input.ToUpperInvariant() == EXIT_TOKEN)
                {
                    break;
                }
                if (input.Length < filters.Length && filters[input.Length].FindWord(input))
                {
                    Console.WriteLine($"Your word {input} is likely in the database.");
                }
                else
                {
                    Console.WriteLine($"Your word {input} is not in the database.");
                }
            }
        }

        protected static string ArrayToString(Array array)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (var item in array)
            {
                sb.Append('\'');
                sb.Append(item.ToString());
                sb.Append("',");
            }
            sb.Append(']');
            return sb.ToString();
        }

        protected static async Task WriteJsonHeader(StreamWriter sw)
        {
            await sw.WriteLineAsync('[');
        }

        protected static async Task WriteJsonFooter(StreamWriter sw)
        {
            await sw.WriteLineAsync(']');
        }

        protected static async Task WriteTypescriptHeader(StreamWriter sw)
        {
            await sw.WriteLineAsync(@"/**
 * Using Game Words 2020 Provisional
 * https://www.tylerhosting.com/gamewords/
 * Game Words 2020 (provisional) (C) Dana Bell
 */
interface BloomFilter {
    n: number,
    m: number,
    k: number,
    p: string,
    prob: number,
    aValues: string[],
    bValues: string[],
    filter: string,
}

namespace GameWords {
    export const FILTERS: BloomFilter[] = [ null, null,");
        }

        protected static async Task WriteTypescriptFilter(StreamWriter sw, BloomFilter bf, int wordLength)
        {
            await sw.WriteLineAsync($$"""
{
    // {{wordLength}}-letter words.
    n: {{bf.n}},
    m: {{bf.m}},
    k: {{bf.k}},
    p: '{{bf.p}}',
    prob: {{bf.prob}},
    aValues: {{ArrayToString(bf.aValues)}},
    bValues: {{ArrayToString(bf.bValues)}},
    filter: '{{bf.filter}}',
},
""");
        }

        protected static async Task WriteTypescriptFooter(StreamWriter sw)
        {
            await sw.WriteLineAsync(@"
    ]

    export function findWord(word: string): boolean {
        const value: JSBI.BigInt = getWordValue(word)
        const k: number = FILTERS[word.length].k
        // game.splash(value)
        for (let i: number = 0; i < k; i++) {
            if (!getFilterBit(getHashForWordValue(value, word.length, i), word.length)) {
                return false
            }
        }
        return true
    }

    function getFilterBit(location: number, wordLength: number): boolean {
        const blockNum: number = Math.floor(location / 24)
        const byteNum: number = Math.floor((location % 24) / 8)
        const bitNum: number = location % 8
        const block: string = FILTERS[wordLength].filter.substr(blockNum * 4, 4)
        const byte: number = Buffer.fromBase64(block).getUint8(byteNum)
        return (byte & (1 << bitNum)) != 0
    }

    function getHashForWord(word: string, hash: number) {
        return getHashForWordValue(getWordValue(word), word.length, hash)
    }

    function getHashForWordValue(value: JSBI.BigInt, wordLength: number, hash: number): number {
        const a: JSBI.BigInt = JSBI.CreateBigInt(FILTERS[wordLength].aValues[hash])
        const b: JSBI.BigInt = JSBI.CreateBigInt(FILTERS[wordLength].bValues[hash])
        const p: JSBI.BigInt = JSBI.CreateBigInt(FILTERS[wordLength].p)
        const m: JSBI.BigInt = JSBI.CreateBigInt(FILTERS[wordLength].m)
        // ax + b % p % m
        let r: JSBI.BigInt = JSBI.multiply(a, value)
        r = JSBI.add(r, b)
        r = JSBI.mod(r, p)
        r = JSBI.mod(r, m)
        return r.toNumber()
    }

    function getWordValue(word: string): JSBI.BigInt {
        const ucWord: string = word.toUpperCase()
        let toReturn: JSBI.BigInt = JSBI.CreateBigInt(0)
        for (let c of ucWord) {
            // toReturn = (toReturn << 5) + c.charCodeAt(0) - 'A'.charCodeAt(0)
            toReturn = JSBI.leftShift(toReturn, 5)
            toReturn = JSBI.add(toReturn, JSBI.CreateBigInt(c.charCodeAt(0) - 'A'.charCodeAt(0)))
        }
        return toReturn.length == 0 ? JSBI.CreateBigInt(1) : toReturn
    }
}

const EXIT_TOKEN: string = 'EXIT NOW'
let input: string = ''
let msg: string
while (input != EXIT_TOKEN) {
    input = game.askForString('Enter a word.')
    if (input.toUpperCase() == EXIT_TOKEN) {
        break
    }
    if (input.length < GameWords.FILTERS.length && GameWords.findWord(input)) {
        msg = `Your word ${input} is likely in the database.`
    } else {
        msg = `Your word ${input} is not in the database.`
    }
    game.showLongText(msg, DialogLayout.Center)
}
");
        }
    }
}
