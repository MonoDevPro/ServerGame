using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Queries.Models;

public class CharacterSummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Character, CharacterSummaryDto>();
        }
    }
}
