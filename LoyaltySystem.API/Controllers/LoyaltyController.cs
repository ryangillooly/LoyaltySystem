using Microsoft.AspNetCore.Mvc;
using System;

namespace LoyaltySystem.Controllers;


[ApiController]
[Route("[controller]")]
public class LoyaltyController : ControllerBase
{
    CoffeeShopLoyaltySystem system = new CoffeeShopLoyaltySystem();

    [HttpGet("AddCustomer")]
    public void AddCustomer(string customerEmail) => system.AddCustomer(new Customer(customerEmail));
    
    [HttpGet("ScanQR")]
    public void ScanQR(string customerEmail) => system.ScanQR(customerEmail);
}

public class Customer
{
    public string Email { get; set; }
    public int Points { get; set; }

    public Customer(string email)
    {
        Email = email;
        Points = 0;
    }
}

public class CoffeeShopLoyaltySystem
{
    private Dictionary<string, Customer> customers;

    public CoffeeShopLoyaltySystem()
    {
        customers = new Dictionary<string, Customer>();
    }

    public void AddCustomer(Customer customer)
    {
        if (!customers.ContainsKey(customer.Email))
        {
            customers[customer.Email] = customer;
        }
        else
        {
            Console.WriteLine("Customer already exists.");
        }
    }

    public void ScanQR(string email)
    {
        if (customers.ContainsKey(email))
        {
            customers[email].Points += 1;
            Console.WriteLine($"{email} now has {customers[email].Points} points.");

            if (customers[email].Points >= 10)
            {
                Console.WriteLine($"{email} earned a free coffee!");
                customers[email].Points = 0;
            }
        }
        else
        {
            Console.WriteLine("Customer not found.");
        }
    }

    public void PrintCustomerPoints()
    {
        Console.WriteLine("\nCustomer Points:");
        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Value.Email}: {customer.Value.Points} points");
        }
    }
}