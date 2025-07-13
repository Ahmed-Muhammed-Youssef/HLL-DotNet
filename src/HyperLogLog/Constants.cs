namespace HyperLogLog
{
    internal class Constants
    {
        public const int BYTE = 8;
        public const int TWO_BYTES = 16;   
        public const int FOUR_BYTES = 32;  
        public const int EIGHT_BYTES = 64;
        public const double HLL_ALPHA_16 = 0.673;
        public const double HLL_ALPHA_32 = 0.697;
        public const double HLL_ALPHA_64 = 0.709;
        public const double HLL_ALPHA_LARGE = 0.7213;
        public const double HLL_BIAS_CORRECTION = 1.079;
        public const double FLOAT_SCALING = 2.5;
        public const double THRESHOLD = 30.0;
    }
}
