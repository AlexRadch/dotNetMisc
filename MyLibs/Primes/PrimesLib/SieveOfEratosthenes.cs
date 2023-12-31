using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace PrimesLib
{
    public class SieveOfEratosthenes
    {
        public static byte[] EvaluateSegment(IEnumerable<BigInteger> storedPrimes, BigInteger segmentStart, int segmentSize = 0)
        {
            if (segmentStart < 3 || segmentStart.IsEven)
                throw new ArgumentException(nameof(segmentStart));

            if (segmentSize <= 0)
                segmentSize = 1024 * 1024 * 2; // 2 MiB

            var segment = new BitArray(segmentSize * 8, true);

            void Exclude(ref BigInteger prime)
            {
                var times = segmentStart / prime;
                if (times < prime)
                    times = prime;

                var start = times * prime;
                if (start < prime)
                    return;

                while (start < stop)
                {
                    if (start >= segmentStart)
                    {
                        segment[(int)(start - segmentStart) / 2] = false;
                    }

                    start += prime;
                }
            }

            using (var primes = storedPrimes.GetEnumerator())
            {
                while (primes.MoveNext())
                {
                    var prime = primes.Current;

                    Exclude(prime);
                }
            };

            for (var i = 0; i <= segmentSize * 8; i++)
                if (segment[i])
                {
                    var prime = segmentStart + (ulong)i * 2;
                    if (prime < segmentStart)
                        goto SaveAll;

                    Exclude(prime);
                }

            SaveAll:
            var result = new byte[segmentSize];
            segment.CopyTo(result, 0);

            return result;
        }

    }
}
