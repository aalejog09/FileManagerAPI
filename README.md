# FileManagerAPI
Esta aplicacion es una **API REST** desarrollada en **.NET 8.0** para gestion de archivos.

## Descripcion

La API permite la gestion de archivos para ser almacenados en ubicaciones especificadas y por una **clave unica** para serparar los "file owners".
la configuracion de la aplicacion se puede ajustar para adaptarse a tus necesidades, ya que permite administrar las extenciones de archivos y el espacio que ocupan, 
ademas de activar y desactivarlos, asi como tambien el puerto de despliegue y la configuracion de la base de datos.

## Caracteristicas


- **Almacenamiento y Descarga de archivos en formato original y Base64**
- **Configuraci�n flexible** para los tipos de extencion de archivos soportados y el espacio que ocuparan cada tipo de extension.
- **Base de datos SQL Server** para la configuracion de tipos de extencion y las rutas de cada archivo
- **Desarrollado en .NET 8.0**.

## Requisitos

- **.NET 8.0**: Asegurate de tener instalada la version correcta de .NET en tu maquina. Puedes descargarla desde [aqui](https://dotnet.microsoft.com/download/dotnet).
- **SQL Server**: La aplicacion usa una base de datos SQL Server para almacenar la configuracion del servidor SMTP. Puedes descargarla desde [aqui](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)
- **Nugget packages** en la configuracion del proyecto podras observar la paqueteria requerida para su correcto funcionamiento.

```xml
 <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.14">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.14" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.14">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>

```

## Config

### Base de Datos

La aplicacion utiliza una base de datos SQL Server (v20.2) para almacenar la configuracion relacionada con los tipos de extension de archivos que podran almacenarse,
el peso de los mismos y la disponibilidad para gestionar archivos del tipo especificado. Asi como tambien la informacion asociada al archivo (Nombre, ruta completa 
y un campo clave que identifica la ubicacion especifica del archivo, es decir, la ultima carpeta donde esta almacenado el archivo)


La descripcion de la tabla para configurar los tipos de extension soportados para almacenar y su peso es :

```sql
CREATE TABLE file_manager_db.dbo.SupportedFiles (
	Id int IDENTITY(1,1) NOT NULL,
	Extension nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	MaxSizeKB decimal(10,2) NOT NULL,
	Status bit NOT NULL,
	CONSTRAINT PK_SupportedFiles PRIMARY KEY (Id)
);
 CREATE UNIQUE NONCLUSTERED INDEX IX_SupportedFiles_Extension ON file_manager_db.dbo.SupportedFiles (  Extension ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
```

La descripcion de la tabla de archivos almacenados es :

```sql
CREATE TABLE file_manager_db.dbo.Files (
	Id int IDENTITY(1,1) NOT NULL,
	Clave nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	FilePath nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	FileName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CreatedAt datetime2 NOT NULL,
	FileSize float NOT NULL,
	CONSTRAINT PK_Files PRIMARY KEY (Id)
);
 CREATE NONCLUSTERED INDEX IX_Files_Clave ON file_manager_db.dbo.Files (  Clave ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
```

### AppSettings.json


el appsettings  contiene la configuracion inicial de la aplicacion, donde se debe indicar la ubicacion de la base de datos, y el puerto donde despliega la aplicacion:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "filemanager_db": "Server=localhost\\SQLEXPRESS;Database=file_manager_db;TrustServerCertificate=True;Trusted_Connection=True;"
  },
  "Urls": "http://localhost:8082"
}
```

## Funcionamiento del API

### TEST 

La app cuenta con el apartado de SWAGGER una vez ha sido desplegada, asi como la coleccion de postman con la informacion necesaria en el siguiente enlace:
[Descargar colección de Postman](https://github.com/aalejog09/FileManagerAPI/blob/main/doc/postman/FileManagerAPI.postman_collection.json)

Asi mismo, se detallan los servicios desarrollados a continuacion: 

### Servicios de Tipos de extension soportados (SupportedFiles)
El Api cuenta con rutas para realizar las operaciones de creacion, lectura, listas y actualizacion de datos y estados para los tipos de extension (**SupportedFiles**)

Se puede crear un tipo de extencion especificando el nombre de la extension y el peso maximo permitido para los archivos que seran almacenados con esa extension.
Adicionalmente se puede actualizar el nombre y peso maximo permitido, asi como habilitar o deshabilitar el tipo de extension. 
**NOTA: Si el tipo de extension no existe o esta deshabilitado, no se permitira la gestion de los archivo de ese tipo de extension**


Los datos de los Json estan especificados en el servicio de **SWAGGER** configurado.


##### **Crear extension** 
HTTP POST **Registrar tipo de extencion: server/api/supportedFile/save**
```json
{
  "extension": "pdf", // nombre del tipo de extension
  "maxSizeKB": 185000 // peso maximo permitido para los archivos de este tipo de extension especificado en KB
}
```

PD: todas las extenciones se registran con status True, lo que indicara Activo. 
    Todas las extenciones son unicas, por lo que solo puede existir un tipo de extencion con nombre unico. (no sensible a Mayus o Minus pdf y PDF es lo mismo.)


##### **Listar un tipo de extension**  
HTTP GET **ListarExtencionPorTipo server/api/supportedFile/extension/{extencionNombre}**

Se muestra el tipo de extension solicitado si esta se encuentra registrada:
```json
{
    "message": "OK",
    "data": {
        "extension": "pdf",
        "maxSizeKB": 1024000.00,
        "status": false
    }
}
```
Si no existe se muestra el mensaje:

```json
{
    "message": "the extension [.aaaa] does not exist.",
    "data": "{}"
}
```

##### **Listar los tipos de extension permitidas** 
HTTP GET **lista de extensiones registradas server/api/supportedFile/allowed-extensions** 

se muestra una lista de las extensiones con su tama�o de archivo soportado (en KB y Decimal) y si estan activas o no.

```json
{
    "message": "OK",
    "data": [
        {
            "extension": "pdf",
            "maxSizeKB": 1111.00,
            "status": true
        },
        {
            "extension": "svg",
            "maxSizeKB": 185000.00,
            "status": true
        },
  
        {
            "extension": "jpeg",
            "maxSizeKB": 1024000.00,
            "status": false
        }
    ]
}
```

##### **Actualizar el peso de tipo de extension** 
HTTP PUT **UpdateExtensionMaxSizeKB server/api/supportedFile/update?extension={extension}&maxSizeKB={maxSizeKB}**
Solo actualiza el peso de la extension.

Si existe mostrara el siguiente mensaje (extension=pdf , maxSizeKB=10000)

```json
{
    "message": "The file extension [.pdf] was updated successfully.",
    "data": {
        "extension": "pdf",
        "maxSizeKB": 11111,
        "status": false
    }
}
```

##### **Actualizar el status de tipo de extension** 
HTTP PUT **UpdateExtensionStatus server/api/supportedFile/update?extension={extension}&status={status}**
Solo actualiza el peso de la extension.

Si existe mostrara el siguiente mensaje (extension=pdf , status=true)

```json
{
    "message": "The status of the file extension [.pdf ] was updated successfully.",
    "data": {
        "extension": "pdf",
        "maxSizeKB": 11111,
        "status": true
    }
}
```


### Servicios para la Gestion de archivos

Una vez configurados los tipos de extencion que soportara la API, se puede realizar la carga y descarga de archivos de esos tipos de extension con sus pesos
permitidos. 

Es importante mencionar que de no existir el tipo de extension o de superar el peso configurado para una extencion no se permitira la carga de estos archivos.

#### **Cargar archivo**  
HTTP POST **UploadFile server/api/files/upload**

El Cuerpo de la peticion debe ser un Form-data 



```plaintext
clave: CARPETA_FINAL // es la ultima carpeta para identificar al "due�o del archivo", por ejemplo un rif V123456
path: CARPETA_RAIZ  // ejemplo C:\\uploads para guardar en la carpeta del disco C: llamada uploads 
archivo: [Archivo adjunto] // el archivo a subir
```

asi como se muestra en la imagen :

![Imagen de prueba](https://github.com/aalejog09/FileManagerAPI/blob/main/doc/Example_Form-data.png?raw=true)

el resultado de guardar exitosamente un archivo seria: 

teniendo en cuenta los datos : Clave=V1234566 , path= C:\\UPLOADS\\ , file=imagenDePrueba.png

```json
{
    "message": "File uploaded successfully.",
    "filePath": "C:\\\\UPLOADS\\\\V1234566\\imagenDePrueba.png"
}
```

Y en el sistema de archivos se podria observar de la siguiente manera:

![Imagen de prueba](https://github.com/aalejog09/FileManagerAPI/blob/main/doc/ExampleUploadSuccess.png?raw=true)

##### **Lista de rutas de archivos**  
HTTP GET **getFileRecordPathsByKey server/api/files/list?key={clave}**

Este servicio permite listar las rutas a los archivos de acuerdo al valor "clave" contenido en la ruta (QueryParam)

teniendo en cuenta el ejemplo anterior, se haria la consulta con el key=V1234566  y la respuesta seria:


```json
[
    {
        "filePath": "C:\\\\UPLOADS\\\\V1234566\\imagenDePrueba.png", //ruta absoluta al archivo
        "createdAt": "2025-03-27", //fecha de creacion del archivo
        "size": 182.15  //recordar que esto esta expresado en KB
    }
]
```

##### **Descargar archivo por ruta (archivo)**  
HTTP GET **downloadFileByPath server/api/files/download?filePath={absolutePath}**

Este servicio permite descargar un archivo incluyendo en la peticion la ruta donde se encuentra. La forma ideal seria que al consultar el servicio getFileRecordPathsByKey, se obtiene el valor filePath 
y se pasa por QueryParam a este servicio para descargar el archivo que se contiene en la ruta absoluta. 

ejemplo: 
  filePath=C:/UPLOADS/V1234566/imagneDePrueba.png

Al consultar el servicio se obtiene el archivo para ser descargado de manera inmediata y se debe observar en el navegador la siguiente forma:

![Imagen de prueba](https://github.com/aalejog09/FileManagerAPI/blob/main/doc/ExampleDownloadSuccess.png?raw=true)

##### **Descargar archivo por ruta (base64)**  
HTTP GET **downloadFileByPath server/api/files/download/base64?filePath={absolutePath}**

Este servicio es similar al servicio downloadFileByPath con la diferencia de que la respuesta es en Formato Json y el contenido de la misma es el archivo expresado en Base64
lo que permite usarlo para mostrar previews. el formato de la respuesta seria :

```json
{
    "fileBase64":"base64 data"
}
```
si desea verificar el contenido del base64 puede usar el siguiente enlace : [DecodeBase64 to File](https://base64.guru/converter/decode/file)

y puede observar el resultado de la siguiente forma:

![Imagen de prueba](https://github.com/aalejog09/FileManagerAPI/blob/main/doc/ExampleDownloadSuccessBase64.png?raw=true)


## Creditos
<div style="margin-top: 50px; text-align: center; font-size: 12px;">
    <hr>
    <p><strong>Desarrollado por Andres Alejo</strong></p>
    <p>Marzo de 2025</p>
    <hr>
    <p><a href="https://github.com/aalejog09" target="_blank">Visita mi GitHub</a></p>
    <p>CR 2025. Todos los derechos reservados.</p>
</div>