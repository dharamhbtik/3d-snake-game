# 🏪 Master Store Publishing & Partner Center Setup Guide

This guide explains how to set up and publish **Snake 3D: Slither Arena** to the **Microsoft Store (Windows)** and **Canonical Snap Store (Linux)** using automated GitHub Actions CI/CD workflows.

---

## 🪟 1. Microsoft Store (Microsoft Partner Center) Setup

> [!IMPORTANT]
> **New Partner Center Game Publishing Options Explained:**
> When creating a new game in Microsoft Partner Center, you will be prompted with two options:
> 1. **GDK Game**: Dedicated for Xbox console & Windows PC games using the Xbox Game Development Kit (C++/DirectX/Xbox Live).
> 2. **MSIX or PWA game**: For Windows PC games packaged with .NET, WinUI, Uno Platform, or PWA technologies packaged as `.msix` / `.msixbundle`.
>
> 👉 **ALWAYS CHOOSE: `MSIX or PWA game`** for Snake 3D!

---

### Step 1.1: Reserve Product Name in Partner Center
1. Log in to the [Microsoft Partner Center Dashboard](https://partner.microsoft.com/dashboard).
2. Go to **Apps and games** -> Click **New product**.
3. When prompted to select your product type, choose **MSIX or PWA game**.
4. Enter your reserved title: `Snake 3D: Slither Arena` (or check availability for your preferred variant).
5. Click **Reserve product name**.

---

### Step 1.2: Sync Product Identity with Code
1. In Partner Center, go to **Product management** -> **Product Identity**.
2. Note the values for:
   - **Package/Identity/Name** (e.g., `12345YourName.Snake3DSlitherArena`)
   - **Package/Identity/Publisher** (e.g., `CN=XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`)
   - **Store ID** (e.g., `9NXXXXXXXXXX`)
3. These identity strings ensure your packaged MSIX matches Microsoft's store identity when uploading.

---

### Step 1.3: Enable Partner Center API Access for GitHub Actions
To allow GitHub Actions to automatically upload and submit your builds:
1. In Partner Center, click the **Settings** gear icon (top right) -> **Account settings**.
2. Navigate to **User management** -> **Azure AD applications**.
3. Click **Create Azure AD application** (or link an existing one).
4. Assign the role **Manager** to this Azure AD application.
5. In the Azure AD App details, copy:
   - **Tenant ID**
   - **Client ID (Application ID)**
   - **Client Secret (Key)** (generate a new secret key and save it immediately).

---

### Step 1.4: Add GitHub Repository Secrets
In your GitHub repository, go to **Settings** -> **Secrets and variables** -> **Actions** -> **New repository secret**:

| Secret Name | Description | Where to find |
|---|---|---|
| `MICROSOFT_TENANT_ID` | Azure AD Directory (Tenant) ID | Azure AD App details in Partner Center |
| `MICROSOFT_CLIENT_ID` | Azure AD Application (Client) ID | Azure AD App details in Partner Center |
| `MICROSOFT_CLIENT_SECRET` | Azure AD Client Secret Key | Generated under Azure AD App in Partner Center |
| `MICROSOFT_APP_ID` | Store Product / Store ID | Partner Center -> *Product Identity* -> *Store ID* |
| `WINDOWS_CERT_BASE64` | *(Optional)* Base64 code-signing `.pfx` | If self-signing offline builds |
| `WINDOWS_CERT_PASSWORD` | *(Optional)* Certificate Password | Password for `.pfx` certificate |

---

### Step 1.5: Fill in Store Listing Details
Copy and paste the ready-to-use metadata, descriptions, and keywords directly from:
📄 [GAME_STORE_METADATA_AND_COPY.md](GAME_STORE_METADATA_AND_COPY.md)

Upload the visual assets generated in:
📁 `assets/store/`:
- `app_icon_512.jpg` / `app_icon.png` (Store Icon)
- `store_hero_banner.jpg` (16:9 Hero Feature Banner)
- `real_screenshot_1_gameplay.png` (Gameplay Screenshot)
- `real_screenshot_2_action.png` (Golden Apple Action)
- `real_screenshot_3_menu.png` (Menu & Customization)

---

## 🐧 2. Snap Store (Linux Snapcraft) Setup

### Step 2.1: Register Package Name on Snapcraft
1. Log in to the [Snapcraft.io Developer Dashboard](https://snapcraft.io).
2. Click **Register a name** and reserve `snake-3d` (or your chosen package name).
3. Verify that the name in `snap/snapcraft.yaml` matches your reserved name.

### Step 2.2: Export Store Login Credentials
In your local Linux terminal (or macOS/WSL with snapcraft installed):
```bash
# Install snapcraft if needed
sudo snap install snapcraft --classic

# Log in with your Canonical Snap developer account
snapcraft login

# Export your encrypted login credential token
snapcraft export-login --snaps snake-3d --channels stable store_creds.txt
```
Copy the full string inside `store_creds.txt`.

### Step 2.3: Add GitHub Repository Secret
In GitHub (**Settings** -> **Secrets and variables** -> **Actions** -> **New repository secret**):

| Secret Name | Value |
|---|---|
| `SNAPCRAFT_STORE_CREDENTIALS` | Paste the exported credentials string from `store_creds.txt` |

---

## 🚀 3. Triggering Automated Store Releases

Once your GitHub Secrets are configured, you can publish updates with a single command:

```bash
# Tag a new release version
git tag v1.0.0
git push origin v1.0.0
```

Or trigger manually:
1. Go to the **Actions** tab in your GitHub repository.
2. Select **Build & Publish Windows App to Microsoft Store** or **Build & Publish Linux Snap to Snap Store**.
3. Click **Run workflow** -> Select `main` branch.
