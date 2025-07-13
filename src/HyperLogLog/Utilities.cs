using System;

namespace HyperLogLog
{
    internal static class Utilities
    {
        /// <summary>
        /// Extracts the first <paramref name="prefixLength"/> bits from a hash and returns them as an integer.
        /// </summary>
        /// <param name="hash">The input hash as a read-only span of bytes.</param>
        /// <param name="prefixLength">The number of bits to extract (1 to 32).</param>
        /// <returns>An integer containing the first <paramref name="prefixLength"/> bits of the hash.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="hash"/> is too short for <paramref name="prefixLength"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="prefixLength"/> is less than 1 or greater than 32.</exception>
        internal static int ExtractPrefix(ReadOnlySpan<byte> hash, int prefixLength)
        {
            if (prefixLength < 1 || prefixLength > 32)
                throw new ArgumentOutOfRangeException(nameof(prefixLength), "Prefix length must be between 1 and 32.");

            int fullBytes = prefixLength / 8;
            int extraBits = prefixLength % 8;
            int requiredBytes = fullBytes + (extraBits > 0 ? 1 : 0);

            if (hash.Length < requiredBytes)
                throw new ArgumentException($"Hash length ({hash.Length} bytes) is too short for prefix length ({prefixLength} bits).", nameof(hash));

            int result = 0;

            // Process full bytes
            for (int i = 0; i < fullBytes; i++)
            {
                result = (result << 8) | hash[i];
            }

            // Process extra bits from the next byte, if any
            if (extraBits > 0)
            {
                int nextByte = hash[fullBytes];
                // Extract the high-order extraBits from the next byte
                int bits = nextByte >> (8 - extraBits);
                result = (result << extraBits) | bits;
            }

            // Mask to ensure only prefixLength bits are returned
            return result & ((1 << prefixLength) - 1);
        }

        /// <summary>
        /// Counts the number of consecutive zero bits in the hash starting from the specified bit index.
        /// </summary>
        /// <param name="startIndex">The bit index to start counting zeros from (0-based).</param>
        /// <param name="hash">The input hash as a read-only span of bytes.</param>
        /// <returns>The number of consecutive zero bits until a 1 bit is encountered or the hash ends.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="hash"/> is empty or <paramref name="startIndex"/> is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> is negative.</exception>
        internal static int CalculateZeros(int startIndex, ReadOnlySpan<byte> hash)
        {
            if (hash.IsEmpty)
                throw new ArgumentException("Hash cannot be empty.", nameof(hash));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index cannot be negative.");
            if (startIndex >= hash.Length * 8)
                throw new ArgumentException($"Start index ({startIndex}) exceeds hash length ({hash.Length * 8} bits).", nameof(startIndex));

            int result = 0;
            int byteIndex = startIndex / 8;
            int bitInByte = 7 - (startIndex % 8); // Most significant bit first

            while (byteIndex < hash.Length)
            {
                if (((hash[byteIndex] >> bitInByte) & 1) != 0)
                    return result;

                result++;
                bitInByte--;
                if (bitInByte < 0)
                {
                    byteIndex++;
                    bitInByte = 7;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the HyperLogLog alpha constant for the given number of registers.
        /// </summary>
        /// <param name="numberOfRegisters">The number of registers (must be a power of 2).</param>
        /// <returns>The alpha constant for the HyperLogLog algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numberOfRegisters"/> is not positive or not a power of 2.</exception>
        internal static double GetAlpha(int numberOfRegisters)
        {
            if (numberOfRegisters <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfRegisters), "Number of registers must be positive.");
            if ((numberOfRegisters & (numberOfRegisters - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfRegisters), "Number of registers must be a power of 2.");

            if (numberOfRegisters == Constants.TWO_BYTES)
            {
                return Constants.HLL_ALPHA_16;
            }
            if (numberOfRegisters == Constants.FOUR_BYTES)
            {
                return Constants.HLL_ALPHA_32;
            }
            if (numberOfRegisters == Constants.EIGHT_BYTES)
            {
                return Constants.HLL_ALPHA_64;
            }

            return Constants.HLL_ALPHA_LARGE / (1.0 + Constants.HLL_BIAS_CORRECTION / numberOfRegisters);
        }
    }
}
