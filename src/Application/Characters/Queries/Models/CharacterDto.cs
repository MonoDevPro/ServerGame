using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Queries.Models;

public class CharacterDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; }
    public long Experience { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Character, CharacterDto>()
                .ForMember(c => c.Created,
                    opt => opt.MapFrom(src => src.Created.UtcDateTime));
        }
    }
}
