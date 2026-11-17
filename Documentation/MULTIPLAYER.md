# Multiplayer lobby and ship command (protocol 7)

Version **1.7.0-dev**, `Builds/Windows-Multiplayer/Ironclads.exe`. All participants must use this build; protocol 6 players cannot join. Supports Ferrol, Hampton Roads, Iron & Fire, Ulsan and the six-ironclad **Stonewall at the Gates** scenario. This delivery also retains the broadside measuring tools.

## Connect and start

1. Run `Start-Ironclads.ps1 -James1865` for the six-ship scenario. Open **Multiplayer**. Other modules provide the same lobby under their multiplayer/TCP tab.
2. Enter your captain name. The host selects **Host as separate umpire**, or selects an initial ship, chooses a TCP port (default 7777) and session code, then clicks **Open lobby**.
3. Guests enter the host IPv4 address, port and code and click **Join**. Joining replaces their local table with the host's current table. There can be six guests plus the host: six human commanders and an optional separate umpire, or a playing host with fewer guests.
4. Choose a preferred side and request a ship. The host approves requests or assigns ships directly using **Assign ships to** and the ship's **Assign** button. One captain can command several ships. Unassigned participants can observe.
5. After assignments, every connected participant marks ready. The host clicks **Start match**. Assignment changes in the lobby clear readiness. The host can pause orders and return to the lobby.

Players can inspect and measure every ship. Movement, turret training, engine orders and firing require assignment and a running match. Host authority covers weather, clock, shared notes, scenario selection, loading, resets and undo. These remain an umpired tabletop game: movement legality, combat results and victory are manual.

## During a match

Open the lobby while playing and click **Request control** on any ship. The host sees the requesting captain and current commander, with **Approve / Decline**. Preferred sides do not restrict requests; cross-side requests are identified for the host. No request grants control automatically.

Approval replaces the current commander and announces the new assignment to everyone. Each ship has exactly one commander. A captain may own multiple ships. The host may take a ship, assign it to another connected captain, or release it. Orders carry an assignment epoch and table generation; late orders from previous assignments are rejected by the host. Requests and membership are live session data, not part of saved table files.

## Addresses and forwarding

On the same LAN, share the host's LAN IPv4 address. Across the internet, share the host's public IPv4 address and forward the chosen **TCP** port to the host computer. Allow the host executable through the operating-system firewall. Guests initiate outbound connections and ordinarily need no forwarding. The app displays local adapter addresses, not an automatically discovered public IP.

Carrier-grade NAT, double NAT and restrictive networks may prevent direct hosting. A shared VPN is an alternative. `127.0.0.1` only connects processes on the same computer. The lobby runs inside the host application; there is no public server directory, relay, automatic NAT traversal or automatic router/firewall configuration. Direct TCP is unencrypted; use a trusted network or VPN when appropriate. The session code gates admission and does not encrypt traffic.

## Disconnects and reconnects

Disconnected captains remain on the roster and their ships remain reserved on the host's table. The host can immediately reassign those ships, or remove the captain to release ships and free a roster slot. Other clients remain connected.

A captain can reconnect from the same running app using the same host address, port and code. A private, random session token restores their identity and any ships still reserved for them; a matching display name alone does not recover ownership. The token is retained only in app memory. After restarting the app, reconnect as a new participant and have the host approve your ships again. There is no automatic reconnect or host migration. Host shutdown returns guests to the retained table with offline umpire control.

Changing scenario or tabletop ruleset returns everyone to the lobby, clears assignments and readiness, and requires a fresh match start. Cameras, measuring guides, sound preferences and rendered smoke remain local. Old salvos are not replayed to late joiners.

## Build and verify

Build using Unity 6000.2.12f1:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod Ironclads.Editor.ProjectBuilder.DeliverLobby -logFile Artifacts/lobby-build.log
& .\.tools\verify-lobby.ps1
& .\.tools\verify-lobby.ps1 -Mode ferrol -Guests 2
& .\.tools\verify-lobby.ps1 -Mode hampton -Guests 2
& .\.tools\verify-lobby.ps1 -Mode ironfire -Guests 2
& .\.tools\verify-lobby.ps1 -Mode ulsan -Guests 6
& .\.tools\verify-player.ps1 -BuildDirectory Builds/Windows-Multiplayer -James
```

The lobby helper starts a host and up to six independent headless players on a loopback-only ephemeral TCP port. Reports go to a unique `Artifacts/Lobby` directory. It exercises real lobby requests, host approvals/declines, readiness/start, simultaneous ship orders, turrets, engines, salvos, control transfers, old-owner command rejection, authenticated reconnect and host shutdown recovery. Six-guest runs also check rejection of an extra participant. Loopback validation does not establish public internet reachability.

Frames remain length-prefixed JSON, maximum 65,536 bytes. Each connection has its own acknowledgement, handshake/heartbeat deadlines and message-rate limit. Socket threads perform byte I/O; Unity's main thread applies validated commands. The host rejects guest-authored table snapshots. Snapshots are coalesced to at most 10 Hz in normal updates, with idle heartbeats and immediate state publication for lobby changes and firing.

Local validation on 13 November 2026 passed: Unity Windows build; seven-process James 1865 lobby/match; three-process Ferrol, Hampton Roads, Iron & Fire and Ulsan networking; and graphical James, Ulsan and Ferrol/offline regression checks. Validation also covers guest-authored world snapshot rejection, assignment epochs after returning a ship to its previous commander, table replacement generations, and live scenario changes. The lobby screenshot is `Artifacts/James1865/James-multiplayer-lobby.png`. Public internet reachability remains dependent on the host network and was not tested by the loopback suite.
