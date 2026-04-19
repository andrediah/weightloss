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

## GitHub Actions Workflow

**File:** `.github/workflows/deploy.yml`  
**Trigger:** `push` to `main`

**Steps:**
1. Checkout code
2. Setup .NET 10
3. Run `WeightLossTracker.Tests` — abort on failure
4. `dotnet publish -c Release -r linux-x64 --self-contained true`
5. SSH into Droplet, ensure `/opt/weightloss/` exists
6. `rsync` published output to `/opt/weightloss/`
7. `systemctl restart weightloss.service`
8. `systemctl is-active weightloss.service` — fail the Action if not running

**GitHub Secrets (repo Settings → Secrets → Actions):**

| Secret | Value |
|--------|-------|
| `DO_SSH_HOST` | Droplet IP address |
| `DO_SSH_USER` | `weightloss` (deploy user) |
| `DO_SSH_KEY` | Private key of dedicated deploy SSH keypair |

---

## Droplet Setup (one-time manual)

### Users & Directories

```bash
useradd --system --no-create-home weightloss
mkdir -p /opt/weightloss /var/lib/weightloss
chown weightloss:weightloss /opt/weightloss /var/lib/weightloss
chmod 750 /var/lib/weightloss
```

- App runs as the `weightloss` system user (no login shell, no home directory)
- SQLite file lives in `/var/lib/weightloss/` — separate from app binaries, `chmod 750` so only the app user can access it
- Deploy SSH public key added to `weightloss` user's `~/.ssh/authorized_keys`
- `weightloss` user granted passwordless `sudo` for `systemctl restart weightloss.service` only (via `/etc/sudoers.d/weightloss`)

### systemd Unit

**File:** `/etc/systemd/system/weightloss.service`  
**Owner:** `root`, **Mode:** `600`

```ini
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
Environment=ConnectionStrings__DefaultConnection=Data Source=/var/lib/weightloss/weightloss.db

[Install]
WantedBy=multi-user.target
```

Secrets are set here only — not in `appsettings.json` or the repository. The unit file is `root:root 600`.

### nginx Reverse Proxy

- Listens on port 80 and 443
- Redirects HTTP → HTTPS
- Forwards HTTPS traffic to `http://localhost:5000`
- TLS certificate provisioned via Certbot (Let's Encrypt)

---

## Security Practices

| Concern | Mitigation |
|---------|-----------|
| App runs as root | Dedicated `weightloss` system user, no login shell |
| SQLite accessible by other users | `/var/lib/weightloss/` is `chmod 750`, owned by `weightloss` |
| Secrets in repository | All secrets in systemd unit file (`root:root 600`), not in `appsettings.json` |
| Personal SSH key used for deploy | Dedicated deploy SSH keypair stored as GitHub Secret |
| HTTP traffic | nginx forces HTTPS redirect; Let's Encrypt cert auto-renews |
| Bad deploy reaches server | Tests must pass before deploy; health check verifies service starts |

---

## appsettings.json Changes Required

Remove hardcoded secrets before deployment:

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

Values will be injected via environment variables at runtime. ASP.NET Core's configuration system automatically maps `Gemini__ApiKey` env var to `Gemini:ApiKey` in config.

---

## Deployment Flow (steady state)

1. Developer merges PR to `main`
2. GitHub Actions triggers automatically
3. Tests run — deploy aborts if any fail
4. App is published and synced to `/opt/weightloss/`
5. Service restarts; health check confirms it's active
6. GitHub Actions reports success or failure
