  // This function is used to get the HMAC digest of a given body using a secret key
  // It uses the CryptoJS library to perform the HMAC SHA256 hashing and then encodes it in Base64 URL format
  function getHMACDigest(secret, body) {
    const hash = CryptoJS.HmacSHA256(body, secret);
    const base64 = CryptoJS.enc.Base64.stringify(hash);
    return toBase64Url(base64);
  }

  function toBase64Url(str) {
    return str.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
  }

  function useBaseUrlFromUserSettings(client, authContext) {
    if (authContext.isAuthenticated && authContext.user) {
      client.setConfig({
        baseURL: authContext.user.server,
      });
    }
  }

  function getGETHeaders(user) {
    return {
      "X-YB-API-Key": user?.apiKey,
    };
  }

  function getPOSTHeaders(user, data, authenticationMethod = "nonce", signWith = "signingToken") {
    let _body = "";
    let headers = {};
    if (authenticationMethod === "timestamp") {
      const {body, timestamp} = createBodyAndTimestamp(data);
      headers["X-YB-Timestamp"] = timestamp;
      _body = body;
    } else {
      const {body, nonce} = createBodyAndNonce(data);
      headers["X-YB-Nonce"] = nonce;
      _body = body;
    }
    const sign = getHMACDigest(user[signWith], _body);
    return {
      ...headers,
      "X-YB-API-Key": user.apiKey,
      "X-YB-Sign": sign,
    };
  }

  function createBodyAndNonce(data) {
    const nonce = Date.now().toString();
    const body = `Content=${JSON.stringify(data)}\nNonce=${nonce}`;
    return {body, nonce};
  }

  function createBodyAndTimestamp(data) {
    const timestamp = Date.now() * 1000;
    const body = `Content=${JSON.stringify(data)}\nTimestamp=${timestamp}`;
    return {body, timestamp};
  }
