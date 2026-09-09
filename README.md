# 📚 Timely — Student Time Manager
 
**Planificador académico web** construido con **ASP.NET Core MVC (.NET 8)** que ayuda a los estudiantes a organizar tareas, proyectos, notas y eventos de calendario en un solo lugar, con autenticación de usuarios y control de roles.
 
## 📑 Tabla de contenidos
 
- [Descripción](#-descripción)
- [Funcionalidades](#-funcionalidades)
- [Arquitectura y stack técnico](#-arquitectura-y-stack-técnico)
- [Estructura del proyecto](#-estructura-del-proyecto)
- [Requisitos previos](#-requisitos-previos)
- [Instalación y ejecución](#-instalación-y-ejecución)
- [Configuración](#-configuración)
- [Roadmap](#-roadmap)
- [Autor](#-autor)
  
## 🧭 Descripción
 
Timely nace como respuesta a un problema común entre estudiantes: la dificultad para organizar tareas, proyectos y fechas límite entre distintas materias. La aplicación centraliza esa información en un panel único, con vistas dedicadas para proyectos académicos, notas personales y un calendario interactivo de eventos.
 
El proyecto está desarrollado siguiendo el patrón **MVC** con una **capa de servicios desacoplada mediante interfaces** (inyección de dependencias), lo que facilita las pruebas, el mantenimiento y la futura extensión de funcionalidades.
 
## ✨ Funcionalidades
 
- 🔐 **Autenticación y roles** — Login basado en cookies, con políticas de autorización (p. ej. rol `Administrador`) y expiración de sesión configurable.
- ✅ **Gestión de proyectos** — Tablero con creación, edición y eliminación de proyectos académicos; cálculo automático de estado (`En proceso`, `Hecho`, `Vencido`) según fechas de vencimiento.
- 📝 **Notas** — CRUD completo de notas organizadas por carpetas.
- 📅 **Calendario interactivo** — Eventos con fecha de inicio/fin y descripción, integrados vía **FullCalendar**, con endpoints JSON para crear, editar y eliminar eventos de forma asíncrona.
- 👤 **Gestión de usuarios** — Alta, edición, listado y perfil de usuario, con validación de datos (correo, contraseña, confirmación).
## 🛠 Arquitectura y stack técnico
 
| Categoría | Tecnología |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Lenguaje | C# |
| Acceso a datos | Entity Framework Core 8 + SQL Server (LocalDB) |
| Autenticación | Cookie Authentication + Authorization Policies |
| Frontend | Razor Views (.cshtml), FullCalendar, CSS/JS |
| Patrón | MVC con capa de **Services** e **Interfaces** (inyección de dependencias) |
 
**Decisiones de diseño destacadas:**
- Separación de responsabilidades entre `Controllers`, `Services` (lógica de negocio) y `Data` (acceso a datos vía `ApplicationDbContext`).
- Cada servicio expone una interfaz (`IProyectoService`, `INotaService`, `IUsuarioService`, `ICalendarioService`), registrada en el contenedor de dependencias de `Program.cs`.
- Uso de migraciones de EF Core para versionar el esquema de base de datos.
## 📂 Estructura del proyecto
 
```
Timely/
├── Controllers/        # Controladores MVC (Proyectos, Notas, Calendarios, Usuarios, Home)
├── Data/                # DbContext y configuración de EF Core
├── Migrations/          # Migraciones de base de datos
├── Models/              # Entidades del dominio
├── Services/
│   └── Interfaces/      # Contratos de servicios (inyección de dependencias)
├── Views/                # Vistas Razor organizadas por controlador
├── wwwroot/              # Recursos estáticos (CSS, JS, librerías)
├── Program.cs            # Configuración de la aplicación (DI, auth, middlewares)
└── appsettings.json       # Configuración (cadena de conexión, logging)
```
 
## ✅ Requisitos previos
 
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (incluido con Visual Studio) o una instancia de SQL Server
- Visual Studio 2022 
## 🚀 Instalación y ejecución
 
```bash
# 1. Clonar el repositorio
git clone https://github.com/JDanhy07/student-time-manager.git
cd student-time-manager/Timely
 
# 2. Restaurar dependencias
dotnet restore
 
# 3. Aplicar migraciones para crear la base de datos
dotnet ef database update
 
# 4. Ejecutar la aplicación
dotnet run
```
 
La aplicación quedará disponible por defecto en `https://localhost:5001` (o el puerto indicado en la consola).
 
## ⚙️ Configuración
 
La cadena de conexión se define en `appsettings.json`:
 
```json
"ConnectionStrings": {
  "Timely": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Timely;Integrated Security=True;"
}
```
 
Ajusta este valor según tu entorno (por ejemplo, para usar una instancia de SQL Server distinta a LocalDB). Para entornos de producción, se recomienda mover credenciales sensibles a variables de entorno o a un gestor de secretos (`dotnet user-secrets`).
 
## 🚧 Próximas mejoras
 
- [ ] Pruebas unitarias para la capa de servicios
- [ ] Notificaciones/recordatorios de tareas próximas a vencer
- [ ] Mejoras en la UI/UX del proyecto
## 👤 Autor
 
**JDanhy07**
[GitHub](https://github.com/JDanhy07)
 

