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
    private const int SentencesSeparatorLength = 1;
    //private const int SentencesSeparatorLen = 100;

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
        var sentenceSeparator = new string(' ', SentencesSeparatorLength);
        var lorem = new Bogus.DataSets.Lorem();
        var wordsCount = 0;
        while (wordsCount < WordsCount)
        {
            var sentence = lorem.Sentence();
            var sentenceWordsCount = RegexNotAsciiWords().Split(sentence).Where(word => !string.IsNullOrWhiteSpace(word)).Count();
            if (sentenceWordsCount > WordsCount - wordsCount)
            {
                sentence = lorem.Sentence(WordsCount - wordsCount);
                sentenceWordsCount = RegexNotAsciiWords().Split(sentence).Where(word => !string.IsNullOrWhiteSpace(word)).Count();
            }

            sentences.Append(sentence);
            sentences.Append(sentenceSeparator);
            wordsCount += sentenceWordsCount;
        }
        Words = sentences.ToString();
        LongestWordLength = RegexNotAsciiWords().Split(Words).Max(word => word.Length);

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
    public void Split_EachLoop()
    {
        var result = FindLongestWordLength.Split_EachLoop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Split_EachLoop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void Split_ForLoop()
    {
        var result = FindLongestWordLength.Split_ForLoop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(Split_ForLoop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void MemorySplit()
    {
        var result = FindLongestWordLength.MemorySplit(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(MemorySplit)} return {result} should {LongestWordLength}");
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

    [Benchmark]
    public void SeqWords_Memory3Loops_Linq()
    {
        var result = FindLongestWordLength.SeqWords_Memory3Loops_Linq(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(SeqWords_Memory3Loops_Linq)} return {result} should {LongestWordLength}");
    }

    [Benchmark]
    public void SeqWords_Memory3LoopsSV_Linq()
    {
        var result = FindLongestWordLength.SeqWords_Memory3LoopsSV_Linq(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(SeqWords_Memory3LoopsSV_Linq)} return {result} should {LongestWordLength}");
    }

    [Benchmark]
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

    [Benchmark]
    public void SeqRegexMatch_Linq()
    {
        var result = FindLongestWordLength.SeqRegexMatch_Linq(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(SeqRegexMatch_Linq)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void SeqRegexMatch_EachLoop()
    {
        var result = FindLongestWordLength.SeqRegexMatch_EachLoop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(SeqRegexMatch_EachLoop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoops()
    {
        var result = FindLongestWordLength.ThreeLoops(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoops)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void TwoLoops()
    {
        var result = FindLongestWordLength.TwoLoops(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void RegexMatch_Loop()
    {
        var result = FindLongestWordLength.RegexMatch_Loop(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(RegexMatch_Loop)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoops1Jump()
    {
        var result = FindLongestWordLength.ThreeLoops1Jump(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoops1Jump)} return {result} should {LongestWordLength}");
    }

    //[Benchmark]
    public void ThreeLoops2Jumps()
    {
        var result = FindLongestWordLength.ThreeLoops2Jumps(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoops2Jumps)} return {result} should {LongestWordLength}");
    }

    //[Benchmark()]
    public void FourLoops2Jumps()
    {
        var result = FindLongestWordLength.FourLoops2Jumps(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(FourLoops2Jumps)} return {result} should {LongestWordLength}");
    }

    [Benchmark(Baseline = true)]
    public void FiveLoops2Jumps()
    {
        var result = FindLongestWordLength.FiveLoops2Jumps(Words);
        if (result != LongestWordLength)
            throw new InvalidOperationException(
                $"{nameof(FiveLoops2Jumps)} return {result} should {LongestWordLength}");
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

    [GeneratedRegex("[^A-Za-z]+")]
    private static partial Regex RegexNotAsciiWords();
}
