namespace LoyaltySystem.Tests;

public class Test 
{
    [Fact]
    public void Test1()
    {
        var car1 = new Car("Ford", "Fiesta");
        var car2 = new Car("Ford", "Mustang");
        var car3 = new Car("Ford", "Ka");
        
        var cars = new List<Car>{car1, car2, car3};

        foreach (var car in cars)
        {
            Console.WriteLine($"I bought a new Car. It's a {car.Make}, {car.Model}");
        }

        Console.WriteLine();
    }

    public class Car {
        public Car(string model, string make) =>
            (Make, Model) = (make, model);
        
        public string Make { get; set; }
        public string Model { get; set; }
    }
    
    
}
