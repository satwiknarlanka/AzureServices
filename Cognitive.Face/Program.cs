using System;
using System.IO;
using System.Threading.Tasks;

namespace Cognitive.Face
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if(args.Length < 2)
            {
                throw new ArgumentException(@"Insufficient arguments provided. 
                    Usage: dotnet run <Face Subscription Key> <Face Subscription Endpoint>");
            }
            var personGroupId = "1";
            var client = new FaceService(args[0], args[1]);
            var bill1 = File.OpenRead("people/bill.jpg");
            var personId = await client.CreatePerson(personGroupId,"bill");
            await client.EnrollFace(personGroupId,personId,bill1,"bill gates");
            var bill2 = File.OpenRead("people/bill2.jpg");
            var personId2 = await client.IdentifyFace(personGroupId,bill2);
            Console.WriteLine($"Image match status: {personId == personId2}");
            await client.DeletePerson(personGroupId,personId);
        }
    }
}
