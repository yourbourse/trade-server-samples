# trade-server-samples
Trade server samples


# YourBourse Postman Samples

This repo contains sample Postman collections and environments for testing the YourBourse API.

## 🚀 Getting Started

1. Install [Postman](https://www.postman.com/downloads/)
2. Clone or download this repo.
3. Import:
   - Collections from `/postman/*.json`
   - Environment files from `/postman/*.json`

## 🔐 Authentication

You can use either:
- **Nonce-based auth**
- **Timestamp-based auth**

Set your `apiKey` and `password` in the selected environment.

## 📚 Sample Collection Includes

- Auth → Login using nonce/timestamp
- Market → Get symbols
- Account → Get balances

## 🛠️ Scripts

If you want to add HMAC signing automatically, check `/scripts/pre-request-scripts.js`
