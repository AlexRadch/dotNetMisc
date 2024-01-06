using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LongestWordLength;

internal static partial class FindLongestWordLength
{
    #region Static constants

    private static char[] SplitFastSeparators { get; } = [' ', '.', ','];

    private static string AsciiLetters { get; } = string.Create(('Z' - 'A' + 1) * 2, 0,
        (span, state) =>
        {
            for (var c = 'A'; c <= 'Z'; c++)
                span[state++] = c;
            for (var c = 'a'; c <= 'z'; c++)
                span[state++] = c;
        });

    private static SearchValues<char> AsciiLettersSearchValues { get; } = SearchValues.Create(AsciiLetters);

    #endregion

    #region Split

    public static int Split_Linq(string str)
        => str.Split(SplitFastSeparators).Max(word => word.Length);

    public static int Split_EachLoop(string str)
    {
        var maxLength = 0;
        foreach (var word in str.Split(SplitFastSeparators))
            maxLength = Math.Max(maxLength, word.Length);
        return maxLength;
    }

    public static int Split_ForLoop(string str)
    {
        var maxLength = 0;
        var words = str.Split(SplitFastSeparators);
        for (var i = 0; i < words.Length; i++)
            maxLength = Math.Max(maxLength, words[i].Length);
        return maxLength;
    }

    #endregion

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

    #region SeqRegexMatch

    public static int SeqRegexMatch_Linq(string str)
        => SeqMatch(RegexAsciiWords().Match(str)).Max(match => match.Length);

    public static int SeqRegexMatch_EachLoop(string str)
    {
        var maxLength = 0;
        foreach (var match in SeqMatch(RegexAsciiWords().Match(str)))
            maxLength = Math.Max(maxLength, match.Length);
        return maxLength;
    }

    private static IEnumerable<Match> SeqMatch(Match match)
    {
        for (; match.Success; match = match.NextMatch())
            yield return match;
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

        for (var i = 0; i < str.Length; i++)
        {
            if (char.IsAsciiLetter(str[i]))
            {
                var startIndex = i;
                while (++i < str.Length && char.IsAsciiLetter(str[i])) ;
                maxLength = Math.Max(maxLength, i - startIndex);
            }
        }

        return maxLength;
    }

    public static int RegexMatch_Loop(string str)
    {
        var maxLength = 0;
        for (var match = RegexAsciiWords().Match(str); match.Success; match = match.NextMatch())
            maxLength = Math.Max(maxLength, match.Length);
        return maxLength;
    }

    public static int ThreeLoops1Jump(string str)
    {
        var maxLength = 0;

        // Can IndexOf be faster here?
        for (int i = 0, len = str.Length; i < len; i++)
        {
            if (!char.IsAsciiLetter(str[i]))
                continue;

            int startIndex = i;

        Found:
            // Can IndexOf be faster here?
            while (++i < len && char.IsAsciiLetter(str[i])) ;

            maxLength = i - startIndex;

            for (i += maxLength + 1; i < len; i += maxLength + 1)
            {
                if (!char.IsAsciiLetter(str[i]))
                    continue;

                // Can IndexOf be faster here?
                startIndex = i - maxLength;
                while (--i > startIndex)
                {
                    if (!char.IsAsciiLetter(str[i]))
                        goto Continue;
                }
                if (!char.IsAsciiLetter(str[startIndex]))
                    startIndex++;
                
                i += maxLength;
                goto Found;

            Continue:
                ;
            }

            break;
        }

        return maxLength;
    }

    public static int ThreeLoops2Jumps(string str)
    {
        var maxLength = 0;

        // Can IndexOf be faster here?
        for (int i = 0, len = str.Length; i < len; i++)
        {
            if (!char.IsAsciiLetter(str[i]))
                continue;

            int startIndex = i;

        Found:
            // Can IndexOf be faster here?
            while (++i < len && char.IsAsciiLetter(str[i])) ;

            maxLength = i - startIndex;

            int checkedIndex = ++i;
            for (i += maxLength; i < len; i += maxLength)
            {
                if (!char.IsAsciiLetter(str[i]))
                {
                    checkedIndex = ++i;
                    continue;
                }

                // Can IndexOf be faster here?
                startIndex = i - maxLength;
                var checkedIndex2 = i;
                while (--i > checkedIndex)
                {
                    if (!char.IsAsciiLetter(str[i]))
                    {
                        checkedIndex = checkedIndex2;
                        i++;
                        goto Continue;
                    }
                }
                i = startIndex + maxLength;
                if (!char.IsAsciiLetter(str[startIndex]))
                    startIndex++;

                goto Found;

            Continue:
                ;
            }

            break;
        }

        return maxLength;
    }

    public static int FourLoops2Jumps(string str)
    {
        var maxLength = 0;

        for (int i = 0, len = str.Length; i < len; i += maxLength + 1)
        {
            if (!char.IsAsciiLetter(str[i]))
                continue;

            // Can IndexOf be faster here?
            var startIndex = i - maxLength;
            var checkedIndex = i;
            while (--i > startIndex)
            {
                if (char.IsAsciiLetter(str[i]))
                    continue;

                for (i += maxLength + 1; i < len; i += maxLength + 1)
                {
                    if (!char.IsAsciiLetter(str[i]))
                        goto Continue;

                    startIndex = i - maxLength;
                    var checkedIndex2 = i;
                    while (--i > checkedIndex)
                    {
                        if (!char.IsAsciiLetter(str[i]))
                        {
                            checkedIndex = checkedIndex2;
                            goto ContinueChecked;
                        }
                    }

                    i = startIndex;
                    goto Found;

                ContinueChecked:
                    ;
                }

                return maxLength;
            }
            if (!char.IsAsciiLetter(str[startIndex]))
            {
                startIndex++;
            }


        Found:
            i += maxLength;
            // Can IndexOf be faster here?
            while (++i < len && char.IsAsciiLetter(str[i])) ;

            maxLength = i - startIndex;

        Continue:
            ;
        }

        return maxLength;
    }

    public static int FiveLoops2Jumps(string str)
    {
        var maxLength = 0;

        // Can IndexOf be faster here?
        for (int i = 0, len = str.Length; i < len; i++)
        {
            if (!char.IsAsciiLetter(str[i]))
                continue;

            int startIndex = i;

        Found:
            // Can IndexOf be faster here?
            while (++i < len && char.IsAsciiLetter(str[i])) ;

            maxLength = i - startIndex;

            for (i += maxLength + 1; i < len; i += maxLength + 1)
            {
                if (!char.IsAsciiLetter(str[i]))
                    continue;

                // Can IndexOf be faster here?
                startIndex = i - maxLength;
                var checkedIndex = i;
                while (--i > startIndex)
                {
                    if (char.IsAsciiLetter(str[i]))
                        continue;

                    for (i += maxLength + 1; i < len; i += maxLength + 1)
                    {
                        if (!char.IsAsciiLetter(str[i]))
                            goto Continue;

                        // Can IndexOf be faster here?
                        startIndex = i - maxLength;
                        var checkedIndex2 = i;
                        while (--i > checkedIndex)
                        {
                            if (!char.IsAsciiLetter(str[i]))
                            {
                                checkedIndex = checkedIndex2;
                                goto ContinueChecked;
                            }
                        }
                        i = startIndex + maxLength;
                        goto Found;

                    ContinueChecked:
                        ;
                    }

                    return maxLength;
                }
                i = startIndex + maxLength;
                if (!char.IsAsciiLetter(str[startIndex]))
                    startIndex++;
                goto Found;

            Continue:
                ;
            }

            return maxLength;
        }

        return maxLength;
    }

    public static int TwoLoopsSVContains(string str)
    {
        var maxLength = 0;

        for (var i = 0; i < str.Length; i++)
        {
            if (AsciiLettersSearchValues.Contains(str[i]))
            {
                var startIndex = i;
                while (++i < str.Length && AsciiLettersSearchValues.Contains(str[i])) ;
                maxLength = Math.Max(maxLength, i - startIndex);
            }
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
            span = [];
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
            span = [];
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

    #region Jump

    public static int Jump(string str)
    {
        // Jump
        int maxLen = 0, //maxIndex = -1, 
            len = str.Length;
        for (int i = 0; i < len - maxLen; i++)
        {
            int index = i;
            i += maxLen;
            if (char.IsAsciiLetter(str[i]) && i < len)
            {
                int k = maxLen > 0 ? 1 : 0;
                do i -= k;
                while (char.IsAsciiLetter(str[i]) && i > index);
                if (i == index)
                {
                    i += maxLen;
                    do i++;
                    while (char.IsAsciiLetter(str[i]) && i < len);
                    if (maxLen < (i - index))
                    {
                        //maxIndex = index;
                        maxLen = i - index;
                    }
                }
            }
        }
        return maxLen;
    }

    public static int JumpNew(string str)
    {
        int maxLen = 0, //maxIndex = -1,
            tail = 0, len = str.Length;
        for (int i = 0; i < len - maxLen; i++)
        {
            int index = i;
            i += maxLen;
            if (char.IsAsciiLetter(str[i]) && i < len)
            {
                int k = maxLen > 0 ? 1 : 0;
                do i -= k;
                while (char.IsAsciiLetter(str[i]) && i > (index + tail));
                if (i == (index + tail))
                {
                    i = index + maxLen;
                    do i++;
                    while (char.IsAsciiLetter(str[i]) && i < len);
                    if (maxLen < (i - index))
                    {
                        //maxIndex = index;
                        maxLen = i - index;
                    }
                }
                else
                {
                    tail = index + maxLen - i - 1;
                    continue;
                }
            }
            tail = 0;
        }
        return maxLen;
    }

    #endregion

    #region Generated

    [GeneratedRegex("[A-Za-z]+")]
    private static partial Regex RegexAsciiWords();

    #endregion
}
