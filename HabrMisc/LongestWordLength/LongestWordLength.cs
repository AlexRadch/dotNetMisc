using System.Buffers;
using System.Runtime.CompilerServices;

namespace LongestWordLength;

internal static class FindLongestWordLength
{
    internal static char[] SplitFastSeparators { get; } = [' ', '.', ','];

    internal static string AsciiLetters { get; } = string.Create(('Z' - 'A' + 1) * 2, 0,
        (span, state) =>
        {
            for (var c = 'A'; c <= 'Z'; c++)
                span[state++] = c;
            for (var c = 'a'; c <= 'z'; c++)
                span[state++] = c;
        });

    internal static SearchValues<char> AsciiLettersSearchValues { get; } = SearchValues.Create(AsciiLetters);

    public static int Split_Linq(string str)
        => str.Split(SplitFastSeparators).Max(word => word.Length);

    public static int Split_Loop(string str)
    {
        var words = str.Split(SplitFastSeparators);
        var maxLength = 0;
        for (var i = 0; i < words.Length; i++)
            maxLength = Math.Max(maxLength, words[i].Length);
        return maxLength;
    }

    #region MemorySplit

    const int StackLimitBytes = 8 * 1024;
    const int StackLimitRanges = StackLimitBytes / (sizeof(int) * 2);
    const int HeapLimitBytes = 1024 * 1024;
    const int HeapLimitRanges = HeapLimitBytes / (sizeof(int) * 2);

    public static int MemorySplit(string str)
    {
        var maxLength = 0;

        ReadOnlySpan<char> span = str;
        var rangesMaxLength = Math.Min(HeapLimitRanges, (span.Length + 1) / 2 + 1);
        Span<Range> ranges = rangesMaxLength <= StackLimitRanges
            ? stackalloc Range[rangesMaxLength]
            :  new Range[rangesMaxLength];
        while (true)
        {
            var count = MemoryExtensions.SplitAny(span, ranges, SplitFastSeparators, 
                StringSplitOptions.RemoveEmptyEntries);
                //StringSplitOptions.None);

            var splitCount = count;
            if (count >= ranges.Length)
                splitCount--;
            for (int i = 0; i < splitCount; i++)
            {
                var range = ranges[i];
                maxLength = Math.Max(maxLength, range.End.Value - range.Start.Value);
            }
            if (count < ranges.Length)
                break;
            span = span.Slice(ranges[splitCount].Start.Value);
        }

        return maxLength;
    }

    #endregion

    #region Seq3Loops

    public static int Seq3Loops_Linq(string str)
        => SeqLengths3Loops(str).Max();

    public static int Seq3Loops_Loop(string str)
    {
        var maxLength = 0;
        foreach (var len in SeqLengths3Loops(str))
            maxLength = Math.Max(maxLength, len);
        return maxLength;
    }

    private static IEnumerable<int> SeqLengths3Loops(string str)
    {
        var startIndex = 0;
        while (true)
        {
            NextIndexes2Loops(str, ref startIndex, out int endIndex);
            if (startIndex >= str.Length)
                yield break;
            yield return endIndex - startIndex;
            startIndex = endIndex + 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NextIndexes2Loops(string str, ref int startIndex, out int endIndex)
    {
        //Console.WriteLine($"NextIndexes2Loops({str.Length}, {startIndex})");
        while (startIndex < str.Length)
        {
            if (char.IsAsciiLetter(str[startIndex]))
            {
                //endIndex = startIndex + 1;
                //while (endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) endIndex++;
                endIndex = startIndex;
                while (++endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) ;
                return;
            }
            startIndex++;
        }
        endIndex = 0;
        return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int NextMaxLength(string str, ref int startIndex, int maxLength)
    {
        var nextIndex = startIndex + maxLength + 1;
        while (nextIndex < str.Length)
        {
            if (!char.IsAsciiLetter(str[nextIndex]))
            {
                startIndex = nextIndex + 1;
                goto Continue;
            }

            for (var newStartIndex = nextIndex - 1; newStartIndex >= startIndex; newStartIndex--)
            {
                if (!char.IsAsciiLetter(str[newStartIndex]))
                {
                    startIndex = newStartIndex + 1;
                    goto Continue;
                }
            }

            do
            {
                nextIndex++;
            }
            while (char.IsAsciiLetter(str[nextIndex]));

            startIndex = nextIndex;
            return maxLength;

        Continue:
            nextIndex = startIndex + maxLength + 1;
        }
        startIndex = nextIndex;
        return maxLength;
    }

    #endregion

    #region Seq2Loops

    public static int Seq2Loops_Linq(string str)
        => SeqLengths2Loops(str).Max();

    public static int Seq2Loops_Loop(string str)
    {
        var maxLength = 0;
        foreach (var len in SeqLengths2Loops(str))
            maxLength = Math.Max(maxLength, len);
        return maxLength;
    }

    private static IEnumerable<int> SeqLengths2Loops(string str)
    {
        var startIndex = 0;
        while (startIndex < str.Length)
        {
            if (char.IsAsciiLetter(str[startIndex]))
            {
                //var endIndex = startIndex + 1;
                //while (endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) endIndex++;
                var endIndex = startIndex;
                while (++endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) ;
                yield return endIndex - startIndex;
                startIndex = endIndex;
            }
            startIndex++;
        }
    }

    #endregion

    public static int ThreeLoops(string str)
    {
        var maxLength = 0;

        var startIndex = 0;
        while (true)
        {
            NextIndexes2Loops(str, ref startIndex, out int endIndex);
            if (startIndex >= str.Length)
                break;
            maxLength = Math.Max(maxLength, endIndex - startIndex);
            startIndex = endIndex + 1;
        }

        return maxLength;
    }

    public static int TwoLoops(string str)
    {
        var maxLength = 0;

        var startIndex = 0;
        while (startIndex < str.Length)
        {
            if (char.IsAsciiLetter(str[startIndex]))
            {
                //var endIndex = startIndex + 1;
                //while (endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) endIndex++;
                var endIndex = startIndex;
                while (++endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) ;
                maxLength = Math.Max(maxLength, endIndex - startIndex);
                startIndex = endIndex;
            }
            startIndex++;
        }

        return maxLength;
    }

    public static int TwoLoopsSVContains(string str)
    {
        var maxLength = 0;

        var startIndex = 0;
        while (startIndex < str.Length)
        {
            if (AsciiLettersSearchValues.Contains(str[startIndex]))
            {
                //var endIndex = startIndex + 1;
                //while (endIndex < str.Length && AsciiLettersSearchValues.Contains(str[endIndex])) endIndex++;
                var endIndex = startIndex;
                while (++endIndex < str.Length && AsciiLettersSearchValues.Contains(str[endIndex])) ;
                maxLength = Math.Max(maxLength, endIndex - startIndex);
                startIndex = endIndex;
            }
            startIndex++;
        }

        return maxLength;
    }

    public static int TwoLoops1Jump(string str)
    {
        var maxLength = 0;

        for (int startIndex = 0, endIndex = 0; endIndex < str.Length;
            startIndex = ++endIndex, 
            endIndex += maxLength)
        {
            if (!char.IsAsciiLetter(str[endIndex]))
                continue;

            // Can IndexOf be faster here?
            for (var breakIndex = endIndex - 1; breakIndex > startIndex; breakIndex--)
            {
                if (!char.IsAsciiLetter(str[breakIndex]))
                {
                    endIndex = breakIndex;
                    goto Continue;
                }
            }
            if (!char.IsAsciiLetter(str[startIndex]))
                startIndex++;

            // Can IndexOf be faster here?
            while (++endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) ;

            maxLength = endIndex - startIndex;

        Continue:
            ;
        }

        return maxLength;
    }

    public static int TwoLoops2Jumps(string str)
    {
        var maxLength = 0;

        for (int startIndex = 0, endIndex = 0, checkedLen = 0; endIndex < str.Length;
            startIndex = ++endIndex,
            endIndex += maxLength)
        {
            if (!char.IsAsciiLetter(str[endIndex]))
            {
                checkedLen = 0;
                continue;
            }

            // Can IndexOf be faster here?
            checkedLen += startIndex;
            for (var breakIndex = endIndex - 1; breakIndex >= checkedLen; breakIndex--)
            {
                if (!char.IsAsciiLetter(str[breakIndex]))
                {
                    checkedLen = endIndex - breakIndex;
                    endIndex = breakIndex;
                    goto Continue;
                }
            }
            //Console.WriteLine($"checkedLen = {checkedLen - startIndex}");
            checkedLen = 0;

            // Can IndexOf be faster here?
            while (++endIndex < str.Length && char.IsAsciiLetter(str[endIndex])) ;

            maxLength = endIndex - startIndex;

        Continue:
            ;
        }

        return maxLength;
    }

    #region ThreeLoopsIndexOf

    public static int ThreeLoopsIndexOf(string str)
    {
        var maxLength = 0;

        ReadOnlySpan<char> span = str;
        do
        {
            NextIndexes2IndexOf(ref span, out int endIndex);
            maxLength = Math.Max(maxLength, endIndex);
            span = span.Slice(endIndex);
        } while (span.Length > 0);

        return maxLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NextIndexes2IndexOf(ref ReadOnlySpan<char> span, out int endIndex)
    {
        var startIndex = span.IndexOfAny(AsciiLetters);
        if (startIndex < 0)
        {
            span = ReadOnlySpan<char>.Empty;
            endIndex = 0;
        }
        else
        {
            span = span.Slice(startIndex);
            endIndex = span.IndexOfAnyExcept(AsciiLetters);
            if (endIndex < 0)
                endIndex = span.Length;
        }
    }

    #endregion

    #region ThreeLoopsIndexOfSV

    public static int ThreeLoopsIndexOfSV(string str)
    {
        var maxLength = 0;

        ReadOnlySpan<char> span = str;
        do
        {
            NextIndexes2IndexOfSV(ref span, out int endIndex);
            maxLength = Math.Max(maxLength, endIndex);
            span = span.Slice(endIndex);
        } while (span.Length > 0);

        return maxLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NextIndexes2IndexOfSV(ref ReadOnlySpan<char> span, out int endIndex)
    {
        var startIndex = span.IndexOfAny(AsciiLettersSearchValues);
        if (startIndex < 0)
        {
            span = ReadOnlySpan<char>.Empty;
            endIndex = 0;
        }
        else
        {
            span = span.Slice(startIndex);
            endIndex = span.IndexOfAnyExcept(AsciiLettersSearchValues);
            if (endIndex < 0)
                endIndex = span.Length;
        }
    }

    #endregion

    #region ThreeLoopsIndexOfSV_Inlined

    public static int ThreeLoopsIndexOfSV_Inlined(string str)
    {
        var maxLength = 0;

        ReadOnlySpan<char> span = str;
        while (true)
        {
            var startIndex = span.IndexOfAny(AsciiLettersSearchValues);
            if (startIndex < 0)
                break;

            span = span.Slice(startIndex);
            var endIndex = span.IndexOfAnyExcept(AsciiLettersSearchValues);
            if (endIndex < 0)
            {
                maxLength = Math.Max(maxLength, span.Length);
                break;
            }

            maxLength = Math.Max(maxLength, endIndex);
            span = span.Slice(endIndex);
        }

        return maxLength;
    }

    #endregion
}

