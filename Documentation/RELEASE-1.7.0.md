# Ironclads v1.7.0 — enhanced multiplayer

Windows x64 virtual naval table with a host/umpire and multiple human ship commanders. Includes Ferrol, Hampton Roads, Stonewall at the Gates (six ironclads), Iron & Fire movement tools, and the seven-cruiser Ulsan module.

- Direct TCP lobby: captain names, preferred sides, multiple ships per player, ready/start, host-approved control requests, reassignment and authenticated reconnect. All participants need this matching protocol 7 build.
- Expanded procedural ships, geography, battery effects, engine exhaust, sea audio controls, save/load, rulers and adjustable broadside arcs.
- Checks GitHub for newer stable releases on startup, with a manual check and a remembered startup preference in every scenario. New releases link to the GitHub download page; installation is manual. Offline play remains available if the check fails. No account or API key is needed.

Download `Ironclads-v1.7.0-Windows-x64.zip`, extract the entire archive and run `Ironclads.exe`. Keep all accompanying folders and DLLs. Select a module with `Start-Ironclads.ps1 -James1865`, `-Ulsan`, `-Ferrol`, or `-IronFire`, or use the app's scenario controls. See `Documentation/MULTIPLAYER.md` for connections and firewall instructions. Internet hosting requires a reachable TCP host and appropriate port forwarding; no relay is included.

Combat, damage, movement eligibility and victory remain umpire decisions. Models and coastlines are historical approximations. Private rulebooks, reference sheets, research images and optional user-supplied models are excluded. Use the included procedural models. Geographic data notices and licenses are bundled under `Ironclads_Data/StreamingAssets/GeographySource`.

To update, download a newer ZIP, extract it to a new folder and run its executable. Saves and preferences live outside the install folder in Unity's persistent data directory. All multiplayer participants should update together. The older v1.0.0 executable cannot discover updates itself.
