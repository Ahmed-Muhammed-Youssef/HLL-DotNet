using System.Text;

namespace HyperLogLog.Tests;

public class HllEstimatorTests
{
    [Fact]
    public void Estimate_SmallDataset_ReturnsReasonableEstimate()
    {
        // Arrange
        var data = new List<byte[]>
        {
            Encoding.UTF8.GetBytes("item1"),
            Encoding.UTF8.GetBytes("item2"),
            Encoding.UTF8.GetBytes("item1"), // Duplicate to test cardinality
        };
        int prefixLength = 4; // 16 registers

        // Act
        double estimate = HllEstimator.Estimate(data, prefixLength);

        // Assert
        Assert.True(estimate > 0, "Estimate should be positive.");
        Assert.True(estimate < 3 && estimate > 1, "Estimate should be close to 2 for small distinct set.");
    }

    [Fact]
    public void Estimate_EmptyDataset_ReturnsZero()
    {
        // Arrange
        var data = new List<byte[]>();
        int prefixLength = 4;

        // Act
        double estimate = HllEstimator.Estimate(data, prefixLength);

        // Assert
        Assert.Equal(0, estimate, precision: 6); // Expect zero for empty input
    }

    [Fact]
    public void Estimate_NullData_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<byte[]> data = null;
        int prefixLength = 4;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HllEstimator.Estimate(data, prefixLength));
    }

    [Fact]
    public void Estimate_InvalidPrefixLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var data = new List<byte[]> { Encoding.UTF8.GetBytes("item1") };
        int invalidPrefixLength = 33; // Too large

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => HllEstimator.Estimate(data, invalidPrefixLength));
    }

    [Fact]
    public void Estimate_LargeDataset_ReturnsAccurateEstimate()
    {
        // Arrange
        int numberOfItems = 10_000_000;
        int prefixLength = 14; // 2^14 = 16384 registers, good for high accuracy
        var data = new List<byte[]>(numberOfItems);

        for (int i = 0; i < numberOfItems; i++)
        {
            string value = "item_" + i;
            data.Add(Encoding.UTF8.GetBytes(value));
        }

        // Act
        double estimate = HllEstimator.Estimate(data, prefixLength);

        // Assert
        double error = Math.Abs(estimate - numberOfItems) / numberOfItems;

        // HyperLogLog has an expected standard error of 1.04 / sqrt(m), where m = 2^prefixLength
        double expectedError = 1.04 / Math.Sqrt(Math.Pow(2, prefixLength));

        Assert.InRange(error, 0, expectedError * 3); // Allow a 3σ error margin for safety
    }
}