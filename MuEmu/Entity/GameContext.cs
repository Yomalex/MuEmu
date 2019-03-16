using Microsoft.EntityFrameworkCore;
using MU.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Entity
{
    public class GameContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(Program.ConnectionString);
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

            //modelBuilder.Entity<ItemDto>(
            //    x =>
            //    {
            //        //x.Property(y => y.Number)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Agility)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Energy)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Vitality)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Command)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Level)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.LevelUpPoints)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Life)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.MaxLife)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.Mana)
            //        //.HasConversion<int>();
            //        //x.Property(y => y.MaxMana)
            //        //.HasConversion<int>();
            //    });
        }

        public DbSet<AccountDto> Accounts { get; set; }
        public DbSet<CharacterDto> Characters { get; set; }
        public DbSet<GuildDto> Guilds { get; set; }
        public DbSet<ItemDto> Items { get; set; }
        public DbSet<FriendDto> Friends { get; set; }
        public DbSet<MemoDto> Letters { get; set; }
        public DbSet<SpellDto> Spells { get; set; }
        public DbSet<QuestDto> Quests { get; set; }
        public DbSet<SkillKeyDto> Config { get; set; }
    }
}
