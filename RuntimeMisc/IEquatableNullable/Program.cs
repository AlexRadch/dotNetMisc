using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

int?[] nInts = [9, 1, null, 3, null, 5, null, 7, null];

var i5 = Array.IndexOf(nInts, 5);
Console.WriteLine($"i5 {i5}");
var iN = Array.IndexOf(nInts, null);
Console.WriteLine($"iN {iN}");
var iD = Array.IndexOf(nInts, default);
Console.WriteLine($"iD {iD}");
var i0 = Array.IndexOf(nInts, 0);
Console.WriteLine($"i0 {i0}");
Console.WriteLine();

Equitable<int?>[] nStructs = [new(9), new(1), new(null), new(3), new(null), new(5), new(null), new(7), new(null)];

i5 = Array.IndexOf(nStructs, new(5));
Console.WriteLine($"i5 {i5}");
iN = Array.IndexOf(nStructs, new(null));
Console.WriteLine($"iN {iN}");
iD = Array.IndexOf(nStructs, new(default));
Console.WriteLine($"iD {iD}");
i0 = Array.IndexOf(nStructs, new(0));
Console.WriteLine($"i0 {i0}");
Console.WriteLine();

var nIntsSpan = nInts.AsSpan();

i5 = nIntsSpan.IndexOf(5);
//Console.WriteLine($"i5 {i5}");
//iN = nIntsSpan.IndexOf(null);
//Console.WriteLine($"iN {iN}");
//iD = nIntsSpan.IndexOf(default);
//Console.WriteLine($"iD {iD}");
//i0 = nIntsSpan.IndexOf(0);
//Console.WriteLine($"i0 {i0}");
//Console.WriteLine();

//var nIntsSpan2 = MemoryMarshal.Cast<int?, ClassT<int?>>(nIntsSpan);
var nIntsSpan2 = MemoryMarshal.CreateSpan(ref Unsafe.As<int?, Equitable<int?>>(ref MemoryMarshal.GetReference(nIntsSpan)), nIntsSpan.Length);

i5 = nIntsSpan2.IndexOf(new Equitable<int?>(5));
Console.WriteLine($"i5 {i5}");
iN = nIntsSpan2.IndexOf(new Equitable<int?>(null));
Console.WriteLine($"iN {iN}");
iD = nIntsSpan2.IndexOf(default(Equitable<int?>));
Console.WriteLine($"iD {iD}");
i0 = nIntsSpan2.IndexOf(new Equitable<int?>(0));
Console.WriteLine($"i0 {i0}");
Console.WriteLine();

//i5 = nStructs.AsSpan().IndexOf(new StructT<int?>(5));
//Console.WriteLine($"i5 {i5}");

//var sorted = nStructs.Order();

//var min = sorted.Min();
//var max = sorted.Max();

//Console.WriteLine($"min {min}, max {max}, first {sorted.First()}, second {sorted.Skip(1).First()}, last {sorted.Last()}");



public class ClassT<T>
{
    public T Value;

    public ClassT() => Value = default!;
    public ClassT(T value) => Value = value;
}

public struct Equitable<T> : IEquatable<Equitable<T>>
{
    public T Value;
    public Equitable(T value) => Value = value;
    public bool Equals(Equitable<T> value) => EqualityComparer<T>.Default.Equals(this.Value, value.Value);

}

