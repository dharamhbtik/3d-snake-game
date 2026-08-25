# 🏪 Master Store Publishing Guide (Zero-Cost Microsoft Partner Center & Snap Store)

This guide provides the **exact, zero-cost method** to publish **Snake 3D: Slither Arena** using your paid Microsoft Developer Account and Canonical Snapcraft account **without requiring paid Entra ID or Azure subscriptions**.

---

## 🪟 1. Microsoft Store (Microsoft Partner Center) Publishing

> [!NOTE]
> **No Entra ID / Azure Subscription Needed!**
> With your paid Microsoft Developer Account ($19 individual / $99 company), you **do NOT need Entra ID, Azure AD, or any paid enterprise subscriptions**.
> The standard and official workflow is to let GitHub Actions automatically compile and generate the `.msixupload` / `.msixbundle` package, download it from GitHub, and drag-and-drop it into Microsoft Partner Center.

---

### Step 1.1: Reserve Your Product Title in Partner Center
1. Log in to [Microsoft Partner Center](https://partner.microsoft.com/dashboard).
2. Go to **Apps and games** -> Click **New product**.
3. When prompted to choose your game type:
   👉 **Select: `MSIX or PWA game`** *(Do NOT select GDK Game, as that is for Xbox C++ titles)*.
4. Enter your product name: `Snake 3D: Slither Arena` (or your reserved variant).
5. Click **Reserve product name**.

---

### Step 1.2: Sync Product Identity to Code (Already Configured)
The project code and package manifest have already been updated with your Partner Center identity:
- **Package/Identity/Name**: `3774DKTech.Snake3DSlitherArena`
- **Package/Identity/Publisher**: `CN=29A7D010-A2C9-4F61-BD86-B10842B1EBC7`
- **Package/Properties/PublisherDisplayName**: `DKTech India`

*(Configured in `Snake3D/Package.appxmanifest` and `Snake3D/Snake3D.csproj`)*.

---

### Step 1.3: Generate the Store Package via GitHub Actions
You don't need to manually configure MSBuild or Visual Studio on your machine:
1. Go to your GitHub repository -> **Actions** -> **Build & Publish Windows App to Microsoft Store** -> Click **Run workflow**.
2. (Or push a release tag like `git tag v1.0.0 && git push origin v1.0.0`).
3. Once the workflow finishes (takes ~2 minutes):
   - Go to the workflow run -> Download the **`windows-store-package`** artifact (or download it directly from the **Releases** tab).
   - Inside the zip, you will find the `.msixupload` / `.msixbundle` file.

---

### Step 1.4: Complete Submission in Partner Center Dashboard
In Partner Center, click **Start your submission** and complete the 5 simple sections:

1. **Pricing and availability**:
   - Base price: **Free** (Markets: All / Worldwide).
2. **Properties**:
   - Category: **Games > Action & Adventure** (Secondary: **Classics** or **Casual**).
3. **Age ratings (IARC Questionnaire)**:
   - Fill out the quick 2-minute questionnaire (select "No" to violence, gambling, mature themes) to get an immediate **PEGI 3 / ESRB Everyone** rating certificate.
4. **Packages**:
   - Drag and drop the `.msixupload` / `.msixbundle` file you downloaded from GitHub.
5. **Store listings**:
   - Copy and paste the pre-written marketing text, keywords, and features directly from:
     📄 [GAME_STORE_METADATA_AND_COPY.md](GAME_STORE_METADATA_AND_COPY.md)
   - Upload the visual assets from the `assets/store/` folder:
     - Icon: `assets/store/app_icon_512.jpg`
     - Hero Banner: `assets/store/store_hero_banner.jpg`
     - Screenshots: `assets/store/real_screenshot_1_gameplay.png`, `real_screenshot_2_action.png`, `real_screenshot_3_menu.png`
6. Click **Submit to the Store**! Microsoft will certify and publish your game to the Windows Store worldwide.

---

## 🐧 2. Canonical Snap Store (Linux Snapcraft) Setup

### Step 2.1: Register Package Name on Snapcraft
1. Log in to the [Snapcraft.io Developer Dashboard](https://snapcraft.io).
2. Click **Register a name** and reserve `snake-3d`.

### Step 2.2: Export Store Login Credentials (One-Time)
On your local Linux terminal (or macOS/WSL with snapcraft installed):
```bash
# Log in with your Canonical account
snapcraft login

# Export your credentials token
snapcraft export-login --snaps snake-3d --channels stable store_creds.txt
```
Copy the contents of `store_creds.txt`.

### Step 2.3: Add GitHub Secret
In your GitHub repository (**Settings** -> **Secrets and variables** -> **Actions** -> **New repository secret**):
- **Name**: `SNAPCRAFT_STORE_CREDENTIALS`
- **Value**: Paste the exported token from `store_creds.txt`.

Now whenever you push a tag (e.g. `v1.0.0`), GitHub Actions will automatically compile, package, and publish the Linux Snap directly to the Snap Store!

---

## 🚀 Summary of Publishing Steps

| Step | Windows (Microsoft Store) | Linux (Snap Store) |
|---|---|---|
| **Account** | Microsoft Developer Account (Paid) | Canonical Snapcraft Account (Free) |
| **Product Type** | **MSIX or PWA game** | Snap Package (`snake-3d`) |
| **Package Creation** | Automated via GitHub Actions (`.msixupload`) | Automated via GitHub Actions (`.snap`) |
| **Publishing Method** | Download package from GitHub & drop into Partner Center | Fully automated via `SNAPCRAFT_STORE_CREDENTIALS` |
| **Metadata & Text** | [GAME_STORE_METADATA_AND_COPY.md](GAME_STORE_METADATA_AND_COPY.md) | [GAME_STORE_METADATA_AND_COPY.md](GAME_STORE_METADATA_AND_COPY.md) |
| **Screenshots & Icon** | `assets/store/` | `assets/store/` |
