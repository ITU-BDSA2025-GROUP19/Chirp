---
title: _Chirp!_ Project Report
subtitle: ITU BDSA 2025 Group 19
author:
- "Hans Voldby Larsen <havl@itu.dk>"

numbersections: true
---

# Design and Architecture of _Chirp!_

## Domain model

The domain model is a consists of the classes in the core domain of interest, and the relationships between them. The domain model is visually shown below as a UML diagram. Only the core entities are shown so other classes of the app concerning eg. infrastructure or web pages are not part of the domain model.

The core domain consists of the entities Author, Cheep and Follow, where an Author can publish multiple Cheeps and each Cheep is authored by exactly one Author.

Authors can follow other authors, which is modeled through the Follow entity representing the follower–followee relationship.

Authentication and authorization are handled via ASP.NET Identity and are therefore not part of the core domain model. The attribute ApplicationUserId in the class Author (which is used to connect a user indentity with an Author) is therefore not included in the Domain model.

![Illustration of the _Chirp!_ data model as UML class diagram.](images/Domain model Class diagram.png)

## Architecture — In the small

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