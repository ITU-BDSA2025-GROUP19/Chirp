### Source Projects

| Project | Description |
|----------|-------------|
| **Chirp.Domain** | Core entities. |
| **Chirp.Application** | Application-level logic, interfaces, and DTOs. |
| **Chirp.Infrastructure** | Data access layer, EF Core, repositories, and migrations. |
| **Chirp.Razor** | Presentation/UI layer using Razor Pages; entry point of the app. |

### Test Projects

| Project | Purpose |
|----------|----------|
| **Chirp.Razor.Tests** | UI and integration tests using `WebApplicationFactory`. |
| **Chirp.Infrastructure.Tests** | In-memory EF Core repository tests. |

---

## 🧠 Onion Architecture Diagram
              ┌──────────────────────────────┐
              │        Presentation          │
              │        (Chirp.Razor)         │
              └──────────────┬───────────────┘
                             │
              ┌──────────────▼───────────────┐
              │       Application Layer      │
              │     (Chirp.Application)      │
              └──────────────┬───────────────┘
                             │
              ┌──────────────▼───────────────┐
              │         Domain Layer         │
              │        (Chirp.Domain)        │
              └──────────────▲───────────────┘
                             │
              ┌──────────────┴───────────────┐
              │     Infrastructure Layer     │
              │   (Chirp.Infrastructure)     │
              └──────────────────────────────┘
##  Layer Responsibilities

###  Domain Layer (`Chirp.Domain`)
**Purpose:**  
Defines the core *entities* independent of persistence or UI.

**Contains:**
- Entities such as `Author` and `Cheep`
- No dependencies on other projects

**Dependencies:** None  
**Referenced by:** Application, Infrastructure

---

### Application Layer (`Chirp.Application`)
**Purpose:**  
Acts as a *bridge* between the UI and the domain model.  
Defines DTOs, and interfaces for repositories and services.

**Contains:**
- DTOs (e.g., `CheepDto`)
- Service interfaces (e.g., `ICheepService`)
- Application logic coordinating between repositories and UI

**Depends on:** Domain  
**Referenced by:** Razor, Infrastructure

---

### Infrastructure Layer (`Chirp.Infrastructure`)
**Purpose:**  
Implements all persistence and external system logic 

**Contains:**
- `ChirpDbContext` (EF Core)
- Repository implementations (e.g., `CheepRepository`, `AuthorRepository`)
- `Migrations` folder for EF Core schema updates
- `Services` that implement application interfaces

**Depends on:** Domain, Application  
**Referenced by:** Razor (via dependency injection)

---

### Presentation Layer (`Chirp.Razor`)
**Purpose:**  
Implements the *user interface* using Razor Pages.  
It is the outermost layer that interacts with users.

**Contains:**
- `Pages/` folder with Razor UI
- `Program.cs` (entry point)
- Dependency injection configuration
- References only service interfaces (not repositories directly)

**Depends on:** Application, Infrastructure  
**Referenced by:** None

---

## 🔗 Dependency Direction

| From | Depends On | Description |
|------|-------------|-------------|
| **Razor (UI)** | Application, Infrastructure | Uses service interfaces and repository implementations. |
| **Infrastructure** | Application, Domain | Implements interfaces and accesses domain entities. |
| **Application** | Domain | Contains service contracts using domain entities. |
| **Domain** | — | Independent core business logic. |

**Rule:**  
> Dependencies always point inward — from UI → Application → Domain.  
> The Infrastructure layer supports these layers by implementing contracts 
