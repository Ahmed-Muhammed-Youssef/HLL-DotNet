using Murmur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace HyperLogLog
{
    /// <summary>
    /// Provides an implementation of the HyperLogLog algorithm for estimating the cardinality of a dataset.
    /// </summary>
    public class HllEstimator
    {
        private static readonly HashAlgorithm _murmur128 = MurmurHash.Create128(managed: false); // returns a 128-bit algorithm using "unsafe" code with default seed

        /// <summary>
        /// Estimates the cardinality of a dataset using the HyperLogLog algorithm.
        /// </summary>
        /// <param name="data">The collection of byte arrays to estimate cardinality from.</param>
        /// <param name="prefixLength">The number of bits to use for register indexing (typically 4 to 16).</param>
        /// <returns>An estimate of the number of distinct elements in the dataset.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="prefixLength"/> is less than 1 or greater than 32.</exception>
        /// <remarks>
        /// The method uses the MurmurHash128 algorithm to hash input data and applies HyperLogLog to estimate cardinality.
        /// For small cardinalities, a correction is applied based on the number of empty registers.
        /// For large cardinalities, a correction is applied to account for hash collisions.
        /// </remarks>
        public static double Estimate(IEnumerable<byte[]> data, int prefixLength)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (prefixLength < 1 || prefixLength > 32)
                throw new ArgumentOutOfRangeException(nameof(prefixLength), "Prefix length must be between 1 and 32.");

            int numberOfRegisters = 1 << prefixLength;
            int[] accumilator = new int[numberOfRegisters];

            foreach (byte[] record in data)
            {
                // hash data
                ReadOnlySpan<byte> hash = _murmur128.ComputeHash(record);

                // extract prefix
                int index = Utilities.ExtractPrefix(hash, prefixLength);

                // calcaulte number of zeros after the prefix
                int numberOfZeros = Utilities.CalculateZeros(index, hash) + 1;

                accumilator[index] = Math.Max(accumilator[index], numberOfZeros);
            }

            double harmonicMean = 0.0;

            foreach (int item in accumilator)
            {
                harmonicMean += 1.0 / ((ulong)1 << item); // 2^(-M[i])
            }

            double estimate = Utilities.GetAlpha(numberOfRegisters) * numberOfRegisters * numberOfRegisters / harmonicMean;

            // Small cardinality correction
            int zeros = accumilator.Count(i => i == 0);
            if (estimate <= Constants.FLOAT_SCALING * numberOfRegisters)
            {
                if (zeros > 0)
                {
                    estimate = numberOfRegisters * Math.Log((double)(numberOfRegisters) / zeros);
                }
            }
            // Large cardinality correction (typically for very large datasets)
            else if (estimate > ((ulong)1 << Constants.FOUR_BYTES) / Constants.THRESHOLD)
            {
                estimate = -(double)((ulong)1 << Constants.FOUR_BYTES) * Math.Log(1.0 - (estimate / ((ulong)1 << Constants.FOUR_BYTES)));
            }

            return estimate;
        }
    }
}
