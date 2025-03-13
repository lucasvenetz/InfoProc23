# System Architecture
- P2P TCP/UDP connections for real-time gameplay (Unity clients and AWS backend)
- REST API architecture for leaderboard/score persistence
- Stateless Lambda functions with DynamoDB

# Database Design
- NoSQL best for this usecase because of simple key-value structure, no complex relationships requring sql joins
- Easy scalability
- low latency access patterns for leaderboard
- TTL implementation for automatic cleanup of scores of zero

# API
- RESTful endpoints for submitting scores and retrieving leaderboard
- Error handling and validation in API layer
- CORS configuration for cross-origin requests
- async API calls that don't block main thread

# Unity integration
- Asynchronous operations using Coroutines (functions that suspend execution to be resumed later)
"Coroutines are stackless: they suspend execution by returning to the caller, and the data that is required to resume execution is stored separately from the stack."

# Improvements:
- Client-side caching to reduce api calls
- Could use token-based authentication considerations (identity pools using AWS Cognito, assign guest users seperate token)
- Session management, role-based access
