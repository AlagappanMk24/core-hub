namespace Core_API.Infrastructure.Helpers
{
    public static class FileSizeHelper
    {
        public static long Bytes(long bytes) => bytes;
        public static long KB(double kb) => (long)(kb * 1024);
        public static long MB(double mb) => (long)(mb * 1024 * 1024);
        public static long GB(double gb) => (long)(gb * 1024 * 1024 * 1024);
    }
}