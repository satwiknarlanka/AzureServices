using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Cognitive.Face
{
    public class FaceService
    {
        private IFaceClient _client;
        private string _recognitionModel;
        public FaceService(string key,string endpoint)
        {
            _client = new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            _recognitionModel = "recognition_03";
        }

        public async Task DeleteAllOtherFaces(string personGroupId,Guid personId,Guid persistedFaceId)
        {
            var person = await _client.PersonGroupPerson.GetAsync(personGroupId,personId);
            var removeFaces = person.PersistedFaceIds.Where(f => f.Value!=persistedFaceId).Select(p => p.Value);
            foreach (var face in removeFaces)
            {
                await _client.PersonGroupPerson.DeleteFaceAsync(personGroupId,personId,face);
            }
        }
        public async Task<Guid> EnrollFace(string personGroupId,Guid personId,Stream imageStream,string userData)
        {
            MemoryStream localStream = new MemoryStream();
            await imageStream.CopyToAsync(localStream);
            var imageBytes = localStream.ToArray();
            IList<DetectedFace> faces = await _client.Face.DetectWithStreamAsync(new MemoryStream(imageBytes));
            if(faces.Count == 0) throw new ArgumentException("No face detected");
            if(faces.Count > 1) throw new ArgumentException("More than one face detected");
            var persistedFace = await _client
                                        .PersonGroupPerson
                                        .AddFaceFromStreamAsync
                                        (
                                            personGroupId: personGroupId,
                                            personId: personId,
                                            image: new MemoryStream(imageBytes),
                                            userData: userData
                                        );
            await _client.PersonGroup.TrainAsync(personGroupId);
            return persistedFace.PersistedFaceId;
        }
        public async Task DeletePerson(string personGroupId, Guid personId)
        {
            await _client.PersonGroupPerson.DeleteAsync(personGroupId,personId);
        }

        public async Task<Guid> CreatePerson(string personGroupId,string name,string userData="")
        {
            var person = await _client.PersonGroupPerson.CreateAsync(personGroupId,name,userData);
            return person.PersonId;
        }

        public async Task<Guid> IdentifyFace(string personGroupId,Stream imageStream)
        {
            var faces = await _client.Face.DetectWithStreamAsync(imageStream,recognitionModel:_recognitionModel);
            if(faces.Count == 0) throw new ArgumentException("No face detected");
            if(faces.Count > 1) throw new ArgumentException("More than one face detected");
            var identifyResult = await _client.Face.IdentifyAsync(faceIds: faces.Select(f => f.FaceId).ToList(),personGroupId:personGroupId);
            try
            {
                return identifyResult[0].Candidates[0].PersonId;
            }
            catch(ArgumentOutOfRangeException)
            {
                throw new ArgumentException("No person could be identified");
            }
        }
    }
}