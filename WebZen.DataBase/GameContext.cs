using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MU.DataBase;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

namespace MuEmu.Entity
{
    public class GameContext : DbContext
    {
        private static readonly LoggerFactory ChangeTrackingAndSqlConsoleLoggerFactory
            = new LoggerFactory(new[] {
                new SerilogLoggerProvider (Log.ForContext(Constants.SourceContextPropertyName, nameof(GameContext)), true)
            }, new LoggerFilterOptions().AddFilter((str, level) => str.Contains("Command")));
        public static string ConnectionString;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseLoggerFactory(ChangeTrackingAndSqlConsoleLoggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseMySQL(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ItemDto>(
            //    x => {
            //        x.Property(y => y.Luck).HasConversion<short>();
            //        x.Property(y => y.Skill).HasConversion<short>();
            //    });

            modelBuilder
                //.Entity<CharacterDto>(
                //x =>
                //{
                //    x.Property(y => y.Str)
                //    .HasConversion<int>();
                //    x.Property(y => y.Agility)
                //    .HasConversion<int>();
                //    x.Property(y => y.Energy)
                //    .HasConversion<int>();
                //    x.Property(y => y.Vitality)
                //    .HasConversion<int>();
                //    x.Property(y => y.Command)
                //    .HasConversion<int>();
                //    x.Property(y => y.Level)
                //    .HasConversion<int>();
                //    x.Property(y => y.LevelUpPoints)
                //    .HasConversion<int>();
                //    x.Property(y => y.Life)
                //    .HasConversion<int>();
                //    x.Property(y => y.MaxLife)
                //    .HasConversion<int>();
                //    x.Property(y => y.Mana)
                //    .HasConversion<int>();
                //    x.Property(y => y.MaxMana)
                //    .HasConversion<int>();
                //})
                .Entity<CharacterDto>()
                .HasOne(x => x.Account)
                .WithMany(y => y.Characters);

            modelBuilder.Entity<CharacterDto>()
                .HasMany(x => x.Quests)
                .WithOne(y => y.Character);

            modelBuilder.Entity<CharacterDto>()
                .HasMany(x => x.Spells)
                .WithOne(y => y.Character);

            modelBuilder.Entity<GuildDto>()
                .HasMany(x => x.MembersInfo)
                .WithOne(y => y.Guild);

            modelBuilder.Entity<GuildDto>()
                .Navigation(x => x.MembersInfo)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();

            modelBuilder.Entity<GuildMemberDto>()
                .HasOne(x => x.Guild)
                .WithMany(y => y.MembersInfo);

            modelBuilder.Entity<GuildMemberDto>()
                .HasOne(x => x.Memb)
                .WithOne();

            modelBuilder.Entity<GuildMemberDto>()
                .Navigation(x => x.Guild)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();

            modelBuilder.Entity<GuildMemberDto>()
                .Navigation(x => x.Memb)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();

            modelBuilder.Entity<AccountDto>()
                .HasMany(x => x.Characters)
                .WithOne(y => y.Account);

            modelBuilder.Entity<AccountDto>()
                .Navigation(x => x.Characters)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();

            modelBuilder.Entity<ItemDto>()
                .HasOne(x => x.Account)
                .WithMany();

            modelBuilder.Entity<ItemDto>()
                .HasOne(x => x.Character)
                .WithMany(x => x.Items);

            modelBuilder.Entity<CharacterDto>()
                .HasMany(x => x.Items)
                .WithOne(y => y.Character);

            modelBuilder.Entity<CharacterDto>()
                .HasMany(x => x.Friends)
                .WithOne(y => y.Character);

            modelBuilder.Entity<CharacterDto>()
                .HasOne(x => x.Gens)
                .WithOne(y => y.Character);

            modelBuilder.Entity<CharacterDto>()
                .HasOne(x => x.BloodCastle)
                .WithOne(y => y.Character);

            modelBuilder.Entity<CharacterDto>()
                .Navigation(x => x.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();

            modelBuilder.Entity<CharacterDto>()
                .Navigation(x => x.Gens)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();

            modelBuilder.Entity<CharacterDto>()
                .Navigation(x => x.BloodCastle)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .AutoInclude();
        }

        public DbSet<AccountDto> Accounts { get; set; }
        public DbSet<CharacterDto> Characters { get; set; }
        public DbSet<GuildDto> Guilds { get; set; }
        public DbSet<GuildMemberDto> GuildMembers { get; set; }
        public DbSet<ItemDto> Items { get; set; }
        public DbSet<FriendDto> Friends { get; set; }
        public DbSet<MemoDto> Letters { get; set; }
        public DbSet<SpellDto> Spells { get; set; }
        public DbSet<QuestDto> Quests { get; set; }
        public DbSet<SkillKeyDto> Config { get; set; }
        public DbSet<MasterInfoDto> MasterLevel { get; set; }

        public DbSet<GensDto> Gens { get; set; }
        public DbSet<BloodCastleDto> BloodCastles { get; set; }
    }
}
