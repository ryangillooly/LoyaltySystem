using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Provided values
            string storedSalt = "YnJvSEcwY2Q0OExSYUlvR0M5blkrQm1KMy85SGtuUGg1aWRUeWJXeXBmbFFvY3FXWUxsdk5IYWxxdzlLYWo5RmxSbVlvR0pwZmg0VE1iOHRaMFRlNnFpWCs5Y3l3RFpibUhnT25kU3ZKc3QxcFc1UnhLelFpUS9QUEYyZnA3VDRXMUlDQ3J1blVXVXQ2Yi9EaDVNZjZXRGNYSXR5K2xSWVFsVUIxcUU1L2VBPQ==";
            string storedHash = "TkZNTi82b0JiaWZaYS9scmNaMkZnRk84VERmZTRzRThzbUtNblp1T3RvVFExcmliSlFOSjZiSzhtbTM3NExMUXRYd3dNbzEveW5ISm1KdVpUdFFKSXc9PQ==";
            string password = "Admin123!";

            // Debug information
            Console.WriteLine("Salt length (Base64): " + storedSalt.Length);
            Console.WriteLine("Hash length (Base64): " + storedHash.Length);
            
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            
            Console.WriteLine("Salt length (bytes): " + saltBytes.Length);
            Console.WriteLine("Hash length (bytes): " + hashBytes.Length);
            
            // Testing password verification
            using var hmac = new HMACSHA512(saltBytes);
            byte[] computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            string computedHash = Convert.ToBase64String(computedHashBytes);
            
            Console.WriteLine("\nStored hash:  " + storedHash);
            Console.WriteLine("Computed hash: " + computedHash);
            Console.WriteLine("Match: " + (computedHash == storedHash));
            
            // Try creating a new hash with the same salt
            Console.WriteLine("\nGenerating new hash with the same salt for 'Admin123!'");
            var hmac2 = new HMACSHA512(saltBytes);
            var newHash = Convert.ToBase64String(hmac2.ComputeHash(Encoding.UTF8.GetBytes("Admin123!")));
            Console.WriteLine("New hash: " + newHash);
            
            // Try a completely new hash generation
            Console.WriteLine("\nGenerating completely new hash with new salt for 'Admin123!'");
            using var hmac3 = new HMACSHA512();
            var newSalt = Convert.ToBase64String(hmac3.Key);
            var brandNewHash = Convert.ToBase64String(hmac3.ComputeHash(Encoding.UTF8.GetBytes("Admin123!")));
            Console.WriteLine("New salt: " + newSalt.Substring(0, 50) + "...");
            Console.WriteLine("New hash: " + brandNewHash);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
