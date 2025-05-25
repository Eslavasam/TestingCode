# Hacker News Best Stories API
This API retireves the best n stories from HAcker news, ordered by score
## How to run
1. Clone the repository
2. Ensure you have atleast .NET 6.0 or higher.
3. Run the application
4. The API will be available at 'https://localhost:5001/api/stories/best'
## API Endpoint
- GET api/stories/best?n?={number}
- Retrieves the top n stories by score
- Example: '/api/stories/best?n=5' returns top 5 stories
## Asumptions
1. Limiting to 20 concurrent requests to Hacker Newa API
2. Only retrieve "story" type items (not comments, polls,etc.)
3. If a story is missing some fields we will use default values (empty string, 0, etc.)
## Enhancements for future
1.Add Swagger/OpenAPI documentation with more details
2.Add more detailed logging and metrics
