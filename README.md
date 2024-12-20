# Referral Service Coding Exercise

## Setup
Requires .NET 9.

## Build
```bash
dotnet build
```

## Test
```bash
dotnet test
```

## Running 
```bash
dotnet run --environment "Development"
```

If running on localhost you can find docs at (http://localhost:5000/swagger/index.html). Swagger JSON can be found linked from docs.

## Generate JWT Token
This mock service uses `dotnet user-jwts` to make it easier to generate auth tokens. Run this command from the base of the project to generate a JWT token for testing:

```bash
dotnet user-jwts create \
    --name "user123" \
    --valid-for "1h" --audience localhost
```

Use the returned token as the Bearer token to test the API directly.

## Test endpoints with curl

### Create a referral record
```bash
curl -X 'POST' \
  'http://localhost:5000/referrals' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer <YOUR_JWT_TOKEN>' \
  -d '{
  "referredUserId": "test"
}'
```

### Fetch referral records
```bash
curl -X 'GET' \
  'http://localhost:5000/referral-link' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer <YOUR_JWT_TOKEN>' 
```

### Fetch a referral URL for a user
```bash
curl -X 'GET' \
  'http://localhost:5000/referral-link' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer <YOUR_JWT_TOKEN>' 
  ```