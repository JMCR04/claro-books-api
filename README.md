# Biblioteca — API

Backend de la prueba técnica Full Stack para Claro Dominicana. Expone una API REST para libros y autores y consume `https://fakerestapi.azurewebsites.net/api/v1/` mediante repositorios HTTP. No utiliza base de datos.

## Requisitos

- .NET SDK 9.0.
- Conexión a Internet para restaurar dependencias y acceder a FakeRESTApi.
- Visual Studio Code, Visual Studio o una terminal.

## Instalación y ejecución

Clona este repositorio y abre una terminal en su carpeta raíz.

```powershell
dotnet restore ClaroBooks.sln
dotnet build ClaroBooks.sln
dotnet dev-certs https --trust
dotnet run --project src/ClaroBooks.Api/ClaroBooks.Api.csproj --launch-profile https
```

Dirección HTTPS de desarrollo: `https://localhost:7275`.

Ejemplos:

- `https://localhost:7275/api/books`
- `https://localhost:7275/api/authors`
- `https://localhost:7275/openapi/v1.json` (documento OpenAPI en Development).

La ruta raíz `/` no contiene una página web. El frontend se ejecuta por separado en `http://localhost:5173`.

## Configuración

`src/ClaroBooks.Api/appsettings.json` contiene:

```json
"FakeRestApi": {
  "BaseUrl": "https://fakerestapi.azurewebsites.net/api/v1/"
}
```

La URL debe terminar en `/`. Los clientes HTTP tienen un tiempo de espera de 30 segundos. La política CORS permite el frontend de desarrollo en `http://localhost:5173`.

## Arquitectura

La solución separa responsabilidades en cuatro proyectos:

- **Domain:** modelos de libros y autores.
- **Application:** contratos de repositorios.
- **Infrastructure:** implementaciones HTTP y cambios temporales en memoria.
- **Api:** controladores, contratos de entrada, validaciones, configuración e identificación de errores.

Los repositorios reciben clientes configurados mediante `IHttpClientFactory`. Las solicitudes propagan `CancellationToken`. Los controladores dependen de interfaces y el manejo de excepciones se centraliza con `IExceptionHandler` y respuestas Problem Details.

## Endpoints

| Método | Ruta | Operación |
|---|---|---|
| GET | `/api/books` | Listar libros |
| GET | `/api/books/{id}` | Consultar un libro |
| POST | `/api/books` | Crear un libro |
| PUT | `/api/books/{id}` | Editar un libro |
| DELETE | `/api/books/{id}` | Eliminar un libro |
| GET | `/api/books/{id}/authors` | Consultar autores relacionados |
| GET | `/api/authors` | Listar autores |
| GET | `/api/authors/{id}` | Consultar un autor |
| POST | `/api/authors` | Crear un autor |
| PUT | `/api/authors/{id}` | Editar un autor |
| DELETE | `/api/authors/{id}` | Eliminar un autor |

POST y PUT devuelven `200` con el objeto procesado; DELETE devuelve `204`. Las escrituras se envían a la API externa y solo se registran temporalmente si esta responde correctamente.

Las validaciones devuelven `400`; los recursos inexistentes, `404`; los IDs duplicados al crear, `409`. Los fallos de comunicación o formato externo devuelven `502`, y los tiempos de espera, `504`. Los errores inesperados devuelven `500` sin exponer detalles internos.

## Comportamiento de FakeRESTApi

El proveedor acepta las escrituras de prueba, pero no conserva sus efectos en consultas posteriores. Para permitir el recorrido CRUD completo se utiliza `TemporaryChanges<T>`, registrado como singleton por tipo de entidad.

Las lecturas siguen consultando al proveedor y combinan su respuesta con altas, modificaciones y eliminaciones temporales. El acceso a los cambios está protegido con `lock`. Un `404` externo puede resolverse con una entidad creada durante la ejecución actual. Las consultas de autores por libro filtran el listado combinado para reflejar también esos cambios.

Este estado vive únicamente en la memoria del proceso: se pierde al reiniciar la API y no se comparte entre varias instancias. Es una adaptación al proveedor de demostración, no persistencia de producción. No se utiliza Entity Framework, SQL ni archivos de datos.

El contrato de autores contiene un único `idBook` por registro. El frontend documenta esta limitación al mostrar la cantidad de libros relacionados.

## Validación

```powershell
dotnet build ClaroBooks.sln
```

El recorrido de integración consiste en crear, consultar, actualizar y eliminar libros y autores; comprobar sus relaciones; y verificar errores por datos vacíos, IDs inválidos, IDs duplicados y recursos inexistentes. Tras reiniciar el backend, los datos vuelven al estado que devuelve el proveedor.

Las pruebas unitarias son opcionales según el enunciado y no se incluyen.
