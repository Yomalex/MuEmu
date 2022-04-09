[![Discord](https://img.shields.io/discord/419270829562396672)](https://discord.gg/Yfwu8hQ)
 [![Issues](https://img.shields.io/github/issues/Yomalex/MuEmu)](https://github.com/Yomalex/MuEmu/issues)
 [![Commits](https://img.shields.io/github/commit-activity/m/Yomalex/MuEmu)](https://github.com/Yomalex/MuEmu/commits/master)
[Client Season 9](https://github.com/Yomalex/IGCN-v9.5-MuServer-S9EP2/tree/master/zClient)

# Mu Online Server Emulator C#

It is a version of the MU Online Server written in C # NetCore3.1 for compatibility on all platforms
Now is listed to support:
- Season 6 Korean
- Season 9 English
- Season 12 Korean
- Season 16 Korean
- Season 17 Korean


# Servers included

This repository includes several projects where the code is distributed, additionally the distribution could be changed to improve efficiency

## ConnectServer

Automatically detects the servers connected to it and depending on its configuration, shows or not in the list of servers that are linked to it

## GameServer

He is the star of this project and in charge of managing all the features of the game. It connects to the ConnectServer and sends important information such as the IP, usage statistics and if it will be shown in the list of servers or not.
This server automatically creates the structure of your Database, it works with MySql Server.
It includes a list of commands that will grow over time.

### Configuration file
The configuration comes in XML, in the server.xml file, generated automatically when the Server is opened.

    <?xml version="1.0"?>
    <Server xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <Name>GameServer</Name>
      <Code>0</Code>
      <Show>1</Show>
      <Lang>es</Lang>
      <AutoRegister>true</AutoRegister>
      <Season>Season9Eng</Season>
      <Connection>
        <IP>127.0.0.1</IP>
        <Port>55901</Port>
        <ConnectServerIP>127.0.0.1</ConnectServerIP>
        <APIKey>2020110116</APIKey>
      </Connection>
      <Database>
        <DBIp>127.0.0.1</DBIp>
        <DataBase>MuOnline</DataBase>
        <BDUser>root</BDUser>
        <DBPassword>1234</DBPassword>
      </Database>
      <Client>
        <Version>10525</Version>
        <Serial>fughy683dfu7teqg</Serial>
        <CashShopVersion>512.2014.124</CashShopVersion>
      </Client>
      <GamePlay>
        <Experience>10</Experience>
        <Zen>10</Zen>
        <DropRate>60</DropRate>
        <MaxPartyLevelDifference>400</MaxPartyLevelDifference>
      </GamePlay>
      <Files>
        <Monsters>./Data/Monsters/Monster</Monsters>
        <MonsterSetBase>./Data/Monsters/MonsterSetBase</MonsterSetBase>
        <MapServer>./Data/MapServer.xml</MapServer>
      </Files>

      <!-- Events -->
      <Event name="Sphere" active="1" rate="15">
        <Condition item="7209" itemLevel="0" mobMinLevel="1" mobMaxLevel="1000" map="Kantru2" />
      </Event>
      <Event name="Acheron Spirit Map Fragment" active="1" rate="15">
        <Condition item="6801" itemLevel="0" mobMinLevel="1" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
      <Event name="Kanturu" active="1" rate="5">
        <!--Gemstone-->
        <Condition item="7209" itemLevel="0" mobMinLevel="1" mobMaxLevel="1000" map="Kantru1" />
        <Condition item="7209" itemLevel="0" mobMinLevel="1" mobMaxLevel="1000" map="Kantru2" />
      </Event>
      <Event name="Special Item drop" active="1" rate="10">
        <!--DarkHorse Spirit-->
        <Condition item="6687" itemLevel="0" mobMinLevel="60" mobMaxLevel="150" map="InvalidMap" />
        <!--DarkRaven Spirit-->
        <Condition item="6687" itemLevel="1" mobMinLevel="60" mobMaxLevel="150" map="InvalidMap" />
        <!--Sing of lord-->
        <Condition item="7189" itemLevel="3" mobMinLevel="95" mobMaxLevel="150" map="InvalidMap" />
      </Event>
      <Event name="Kalima" active="1" rate="10">
        <Condition item="7197" itemLevel="1" mobMinLevel="25" mobMaxLevel="46" map="InvalidMap" />
        <Condition item="7197" itemLevel="2" mobMinLevel="47" mobMaxLevel="65" map="InvalidMap" />
        <Condition item="7197" itemLevel="3" mobMinLevel="66" mobMaxLevel="77" map="InvalidMap" />
        <Condition item="7197" itemLevel="4" mobMinLevel="78" mobMaxLevel="83" map="InvalidMap" />
        <Condition item="7197" itemLevel="5" mobMinLevel="84" mobMaxLevel="91" map="InvalidMap" />
        <Condition item="7197" itemLevel="6" mobMinLevel="92" mobMaxLevel="113" map="InvalidMap" />
        <Condition item="7197" itemLevel="7" mobMinLevel="114" mobMaxLevel="150" map="InvalidMap" />
      </Event>
      <Event name="BoxOfRibbon" active="1" rate="10">
        <Condition item="6176" itemLevel="0" mobMinLevel="12" mobMaxLevel="49" map="InvalidMap" />
        <Condition item="6177" itemLevel="0" mobMinLevel="50" mobMaxLevel="69" map="InvalidMap" />
        <Condition item="6178" itemLevel="0" mobMinLevel="70" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
      <Event name="Medals" active="1" rate="10">
        <Condition item="7179" itemLevel="5" mobMinLevel="0" mobMaxLevel="1000" map="Dugeon" />
        <Condition item="7179" itemLevel="5" mobMinLevel="0" mobMaxLevel="1000" map="Davias" />
        <Condition item="7179" itemLevel="6" mobMinLevel="0" mobMaxLevel="1000" map="LostTower" />
        <Condition item="7179" itemLevel="6" mobMinLevel="0" mobMaxLevel="1000" map="Atlans" />
        <Condition item="7179" itemLevel="6" mobMinLevel="0" mobMaxLevel="1000" map="Tarkan" />
      </Event>
      <Event name="HeartOfLove" active="1" rate="10">
        <Condition item="7179" itemLevel="3" mobMinLevel="15" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
      <Event name="EventChip" active="1" rate="10">
        <Condition item="7179" itemLevel="7" mobMinLevel="0" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
      <Event name="FireCracker" active="1" rate="10">
        <Condition item="7179" itemLevel="2" mobMinLevel="17" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
      <Event name="Heart" active="1" rate="10">
        <Condition item="7180" itemLevel="1" mobMinLevel="0" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
      <Event name="StarOfXMas" active="1" rate="10">
        <Condition item="7179" itemLevel="1" mobMinLevel="0" mobMaxLevel="1000" map="Davias" />
        <Condition item="7179" itemLevel="1" mobMinLevel="0" mobMaxLevel="1000" map="Raklion" />
        <Condition item="7179" itemLevel="1" mobMinLevel="0" mobMaxLevel="1000" map="Selupan" />
      </Event>
      <Event name="Halloween" active="1" rate="10">
        <Condition item="7213" itemLevel="0" mobMinLevel="50" mobMaxLevel="1000" map="InvalidMap" />
      </Event>
    </Server>

### Commands
 1. **db:** Manages the database, allows to create, delete or update its structure. Subcommands:
  ```
  - create: Create a structure in an empty db. (Example: db create)
  - migrate: Update the structure of the db. (Example: db migrate)
  - delete: Delete the db you are connected to. (Example: db delete)
  ```
 2. **reload:** takes care of reloading server configuration files. Subcommands:
 ```
  - shops: reload the stores. (Example: reload shops)
  - gates: reload the gate file. (Example: reload gates)
  ```
 3. **set:** cdevelopment command to modify internal variables. Subcommands:
  ```
  - hp: change the HP of the player entering the command. Argument, hp. (Example: chat: set hp 100).
  - zen: change the amount of zen of the player entering the command. Argument, zen. (Example: chat: set zen 99999).
  ```
 4. **exit, quit, stop:** they close the server.
 5. **!\<texto>:** Global message.
 6. **/\<texto>:** Typical MuOnline Commands:
  ```
  - addstr: Add strength points. (Example: chat: /addstr 10)
  - addagi: Add agility points. (Example: chat: /addagi 10)
  - addvit: Add vitality points. (Example: chat: /addvit 10)
  - addene: Add energy points. (Example: chat: /addene 10)
  - addcmd: Add command points. (Example: chat: /addcmd 10)
  - p: Post command (Example: chat: /p Text)
  ```

### Operation files

It requires common files from MuOnline servers, it is designed to read Season 6 version files and others of our own design in XML, some files are automatically translated from .txt to .xml for better handling on the server.
