using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ResolveGraphQL.DataModel;

namespace ResolveDataModel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var db = new StarWarsContext())
            {
                if (db.Database.EnsureCreated())
                {
                    db.Humans.Add(new Human
                    {
                        HumanId = 1,
                        Name = "Luke",
                        HomePlanet = "Tatooine"
                    });

                    db.Humans.Add(new Human
                    {
                        HumanId = 2,
                        Name = "Vader",
                        HomePlanet = "Tatooine"
                    });

                    db.Droids.Add(new Droid
                    {
                        DroidId = 1,
                        Name = "R2-D2",
                        PrimaryFunction = "Astromech"
                    });

                    db.Droids.Add(new Droid
                    {
                        DroidId = 2,
                        Name = "C-3PO",
                        PrimaryFunction = "Protocol"
                    });

                    db.HumanFriends.Add(new HumanFreind
                    {
                        HumanId = 1,
                        DroidId = 1
                    });

                    db.HumanFriends.Add(new HumanFreind
                    {
                        HumanId = 1,
                        DroidId = 2
                    });

                    db.HumanFriends.Add(new HumanFreind
                    {
                        HumanId = 2,
                        DroidId = 1
                    });

                    var count = db.SaveChanges();
                    Console.WriteLine("{0} records saved to database", count);
                    Console.WriteLine();
                }
                
                Console.WriteLine("All humans in database:");

                var data = db.Humans.
                    Include(h => h.Friends).
                        ThenInclude(f => f.Droid).
                    ToList();

                foreach (var human in data)
                {
                    Console.WriteLine(" - Human: {0}", human.Name);
                    foreach (var droid in human.Friends.Select(f => f.Droid))
                        Console.WriteLine(" --- Droid {0}", droid.Name);
                }
            }
        }
    }
}
