using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Services
{
    public class AccountService : IAccountService
    {
        public ValueTask<GetCharactersReply> GetCharactersAsync(GetCharactersRequest value, CallContext context = default)
        {
            var characterAId = Guid.Parse("35C06C25-2963-4F3E-83D7-9D5B967646F0");
            var characterBId = Guid.Parse("92E30C93-1F31-461F-81D3-8EFC7EEC79E0");
            
            var getCharactersReply = new GetCharactersReply();
            getCharactersReply.Characters.Add( new Character { Id = characterAId.ToUuid(), Name = "A Mazing"} );
            getCharactersReply.Characters.Add( new Character { Id = characterBId.ToUuid(), Name = "D Strongest"} );
            return ValueTask.FromResult(getCharactersReply);
        }
    }
}
