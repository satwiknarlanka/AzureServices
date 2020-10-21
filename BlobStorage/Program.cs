using System;
using System.IO;
using System.Threading.Tasks;

namespace BlobStorage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if(args.Length == 0)
            {
                throw new ArgumentException("Azure Storage Connection string not found. Usage: dotnet run <Storage Connection String>");
            }
            var storageConnectionString = args[0];
            var fileStream = File.OpenRead("SampleFiles/LoreumIpsum.txt");
            var blobService = new BlobService(storageConnectionString);
            await blobService.UploadFileFromStreamAsync(fileStream,"LoreumIpsum.txt","loreum");
        }
    }
}
