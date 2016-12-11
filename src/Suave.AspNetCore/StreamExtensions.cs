using System.IO;
using System.Threading.Tasks;

namespace Suave.AspNetCore
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadBytesAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}