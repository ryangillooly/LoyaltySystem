using System;
using System.Security.Cryptography;
using System.Text;

namespace LoyaltySystem.PasswordTester
{
    class Program
    {
        static void Main(string[] args)
        {
            // Provided values
            string storedSalt = "YnJvSEcwY2Q0OExSYUlvR0M5blkrQm1KMy85SGtuUGg1aWRUeWJXeXBmbFFvY3FXWUxsdk5IYWxxdzlLYWo5RmxSbVlvR0pwZmg0VE1iOHRaMFRlNnFpWCs5Y3l3RFpibUhnT25kU3ZKc3QxcFc1UnhLelFpUS9QUEYyZnA3VDRXMUlDQ3J1blVXVXQ2Yi9EaDVNZjZXRGNYSXR5K2xSWVFsVUIxcUU1L2VBPQ==";
            string storedHash = "TkZNTi82b0JiaWZaYS9scmNaMkZnRk84VERmZTRzRThzbUtNblp1T3RvVFExcmliSlFOSjZiSzhtbTM3NExMUXRYd3dNbzEveW5ISm1KdVpUdFFKSXc9PQ==";
            string password = "Admin123!";

            // Display basic information
            Console.WriteLine("=== Password Verification Diagnostic ===");
            Console.WriteLine($"Password to verify: {password}");
            Console.WriteLine($"Stored salt (Base64 length): {storedSalt.Length}");
            Console.WriteLine($"Stored hash (Base64 length): {storedHash.Length}");

            try
            {
                // Convert Base64 strings to bytes
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                byte[] hashBytes = Convert.FromBase64String(storedHash);
                
                Console.WriteLine($"Stored salt (Byte length): {saltBytes.Length}");
                Console.WriteLine($"Stored hash (Byte length): {hashBytes.Length}");
                
                // Attempt to verify the password
                Console.WriteLine("\n=== Verification Attempt ===");
                using var hmac = new HMACSHA512(saltBytes);
                byte[] computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                string computedHash = Convert.ToBase64String(computedHashBytes);
                
                Console.WriteLine($"Stored hash:  {storedHash}");
                Console.WriteLine($"Computed hash: {computedHash}");
                Console.WriteLine($"Match: {computedHash == storedHash}");
                
                // Try a completely new hash/salt generation
                Console.WriteLine("\n=== Generate New Credentials ===");
                using var hmacNew = new HMACSHA512();
                var newSalt = Convert.ToBase64String(hmacNew.Key);
                var newHash = Convert.ToBase64String(hmacNew.ComputeHash(Encoding.UTF8.GetBytes(password)));
                
                Console.WriteLine($"New salt: {newSalt}");
                Console.WriteLine($"New hash: {newHash}");
                Console.WriteLine("\nYou can use these credentials for testing.");
                
                // Try some common variations
                Console.WriteLine("\n=== Testing Common Password Variations ===");
                string[] variations = new[] {
                    "admin123!",
                    "Admin123",
                    "ADMIN123!"
                };
                
                foreach (var variant in variations)
                {
                    string variantHash = Convert.ToBase64String(
                        hmac.ComputeHash(Encoding.UTF8.GetBytes(variant)));
                    Console.WriteLine($"Password '{variant}': {(variantHash == storedHash ? "MATCH" : "No match")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n=== ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
