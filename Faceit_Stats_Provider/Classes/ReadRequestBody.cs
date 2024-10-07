using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Faceit_Stats_Provider.Classes
{
    public class ReadRequestBody
    {
        public static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            string jsonString;

            if (request.Headers["Content-Encoding"] == "gzip")
            {
                using (var decompressionStream = new GZipStream(request.Body, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressionStream))
                {
                    jsonString = await reader.ReadToEndAsync();
                }
            }
            else
            {
                using (var reader = new StreamReader(request.Body))
                {
                    jsonString = await reader.ReadToEndAsync();
                }
            }

            return jsonString;
        }
    }
}
