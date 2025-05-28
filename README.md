# ZentrixLabs.WustatusBeacon

WustatusBeacon was a lightweight Windows service designed to expose local Windows Update state via a simple HTTP API (`/wustatus`), making it easily consumable by PRTG or other monitoring tools — especially in secured or OT network segments.

## 🛠 Purpose

This project aimed to solve data visibility gaps across environments with mixed antivirus and update tools (Defender, CrowdStrike, Azure Update Manager). It would:

- Run as a Windows Service
- Serve a small local HTTP endpoint (e.g., `http://localhost:7341/wustatus`)
- Return JSON containing:
  - Installed KBs
  - UBR / Build number
  - Pending reboot flag
  - Last patch date (event log driven)

## 🧪 Status

Development reached MVP state with:

- Windows service functionality tested
- JSON response endpoint implemented
- Silent installer/uninstaller built with Inno Setup
- GitHub Actions CI building both service and installer

## 🧾 Lessons Learned

- Mixed .NET + C++ project builds in GitHub Actions
- Deploying and managing Windows Services via custom installers
- Handling .NET 4.8 dependencies and runtime packaging
- Logging and crash tracing via `Event Viewer` and AppDomain handlers

## ❌ Archived

This project has been **archived** as our security team consolidated endpoint visibility into CrowdStrike with delayed content update support. The original data gap is no longer a concern, and a redundant agent is no longer necessary.

## 🔁 Reuse

If future environments require offline or proxy-hardened update monitoring:

- Use this as a baseline for a .NET service agent
- Reuse the Inno Setup script and GitHub Actions flow
- Consider extending for `/health`, `/osinfo`, etc.

---

Maintained by [ZentrixLabs](https://zentrixlabs.net)

