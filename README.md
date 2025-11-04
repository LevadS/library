# LevadS: mediator on steroi... topics.

*Decouple your logic. Fine-tune your flows.*

# What is LevadS?

LevadS (pronounced "*levadas*") is a lightweight mediator library for .NET, designed to facilitate message-based, topic-driven communication within applications. It enables developers to implement the Mediator pattern, promoting loose coupling and separation of concerns by allowing different parts of an application to communicate through messages without direct dependencies.

Topics, concept known from multiple message brokers, are first-class citizens in LevadS. They enable more granular control over message routing and handling. However, they are completely optional and can be used at the developer's discretion.

LevadS comes with minimal dependencies, is designed for easy integration and follows modern patterns (notably Minimal APIs' way of registration).

Supports .NET 8.0+.

## Openness as a principle

Everything can be a message / request

Use delegates or classes - it's up to you

Use topics or not - it's up to you

Register manually or automatically - it's up to you

Use Dependency Injection everywhere

## Versatility as a goal

Each message is sent with topic and set of headers

Each handler and filter can specify topic pattern to match

Handlers and filters can be generic and support message variance

Filters can be scoped to specific handlers

Autodiscovery of handlers / filters is possible via attributes

Handlers can be registered and disposed at runtime

## Four ways to dispatch a message

**Send** a message to be handled once

**Publish** a message to be handled by many

**Request** using message that expects a response

**Stream** using message that expects a stream of responses

## Filtering pipeline

Two stages of processing = two types of filters:

1. **Dispatch filters** are to gather context and set up routing

2. **Handling filters** are making a separated pipeline to each invoked handler            

Filters are useful for routing, logging, validation, caching, transactions, etc.

Exception handlers provide centralized error handling and recovery