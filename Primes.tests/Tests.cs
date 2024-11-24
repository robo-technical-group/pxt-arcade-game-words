using Primes;
using System.Numerics;
namespace Primes.tests;

[TestClass]
public sealed class Tests
{
    [TestMethod]
    public void TestCompositesSimple()
    {
        Assert.IsFalse(Miller.IsPrime(1), "Test A");
        Assert.IsFalse(Miller.IsPrime(0), "Test B");
        Assert.IsFalse(Miller.IsPrime(-1), "Test C");
        Assert.IsFalse(Miller.IsPrime("1"), "Test D");
        Assert.IsFalse(Miller.IsPrime("0"), "Test E");
        Assert.IsFalse(Miller.IsPrime("-1"), "Test F");
    }

    [TestMethod]
    public void TestCompositesSmall()
    {
        Assert.IsFalse(Miller.IsPrime(4), "Test A");
        Assert.IsFalse(Miller.IsPrime(9), "Test B");
        Assert.IsFalse(Miller.IsPrime(25), "Test C");
        Assert.IsFalse(Miller.IsPrime(121), "Test D");
        Assert.IsFalse(Miller.IsPrime(169), "Test E");
        Assert.IsFalse(Miller.IsPrime(961), "Test F");
        Assert.IsFalse(Miller.IsPrime(1_849), "Test G");
        Assert.IsFalse(Miller.IsPrime(2_209), "Test H");
        Assert.IsFalse(Miller.IsPrime(11_881), "Test I");
        Assert.IsFalse(Miller.IsPrime(994_009), "Test J");
    }

    [TestMethod]
    public void TestCompositesLarger()
    {
        Assert.IsFalse(Miller.IsPrime(3_996_001), "Test A");
        Assert.IsFalse(Miller.IsPrime(9_006_001), "Test B");
        Assert.IsFalse(Miller.IsPrime(16_008_001), "Test C");
        Assert.IsFalse(Miller.IsPrime(24_990_001), "Test D");
        Assert.IsFalse(Miller.IsPrime(35_844_169), "Test E");
        Assert.IsFalse(Miller.IsPrime(49_014_001), "Test F");
        Assert.IsFalse(Miller.IsPrime(63_888_049), "Test G");
        Assert.IsFalse(Miller.IsPrime(81_018_001), "Test H");
        Assert.IsFalse(Miller.IsPrime(99_460_729), "Test I");
        Assert.IsFalse(Miller.IsPrime(120_846_049), "Test J");
    }

    [TestMethod]
    public void TestCompositesLarger2()
    {
        Assert.IsFalse(Miller.IsPrime(256_032_001), "Test A");
        Assert.IsFalse(Miller.IsPrime(441_084_004), "Test B");
        Assert.IsFalse(Miller.IsPrime(675_948_001), "Test C");
        Assert.IsFalse(Miller.IsPrime(959_946_289), "Test D");
        Assert.IsFalse(Miller.IsPrime(1_295_928_001), "Test E");
        Assert.IsFalse(Miller.IsPrime(1_680_426_094), "Test F");
        Assert.IsFalse(Miller.IsPrime(2_114_988_121), "Test G");
        Assert.IsFalse(Miller.IsPrime(2_601_102_001), "Test H");
        Assert.IsFalse(Miller.IsPrime(3_135_664_009), "Test I");
        Assert.IsFalse(Miller.IsPrime(3_721_122_001), "Test J");
    }

    [TestMethod]
    public void TestPrimesSimple()
    {
        Assert.IsTrue(Miller.IsPrime(2), "Test A");
        Assert.IsTrue(Miller.IsPrime(3), "Test B");
        Assert.IsTrue(Miller.IsPrime(5), "Test C");
        Assert.IsTrue(Miller.IsPrime(7), "Test D");
        Assert.IsTrue(Miller.IsPrime(11), "Test E");
        Assert.IsTrue(Miller.IsPrime("2"), "Test F");
        Assert.IsTrue(Miller.IsPrime("3"), "Test G");
        Assert.IsTrue(Miller.IsPrime("5"), "Test H");
        Assert.IsTrue(Miller.IsPrime("7"), "Test I");
        Assert.IsTrue(Miller.IsPrime("11"), "Test J");
    }

    [TestMethod]
    public void TestPrimesSmall()
    {
        Assert.IsTrue(Miller.IsPrime(2_039), "Test A");
        Assert.IsTrue(Miller.IsPrime(2_113), "Test B");
        Assert.IsTrue(Miller.IsPrime(4_327), "Test C");
        Assert.IsTrue(Miller.IsPrime(6_991), "Test D");
        Assert.IsTrue(Miller.IsPrime(8_623), "Test E");
        Assert.IsTrue(Miller.IsPrime(11_273), "Test F");
        Assert.IsTrue(Miller.IsPrime(24_593), "Test G");
        Assert.IsTrue(Miller.IsPrime(39_727), "Test H");
        Assert.IsTrue(Miller.IsPrime(52_543), "Test I");
        Assert.IsTrue(Miller.IsPrime(61_001), "Test J");
    }

    [TestMethod]
    public void TestCompositesLarger3()
    {
        Assert.IsFalse(Miller.IsPrime(5_040_858_001), "Test A");
        Assert.IsFalse(Miller.IsPrime(6_496_198_801), "Test B");
        Assert.IsFalse(Miller.IsPrime(6_561_162_001), "Test C");
        Assert.IsFalse(Miller.IsPrime(8_280_454_009), "Test D");
        Assert.IsFalse(Miller.IsPrime(10_200_798_001), "Test E");
        Assert.IsFalse(Miller.IsPrime(12_318_558_121), "Test F");
        Assert.IsFalse(Miller.IsPrime(14_641_242_001), "Test G");
        Assert.IsFalse(Miller.IsPrime(17_157_594_169), "Test H");
        Assert.IsFalse(Miller.IsPrime(19_877_898_121), "Test I");
        Assert.IsFalse(Miller.IsPrime(22_798_282_081), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarger()
    {
        Assert.IsTrue(Miller.IsPrime(65_993), "Test A");
        Assert.IsTrue(Miller.IsPrime(70_729), "Test B");
        Assert.IsTrue(Miller.IsPrime(80_527), "Test C");
        Assert.IsTrue(Miller.IsPrime(90_787), "Test D");
        Assert.IsTrue(Miller.IsPrime(100_483), "Test E");
        Assert.IsTrue(Miller.IsPrime(110_251), "Test F");
        Assert.IsTrue(Miller.IsPrime(120_763), "Test G");
        Assert.IsTrue(Miller.IsPrime(130_447), "Test H");
        Assert.IsTrue(Miller.IsPrime(140_473), "Test I");
        Assert.IsTrue(Miller.IsPrime(150_473), "Test J");
    }

    [TestMethod]
    public void TestCompositesLarger4()
    {
        Assert.IsFalse(Miller.IsPrime(29_231_082_841), "Test A");
        Assert.IsFalse(Miller.IsPrime(36_479_854_009), "Test B");
        Assert.IsFalse(Miller.IsPrime(44_507_075_089), "Test C");
        Assert.IsFalse(Miller.IsPrime(53_361_462_001), "Test D");
        Assert.IsFalse(Miller.IsPrime(62_997_486_049), "Test E");
        Assert.IsFalse(Miller.IsPrime(73_426_366_729), "Test F");
        Assert.IsFalse(Miller.IsPrime(84_680_418_001), "Test G");
        Assert.IsFalse(Miller.IsPrime(96_719_134_009), "Test H");
        Assert.IsFalse(Miller.IsPrime(109_559_014_009), "Test I");
        Assert.IsFalse(Miller.IsPrime(123_193_278_121), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarger2()
    {
        Assert.IsTrue(Miller.IsPrime(170_441), "Test A");
        Assert.IsTrue(Miller.IsPrime(180_491), "Test B");
        Assert.IsTrue(Miller.IsPrime(200_467), "Test C");
        Assert.IsTrue(Miller.IsPrime(220_793), "Test D");
        Assert.IsTrue(Miller.IsPrime(240_743), "Test E");
        Assert.IsTrue(Miller.IsPrime(260_773), "Test F");
        Assert.IsTrue(Miller.IsPrime(280_711), "Test G");
        Assert.IsTrue(Miller.IsPrime(300_739), "Test H");
        Assert.IsTrue(Miller.IsPrime(320_431), "Test I");
        Assert.IsTrue(Miller.IsPrime(340_777), "Test J");
    }

    [TestMethod]
    public void TestCompositesLarger5()
    {
        Assert.IsFalse(Miller.IsPrime(152_893_962_081), "Test A");
        Assert.IsFalse(Miller.IsPrime(185_760_138_001), "Test B");
        Assert.IsFalse(Miller.IsPrime(221_840_058_001), "Test C");
        Assert.IsFalse(Miller.IsPrime(261_122_022_001), "Test D");
        Assert.IsFalse(Miller.IsPrime(303_597_694_009), "Test E");
        Assert.IsFalse(Miller.IsPrime(349_265_634_169), "Test F");
        Assert.IsFalse(Miller.IsPrime(398_157_214_009), "Test G");
        Assert.IsFalse(Miller.IsPrime(452_906_118_289), "Test H");
        Assert.IsFalse(Miller.IsPrime(508_341_906_361), "Test I");
        Assert.IsFalse(Miller.IsPrime(567_010_506_001), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarger3()
    {
        Assert.IsTrue(Miller.IsPrime(370_493), "Test A");
        Assert.IsTrue(Miller.IsPrime(410_749), "Test B");
        Assert.IsTrue(Miller.IsPrime(450_649), "Test C");
        Assert.IsTrue(Miller.IsPrime(490_537), "Test D");
        Assert.IsTrue(Miller.IsPrime(530_989), "Test E");
        Assert.IsTrue(Miller.IsPrime(570_491), "Test F");
        Assert.IsTrue(Miller.IsPrime(610_619), "Test G");
        Assert.IsTrue(Miller.IsPrime(652_739), "Test H");
        Assert.IsTrue(Miller.IsPrime(692_431), "Test I");
        Assert.IsTrue(Miller.IsPrime(732_827), "Test J");
    }

    // Large primes generated by https://bigprimes.org/

    [TestMethod]
    public void TestPrimesLarge13_1()
    {
        Assert.IsTrue(Miller.IsPrime(8887946897059), "Test A");
        Assert.IsTrue(Miller.IsPrime(5507904248221), "Test B");
        Assert.IsTrue(Miller.IsPrime(2674612653007), "Test C");
        Assert.IsTrue(Miller.IsPrime(2069156659351), "Test D");
        Assert.IsTrue(Miller.IsPrime(6047731003271), "Test E");
        Assert.IsTrue(Miller.IsPrime(9764436551017), "Test F");
        Assert.IsTrue(Miller.IsPrime(1453236382583), "Test G");
        Assert.IsTrue(Miller.IsPrime(2509990269037), "Test H");
        Assert.IsTrue(Miller.IsPrime(2847397176739), "Test I");
        Assert.IsTrue(Miller.IsPrime(9444944308097), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge13_2()
    {
        Assert.IsTrue(Miller.IsPrime(3353380433999), "Test A");
        Assert.IsTrue(Miller.IsPrime(6999984160973), "Test B");
        Assert.IsTrue(Miller.IsPrime(4760774919697), "Test C");
        Assert.IsTrue(Miller.IsPrime(8979756185537), "Test D");
        Assert.IsTrue(Miller.IsPrime(6086319862697), "Test E");
        Assert.IsTrue(Miller.IsPrime(1163229114613), "Test F");
        Assert.IsTrue(Miller.IsPrime(7651932626197), "Test G");
        Assert.IsTrue(Miller.IsPrime(6078633354887), "Test H");
        Assert.IsTrue(Miller.IsPrime(4808076515497), "Test I");
        Assert.IsTrue(Miller.IsPrime(9687393494579), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge14()
    {
        Assert.IsTrue(Miller.IsPrime(98787370476253), "Test A");
        Assert.IsTrue(Miller.IsPrime(42391636541027), "Test B");
        Assert.IsTrue(Miller.IsPrime(60040374072403), "Test C");
        Assert.IsTrue(Miller.IsPrime(88147348049147), "Test D");
        Assert.IsTrue(Miller.IsPrime(82278620138987), "Test E");
        Assert.IsTrue(Miller.IsPrime(30832644213271), "Test F");
        Assert.IsTrue(Miller.IsPrime(21205833202529), "Test G");
        Assert.IsTrue(Miller.IsPrime(16445548768687), "Test H");
        Assert.IsTrue(Miller.IsPrime(43655852950601), "Test I");
        Assert.IsTrue(Miller.IsPrime(66184645580293), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge15()
    {
        Assert.IsTrue(Miller.IsPrime(228282259815481), "Test A");
        Assert.IsTrue(Miller.IsPrime(806091914129573), "Test B");
        Assert.IsTrue(Miller.IsPrime(394218359811851), "Test C");
        Assert.IsTrue(Miller.IsPrime(465353159046403), "Test D");
        Assert.IsTrue(Miller.IsPrime(478924940663497), "Test E");
        Assert.IsTrue(Miller.IsPrime(138362789700491), "Test F");
        Assert.IsTrue(Miller.IsPrime(369961741790651), "Test G");
        Assert.IsTrue(Miller.IsPrime(904446177028211), "Test H");
        Assert.IsTrue(Miller.IsPrime(454733292074221), "Test I");
        Assert.IsTrue(Miller.IsPrime(288949886120131), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge16()
    {
        Assert.IsTrue(Miller.IsPrime(6541328833828991), "Test A");
        Assert.IsTrue(Miller.IsPrime(1411640135739839), "Test B");
        Assert.IsTrue(Miller.IsPrime(9128657707787509), "Test C");
        Assert.IsTrue(Miller.IsPrime(4093890621169703), "Test D");
        Assert.IsTrue(Miller.IsPrime(8437052919978961), "Test E");
        Assert.IsTrue(Miller.IsPrime(9690047830247209), "Test F");
        Assert.IsTrue(Miller.IsPrime(1733737361461717), "Test G");
        Assert.IsTrue(Miller.IsPrime(1270143200974913), "Test H");
        Assert.IsTrue(Miller.IsPrime(8310441193324799), "Test I");
        Assert.IsTrue(Miller.IsPrime(5361562187660659), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge17()
    {
        Assert.IsTrue(Miller.IsPrime(66004992311417933), "Test A");
        Assert.IsTrue(Miller.IsPrime(11586313038130843), "Test B");
        Assert.IsTrue(Miller.IsPrime(22405101392827369), "Test C");
        Assert.IsTrue(Miller.IsPrime(70959167280674221), "Test D");
        Assert.IsTrue(Miller.IsPrime(26510330261316809), "Test E");
        Assert.IsTrue(Miller.IsPrime(45611994008507501), "Test F");
        Assert.IsTrue(Miller.IsPrime(14333741104028833), "Test G");
        Assert.IsTrue(Miller.IsPrime(19699290846960013), "Test H");
        Assert.IsTrue(Miller.IsPrime(28977147354342149), "Test I");
        Assert.IsTrue(Miller.IsPrime(68518621615799077), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge18()
    {
        Assert.IsTrue(Miller.IsPrime(772729902541089013), "Test A");
        Assert.IsTrue(Miller.IsPrime(539060968671562177), "Test B");
        Assert.IsTrue(Miller.IsPrime(418513840173033053), "Test C");
        Assert.IsTrue(Miller.IsPrime(167115131161674383), "Test D");
        Assert.IsTrue(Miller.IsPrime(853909652680291679), "Test E");
        Assert.IsTrue(Miller.IsPrime(210994130232820219), "Test F");
        Assert.IsTrue(Miller.IsPrime(443886811605661421), "Test G");
        Assert.IsTrue(Miller.IsPrime(621825154825872071), "Test H");
        Assert.IsTrue(Miller.IsPrime(186624858175124987), "Test I");
        Assert.IsTrue(Miller.IsPrime(367813060877437009), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge19()
    {
        Assert.IsTrue(Miller.IsPrime(4646828069173917313), "Test A");
        Assert.IsTrue(Miller.IsPrime(1991143383135226211), "Test B");
        Assert.IsTrue(Miller.IsPrime(2082246825223782757), "Test C");
        Assert.IsTrue(Miller.IsPrime(7526965148447107453), "Test D");
        Assert.IsTrue(Miller.IsPrime(6542703549551904389), "Test E");
        Assert.IsTrue(Miller.IsPrime(6750372836910307271), "Test F");
        Assert.IsTrue(Miller.IsPrime(8254392108270983149), "Test G");
        Assert.IsTrue(Miller.IsPrime(3012401104548954509), "Test H");
        Assert.IsTrue(Miller.IsPrime(8869714492706493757), "Test I");
        Assert.IsTrue(Miller.IsPrime(8421236023597757123), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge20()
    {
        Assert.IsTrue(Miller.IsPrime("99494866901006543209"), "Test A");
        Assert.IsTrue(Miller.IsPrime(15216853046660798849), "Test B");
        Assert.IsTrue(Miller.IsPrime("42463726483535663689"), "Test C");
        Assert.IsTrue(Miller.IsPrime("36138254317669557541"), "Test D");
        Assert.IsTrue(Miller.IsPrime("97625764831914564587"), "Test E");
        Assert.IsTrue(Miller.IsPrime("56989824534052560487"), "Test F");
        Assert.IsTrue(Miller.IsPrime("49685771060192706809"), "Test G");
        Assert.IsTrue(Miller.IsPrime("76916876990600138641"), "Test H");
        Assert.IsTrue(Miller.IsPrime("83700731389310917823"), "Test I");
        Assert.IsTrue(Miller.IsPrime("95384499134992630331"), "Test J");
    }

    [TestMethod]
    public void TestCompositesLarge07()
    {
        Assert.IsFalse(Miller.IsPrime("52,615,569,947,315,219,329"), "Test A");
        Assert.IsFalse(Miller.IsPrime("73572728392890987649"), "Test B");
        Assert.IsFalse(Miller.IsPrime("24,983,718,861,618,167,641"), "Test C");
        Assert.IsFalse(Miller.IsPrime("8,801,103,043,757,404,249"), "Test D");
        Assert.IsFalse(Miller.IsPrime("78285386453356449481"), "Test E");
        Assert.IsFalse(Miller.IsPrime("57007719449574613801"), "Test F");
        Assert.IsFalse(Miller.IsPrime("66,765,261,770,683,615,441"), "Test G");
        Assert.IsFalse(Miller.IsPrime("96841420866851800129"), "Test H");
        Assert.IsFalse(Miller.IsPrime("10838472520306235401"), "Test I");
        Assert.IsFalse(Miller.IsPrime("3,637,712,493,406,034,761"), "Test J");
    }

    [TestMethod]
    public void TestPrimesLarge25()
    {
        // Some of these are too large and should thrown an exception.
        List<string> primes = [
            "6754124748341996574379183",
            "8164355342220382350125701",
            "6809639490528104145348007",
            "9773532446025636253384483",
            "2320081381563874858404931",
            "4450516420777041362059469",
            "3692987233784270214534593",
            "7361233086141458295849383",
            "1109879698121307329460557",
            "4958890074618199848632611",
            "6984500820992587225663019",
            "1968893735179147929781297",
            "1835655002390694309386063",
            "1465461994515064453658851",
            "3933330067703949644827531",
            "9804355564623734036605201",
            "1840014617327570694576551",
            "9565799336539544443633243",
            "5795482113114958725208087",
            "3542446728563869834455469",
        ];
        int counter = 0;
        foreach (string prime in primes)
        {
            counter++;
            BigInteger primeBig = BigInteger.Parse(prime);
            if (primeBig < Miller.MAX_PRIME)
            {
                Assert.IsTrue(Miller.IsPrime(prime), $"Test {counter}");
            }
            else
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                    Miller.IsPrime(prime), $"Test {counter}");
            }
        }
    }
}
