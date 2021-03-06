using System;
using System.Collections.Generic;

namespace InvoiceProcessor.Api.Response
{
    public class Employee
    {
        private static readonly Random random = new Random();
        private static readonly List<string> names = new List<string>()
        {
            "Marcie Kindel", "Merrie Gierlach", "Dona Burdine", "Lavera Prosser", "Muoi Cardenas", "Lilliam Persinger", "Agustina Perreira", "Haydee Zaccaria", "Caron Moitoso", "Margarett Heeter", "Harold Eutsey",
            "Scotty Einhorn", "Roman Vivanco", "Elizabet Stermer", "Annis Warrington", "Elenor Koepke", "Arnita Oldham", "Stanford Passmore", "Porfirio Fedler", "Jeanine Cummins"
        };

        public string Name { get; set; }
        public int Salary { get; set; }

        public static Employee CreateRandom() =>
            new Employee
            {
#pragma warning disable CA5394 // Do not use insecure randomness
                Name = names[random.Next(0, names.Count - 1)],
                Salary = random.Next(1_000_000, 2_000_000),
#pragma warning restore CA5394 // Do not use insecure randomness
            };
    }
}
