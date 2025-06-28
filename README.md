# Proof of Reserve Web API (Blockchain App)

This project implements a Merkle Tree–based Proof of Reserve system using C# and ASP.NET Core Web API.

## ✅ Features Implemented - PART 1
### 1. Library to compute Merkle Root from list of string values
- Loads string data statically from a local var
- Calculates a BIP340-style tagged hash Merkle Root
- Returns the Merkle Root

## ✅ Features Implemented - PART 2

### 1. Generate Merkle Root from User List
- Endpoint: `GET /api/merkle-root`
- Loads user data from `UserList.json`
- Calculates a BIP340-style tagged hash Merkle Root
- Returns the Merkle Root

### 2. Generate Merkle Proof for a Specific User
- Endpoint: `GET /api/merkle-proof/{id}`
- Returns:
  - User balance
  - Merkle proof hashes (sibling nodes), Sibling position (left/right)

### 3. Validate a Merkle Proof
- Endpoint: `GET /api/merkle-verify?id=2&balance=2222`
- Reconstructs proof using the Merkle tree logic
- Returns `isValid: true/false`

### 4. Add User (optional)
- Endpoint: `GET /api/add-user?id=4&balance=4444`
- Adds a user to `UserList.json` if not already present

## ✅ Features Implemented - PART 3
### 1. Generate Merkle Proof for a Specific User by generating MerkleTree and calculating merkle root 
- Endpoint: `GET /api/merkle-proof-fast/{id}`
- Loads user data from `UserList.json`
- Returns:
  - User balance
  - Merkle proof hashes (sibling nodes), Sibling position (left/right)

### 2. Validate a Merkle Proof using Merkle Tree
- Endpoint: `GET /api/merkle-verify-fast?id=2&balance=2222`
- Loads user data from `UserList.json`
- Constructs Merkle Tree
- Generates Merkle Proof using the Merkle Tree
- Validates the Merkle proof
- Returns `isValid: true/false`

### 3. Further, the following improvements can be considered:
- Scalability Improvements: Database Optimization, Caching, Load Balancing
- Security Enhancements: Authentication/Authorization, Rate Limiting, Audit Logging
- Reliability and Performance: APIs to check status, Async Processing, CDN Integration
- API Usability: Detailed Documentation, Versioning for backward compitability 
- Monitoring and Maintenance: track performance using monitoring tools, structured logging, Automated Testing with CI/CD integration
- Others: Web Interface, Historical Data support for auditing, Containerization for deployment across environments
---

### 3. Dockerfile support for portability
 - Dockerfile is added to increase portability of the code

## 📁 File Structure

```
/ProofController.cs      -- Main API controller

/MerkleRoot.cs           -- Merkle tree + proof generation logic, BIP340-style tagged hashing

/UserHandler.cs          -- User model with Id, Balance, ToString(), also has an utility to load/save JSON user data

/Program.cs              -- Starting point of the application

UserList.json            -- Persistent user data store
```

---

## 🛠 How to Run

### Using VS code
1. Unzip the project folder
2. Run the project:
   ```bash
   dotnet run
   ```

### Using Docker
1. Building Docker: docker build -t merkle-app -f Dockerfile .
2. Running the app: docker run -d -p 5000:80 --name proof-of-reserve merkle-app
3. Stop the Docker: docker stop proof-of-reserve
4. Delete the Docker app: docker rm proof-of-reserve

### API Usage:
   - `http://localhost:5000/api/add-user?id=9&balance=9999`
   - `http://localhost:5000/api/merkle-root`
   - `http://localhost:5000/api/merkle-proof/1`
   - `http://localhost:5000/api/merkle-verify?id=1&balance=1111`
   - `http://localhost:5000/api/merkle-proof-fast/1`
   - `http://localhost:5000/api/merkle-verify-fast?id=1&balance=1111`
---

## ✅ Sample UserList.json

```
[
  { "Id": 1, "Balance": 1111 },
  { "Id": 2, "Balance": 2222 },
  { "Id": 3, "Balance": 3333 }
]
```

---

## 🧠 Notes

- Tagged hashing follows BIP340-style tag-hashing using SHA256.
- The Merkle proof algorithm handles odd leaf duplication.
- The system is stateless and reloads `UserList.json` per request for reliability.

---