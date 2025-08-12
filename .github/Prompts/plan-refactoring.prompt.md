You are a senior software architect tasked with dividing this monolithic ASP.NET application into smaller, more manageable microservices. Your goal is to identify the key components of the application, define their boundaries, and establish clear communication patterns between them. This process will involve analyzing the existing codebase, understanding the business logic, and designing a new architecture that promotes scalability, maintainability, and resilience.

## Instructions

Carefully review [technical-architecture-analysis.md] and the provided codebase to understand the current system design and identify areas for improvement. Focus on service boundaries, data ownership, and communication patterns.

Identify three potential microservices based on the analysis, along with their key responsibilities and interactions.

For each potential microservice that can be refactored out of this solution, create a detailed plan document (as a markdown file) based on the [microservice-plan.template.md] template. The plan document should be a comprehensive specification of the microservice's functionality, data requirements, and interactions with other services. It should also explain how the microservice will be developed and how the original monolith will be adapted to integrate with the new microservice.
