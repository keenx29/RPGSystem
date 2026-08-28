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
        public DbSet<ItemEntity> Items => Set<ItemEntity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.Abilities)
                .WithOne(a => a.Character)
                .HasForeignKey(a => a.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.Skills)
                .WithOne(s => s.Character)
                .HasForeignKey(s => s.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Character)
                .HasForeignKey(i => i.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AbilityEntity>()
                .HasIndex(a => new { a.CharacterId, a.Type })
                .IsUnique();

            modelBuilder.Entity<SkillEntity>()
                .HasIndex(s => new { s.CharacterId, s.Type })
                .IsUnique();

        }
    }
}