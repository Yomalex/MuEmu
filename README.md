[![Discord](https://img.shields.io/discord/591914197219016707.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/UvmCUc)

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
      <BoxOfRibbon active="1" rate="25" />
      <Medals active="1" rate="50" />
      <HeartOfLove active="1" rate="50" />
      <EventChip active="1" rate="50" /> 
      <FireCracker active="1" rate="50" />
      <Heart active="1" rate="50" />
      <StarOfXMas active="1" rate="50" />
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
