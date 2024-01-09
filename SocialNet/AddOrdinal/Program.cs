//
// https://twitter.com/KarenPayneMVP/status/1744415104354177105
//
using System;
using System.Linq;
using System.Numerics;

foreach (var i in Enumerable.Range(-1, 25))
    Console.WriteLine(Helpers.ToOrdinalString(i));

foreach (var i in Enumerable.Range(12_000, 25).Select(x => new BigInteger(x)))
    Console.WriteLine(Helpers.ToOrdinalString(i, "N0"));

public static class Helpers
{
    public static string ToOrdinalString<TInt>(TInt value,
        string? format = default, IFormatProvider? formatProvider = null)
            where TInt : IBinaryInteger<TInt>
    => value.ToString(format, formatProvider) + (
        value <= TInt.Zero
            ? ""
            : int.CreateTruncating(value % TInt.CreateChecked(10)) is var number &&
                ((number is > 3 or < 1) || int.CreateTruncating(value % TInt.CreateChecked(100)) > 10)
                ? "th"
                : number switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th", // For compiler
                });
}