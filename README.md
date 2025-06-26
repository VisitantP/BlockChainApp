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
  - Merkle proof hashes (sibling nodes)
  - Sibling position (left/right)
  - User balance

### 3. Validate a Merkle Proof
- Endpoint: `GET /api/merkle-verify?id=2&balance=2222`
- Reconstructs proof using the Merkle tree logic
- Returns `isValid: true/false`

### 4. Add User (optional)
- Endpoint: `GET /api/add-user?id=4&balance=4444`
- Adds a user to `UserList.json` if not already present

---

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

1. Unzip the project folder
2. Run the project:
   ```bash
   dotnet run
   ```
3. Access APIs at:
   - `http://localhost:5000/api/merkle-root`
   - `http://localhost:5000/api/merkle-proof/1`
   - `http://localhost:5000/api/merkle-verify?id=1&balance=1111`

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

## 👤 Author

Prajwal B V