using Microsoft.EntityFrameworkCore;
using RPGSystem.Data.Entities;

namespace RPGSystem.Data
{
    public class RpgDbContext : DbContext
    {
        public RpgDbContext(DbContextOptions<RpgDbContext> options)
            : base(options)
        {
        }

        public DbSet<CharacterEntity> Characters => Set<CharacterEntity>();
        public DbSet<AbilityEntity> Abilities => Set<AbilityEntity>();
        public DbSet<SkillEntity> Skills => Set<SkillEntity>();
    }
}