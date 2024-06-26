// See https://aka.ms/new-console-template for more information
using OldConsoleApps;

Console.WriteLine("Hello, World!");
int choice = -1;
const int EXIT = 99;
while (choice != EXIT)
{
    Console.WriteLine("=====");
    Console.WriteLine("MAIN MENU");
    Console.WriteLine("=====");
    Console.WriteLine("1. Base64 Test");
    Console.WriteLine("2. Bloom Filter Test");
    Console.WriteLine("3. Search Test");
    Console.WriteLine($"{EXIT}. Quit");
    Console.Write("Enter selection --> ");
    bool valid = Int32.TryParse(Console.ReadLine(), out choice);
    if (! valid)
    {
        choice = -1;
        continue;
    }
    switch (choice)
    {
        case 1:
            Base64Test.Run();
            break;

        case 2:
            await BloomFilterTest.Run();
            break;

        case 3:
            SearchTest.Run();
            break;
    }
}