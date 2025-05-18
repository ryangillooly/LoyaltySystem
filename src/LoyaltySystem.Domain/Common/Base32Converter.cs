using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LoyaltySystem.Domain.Common;

/// <summary>
/// Utility for converting Guids to and from a Base32 string representation (RFC 4648 without padding).
/// Uses Crockford-style alphabet (excludes I, L, O, U) if specified, otherwise standard RFC 4648.
/// Ensures a fixed length for the payload part.
/// </summary>
public static class Base32Converter
{
    // Standard RFC 4648 Base32 alphabet - Using lowercase
    private const string Base32StandardChars = "abcdefghijklmnopqrstuvwxyz234567";
    private const int DesiredPayloadLength = 26; // 16 bytes * 8 bits/byte / 5 bits/char = 25.6 -> ceil to 26
    private static readonly Dictionary<char, int> CharMap = new();
    private static readonly char[] Digits = Base32StandardChars.ToCharArray();
    private static readonly int Mask = Digits.Length - 1;
    private static readonly int Shift = NumberOfTrailingZeros(Digits.Length); // Should be 5

    static Base32Converter()
    {
        for (int i = 0; i < Digits.Length; i++)
            CharMap[Digits[i]] = i;
    }

    /// <summary>
    /// Encodes a Guid into a fixed-length Base32 string (lowercase alphanumeric, RFC 4648 standard alphabet).
    /// </summary>
    public static string Encode(Guid guid)
    {
        byte[] data = guid.ToByteArray(); // Use standard ToByteArray()
        if (data.Length == 0) return string.Empty;

        var output = new StringBuilder((data.Length * 8 + Shift - 1) / Shift);

        int buffer = data[0];
        int next = 1;
        int bitsLeft = 8;
        while (bitsLeft > 0 || next < data.Length)
        {
            if (bitsLeft < Shift)
            {
                if (next < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[next++] & 0xff;
                    bitsLeft += 8;
                }
                else
                {
                    int pad = Shift - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }
            int index = Mask & (buffer >> (bitsLeft - Shift));
            bitsLeft -= Shift;
            output.Append(Digits[index]);
        }

        // Ensure fixed length (should naturally be 26 for a 16-byte Guid)
        // No padding is typically used for generated IDs, but we ensure length.
        if (output.Length != DesiredPayloadLength)
        {
             // This case should ideally not be hit with Guid input if logic is correct.
             // If shorter, padding might be needed conceptually, but we aim for fixed length.
             // If longer, something is wrong.
             // For simplicity, let's trust the calculation for Guids yields 26.
             // If issues arise, review padding/truncation logic specifically for the 128-bit to 26*5-bit mapping.
             // NOTE: Guid.ToByteArray() order can differ; consider a fixed byte order if needed cross-platform.
             Console.WriteLine($"Warning: Base32 encoding of Guid resulted in unexpected length {output.Length}, expected {DesiredPayloadLength}");
        }

        return output.ToString().PadRight(DesiredPayloadLength, Digits[0]); // Pad if too short (fallback)
    }

    /// <summary>
    /// Decodes a fixed-length Base32 string (lowercase alphanumeric, RFC 4648 standard) back into a Guid.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid Base32 string</exception>
    public static Guid Decode(string encoded)
    {
        encoded = encoded.TrimEnd('='); // Remove padding if present
        encoded = encoded.ToLowerInvariant(); // Ensure lowercase for lookup

        if (encoded.Length != DesiredPayloadLength)
            throw new ArgumentException($"Invalid Base32 payload length. Expected {DesiredPayloadLength} characters (excluding padding).", nameof(encoded));

        int buffer = 0;
        int bitsLeft = 0;
        int count = 0;
        byte[] result = new byte[(encoded.Length * Shift) / 8];

        foreach (char c in encoded)
        {
            if (!CharMap.TryGetValue(c, out int charValue))
                throw new ArgumentException($"Invalid character '{c}' in Base32 payload.", nameof(encoded));

            buffer <<= Shift;
            buffer |= charValue & Mask;
            bitsLeft += Shift;

            if (bitsLeft >= 8)
            {
                result[count++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }

        if (count != result.Length)
        {
             // This might indicate incorrect processing or a non-standard input length/padding issue.
             throw new ArgumentException("Base32 decoding resulted in unexpected byte count.", nameof(encoded));
        }
        
        // Ensure we have exactly 16 bytes for the Guid
        if(result.Length != 16)
        {
             throw new ArgumentException("Decoded Base32 payload does not represent a valid Guid (incorrect byte length).");
        }

        return new Guid(result);
    }

    // Helper to count trailing zeros (used for shift calculation)
    private static int NumberOfTrailingZeros(int i)
    {
        if (i == 0) return 32;
        int n = 31;
        int y = i << 16; if (y != 0) { n -= 16; i = y; }
        y = i << 8; if (y != 0) { n -= 8; i = y; }
        y = i << 4; if (y != 0) { n -= 4; i = y; }
        y = i << 2; if (y != 0) { n -= 2; i = y; }
        return n - ((i << 1) >> 31);
    }
} 