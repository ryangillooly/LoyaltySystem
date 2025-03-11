using System;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Infrastructure.Data.Extensions
{
    /// <summary>
    /// Extension methods for domain entities to provide access to internal methods.
    /// </summary>
    public static class DomainExtensions
    {
        // Brand extensions
        public static void SetContactInfo(this Brand brand, ContactInfo contact)
        {
            // Use reflection to access the internal method
            var method = typeof(Brand).GetMethod("SetContactInfo", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(brand, new object[] { contact });
        }
        
        public static void SetAddress(this Brand brand, Address address)
        {
            // Use reflection to access the internal method
            var method = typeof(Brand).GetMethod("SetAddress", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(brand, new object[] { address });
        }
        
        // Store extensions
        public static void SetAddress(this Store store, Address address)
        {
            // Use reflection to access the internal method
            var method = typeof(Store).GetMethod("SetAddress", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(store, new object[] { address });
        }
        
        public static void SetLocation(this Store store, GeoLocation location)
        {
            // Use reflection to access the internal method
            var method = typeof(Store).GetMethod("SetLocation", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(store, new object[] { location });
        }
        
        // LoyaltyProgram extensions
        public static void SetExpirationPolicy(this LoyaltyProgram program, ExpirationPolicy policy)
        {
            // Use reflection to access the internal method
            var method = typeof(LoyaltyProgram).GetMethod("SetExpirationPolicy", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(program, new object[] { policy });
        }
        
        public static void AddReward(this LoyaltyProgram program, Reward reward)
        {
            // Use reflection to access the internal method
            var method = typeof(LoyaltyProgram).GetMethod("AddReward", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(program, new object[] { reward });
        }
        
        // LoyaltyCard extensions
        public static void AddTransaction(this LoyaltyCard card, Transaction transaction)
        {
            // Use reflection to access the internal method
            var method = typeof(LoyaltyCard).GetMethod("AddTransaction", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            method?.Invoke(card, new object[] { transaction });
        }
    }
} 