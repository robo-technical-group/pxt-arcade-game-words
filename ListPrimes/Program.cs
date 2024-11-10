using System.Collections;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// https://csharphelper.com/howtos/howto_linked_list_primes.html
const int MAX_VAL = 10_000_000;
bool PRINT = false;
System.Diagnostics.Stopwatch sw = new();
List<int> primes;
Console.WriteLine($"Finding prime numbers up to {MAX_VAL:#,###,###,###}.");
sw.Start();
// StandardMethod();
SoE();
sw.Stop();
Console.WriteLine();
Console.WriteLine($"Found {primes.Count:#,###,###,###} prime numbers up to {MAX_VAL:#,###,###,###} in {sw.Elapsed.TotalSeconds:0.00} seconds.");
Console.WriteLine($"Largest prime found: {primes[primes.Count - 1]:#,###,###,###}");

void StandardMethod()
{
    primes = [2,];
    if (PRINT) { Console.Write("2 "); }
    for (int i = 3; i <= MAX_VAL; i++)
    {
        // Standard divisibility method.
        bool isPrime = true;
        int sqrtI = (int)Math.Sqrt(i);
        for (int j = 0; j < primes.Count; j++)
        {
            if (primes[j] > sqrtI)
            {
                break;
            }
            if (i % primes[j] == 0)
            {
                isPrime = false;
                break;
            }
        }
        if (isPrime)
        {
            primes.Add(i);
            if (PRINT)
            {
                Console.Write(i);
                Console.Write(' ');
                if (primes.Count % 5 == 0)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}

// https://stackoverflow.com/questions/1042902/most-elegant-way-to-generate-prime-numbers
void SoE()
{
    primes = [];
    BitArray bits = SoEImpl(MAX_VAL);
    int count = 0;
    for (int i = 0; i < MAX_VAL; i++)
    {
        if (bits[i])
        {
            primes.Add(i);
            count++;
            if (PRINT)
            {
                Console.Write(i);
                Console.Write(' ');
                if (count % 5 == 0)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}

BitArray SoEImpl(int limit)
{
    BitArray bits = new(limit + 1, true);
    bits[0] = false;
    bits[1] = false;
    for (int i = 0; i * i <= limit; i++)
    {
        if (bits[i])
        {
            for (int j = i * i; j <= limit; j += i)
            {
                bits[j] = false;
            }
        }
    }
    return bits;
}