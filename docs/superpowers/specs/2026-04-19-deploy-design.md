# Deployment Design: GitHub → Digital Ocean Droplet

**Date:** 2026-04-19  
**App:** WeightLossTracker (ASP.NET Core 10, SQLite)  
**Trigger:** Push to `main` branch

---

## Overview

Automated CI/CD pipeline: push to `main` → GitHub Actions builds and publishes the app → deploys to a Digital Ocean Droplet via SSH → restarts a systemd service.

```
GitHub (main branch)
       │  push
       ▼
GitHub Actions workflow
  1. Run tests
  2. dotnet publish (self-contained, linux-x64)
  3. rsync files to Droplet
  4. systemctl restart weightloss.service
  5. Health check (fail Action if service is not active)
       │
       ▼
Digital Ocean Droplet (Ubuntu)
  ├── /opt/weightloss/          ← app binaries
  ├── /var/lib/weightloss/      ← SQLite database (restricted permissions)
  ├── systemd service           ← runs app as non-root user
  └── nginx                     ← reverse proxy, TLS termination (Let's Encrypt)
```

---

## Platform Setup: Step-by-Step

### Platform 1: Local Machine (one-time)

**Goal:** Generate a dedicated deploy SSH keypair that GitHub Actions will use to connect to the Droplet.

1. Open a terminal and run:
   ```bash
   ssh-keygen -t ed25519 -C "weightloss-deploy" -f ~/.ssh/weightloss_deploy
   ```
   When prompted for a passphrase, press Enter (no passphrase — Actions needs unattended access).

2. This produces two files:
   - `~/.ssh/weightloss_deploy` — **private key** (goes into GitHub Secrets)
   - `~/.ssh/weightloss_deploy.pub` — **public key** (goes onto the Droplet)

3. Copy the public key to your clipboard:
   ```bash
   cat ~/.ssh/weightloss_deploy.pub
   ```
   Keep this terminal open — you'll need both values in the steps below.

---

### Platform 2: Digital Ocean (Droplet provisioning)

**Goal:** Create an Ubuntu Droplet with your personal SSH access.

1. Log in to [digitalocean.com](https://digitalocean.com).
2. Click **Create → Droplets**.
3. Choose:
   - **Image:** Ubuntu 24.04 LTS
   - **Size:** Basic, $6/mo (1 vCPU, 1GB RAM) is sufficient
   - **Region:** Choose closest to you
   - **Authentication:** Add your **personal** SSH key (not the deploy key — this is for your own admin access)
4. Click **Create Droplet**. Note the assigned IP address.

---

### Platform 3: Droplet (server configuration, run as root via SSH)

**Goal:** Install required software, create the app user, configure directories, systemd, nginx, and TLS.

Connect to your Droplet:
```bash
ssh root@YOUR_DROPLET_IP
```

#### Step 1 — System updates
```bash
apt update && apt upgrade -y
```

#### Step 2 — Install nginx and Certbot
```bash
apt install -y nginx certbot python3-certbot-nginx
```

#### Step 3 — Create the app user and directories
```bash
useradd --system --shell /usr/sbin/nologin weightloss
mkdir -p /opt/weightloss /var/lib/weightloss
chown weightloss:weightloss /opt/weightloss /var/lib/weightloss
chmod 750 /var/lib/weightloss
chmod 755 /opt/weightloss
```

#### Step 4 — Add the deploy SSH public key
```bash
mkdir -p /home/weightloss/.ssh
echo "PASTE_PUBLIC_KEY_HERE" >> /home/weightloss/.ssh/authorized_keys
chown -R weightloss:weightloss /home/weightloss/.ssh
chmod 700 /home/weightloss/.ssh
chmod 600 /home/weightloss/.ssh/authorized_keys
```
Replace `PASTE_PUBLIC_KEY_HERE` with the contents of `~/.ssh/weightloss_deploy.pub` from your local machine.

> Note: The `weightloss` user has no login shell but CAN be used for SSH file transfers and running remote commands via GitHub Actions.

#### Step 5 — Grant passwordless sudo for service restart only

A syntax error in any file under `/etc/sudoers.d/` disables all NOPASSWD rules — sudo falls back to password auth and the deploy fails with `sudo: a password is required`. The most common way this happens is a terminal wrapping a long paste across two physical lines. The block below uses sudoers' backslash line-continuation so each physical line is short and unwrappable. `visudo -c` validates the result.

```bash
rm -f /etc/sudoers.d/weightloss
cat > /etc/sudoers.d/weightloss << 'EOF'
weightloss ALL=(ALL) NOPASSWD: \
    /usr/bin/systemctl restart weightloss.service, \
    /usr/bin/systemctl is-active weightloss.service
EOF
chmod 440 /etc/sudoers.d/weightloss
visudo -c -f /etc/sudoers.d/weightloss
```

The paths are literal — `sudoers` does not resolve symlinks. Any caller (the deploy workflow) must invoke `/usr/bin/systemctl` exactly, with no extra arguments beyond `restart weightloss.service` or `is-active weightloss.service`.

#### Step 6 — Create the systemd service unit
```bash
cat > /etc/systemd/system/weightloss.service << 'EOF'
[Unit]
Description=WeightLossTracker
After=network.target

[Service]
User=weightloss
WorkingDirectory=/opt/weightloss
ExecStart=/opt/weightloss/WeightLossTracker
Restart=always
RestartSec=5

Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=Gemini__ApiKey=REPLACE_WITH_ACTUAL_VALUE
Environment=Admin__ApiKey=REPLACE_WITH_ACTUAL_VALUE
Environment="ConnectionStrings__DefaultConnection=Data Source=/var/lib/weightloss/weightloss.db"

[Install]
WantedBy=multi-user.target
EOF

chmod 600 /etc/systemd/system/weightloss.service
chown root:root /etc/systemd/system/weightloss.service
systemctl daemon-reload
systemctl enable weightloss.service
```

Replace `REPLACE_WITH_ACTUAL_VALUE` with your real Gemini API key and Admin API key **before** saving.

**Quoting matters:** systemd's `Environment=` splits values on whitespace unless the entire assignment is double-quoted. The SQLite connection string contains a space (`Data Source=…`), so without the wrapping quotes systemd would pass only the literal value `Data` to the app and the SQLite driver would throw `Format of the initialization string does not conform to specification`.

**Config precedence:** `Program.cs` reads `ConnectionStrings:DefaultConnection` from configuration and only falls back to a `LocalApplicationData`-derived path when nothing is configured. Setting the environment variable here is therefore authoritative — the same binary runs against `/var/lib/weightloss/weightloss.db` in production and a per-user dev path on a developer machine.

#### Step 7 — Configure nginx

Ubuntu's default nginx package ships with a `sites-enabled/default` that replies "Welcome to nginx" for any request that doesn't match another site's `server_name`. If the weightloss site uses a specific `server_name` and you visit the Droplet by IP (or before DNS has propagated), requests fall through to that default page and the app appears broken. Declare the weightloss site as `default_server` and set `server_name _;` (catch-all) so it handles anything that isn't explicitly routed.

```bash
cat > /etc/nginx/sites-available/weightloss << 'EOF'
server {
    listen 80 default_server;
    server_name _;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
EOF

rm -f /etc/nginx/sites-enabled/default
rm -f /etc/nginx/sites-enabled/weightloss
ln -s /etc/nginx/sites-available/weightloss /etc/nginx/sites-enabled/weightloss
nginx -t
systemctl reload nginx
```

The catch-all on port 80 remains after Certbot (Step 8) adds HTTPS, which is what you want for HTTP→HTTPS redirects.

**Before running Certbot**, replace `server_name _;` with the actual domain so the nginx plugin can attach the cert to the right server block:

```bash
sudo sed -i 's/^\s*server_name .*/    server_name YOUR_DOMAIN_HERE;/' /etc/nginx/sites-available/weightloss
sudo nginx -t
sudo systemctl reload nginx
```

`listen 80 default_server;` stays, so the bare IP still reaches the app for debugging.

#### Step 8 — Provision TLS certificate (required before the app is usable)

The app issues its auth cookie with `CookieSecurePolicy.Always` in Production, so browsers refuse to send it back over plain HTTP — login silently fails with an immediate redirect back to the login page. HTTPS is a functional requirement of the auth flow, not just a security nice-to-have.

First, set your domain's DNS A record to the Droplet IP. Verify propagation from the Droplet (do not proceed until `dig` returns the right IP):
```bash
dig +short YOUR_DOMAIN_HERE
```

Then issue the cert. The `--redirect` flag has Certbot install an HTTP→HTTPS 301 redirect automatically:
```bash
certbot --nginx -d YOUR_DOMAIN_HERE --non-interactive --agree-tos -m YOUR_EMAIL --redirect
```

Certbot modifies `/etc/nginx/sites-available/weightloss` in place (adding a `listen 443 ssl` block and a 301 redirect on port 80) and registers a `certbot.timer` systemd unit that renews the cert every 60 days. Verify:
```bash
curl -sI https://YOUR_DOMAIN_HERE/ | head -1
curl -sI http://YOUR_DOMAIN_HERE/  | head -1
```
Expect `200`/`302` on HTTPS and `301` on HTTP.

#### Step 9 — Open firewall ports
```bash
ufw allow OpenSSH
ufw allow 'Nginx Full'
ufw enable
```

---

### Platform 4: GitHub Repository (secrets and workflow)

**Goal:** Store the deploy credentials and add the Actions workflow file.

#### Step 1 — Add GitHub Secrets
1. Go to your repo on GitHub.
2. Click **Settings → Secrets and variables → Actions → New repository secret**.
3. Add these three secrets:

| Name | Value |
|------|-------|
| `DO_SSH_HOST` | Your Droplet IP address |
| `DO_SSH_USER` | `weightloss` |
| `DO_SSH_KEY` | Full contents of `~/.ssh/weightloss_deploy` (the private key, including `-----BEGIN...` and `-----END...` lines) |

#### Step 2 — Add the workflow file
Create `.github/workflows/deploy.yml` in your repository (this is created as part of implementation — listed here for reference):

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Run tests
        run: dotnet test WeightLossTracker.Tests/WeightLossTracker.Tests.csproj --configuration Release

      - name: Publish
        run: dotnet publish WeightLossTracker/WeightLossTracker.csproj -c Release -r linux-x64 --self-contained true -o ./publish

      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.DO_SSH_HOST }}
          username: ${{ secrets.DO_SSH_USER }}
          key: ${{ secrets.DO_SSH_KEY }}
          script: mkdir -p /opt/weightloss

      - name: Copy files
        uses: appleboy/scp-action@v1
        with:
          host: ${{ secrets.DO_SSH_HOST }}
          username: ${{ secrets.DO_SSH_USER }}
          key: ${{ secrets.DO_SSH_KEY }}
          source: "./publish/*"
          target: "/opt/weightloss"
          strip_components: 1

      - name: Restart and verify service
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.DO_SSH_HOST }}
          username: ${{ secrets.DO_SSH_USER }}
          key: ${{ secrets.DO_SSH_KEY }}
          script: |
            set -e
            sudo /usr/bin/systemctl restart weightloss.service
            sleep 8
            state=$(sudo /usr/bin/systemctl is-active weightloss.service || true)
            echo "service state: $state"
            [ "$state" = "active" ]
```

The two `sudo` invocations match the sudoers rule in Platform 3 Step 5 exactly (same absolute path, no extra flags). Health-check output comes from the echoed `service state:` line in the Actions log; the job fails cleanly when the final bracket test returns non-zero.

---

### Platform 5: Codebase (appsettings.json cleanup)

**Goal:** Remove hardcoded secrets from the repository before first deploy.

In `WeightLossTracker/appsettings.json`, blank out the API key values:

```json
"Gemini": {
  "ApiKey": "",
  "Model": "gemini-2.5-flash",
  "MaxOutputTokens": 1024
},
"Admin": {
  "ApiKey": ""
}
```

ASP.NET Core's configuration system automatically maps environment variables using double-underscore as a separator, so `Gemini__ApiKey` (set in the systemd unit) resolves to `Gemini:ApiKey` at runtime. No code changes needed.

---

## Security Practices

| Concern | Mitigation |
|---------|-----------|
| App runs as root | Dedicated `weightloss` system user, no login shell |
| SQLite accessible by other users | `/var/lib/weightloss/` is `chmod 750`, owned by `weightloss` |
| Secrets in repository | All secrets in systemd unit file (`root:root 600`), not in `appsettings.json` |
| Personal SSH key used for deploy | Dedicated deploy SSH keypair, private key stored as GitHub Secret only |
| HTTP traffic | nginx forces HTTPS redirect; Let's Encrypt cert auto-renews via Certbot |
| Session hijacking via cleartext cookies | `CookieSecurePolicy.Always` in Production — auth cookie carries `Secure` flag, never transmitted over HTTP. This means **the app cannot be used over plain HTTP**; HTTPS setup (Platform 3 Step 8) is a functional prerequisite, not an optional hardening step. |
| Bad deploy reaches server | Tests must pass before deploy; health check verifies service starts |
| Over-privileged deploy user | `weightloss` can only sudo two specific systemctl commands, nothing else |

---

## Runtime State: Data Protection Keys

ASP.NET Core uses a Data Protection subsystem to sign/encrypt auth cookies and antiforgery tokens. If the key ring is in-memory (the default when no persistent store is configured), every service restart invalidates all sessions and forms.

The app persists its key ring to disk next to the SQLite database:

```
/var/lib/weightloss/
    ├── weightloss.db       ← SQLite database
    └── dp-keys/            ← XML key ring, auto-created on first startup
```

This is configured in `WeightLossTracker/Program.cs`:

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("WeightLossTracker");
```

`keysDir` defaults to the SQLite DB's directory; it can be overridden with a `DataProtection:KeysPath` configuration value. The `weightloss` system user must own the parent directory (Pre-3 Step 3 sets `chown weightloss:weightloss /var/lib/weightloss`), so the app can create `dp-keys/` on first run without extra provisioning.

---

## Deployment Flow (steady state)

1. Developer merges PR to `main`
2. GitHub Actions triggers automatically
3. Tests run — deploy aborts if any fail
4. App is published (self-contained linux-x64 binary) and synced to `/opt/weightloss/`
5. Service restarts; 3-second wait; health check confirms it's active
6. GitHub Actions reports success or failure in the repo's Actions tab
