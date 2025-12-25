---
title: _Chirp!_ Project Report
subtitle: ITU BDSA 2025 Group 19
author:
- "Hans Voldby Larsen <havl@itu.dk>"

numbersections: true
---

# Design and Architecture of _Chirp!_

## Domain model

The domain model is a consists of the classes in the core domain of interest, and the relationships between them. The domain model is visually shown in figure 1 as a UML diagram. Only the core entities are shown so other classes of the app concerning eg. infrastructure or web pages are not part of the domain model.

The core domain consists of the entities Author, Cheep and Follow, where an Author can publish multiple Cheeps and each Cheep is authored by exactly one Author.

Authors can follow other authors, which is modeled through the Follow entity representing the follower–followee relationship.

Authentication and authorization are handled via ASP.NET Identity and are therefore not part of the core domain model. The property ApplicationUserId in the class Author (which is used to connect a user indentity with an Author) is therefore not included in the Domain model.

![Illustration of the _Chirp!_ data model as UML class diagram.](images/Domain-model-Class-diagram.png)

## Architecture — In the small
Figure 2 illustrates the internal architecture of the Chirp! application.
The Solution follows a layered architecture with a clear separation between **domain**, **application**, **infrastructure** and **presentation**.

Figure 3 complements the onion architecture overview by showing a UML package diagram that maps the architectural layers to the concrete project structure and illustrates the dependency relationships between them. All dependencies between classes and inhertances are not shown to avoid the illustration to become too overcrowded. 

\begin{figure}[htbp]
  \centering
  \includegraphics[width=0.5\textwidth]{images/Onion-Architecture-of-Chirp.png}
  \caption{Illustration of the Chirp! onion Archhitecture.}
\end{figure}

### Domain layer
The domain layer (project Chirp.Domain) contains the core domain entities and is completely independent of other layers. The class Author in the domain has a property called ApplicationUserId and is used to associate a domain author with an authenticated user. This property is represented as a primitive value and does not introduce a dependency on the authentication framework.

### Appliction layer
The application layer (project Chirp.Application) defines service interfaces and operates exclusively on data transfer objects (DTOs). 

The service interfaces  define the operations that the presentation layer can use to interact with the application’s functionality. For example, the ICheepService interface specifies methods for retrieving cheeps in different contexts, such as paginated lists of cheeps, cheeps authored by a specific author, cheeps associated with a user, and cheeps forming a user timeline. By letting the presentation layer depend abstract interfaces and not concrete implementations of these services, the presentation layer remains decoupled from the underlying infrastructure, allowing service implementations to be replaced or modified without affecting higher layers.  

Data Transfer Objects (DTOs) are used to decouple the application and presentation layers from the domain model by exposing only the data required for a specific use case.

![Illustration of the _Chirp!_ Archhitecture.](images/Chirp-package-diagram.png)

### Infrastructure layer
The infrastructure layer (project Chirp.Infrastructure) is responsible for  data persistence and contains repositories which encapsulate database access logic and isolate the rest of the application from the Entity Framework Core–specific details. Furthermore it provides the concrete implementations of the service interfaces defined in the application layer.

The data persistence infrastructure is centered around the ChirpDbContext, which serves as the Entity Framework Core database context and defines the mapping between the domain entities and the underlying relational database. The ChirpDbContext is kept lightweight and contains only the aggregate entities from the domain layer (Author, Cheep, and Follow).

The ChirpDbContext inherits from IdentityDbContext<IdentityUser>, which integrates ASP.NET Identity into the persistence layer and is responsible for managing the database tables related to authentication and authorization (e.g., user accounts, roles, and claims). 

Supporting components such as database migrations and the DbInitializer are included to manage schema evolution and initial data seeding.

The service implementations (e.g., AuthorService, CheepService, FollowService, and AiFactCheckService) implements the behavior specified by the corresponding application-layer interfaces. These services coordinate domain entities and repositories to fulfill application use cases while remaining hidden behind the abstractions defined in the application layer.

### Presentation layer
The presentation layer (project Chirp.Razor) contains all components related to user interaction and request handling. It includes the Razor Pages that render the web-based user interface, the application entry point (Program.cs) where the system is configured and composed, and the authentication-related pages generated through ASP.NET Identity scaffolding. Together, these components are responsible for presenting data to users, handling input, and delegating application logic to the underlying layers.

Ideally, the presentation layer should depend only on service interfaces and data transfer objects (DTOs) defined in the application layer. In practice, some Razor Pages also reference domain entities directly, resulting in limited direct dependencies on the domain layer. While this slightly weakens the intended separation, the overall responsibility boundaries between layers remain clear.

#### Program.cs

The Program.cs file defines how the system is assembled at runtime. It is responsible for configuring dependency injection, registering services, setting up authentication and authorization.

Within Program.cs, application-layer service interfaces are bound to their concrete infrastructure-layer implementations, and repositories, database contexts, and identity services are registered with the dependency injection container. The file also configures external authentication via GitHub OAuth and integrates ASP.NET Identity for user management. Also authentication, authorization, static file handling, and session management is configured here. By centralizing these concerns, Program.cs keeps configuration and wiring separate from application logic and presentation concerns.

#### Custom Razor Pages

The custom Razor Pages developed for Chirp! implement the core user-facing functionality of the application. These pages handle displaying timelines and creating new cheeps. Each page is responsible for handling HTTP requests, invoking application-layer services, and rendering the resulting data in the user interface.

The Razor Pages primarily interact with the application layer through service interfaces such as ICheepService, IAuthorService, and IFollowService, which encapsulate application logic and data access. In some cases, domain entities are accessed directly from the presentation layer, reflecting pragmatic trade-offs made during development. Nevertheless, the overall flow of control remains from the presentation layer toward the application and infrastructure layers.

#### Razor Pages Generated via ASP.NET Identity Scaffolding

In addition to the custom pages, the presentation layer contains Razor Pages generated using ASP.NET Identity scaffolding. These pages are organized under the Areas/Identity structure and provide functionality related to authentication and account management, including login, logout, registration, and access control.

These scaffolded pages rely on the ASP.NET Identity framework and operate largely independently of the application’s domain and application layers. By using the standard scaffolding approach, authentication and authorization concerns are handled using well-established framework components, reducing the need for custom implementation while keeping identity-related functionality isolated from the core application logic.

The "About Me" Page is a custom made Page but is put together with the Identity Pages since it has a tight connection to the user account.


## Architecture of deployed application


## User activities

## Sequence of functionality/calls trough _Chirp!_

# Process

## Build, test, release, and deployment

## Team work

## How to make _Chirp!_ work locally

## How to run test suite locally

# Ethics

## License

## LLMs, ChatGPT, CoPilot, and others