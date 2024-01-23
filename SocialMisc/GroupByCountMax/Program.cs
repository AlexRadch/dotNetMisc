
List<Person> people =
[
    new("Steve", "Jobs"),
            new("Steve", "Carrel"),
            new("Elon", "Mask"),
        ];

var sameFirstNamesQ =
    from p in people.AsQueryable()
    group p by p.FirstName into sameFirstName
    select new
    {
        FirstName = sameFirstName.Key,
        Count = sameFirstName.Count(),
        MaxLastName = sameFirstName.Max(p => p.LastName),
        LastNames = string.Join(", ", sameFirstName.Select(p => p.LastName)),
    };

var sameFirstNamesE = people.
    GroupBy(p => p.FirstName).
    Select(sameFirstName => (
        FirstName: sameFirstName.Key, 
        Count : sameFirstName.Count(), 
        MaxLastName : sameFirstName.Max(p => p.LastName),
        LastNames: string.Join(", ", sameFirstName.Select(p => p.LastName))
    ));

Console.WriteLine(sameFirstNamesQ.ToString());
Console.WriteLine();

foreach (var sameFirstName in sameFirstNamesE)
    Console.WriteLine($"{sameFirstName.FirstName} " +
        $"Count {sameFirstName.Count} " +
        $"MaxLastName {sameFirstName.MaxLastName} " +
        $"LastNames ({sameFirstName.LastNames})");

#pragma warning disable CA1050 // Declare types in namespaces

public record Person(string FirstName, string LastName);

#pragma warning restore CA1050 // Declare types in namespaces