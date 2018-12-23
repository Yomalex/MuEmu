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
            modelBuilder.Entity<ItemDto>()
                .Property(x => x.Luck)
                .HasConversion<short>();
            modelBuilder.Entity<ItemDto>()
                .Property(x => x.Skill)
                .HasConversion<short>();

            modelBuilder.Entity<CharacterDto>(
                x =>
                {
                    x.Property(y => y.Str)
                    .HasConversion<int>();
                    x.Property(y => y.Agility)
                    .HasConversion<int>();
                    x.Property(y => y.Energy)
                    .HasConversion<int>();
                    x.Property(y => y.Vitality)
                    .HasConversion<int>();
                    x.Property(y => y.Command)
                    .HasConversion<int>();
                    x.Property(y => y.Level)
                    .HasConversion<int>();
                    x.Property(y => y.LevelUpPoints)
                    .HasConversion<int>();
                    x.Property(y => y.Life)
                    .HasConversion<int>();
                    x.Property(y => y.MaxLife)
                    .HasConversion<int>();
                    x.Property(y => y.Mana)
                    .HasConversion<int>();
                    x.Property(y => y.MaxMana)
                    .HasConversion<int>();
                });

            modelBuilder.Entity<ItemDto>(
                x =>
                {
                    x.Property(y => y.Number)
                    .HasConversion<int>();
                    //x.Property(y => y.Agility)
                    //.HasConversion<int>();
                    //x.Property(y => y.Energy)
                    //.HasConversion<int>();
                    //x.Property(y => y.Vitality)
                    //.HasConversion<int>();
                    //x.Property(y => y.Command)
                    //.HasConversion<int>();
                    //x.Property(y => y.Level)
                    //.HasConversion<int>();
                    //x.Property(y => y.LevelUpPoints)
                    //.HasConversion<int>();
                    //x.Property(y => y.Life)
                    //.HasConversion<int>();
                    //x.Property(y => y.MaxLife)
                    //.HasConversion<int>();
                    //x.Property(y => y.Mana)
                    //.HasConversion<int>();
                    //x.Property(y => y.MaxMana)
                    //.HasConversion<int>();
                });
        }

        public DbSet<AccountDto> Accounts { get; set; }
        public DbSet<CharacterDto> Characters { get; set; }
        public DbSet<GuildDto> Guilds { get; set; }
        public DbSet<ItemDto> Items { get; set; }
    }
}
