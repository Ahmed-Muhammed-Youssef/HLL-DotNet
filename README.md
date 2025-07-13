# HyperLogLog Estimator

A .NET Standard 2.1 implementation of the HyperLogLog algorithm for efficient cardinality estimation of large datasets.

## Overview

The HyperLogLog Estimator is a .NET Standard 2.1 library that implements the HyperLogLog algorithm to estimate the number of distinct elements (cardinality) in a dataset with minimal memory usage. The library accepts input as a collection of byte arrays (`IEnumerable<byte[]>`), using the MurmurHash128 algorithm (via the `murmurhash` NuGet package) for hashing. It includes utilities for bit manipulation and is designed for high performance, with comprehensive unit tests using xUnit.

## Features

- **Efficient Cardinality Estimation**: Estimates unique elements in large datasets with low memory footprint.
- **MurmurHash128 Integration**: Uses the `murmurhash` library (version 1.0.3) for fast and robust hashing.
- **Byte Array Input**: Processes `IEnumerable<byte[]>` inputs, with support for converting common types (e.g., strings) to byte arrays.
- **Small and Large Cardinality Corrections**: Applies corrections for accurate estimates across a wide range of dataset sizes.
- **Unit Tested**: Includes xUnit tests for robust validation of core functionality.

## Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/hyperloglog-estimator.git
   cd hyperloglog-estimator
   ```

2. **Install Dependencies**:
   Ensure you have a .NET SDK compatible with .NET Standard 2.1 (e.g., .NET Core 3.0 or later, .NET 5.0 or later). The project depends on the `murmurhash` NuGet package (version 1.0.3) for hashing. For testing, add xUnit packages.

   ```bash
   dotnet restore
   ```

3. **Build the Project**:
   ```bash
   dotnet build
   ```

## Usage

### Estimating Cardinality

The `HllEstimator` class estimates cardinality from a collection of byte arrays (`IEnumerable<byte[]>`). The `prefixLength` parameter determines the number of registers (2^prefixLength), typically between 4 and 16.

```csharp
using HyperLogLog;
using System.Collections.Generic;
using System.Text;

var data = new List<string> { "item1", "item2", "item1", "item3" };
var byteData = data.Select(s => Encoding.UTF8.GetBytes(s)).ToList();
double estimate = HllEstimator.Estimate(byteData, prefixLength: 4);
Console.WriteLine($"Estimated cardinality: {estimate}");
```

### Utilities

The `Utilities` class provides helper methods for the HyperLogLog algorithm:
- `ExtractPrefix`: Extracts the first `prefixLength` bits from a hash for register indexing.
- `CalculateZeros`: Counts consecutive zero bits in a hash starting from a given index.
- `GetAlpha`: Returns the HyperLogLog alpha constant based on the number of registers.

## Running Tests

The project includes xUnit tests to validate the `HllEstimator` and `Utilities` classes. Ensure a test runner compatible with .NET Standard 2.1 is installed (e.g., `xunit.runner.console`).

```bash
dotnet test
```

## Dependencies

- **.NET Standard 2.1**: Compatible with .NET Core 3.0+, .NET 5.0+, or other supporting runtimes.
- **murmurhash**: Version 1.0.3, for MurmurHash128 hashing.
- **xUnit**: For unit testing.

## Notes

- **Thread Safety**: The `HllEstimator` class uses a static `MurmurHash` instance, which may not be thread-safe.
- **Nullable Reference Types**: The project uses `<Nullable>enable</Nullable>`, so ensure nullable annotations are handled appropriately in consuming code.

## Contributing

Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m 'Add YourFeature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a pull request.

Please include tests for new features and ensure all existing tests pass.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
