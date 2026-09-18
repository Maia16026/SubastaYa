# SubastaYa

Trabajo Práctico de la materia **Proyecto de Software**.

SubastaYa es una aplicación web de subastas donde los usuarios pueden consultar publicaciones, crear subastas, realizar pujas y administrar una billetera virtual.

El sistema fue desarrollado separando el Frontend, el Backend y la Base de Datos. Las reglas importantes del negocio se resuelven en el Backend y los datos se almacenan en SQL Server.

---

# 1. Tecnologías utilizadas

## Backend

- .NET 8
- C#
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- Swagger / OpenAPI

## Frontend

- HTML
- CSS
- JavaScript
- Fetch API

## Base de Datos

- SQL Server
- Entity Framework Core
- Code First
- Migrations

---

# 2. Arquitectura

El Backend está separado en cuatro proyectos principales:

```text
Domain
Application
Infrastructure
Api
```

La idea principal es que cada parte del sistema tenga una responsabilidad clara y que la lógica de negocio no quede mezclada con Controllers, Base de Datos o detalles técnicos.

## Domain

Es el núcleo del negocio.

Contiene las entidades y conceptos principales de SubastaYa, por ejemplo:

- Usuario
- Categoria
- Subasta
- Puja
- Billetera
- TransaccionLedger
- AuditoriaLog

También contiene enums y excepciones propias del dominio.

Domain no debe conocer detalles técnicos como:

- Controllers
- API
- Entity Framework Core
- DbContext
- SQL Server
- Repositories concretos

De esta manera, el núcleo del negocio queda separado de la infraestructura.

## Application

Application contiene los casos de uso y procesa las operaciones solicitadas por la API.

Acá se encuentran:

- Commands
- Queries
- Handlers
- DTOs
- interfaces de persistencia
- interfaces de servicios

La lógica de los casos de uso se encuentra principalmente en los Handlers.

Por ejemplo, cuando llega una puja, el Handler correspondiente verifica las reglas necesarias y solicita a las interfaces de persistencia los datos que necesita.

Application sabe **qué necesita hacer**, pero no necesita saber cómo SQL Server guarda los datos.

## Infrastructure

Infrastructure contiene la implementación técnica.

Acá se encuentran:

- AppDbContext
- Entity Framework Core
- configuraciones de entidades
- Repositories concretos
- Unit of Work
- acceso a SQL Server
- Migrations
- Seed

Por ejemplo:

```text
Application
    ISubastaRepository

Infrastructure
    SubastaRepository
        AppDbContext
            EF Core
                SQL Server
```

Application trabaja con la interfaz y Infrastructure implementa el acceso real a los datos.

## Api / Presentation

Es la entrada y salida HTTP del Backend.

Contiene principalmente:

- Controllers
- Middleware
- Program.cs
- configuración de Swagger
- configuración de CORS
- configuración de Dependency Injection
- configuración del Worker

Los Controllers se mantienen simples.

Su responsabilidad principal es:

```text
recibir request
    ↓
enviar la operación a Application
    ↓
recibir resultado
    ↓
devolver response HTTP
```

Las reglas del negocio no se deciden en los Controllers.

---

# 3. Flujo general del sistema

El recorrido general de una operación es:

```text
Usuario
    ↓
Frontend
    ↓
HTTP
    ↓
API / Controller
    ↓
Command o Query
    ↓
Handler
    ↓
Interface de Application
    ↓
Repository de Infrastructure
    ↓
AppDbContext / Entity Framework Core
    ↓
SQL Server
```

Luego el resultado vuelve hacia el Frontend y se muestra al usuario.

Domain participa mediante las entidades, enums y reglas que corresponden al negocio, pero no funciona como un paso HTTP del recorrido.

---

# 4. CQRS

Application utiliza CQRS para separar las operaciones que modifican información de las operaciones que solamente consultan datos.

## Command

Un Command representa una operación que modifica información.

Ejemplos:

- crear una subasta;
- realizar una puja;
- depositar saldo.

## Query

Una Query representa una operación que consulta información.

Ejemplos:

- obtener una subasta;
- listar subastas;
- consultar una billetera;
- consultar movimientos.

La idea simple es:

```text
Command = modificar
Query   = consultar
```

---

# 5. Handlers

Los Commands y Queries son procesados por Handlers.

El Handler es quien ejecuta el caso de uso.

Por ejemplo, en una operación de puja:

```text
Request
    ↓
Controller
    ↓
Command
    ↓
Handler
    ↓
validaciones y reglas
    ↓
Repositories / Unit of Work
    ↓
Base de Datos
```

Esto evita colocar la lógica del caso de uso dentro del Controller.

---

# 6. DTOs

Los DTOs se utilizan para transportar solamente los datos necesarios entre las distintas partes de la aplicación.

No es necesario devolver directamente todas las propiedades internas de una entidad de Domain.

En forma simple:

```text
Command / Query = datos que entran
DTO             = datos que devolvemos
```

---

# 7. Vertical Slice

Dentro de Application los archivos se organizan principalmente por funcionalidad o caso de uso.

Ejemplos reales del proyecto:

```text
Application/
└── UseCases/
    ├── Billetera/
    │   ├── DepositarSaldo/
    │   ├── ObtenerBilletera/
    │   └── ObtenerTransacciones/
    │
    └── Subastas/
        ├── CrearSubasta/
        ├── ObtenerSubastas/
        ├── ObtenerSubastaPorId/
        ├── ObtenerPujas/
        ├── ObtenerPujasPorComprador/
        ├── ObtenerSubastasPorVendedor/
        ├── Pujar/
        └── ProcesarSubastas/
```

Esto permite encontrar rápidamente todos los archivos relacionados con una funcionalidad.

Vertical Slice organiza los casos de uso dentro de Application, pero no reemplaza la separación entre Domain, Application, Infrastructure y Api.

---

# 8. Repository

Repository funciona como intermediario entre Application y el acceso concreto a los datos.

Application trabaja con interfaces.

Infrastructure contiene sus implementaciones.

Ejemplo:

```text
Handler
    ↓
ISubastaRepository
    ↓
SubastaRepository
    ↓
AppDbContext
    ↓
SQL Server
```

Esto evita que el Handler tenga que conocer directamente Entity Framework Core o SQL Server.

---

# 9. Entity Framework Core y DbContext

Entity Framework Core funciona como ORM.

Permite relacionar los objetos del código C# con las tablas de la Base de Datos.

En forma simple:

```text
Clase       ↔ Tabla
Propiedad   ↔ Columna
Objeto      ↔ Registro
```

`AppDbContext` es el punto principal mediante el cual EF Core trabaja con la Base de Datos.

Se encuentra dentro de Infrastructure porque el acceso a datos es un detalle técnico y no una responsabilidad de Domain.

---

# 10. Code First y Migrations

El proyecto utiliza el enfoque **Code First**.

Primero se define el modelo mediante código C# y luego Entity Framework Core permite llevar esa estructura a la Base de Datos.

Las migraciones se encuentran en:

```text
Infrastructure/Migrations
```

Una Migration representa los cambios necesarios para llevar el modelo definido en código a la estructura de la Base de Datos.

---

# 11. Dependency Injection

El sistema utiliza Inyección de Dependencias.

Los Handlers solicitan las interfaces que necesitan en lugar de crear directamente las implementaciones.

Por ejemplo:

```text
ISubastaRepository
        ↓
SubastaRepository
```

Esta relación se configura en `Api/Program.cs`.

De esta manera, Application puede depender de una abstracción y Infrastructure contiene la implementación técnica.

---

# 12. Unit of Work y transacciones

Las operaciones críticas pueden necesitar modificar varios datos relacionados.

Por ejemplo, una puja puede involucrar:

- guardar una nueva puja;
- modificar una billetera;
- retener dinero;
- liberar una retención anterior;
- registrar movimientos;
- modificar datos de la subasta.

No queremos que solamente una parte de esa operación quede guardada.

Por eso se utiliza Unit of Work y transacciones.

La idea es:

```text
todo sale correctamente
        ↓
se confirma

algo falla
        ↓
se revierte la operación
```

Esto permite mantener consistentes los datos.

---

# 13. ACID

Las operaciones críticas buscan respetar las propiedades ACID.

## Atomicidad

La operación se completa completamente o no queda realizada a medias.

## Consistencia

La Base de Datos debe quedar cumpliendo sus reglas antes y después de la operación.

## Aislamiento

Las operaciones simultáneas no deben interferir incorrectamente entre sí.

## Durabilidad

Una vez confirmada una operación, los cambios deben permanecer guardados.

Esto es especialmente importante en las pujas, retenciones, liberaciones y cierres de subastas.

---

# 14. REST Nivel 2

La API utiliza recursos en las rutas y métodos HTTP para representar las acciones.

Por ejemplo, para crear una puja se utiliza el recurso correspondiente a las pujas de una subasta, en lugar de colocar verbos innecesarios en la URL.

La idea es:

```text
GET    = consultar
POST   = crear o iniciar una operación
PUT    = actualizar/reemplazar
PATCH  = modificar parcialmente
DELETE = eliminar
```

Los Controllers reales del proyecto son:

```text
BilleteraController
PujasController
SubastasController
```

---

# 15. Códigos HTTP

La API utiliza códigos HTTP para indicar cómo terminó una petición.

Entre los principales utilizados por el sistema se encuentran:

```text
200 OK
Operación o consulta realizada correctamente.

201 Created
Se creó correctamente un recurso.

400 Bad Request
La petición es inválida o no cumple una regla necesaria.

404 Not Found
El recurso solicitado no existe.

409 Conflict
Existe un conflicto con el estado actual, especialmente en escenarios de concurrencia.

500 Internal Server Error
Ocurrió un error inesperado.
```

---

# 16. Middleware y manejo centralizado de errores

El proyecto utiliza un Middleware para centralizar el manejo de excepciones.

Esto evita repetir bloques `try/catch` en todos los Controllers.

El flujo general es:

```text
Handler detecta un problema
        ↓
lanza una excepción
        ↓
Middleware la captura
        ↓
la transforma en una respuesta HTTP
        ↓
Frontend recibe el error
```

De esta manera, los Controllers pueden mantenerse simples.

---

# 17. Funcionalidades de Subastas

El sistema permite trabajar con las publicaciones de subastas.

Entre las funcionalidades implementadas se encuentran:

- crear una subasta;
- listar subastas;
- consultar una subasta por ID;
- consultar puja actual;
- consultar cantidad de pujas;
- filtrar publicaciones;
- ordenar publicaciones;
- consultar publicaciones de un vendedor;
- manejar distintos estados de una subasta.

Los estados utilizados por el sistema son:

```text
PROGRAMADA
ACTIVA
FINALIZADA
DESIERTA
```

---

# 18. Crear Subasta

El usuario puede publicar una nueva subasta desde el Frontend.

La publicación contiene información como:

- vendedor;
- categoría;
- título;
- descripción;
- imagen;
- precio base;
- incremento mínimo;
- fecha de inicio;
- fecha de finalización.

Entre las validaciones principales se comprueba que:

- el precio base sea positivo;
- el incremento mínimo sea positivo;
- la fecha de finalización sea posterior a la fecha de inicio.

Una nueva publicación puede comenzar como `PROGRAMADA` según sus fechas.

---

# 19. Catálogo de subastas

La pantalla principal muestra el catálogo de publicaciones.

El usuario puede consultar las subastas y trabajar con filtros y ordenamientos.

Las tarjetas muestran la información importante de cada publicación, incluyendo datos como:

- título;
- categoría;
- imagen;
- valor actual;
- cantidad de pujas;
- tiempo restante;
- estado.

Desde el catálogo se puede ingresar a la sala correspondiente a una subasta.

---

# 20. Pujas

Un comprador puede realizar una oferta sobre una subasta activa.

El Backend es responsable de validar la operación.

Entre las reglas importantes se encuentran:

- la subasta debe existir;
- debe encontrarse en un estado que permita pujar;
- la oferta debe respetar el monto mínimo correspondiente;
- el comprador debe contar con saldo disponible;
- los fondos deben quedar correctamente respaldados;
- los cambios relacionados deben realizarse de manera consistente.

La validación importante siempre vuelve a realizarse en el Backend.

El Frontend puede ayudar al usuario mostrando información o deshabilitando botones, pero no decide por sí solo si una puja es válida.

---

# 21. Solvencia y retención de fondos

Una persona no puede pujar dinero que no tiene disponible.

Cuando un comprador pasa a liderar una subasta, el dinero correspondiente debe quedar respaldado por su billetera.

Por eso la billetera diferencia:

```text
Saldo Total
Saldo Retenido
Saldo Disponible
```

El saldo retenido representa dinero que sigue perteneciendo al usuario pero está reservado para respaldar una operación.

Cuando otro comprador supera una oferta, el sistema debe manejar correctamente la liberación de la retención anterior y la nueva retención correspondiente.

Estos cambios forman parte de una operación crítica y deben mantenerse consistentes.

---

# 22. Billetera

Cada usuario de prueba posee una billetera.

Desde la vista de billetera se puede consultar:

- saldo total;
- saldo retenido;
- saldo disponible;
- historial de movimientos.

También existe una acreditación manual simulada para agregar saldo durante las pruebas del sistema.

Cuando se realiza un depósito:

- aumenta el saldo total;
- se recalcula el saldo disponible;
- se registra un movimiento en el ledger;
- se registra la auditoría correspondiente.

---

# 23. Ledger

El sistema utiliza `TransaccionLedger` para registrar los movimientos relacionados con las billeteras.

Los tipos de movimiento contemplados por el dominio son:

```text
DEPOSITO
RETENCION
LIBERACION
PAGO
COBRO
```

El ledger permite conservar el historial de movimientos y entender por qué una billetera tiene determinado saldo total, retenido y disponible.

---

# 24. Historial de pujas

La sala permite consultar el historial de ofertas de una subasta.

Para no exponer directamente la identidad interna de los compradores, el historial público utiliza identificadores o seudónimos del estilo:

```text
Postor 1
Postor 2
```

Cada oferta muestra además:

- monto;
- fecha y hora.

---

# 25. Sala de subasta

La sala concentra la interacción principal durante una subasta.

Muestra información como:

- título;
- estado;
- puja actual;
- cantidad de ofertas;
- incremento mínimo;
- próxima puja sugerida;
- cuenta regresiva;
- historial de pujas.

El usuario puede realizar una oferta sugerida o ingresar un monto válido superior.

La interfaz también informa diferentes situaciones:

- procesando una puja;
- puja realizada correctamente;
- usuario liderando;
- usuario superado por otra oferta;
- saldo insuficiente;
- activación del anti-sniping;
- subasta programada;
- subasta finalizada;
- subasta desierta.

---

# 26. Actualización de la sala

La información de la sala necesita mantenerse actualizada mientras otros usuarios pueden estar pujando.

Para esto el Frontend realiza actualizaciones periódicas consultando nuevamente la API.

De esta forma se pueden actualizar datos como:

- puja actual;
- cantidad de ofertas;
- historial;
- estado de liderazgo;
- tiempo y estado de la subasta.

El Backend continúa siendo la fuente de verdad de la operación.

---

# 27. Anti-sniping

SubastaYa implementa una regla de anti-sniping para evitar que una oferta realizada justo antes del cierre impida que los demás participantes puedan responder.

Cuando una puja válida entra dentro de los últimos **60 segundos**, la fecha de finalización se extiende **2 minutos**.

La modificación de la fecha se realiza en el Backend.

El Frontend informa al usuario cuando se produjo esta extensión.

---

# 28. UTC y manejo de fechas

El Backend utiliza UTC como referencia consistente para las fechas.

Se utiliza:

```text
DateTime.UtcNow
```

Esto evita depender de la zona horaria configurada en la computadora donde se ejecuta la aplicación.

La idea es mantener una única referencia temporal dentro del sistema y realizar las conversiones necesarias solamente al mostrar la información.

---

# 29. Concurrencia optimista

En una subasta pueden llegar operaciones prácticamente al mismo tiempo.

El sistema utiliza concurrencia optimista para detectar modificaciones conflictivas.

Las entidades críticas utilizan una propiedad `Version` configurada como token de concurrencia.

Si Entity Framework Core detecta que otro proceso modificó el mismo estado antes de confirmar una operación, se produce un conflicto de concurrencia.

El sistema contempla `DbUpdateConcurrencyException`, la transforma en una excepción propia de concurrencia y la API puede responder:

```text
409 Conflict
```

Esto evita ignorar silenciosamente una colisión entre operaciones simultáneas.

## Prueba de concurrencia

Antes de la entrega final se ejecuta una prueba enviando múltiples solicitudes de puja de manera simultánea sobre la misma subasta.

El objetivo es comprobar dos cosas:

1. que solamente una modificación incompatible pueda confirmarse;
2. que los conflictos de concurrencia sean detectados y respondidos correctamente.

El resultado exacto de la prueba final debe registrarse después de ejecutarla sobre la Base de Datos limpia.

---

# 30. Worker y cierre automático

Las subastas pueden vencer aunque ningún usuario esté realizando una petición.

Por eso el sistema tiene un proceso automático en segundo plano:

```text
ProcesarSubastasWorker
```

El Worker utiliza el caso de uso:

```text
ProcesarSubastasHandler
```

Su función es revisar y procesar las subastas que necesitan un cambio automático de estado.

Esto permite que el cierre no dependa de que un usuario entre a la página o presione un botón.

---

# 31. Subasta finalizada con ganador

Cuando una subasta vencida posee una puja ganadora, el procesamiento automático debe realizar las operaciones correspondientes al cierre.

El escenario incluye el manejo de los fondos que estaban retenidos y los movimientos necesarios para representar la operación terminada.

El seed contiene un escenario preparado específicamente para comprobar este comportamiento.

---

# 32. Subasta desierta

Una subasta vencida que no recibió ofertas puede finalizar como:

```text
DESIERTA
```

Este escenario también se encuentra representado entre los datos iniciales de prueba.

---

# 33. Auditoría

El sistema posee `AuditoriaLog` para registrar eventos importantes.

La auditoría permite conservar información sobre operaciones críticas y situaciones relevantes del sistema.

Entre los eventos que pueden requerir auditoría se encuentran:

- acreditaciones manuales;
- cambios importantes de estado;
- extensiones producidas por anti-sniping;
- operaciones rechazadas por situaciones críticas;
- conflictos de concurrencia.

La auditoría funciona como registro de lo ocurrido y no como sustituto de una operación válida.

Una operación rechazada no debe modificar los datos de negocio como si hubiera sido aceptada.

---

# 34. CORS

Frontend y Backend son aplicaciones separadas.

Por seguridad, el navegador puede bloquear una petición cuando los orígenes son diferentes.

La API configura CORS para permitir los orígenes locales utilizados por el Frontend.

Actualmente se encuentran permitidos:

```text
http://localhost:5500
http://127.0.0.1:5500
http://localhost:5501
http://127.0.0.1:5501
```

La configuración se realiza en `Api/Program.cs`.

---

# 35. Frontend

El Frontend se encuentra en:

```text
frontend/
```

Las vistas principales son:

```text
index.html
login.html
publicar.html
sala.html
billetera.html
actividades.html
```

Los archivos JavaScript principales son:

```text
actividades.js
api.js
app.js
billetera.js
catalogo.js
login.js
publicar.js
sala.js
```

El Frontend consume la API mediante JavaScript y Fetch.

No accede directamente a SQL Server.

El recorrido es siempre:

```text
Frontend
    ↓
API
    ↓
Backend
    ↓
Base de Datos
```

---

# 36. Login de prueba

El proyecto posee una pantalla de ingreso utilizada para seleccionar los usuarios de prueba y permitir recorrer las distintas funcionalidades del Frontend.

La identidad seleccionada se utiliza desde el Frontend para representar al usuario durante las pruebas.

No se implementó un sistema completo de autenticación porque no forma parte de las funcionalidades centrales requeridas para este Trabajo Práctico.

Las reglas críticas siguen siendo validadas por el Backend.

---

# 37. Mi Billetera

La vista `billetera.html` permite visualizar de manera simple:

- saldo total;
- saldo retenido;
- saldo disponible;
- movimientos;
- acreditación simulada de saldo.

Los valores mostrados se obtienen desde la API.

---

# 38. Mis Actividades

La vista `actividades.html` permite consultar la actividad del usuario.

Incluye información relacionada con:

- actividad como comprador;
- pujas realizadas;
- publicaciones realizadas como vendedor;
- estado de participación en las subastas.

Esta información se obtiene desde los casos de uso correspondientes del Backend.

---

# 39. Experiencia de usuario

El Frontend fue diseñado para informar claramente qué está ocurriendo.

Se contemplan estados como:

- carga de información;
- operación en proceso;
- operación correcta;
- error;
- saldo insuficiente;
- usuario liderando;
- usuario superado;
- subasta todavía no iniciada;
- subasta activa;
- subasta finalizada;
- subasta desierta;
- extensión por anti-sniping.

La interfaz ayuda al usuario, pero las reglas importantes continúan siendo responsabilidad del Backend.

---

# 40. Base de Datos

SubastaYa utiliza SQL Server.

La conexión se configura mediante:

```text
DefaultConnection
```

Entity Framework Core se encarga del mapeo entre las entidades C# y las tablas.

Las configuraciones de EF Core se encuentran dentro de Infrastructure.

---

# 41. Seed

En ambiente Development, al iniciar la API se ejecuta el seed mediante:

```text
await app.Services.SeedAsync();
```

El seed permite contar con escenarios conocidos para probar el sistema.

Incluye:

- usuarios;
- billeteras;
- categorías;
- subastas;
- pujas;
- movimientos del ledger.

---

# 42. Usuarios iniciales

El TP utiliza cuatro usuarios principales.

## Vendedor

```text
Email: vendedor@test.com
Saldo inicial: $0
```

## Comprador 1

```text
Email: comprador1@test.com
Saldo total: $150.000
Saldo retenido: $45.000
Saldo disponible: $105.000
```

## Comprador 2

```text
Email: comprador2@test.com
Saldo total: $200.000
Saldo retenido: $0
Saldo disponible: $200.000
```

## Usuario sin fondos suficientes

```text
Email: sinfondos@test.com
Saldo total: $500
Saldo retenido: $0
Saldo disponible: $500
```

---

# 43. Usuario auxiliar del Worker

Además de los cuatro usuarios principales existe:

```text
compradorworker@test.com
```

Este usuario es auxiliar y se utiliza específicamente para preparar el escenario de una subasta vencida con ganador.

Su billetera inicial contiene:

```text
Saldo total: $3.000
Saldo retenido: $3.000
Saldo disponible: $0
```

Posee una puja de `$3.000` respaldada por esa retención.

Esto permite probar el cierre y liquidación automática de una subasta con ganador sin modificar los escenarios de saldo preparados para los cuatro usuarios principales.

---

# 44. Categorías iniciales

El seed contiene cuatro categorías:

```text
Tecnología
Coleccionables
Indumentaria
Vehículos
```

---

# 45. Subastas iniciales

El seed prepara distintos escenarios.

## Activa estándar

Subasta activa utilizada para probar el flujo normal.

Contiene dos pujas iniciales y una oferta líder de:

```text
$45.000
```

## Activa crítica

Subasta que comienza cerca del cierre para poder probar el comportamiento de anti-sniping.

## Programada

Subasta cuya fecha de inicio todavía no llegó.

Permite probar el estado:

```text
PROGRAMADA
```

## Vencida con ganador

Posee una oferta respaldada por la billetera del usuario auxiliar.

Permite comprobar el procesamiento automático del Worker.

## Vencida sin ofertas

Permite comprobar el cierre como:

```text
DESIERTA
```

---

# 46. Pujas iniciales

La subasta activa estándar contiene dos pujas iniciales.

La última corresponde al comprador que lidera con:

```text
$45.000
```

Además existe una puja asociada al escenario de la subasta vencida con ganador.

---

# 47. Ledger inicial

El seed crea movimientos necesarios para respaldar los saldos iniciales.

Incluye depósitos y retenciones.

Entre ellos se encuentra la retención de:

```text
$45.000
```

correspondiente al comprador que lidera la subasta activa estándar.

También existe la retención del usuario auxiliar utilizada por el escenario del Worker.

---

# 48. Swagger / OpenAPI

Durante el desarrollo, Swagger permite consultar y probar los endpoints expuestos por la API.

Swagger se habilita cuando la aplicación se ejecuta en ambiente Development.

La configuración se encuentra en:

```text
Api/Program.cs
```

---

# 49. Cómo preparar la Base de Datos

El proyecto utiliza SQL Server.

Primero se debe comprobar que la cadena `DefaultConnection` de la API apunte a una instancia disponible de SQL Server.

Desde la raíz del repositorio se pueden aplicar las migraciones con:

```powershell
dotnet ef database update --project Infrastructure --startup-project Api
```

Al iniciar la API en Development se ejecuta el seed para crear los datos iniciales que todavía no existan.

---

# 50. Cómo compilar el proyecto

Ubicarse en la raíz del repositorio:

```powershell
cd C:\Users\mayra\source\repos\Maia-Frontend
```

Ejecutar:

```powershell
dotnet build .\SubastaYa.sln
```

La compilación permite comprobar que los proyectos y sus dependencias se encuentran correctamente configurados.

---

# 51. Cómo ejecutar el Backend

Desde la raíz:

```powershell
cd C:\Users\mayra\source\repos\Maia-Frontend
dotnet run --project .\Api\Api.csproj --launch-profile http
```

En el entorno utilizado durante el desarrollo, la API HTTP se ejecuta en:

```text
http://localhost:5205
```

---

# 52. Cómo ejecutar el Frontend

El Frontend debe servirse mediante un servidor HTTP local cuyo origen esté permitido por CORS.

Durante el desarrollo se utilizó:

```text
http://localhost:5500
```

La página principal es:

```text
http://localhost:5500/index.html
```

No se recomienda abrir los archivos HTML directamente mediante `file://`, ya que el Frontend necesita comunicarse correctamente con la API.

---

# 53. Estructura general del repositorio

```text
SubastaYa.sln

Api/
├── Controllers/
├── Middleware/
├── Background/
└── Program.cs

Application/
├── DTOs/
├── Interfaces/
└── UseCases/
    ├── Billetera/
    └── Subastas/

Domain/
├── Entities/
├── Enums/
└── Exceptions/

Infrastructure/
├── Migrations/
├── Persistence/
│   └── Configurations/
├── Repositories/
└── Seed/

frontend/
├── index.html
├── login.html
├── publicar.html
├── sala.html
├── billetera.html
├── actividades.html
└── js/
```

---

# 54. Dependencias entre proyectos

La separación busca mantener estas responsabilidades:

```text
Domain
    no depende de la parte técnica

Application
    utiliza Domain

Infrastructure
    implementa las necesidades de Application
    y utiliza Domain

Api
    conecta Application e Infrastructure
```

Que Api conozca Infrastructure para configurar Dependency Injection no significa que los Controllers deban acceder directamente al DbContext o a los Repositories concretos.

---

# 55. Principios aplicados

Durante el desarrollo se priorizó:

- separación de responsabilidades;
- Controllers simples;
- casos de uso en Application;
- entidades de negocio en Domain;
- acceso técnico a datos en Infrastructure;
- interfaces para desacoplar Application de Infrastructure;
- Dependency Injection;
- Repository;
- Unit of Work;
- transacciones;
- CQRS;
- Vertical Slice;
- DTOs;
- REST Nivel 2;
- manejo centralizado de errores;
- concurrencia optimista;
- UTC;
- CORS;
- Worker;
- auditoría;
- código simple y explícito.

El objetivo general es que cada parte tenga una responsabilidad concreta y que el flujo del sistema pueda seguirse y explicarse con facilidad.

---

# 56. Resumen para entender el proyecto

Una forma simple de pensar SubastaYa es:

```text
El usuario usa el Frontend.

El Frontend hace una petición HTTP.

El Controller recibe la petición.

Application ejecuta el caso de uso mediante un Handler.

Si necesita datos, Application pide lo necesario mediante una interfaz.

Infrastructure implementa esa interfaz y trabaja con EF Core.

EF Core utiliza AppDbContext para consultar o modificar SQL Server.

Si la operación tiene varios cambios críticos, se coordinan mediante Unit of Work y transacciones.

El Backend devuelve el resultado.

El Frontend se lo muestra al usuario.
```

Las reglas importantes, como solvencia, retención de fondos, anti-sniping, concurrencia y cierre de subastas, se controlan en el Backend.

---

# 57. Estado previo a la entrega

Antes de realizar la entrega final se debe comprobar:

1. Base de Datos restaurada únicamente a los datos definidos por el seed.
2. API iniciando correctamente.
3. Seed ejecutándose correctamente.
4. Catálogo funcionando.
5. Creación de subastas funcionando.
6. Sala funcionando.
7. Pujas y retenciones funcionando.
8. Saldo insuficiente funcionando.
9. Anti-sniping funcionando.
10. Billetera y depósitos funcionando.
11. Mis Actividades funcionando.
12. Worker procesando los cierres.
13. Auditoría registrando los eventos correspondientes.
14. Prueba final de concurrencia.
15. Compilación completa de `SubastaYa.sln`.
16. Repositorio Git sin archivos accidentales ni cambios pendientes de revisar.

---

# 58. Prueba final de concurrencia

La prueba final de concurrencia se realizó sobre una Base de Datos restaurada desde las migraciones y cargada nuevamente con el seed.

Se utilizó la subasta activa estándar. Antes de comenzar tenía una puja actual de $45.000 y 2 pujas registradas.

Se enviaron 50 solicitudes HTTP de puja prácticamente al mismo tiempo, todas por un monto de $46.000.

El resultado real obtenido fue:

201 Created: 1 solicitud
409 Conflict: 7 solicitudes
400 Bad Request: 42 solicitudes

Los conflictos de concurrencia devolvieron HTTP 409 Conflict con el siguiente mensaje:

"La operación no pudo realizarse porque la subasta o la billetera fue modificada por otra operación."

Después de ejecutar las 50 solicitudes se volvió a consultar la subasta.

Estado inicial:
Puja actual: $45.000
Cantidad de pujas: 2

Estado final:
Puja actual: $46.000
Cantidad de pujas: 3

Por lo tanto, solamente se persistió una nueva puja.

Los 409 Conflict demuestran que el sistema detectó operaciones que intentaron modificar concurrentemente el mismo estado. Las solicitudes que devolvieron 400 Bad Request fueron rechazadas por las validaciones de negocio una vez que la puja válida de $46.000 ya había modificado el estado de la subasta.

Después de esta prueba, la Base de Datos se restaura nuevamente para dejar la entrega con los datos definidos por el seed.

---

# 59. Idea principal del diseño

SubastaYa separa claramente:

```text
Frontend       = interacción con el usuario
Api            = entrada y salida HTTP
Application    = casos de uso y reglas
Domain         = conceptos principales del negocio
Infrastructure = acceso técnico a datos
SQL Server     = persistencia
```

Esta separación permite mantener el código organizado, entender dónde se encuentra cada responsabilidad y evitar mezclar las reglas del sistema con los detalles técnicos.
