# Store Publishing & GitHub Secrets Setup Guide

This guide details how to configure your **Microsoft Partner Center** (Windows Store) and **Canonical Snap Store** (Linux Snap) accounts with the automated GitHub Actions workflows.

---

## 🪟 1. Microsoft Store (Windows) Setup

### Step 1.1: Enable Partner Center API Access
1. Go to [Microsoft Partner Center](https://partner.microsoft.com/dashboard).
2. Navigate to **Account settings** (gear icon) -> **User management** -> **Azure AD applications**.
3. Click **Create Azure AD application** (or link an existing Azure AD App).
4. Assign the role **Manager** to this Azure AD application so it has permission to publish app submissions.

### Step 1.2: Obtain Client Credentials
From your Azure AD Application details:
- **Tenant ID**: Located under the Azure AD Directory/Tenant section.
- **Client ID (Application ID)**: Found in the App details.
- **Client Secret (Key)**: Create a new key / secret and copy its value.

### Step 1.3: Find Your App ID in Partner Center
1. In Partner Center, go to **Apps and games** -> Select **Snake 3D** (or your reserved app name).
2. Go to **Product management** -> **Product Identity**.
3. Copy the **Store ID** (or Application ID).

### Step 1.4: Add GitHub Repository Secrets
In your GitHub repository, go to **Settings** -> **Secrets and variables** -> **Actions** -> **New repository secret**:

| Secret Name | Value |
|---|---|
| `MICROSOFT_TENANT_ID` | Your Azure AD Tenant ID |
| `MICROSOFT_CLIENT_ID` | Your Azure AD Application (Client) ID |
| `MICROSOFT_CLIENT_SECRET` | Your Azure AD Client Secret Key |
| `MICROSOFT_APP_ID` | Your Partner Center App / Store ID |
| `WINDOWS_CERT_BASE64` | (Optional) Base64 encoded code signing certificate (.pfx) |
| `WINDOWS_CERT_PASSWORD` | (Optional) Password for the code signing certificate |

---

## 🐧 2. Snap Store (Linux) Setup

### Step 2.1: Register App Name on Snapcraft
1. Log in to [Snapcraft.io Dashboard](https://snapcraft.io).
2. Click **Register a name** and reserve `snake-3d` (or your chosen package name).
3. Ensure the name in `snap/snapcraft.yaml` matches the registered name.

### Step 2.2: Export Store Login Credentials
On your local machine (or any Linux terminal with snapcraft installed):
```bash
# Install snapcraft if needed
sudo snap install snapcraft --classic

# Log in with your Canonical account
snapcraft login

# Export your login credentials token
snapcraft export-login --snaps snake-3d --channels stable store_creds.txt
```
Copy the contents of `store_creds.txt`.

### Step 2.3: Add GitHub Repository Secret
In your GitHub repository, go to **Settings** -> **Secrets and variables** -> **Actions** -> **New repository secret**:

| Secret Name | Value |
|---|---|
| `SNAPCRAFT_STORE_CREDENTIALS` | Paste the exported credentials string from `store_creds.txt` |

---

## 🚀 3. Triggering Releases

To trigger automated builds and publishing to both stores:
1. Push a version tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
2. Or manually trigger either workflow via the **Actions** tab in GitHub (**Run workflow**).
