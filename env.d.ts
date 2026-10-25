interface Env {
 AI: Ai;
 OAUTH_KV: KVNamespace;
 GITHUB_CLIENT_ID: string;
 GITHUB_CLIENT_SECRET: string;
 COOKIE_ENCRYPTION_KEY: string;
}

declare namespace Cloudflare { interface Env { AI: Ai; OAUTH_KV: KVNamespace; GITHUB_CLIENT_ID: string; GITHUB_CLIENT_SECRET: string; COOKIE_ENCRYPTION_KEY: string; } }
