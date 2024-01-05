using BenchmarkDotNet.Attributes;
using Bogus;
using System.Text;
using System.Text.RegularExpressions;

namespace LongestWordLength;

[MemoryDiagnoser]
public partial class LongestWordLengthBench
{
    #region Params

    private const int Seed = 189206201;

    [Params(1, 10, 100, 1_000, 10_000, 100_000)]
    //[Params(100_000)]
    public int WordsCount;

    #endregion

    #region SetupCleanup

    private string Words = string.Empty;
    private int LongestWordLength = 0;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Console.WriteLine("GlobalSetup start");

        var seed = unchecked(Seed + WordsCount + (int)(DateTime.Now.Ticks / TimeSpan.TicksPerDay));
        Randomizer.Seed = new Random(seed);
        Console.WriteLine($"Seed = {seed:N0}");

        var sentences = new StringBuilder();
        var sentenceSeparator = new string(' ', 100);
        var lorem = new Bogus.DataSets.Lorem();
        var wordsCount = 0;
        while (wordsCount < WordsCount)
        {
            var sentence = lorem.Sentence();
            var sentenceWordsCount = RegexNotAsciiLetters().Split(sentence).Where(word => !string.IsNullOrWhiteSpace(word)).Count();
            if (sentenceWordsCount > WordsCount - wordsCount)
            {
                sentence = lorem.Sentence(WordsCount - wordsCount);
                sentenceWordsCount = RegexNotAsciiLetters().Split(sentence).Where(word => !string.IsNullOrWhiteSpace(word)).Count();
            }

            sentences.Append(sentence);
            sentences.Append(sentenceSeparator);
            wordsCount += sentenceWordsCount;
        }
        Words = sentences.ToString();
        LongestWordLength = RegexNotAsciiLetters().Split(Words).Max(word => word.Length);

        //Console.WriteLine(Words);
        Console.WriteLine($"WordsCount = {wordsCount} should {WordsCount}");
        Console.WriteLine($"LongestWordLength = {LongestWordLength}");
        //Console.WriteLine();
        //Console.WriteLine();

        Console.WriteLine("GlobalSetup end");
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Words = string.Empty;
        LongestWordLength = 0;
    }

    #endregion

    //[Benchmark]
    public void Split_Linq()
    {
        var result = FindLongestWordLength.Split_Linq(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Split_Linq)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Split_Loop()
    {
        var result = FindLongestWordLength.Split_Loop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Split_Loop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void MemorySplit()
    {
        var result = FindLongestWordLength.MemorySplit(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Split_Linq)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Seq3Loops_Linq()
    {
        var result = FindLongestWordLength.Seq3Loops_Linq(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Seq3Loops_Linq)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Seq3Loops_Loop()
    {
        var result = FindLongestWordLength.Seq3Loops_Loop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Seq3Loops_Loop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Seq2Loops_Linq()
    {
        var result = FindLongestWordLength.Seq2Loops_Linq(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Seq2Loops_Linq)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Seq2Loops_Loop()
    {
        var result = FindLongestWordLength.Seq2Loops_Loop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Seq2Loops_Loop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoops()
    {
        var result = FindLongestWordLength.ThreeLoops(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoops)} return {result} should {LongestWordLength}");
    }

    [Benchmark]
    public void TwoLoops()
    {
        var result = FindLongestWordLength.TwoLoops(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops)} return {result} should {LongestWordLength}");
    }

    [Benchmark]
    public void TwoLoops1Jump()
    {
        var result = FindLongestWordLength.TwoLoops1Jump(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops1Jump)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void TwoLoops2Jumps()
    {
        var result = FindLongestWordLength.TwoLoops2Jumps(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops2Jumps)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void FourLoops2Jumps()
    {
        var result = FindLongestWordLength.FourLoops2Jumps(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(FourLoops2Jumps)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void TwoLoopsSVContains()
    {
        var result = FindLongestWordLength.TwoLoopsSVContains(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(TwoLoopsSVContains)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoopsIndexOf()
    {
        var result = FindLongestWordLength.ThreeLoopsIndexOf(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoopsIndexOf)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoopsIndexOfSV()
    {
        var result = FindLongestWordLength.ThreeLoopsIndexOfSV(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoopsIndexOfSV)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoopsIndexOfSV_Inlined()
    {
        var result = FindLongestWordLength.ThreeLoopsIndexOfSV_Inlined(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoopsIndexOfSV_Inlined)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Jump()
    {
        var result = FindLongestWordLength.Jump(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Jump)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void JumpNew()
    {
        var result = FindLongestWordLength.JumpNew(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(JumpNew)} return {result} should {LongestWordLength}");
    }

    [GeneratedRegex("[^a-zA-Z]+")]
    private static partial Regex RegexNotAsciiLetters();
}
