﻿using BenchmarkDotNet.Attributes;
using Bogus;
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Text;
using System.Text.RegularExpressions;

namespace LongestWordLength;

[MemoryDiagnoser]
public class LongestWordLengthBench
{
    #region Params

    public int Seed = 189206201;

    [Params(1, 10, 100, 1_000, 10_000, 100_000)]
    //[Params(100_000)]
    public int WordsCount;

    #endregion

    #region SetupCleanup

    private string Words = string.Empty;
    private int LongestWord = 0;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Console.WriteLine("GlobalSetup start");

        var seed = unchecked(Seed + WordsCount);
        Randomizer.Seed = new Random(seed);
        Console.WriteLine($"Seed = {seed:N0}");

        var sentences = new StringBuilder();
        var lorem = new Bogus.DataSets.Lorem();
        var count = 0;
        while (count < WordsCount)
        {
            var sentence = lorem.Sentence();
            var sentenceCount = Regex.Split(sentence, "[^a-zA-Z]+").Length;
            if (sentenceCount > WordsCount - count)
            {
                sentence = lorem.Sentence(WordsCount - count);
                sentenceCount = Regex.Split(sentence, "[^a-zA-Z]+").Length;
            }

            sentences.Append(sentence);
            sentences.Append("   ");
            count += sentenceCount;
        }
        Words = sentences.ToString();
        LongestWord = Regex.Split(Words, "[^a-zA-Z]+").Max(word => word.Length);

        //Console.WriteLine(Words);
        //Console.WriteLine(LongestWord);
        //Console.WriteLine();
        //Console.WriteLine();

        Console.WriteLine("GlobalSetup end");
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Words = string.Empty;
        LongestWord = 0;
    }

    [IterationSetup]
    public void IterationSetup()
    {
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
    }

    #endregion

    //[Benchmark]
    public void Split_Linq()
    {
        var result = FindLongestWordLength.Split_Linq(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Split_Linq)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void Split_Loop()
    {
        var result = FindLongestWordLength.Split_Loop(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Split_Loop)} return {result} should {LongestWord}");
    }

    [Benchmark]
    public void MemorySplit()
    {
        var result = FindLongestWordLength.MemorySplit(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Split_Linq)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void Seq3Loops_Linq()
    {
        var result = FindLongestWordLength.Seq3Loops_Linq(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Seq3Loops_Linq)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void Seq3Loops_Loop()
    {
        var result = FindLongestWordLength.Seq3Loops_Loop(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Seq3Loops_Loop)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void Seq2Loops_Linq()
    {
        var result = FindLongestWordLength.Seq2Loops_Linq(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Seq2Loops_Linq)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void Seq2Loops_Loop()
    {
        var result = FindLongestWordLength.Seq2Loops_Loop(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(Seq2Loops_Loop)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void ThreeLoops()
    {
        var result = FindLongestWordLength.ThreeLoops(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoops)} return {result} should {LongestWord}");
    }

    [Benchmark]
    public void TwoLoops()
    {
        var result = FindLongestWordLength.TwoLoops(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops)} return {result} should {LongestWord}");
    }

    [Benchmark]
    public void TwoLoops1Jump()
    {
        var result = FindLongestWordLength.TwoLoops1Jump(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops1Jump)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void TwoLoops2Jumps()
    {
        var result = FindLongestWordLength.TwoLoops2Jumps(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(TwoLoops2Jumps)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void TwoLoopsSVContains()
    {
        var result = FindLongestWordLength.TwoLoopsSVContains(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(TwoLoopsSVContains)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void ThreeLoopsIndexOf()
    {
        var result = FindLongestWordLength.ThreeLoopsIndexOf(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoopsIndexOf)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void ThreeLoopsIndexOfSV()
    {
        var result = FindLongestWordLength.ThreeLoopsIndexOfSV(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoopsIndexOfSV)} return {result} should {LongestWord}");
    }

    //[Benchmark]
    public void ThreeLoopsIndexOfSV_Inlined()
    {
        var result = FindLongestWordLength.ThreeLoopsIndexOfSV_Inlined(Words);
        if (result != LongestWord)
            throw new InvalidOperationException(
                $"{nameof(ThreeLoopsIndexOfSV_Inlined)} return {result} should {LongestWord}");
    }
}
