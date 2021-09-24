[![Discord](https://img.shields.io/discord/591914197219016707.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/Yfwu8hQ)
[Client](https://github.com/Yomalex/IGCN-v9.5-MuServer-S9EP2/tree/master/zClient)

# Mu Online Server Emulator C#

Es una versión del Servidor de MU Online escrita en C# NetCore2 para compatibilidad en todas las plataformas


# Servidores incluidos

Este Repositorio incluye varios proyectos donde se distribuye el código, adicionalmente se podría cambiar la distribución para mejorar la eficiencia

## ConnectServer

Detecta automáticamente los servidores conectados a el y según la configuración del mismo, muestra o no en la lista de servidores a los que están vinculados a el

## GameServer

Es la estrella de este proyecto y el encargado de manejar todas las características del juego. Se conecta al ConnectServer y envía información importante como lo son la IP, Estadísticas de uso y si se mostrará en la lista de servidores o no.
Este servidor crea automáticamente la estructura de su Base de datos, funciona con MySql Server.
Incluye una lista de comandos que irá creciendo con el tiempo.
### Archivo de configuración
la configuracion viene en XML, en el archivo server.xml generado de forma automática al abrir el Servidor.

    <?xml version="1.0"?>
	<Server xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
	  <Version>10635</Version>                              
	  <Serial>fughy683dfu7teqg</Serial>
	  <Name>GameServer</Name>
	  <Code>0</Code>
	  <Show>1</Show>
	  <IP>127.0.0.1</IP>
	  <Port>55901</Port>
	  <ConnectServerIP>127.0.0.1</ConnectServerIP>
	  <Experience>10</Experience>
	  <AutoRegistre>true</AutoRegistre>
	  <Zen>10</Zen>
	  <DropRate>60</DropRate>
	  <DBIp>127.0.0.1</DBIp>
	  <DataBase>MuOnline</DataBase>
	  <BDUser>root</BDUser>
	  <DBPassword></DBPassword>
	  <Rijndael>0</Rijndael>
      
      <!-- Events -->
  <Event name="Sphere" active="1" rate="15">
    <Condition item="7209" itemLevel="0" mobMinLevel="1" mobMaxLevel="1000" map="Kantru2" />
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

### Comandos
 - **db:** Maneja la base de datos, permite crear, borrar o actualizar la estructura de la misma. Subcomandos:
 -- **create:** Crea una estructura en una db vacia.
 -- **migrate:** Actualiza la estructura de la db.
 -- **delete:** Borra la db a la que esta conectado.
 - **reload:** se encarga de recargar archivos de configuración del servidor. Subcomandos:
 -- **shops:** recarga las tiendas.
 -- **gates:** recarga el archivo de puertas.
 - **set:** comando de desarrollo para modificar variables internas. Subcomandos:
 -- **hp:** cambia el **HP** del player que ingresa el comando. Argumento, hp. (**Ejemplo:** chat: set hp 100).
 -- **zen:** cambia la cantidad de **zen** del player que ingresa el comando. Argumento, zen. (**Ejemplo:** chat: set zen 99999).
 - **exit, quit, stop:** cierran el servidor.
 - **!\<texto>:** Mensaje global.
 - **/\<texto>:** Comandos típicos de MuOnline:
 -- **addstr:** Agrega puntos de fuerza. (**Ejemplo:** chat: /addstr 10)
 -- **addagi:** Agrega puntos de agilidad. (**Ejemplo:** chat: /addagi 10)
 -- **addvit:** Agrega puntos de vitalidad. (**Ejemplo:** chat: /addvit 10)
 -- **addene:** Agrega puntos de energía. (**Ejemplo:** chat: /addene 10)
 -- **addcmd:** Agrega puntos de comando. (**Ejemplo:** chat: /addcmd 10)

### Archivos de funcionamiento

Requiere de archivos comunes de servidores MuOnline, esta diseñado para leer archivos de la versión S4 y otros de diseño propio en XML, algunos archivos son traducidos de manera automática de .txt a .xml para mejor manejo en el servidor.
