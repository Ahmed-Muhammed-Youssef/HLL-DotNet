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
}