async function getHMACDigest(secret, body) {
    const enc = new TextEncoder();
    // It uses the browser's crypto.subtle API for HMAC (instead of Node.js crypto module)
    const key = await crypto.subtle.importKey(
      "raw",
      enc.encode(secret),
      { name: "HMAC", hash: "SHA-256" },
      false,
      ["sign"]
    );

    const signature = await crypto.subtle.sign("HMAC", key, enc.encode(body));
    const base64 = btoa(String.fromCharCode(...new Uint8Array(signature)));
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

  async function getPOSTHeaders(user, data, authenticationMethod = "nonce") {
    if (authenticationMethod === "timestamp") {
      return await getPOSTHeadersWithTimestamp(user, data);
    } else {
      return await getPOSTHeadersWithNonce(user, data);
    }
  }

  async function getPOSTHeadersWithNonce(user, data) {
    const nonce = Date.now().toString();
    const body = `Content=${JSON.stringify(data)}\nNonce=${nonce}`;
    const sign = await getHMACDigest(user.password, body);

    return {
      "X-YB-Nonce": nonce,
      "X-YB-API-Key": user.apiKey,
      "X-YB-Sign": sign,
    };
  }

  async function getPOSTHeadersWithTimestamp(user, data) {
    const timestamp = Date.now() * 1000;
    const body = `Content=${JSON.stringify(data)}\nTimestamp=${timestamp}`;
    const sign = await getHMACDigest(user.password, body);

    return {
      "X-YB-Timestamp": timestamp.toString(),
      "X-YB-API-Key": user.apiKey,
      "X-YB-Sign": sign,
    };
  }
