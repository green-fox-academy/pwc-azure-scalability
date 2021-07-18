using System;
using System.Collections.Generic;

namespace InvoiceProcessor.Api.Response
{
    public class Employee
    {
        private static Random random = new Random();
        private static List<string> names = new List<string>() { "Marcie Kindel", "Merrie Gierlach","Dona Burdine", "Lavera Prosser", "Muoi Cardenas", "Lilliam Persinger", "Agustina Perreira", "Haydee Zaccaria", "Caron Moitoso", "Margarett Heeter", "Harold Eutsey",
            "Scotty Einhorn", "Roman Vivanco", "Elizabet Stermer", "Annis Warrington", "Elenor Koepke", "Arnita Oldham", "Stanford Passmore", "Porfirio Fedler", "Jeanine Cummins" };

        public string Name { get; set; }
        public int Salary { get; set; }

        public static Employee CreateRandom() =>
            new Employee
            {
                Name = names[random.Next(0, names.Count - 1)],
                Salary = random.Next(1_000_000, 2_000_000),
            };
    }
}
