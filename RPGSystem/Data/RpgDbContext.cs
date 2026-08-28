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
        public DbSet<ConditionEntity> Conditions => Set<ConditionEntity>();
        public DbSet<FeatureStateEntity> FeatureStates => Set<FeatureStateEntity>();
        public DbSet<FeatureResourceEntity> FeatureResources => Set<FeatureResourceEntity>();
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
            
            modelBuilder.Entity<AbilityEntity>()
                .HasIndex(a => new { a.CharacterId, a.Type })
                .IsUnique();

            modelBuilder.Entity<SkillEntity>()
                .HasIndex(s => new { s.CharacterId, s.Type })
                .IsUnique();

            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Character)
                .HasForeignKey(i => i.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.Conditions)
                .WithOne(c => c.Character)
                .HasForeignKey(c => c.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.FeatureStates)
                .WithOne(f => f.Character)
                .HasForeignKey(f => f.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CharacterEntity>()
                .HasMany(c => c.FeatureResources)
                .WithOne(r => r.Character)
                .HasForeignKey(r => r.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeatureStateEntity>()
                .HasIndex(f => new { f.CharacterId, f.FeatureName })
                .IsUnique();

            modelBuilder.Entity<FeatureResourceEntity>()
                .HasIndex(r => new { r.CharacterId, r.Name })
                .IsUnique();

            modelBuilder.Entity<ConditionEntity>()
                .HasIndex(c => new { c.CharacterId, c.Type })
                .IsUnique();
        }
    }
}