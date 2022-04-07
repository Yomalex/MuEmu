using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Data
{
    public interface IPartyDto
    {
        public abstract IPartyDto Set(
            int number, 
            string name, 
            Maps map, 
            byte x, 
            byte y, 
            int life, 
            int maxLife, 
            int mana, 
            int maxMana,
            int channel,
            byte assistant);
    }
    [WZContract]
    public class PartyDto : IPartyDto
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)] public string Id { get; set; }
        [WZMember(1)] public byte Number { get; set; }
        [WZMember(2)] public byte Map { get; set; }
        [WZMember(3)] public byte X { get; set; }
        [WZMember(4)] public byte Y { get; set; }
        [WZMember(5)] public ushort Padding1 { get; set; }
        [WZMember(6)] public int Life { get; set; }
        [WZMember(7)] public int MaxLife { get; set; }

        public IPartyDto Set(int number, string name, Maps map, byte x, byte y, int life, int maxLife, int mana, int maxMana, int channel, byte assistant)
        {
            Number = (byte)number;
            Id = name;
            Map = (byte)Map;
            X = x;
            Y = y;
            Life = life;
            MaxLife = maxLife;
            return this;
        }
    }
    [WZContract]
    public class PartyS9Dto : PartyDto
    {
        [WZMember(8)]
        public int ServerChannel { get; set; }

        [WZMember(9)]
        public int Mana { get; set; }

        [WZMember(10)]
        public int MaxMana { get; set; }
    }

    [WZContract]
    public class PartyS16Dto : IPartyDto
    {
        [WZMember(0, typeof(BinaryStringSerializer), 10)] public string Id { get; set; }
        [WZMember(1)] public ushort Number { get; set; }
        [WZMember(2)] public ushort Level { get; set; }
        [WZMember(3)] public Maps Map { get; set; }
        [WZMember(4)] public byte X { get; set; }
        [WZMember(5)] public byte Y { get; set; }
        [WZMember(6)] public ushort Padding1 { get; set; }
        [WZMember(7)] public int Life { get; set; }
        [WZMember(8)] public int MaxLife { get; set; }
        [WZMember(9)] public int ServerChannel { get; set; }
        [WZMember(10)] public int Mana { get; set; }
        [WZMember(11)] public int MaxMana { get; set; }
        [WZMember(12)] public int Helper { get; set; }
        [WZMember(13)] public byte Assistant { get; set; }
        [WZMember(14)] public ushort unk42 { get; set; }

        public IPartyDto Set(int number, string name, Maps map, byte x, byte y, int life, int maxLife, int mana, int maxMana, int channel, byte assistant)
        {
            Number = (ushort)number;
            Id = name;
            Map = map;
            X = x;
            Y = y;
            Life = life;
            MaxLife = maxLife;
            Mana = mana;
            MaxMana = maxMana;
            ServerChannel = channel;
            Assistant = assistant;
            return this;
        }
    }
}
