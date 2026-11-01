# Direct TCP/IP one-versus-one multiplayer

**Latest: 1.2.1-dev / protocol 4**, `Builds/Windows-HamptonRoads-Union/Ironclads.exe`. The Monitor player also fires grounded Minnesota's batteries; the host sets Minnesota's availability and grounded position. Salvos and support state replicate. The wrecks of Congress and Cumberland are common scenery. Both players need this build. [Supporting ships](Hampton-Union-Ships.md).

The latest player is **1.2.0-dev**, `Builds/Windows-HamptonRoads/Ironclads.exe`, built with `Ironclads.Editor.ProjectBuilder.DeliverHamptonRoads`. It supports either scenario and either tabletop ruleset. Protocol **3** requires matching builds. The host chooses the scenario/ruleset and either ship; exact miniatures positions, courses, current/ordered speeds, distance used, turn class and Monitor turret angle synchronize as well as the original hex controls. See [Hampton Roads](Hampton-Roads.md) and [Iron & Fire](Iron-and-Fire.md). The earlier development-build instructions below remain useful for the original Ferrol TCP player.

The development player is `Builds/Windows-TCP/Ironclads.exe`, version `1.1.0-dev`. This does not modify the published v1.0.0 release. It uses built-in .NET TCP sockets with no Unity lobby/relay account, added networking package, automatic router configuration, or external service.

## Connect

1. Both players run the same development build and open **Multiplayer · direct TCP/IP**.
2. The host chooses Niagara or Stonewall, a TCP port (default **7777**) and a session code, then clicks **Host this table**. The host's active IPv4 addresses are displayed; choose the address reachable from the guest.
3. The guest enters the host's IPv4 address, matching port, and session code, then clicks Join. Joining replaces the guest's local table with the host's current table. Each player can inspect either ship; **Inspect my ship** focuses the assigned one.
4. The host may need to allow the game through the operating-system firewall. On the same LAN use the host's LAN address. Across the internet the host needs a reachable public IPv4 address and TCP forwarding to the hosting computer, or a VPN connecting both computers. Carrier-grade NAT and some network policies prevent direct inbound hosting. There is no automatic NAT traversal.

`127.0.0.1` works only when running both players on the same computer. Direct TCP is unencrypted; the session code gates joining, but does not encrypt the connection. Use a trusted LAN/VPN. The game does not change firewall or router rules on your behalf.

## Roles and shared state

The host keeps the authoritative table. Each player places/turns its own ship by hex, changes its engine/boiler settings, and requests its batteries to fire. Both see shared positions, headings, engine smoke settings, location, weather, date/time, turn and umpire notes. Only the host changes weather, table location/time, shared notes, global undo, reset or load. Either player can save a local copy/export positions. Multiplayer forces manual hex mode and disables continuous visual movement. It does not enforce movement allowances, gun eligibility, hits, damage or victory.

Cameras, globe/sea view, chart/arc guide visibility and local sound/smoke preferences are local. Salvo triggers synchronize; atmospheric particles and audio are rendered on each machine, not simulated in deterministic lockstep. A player looking at the globe does not see the local sea's effects; old salvos are not replayed on returning or joining.

Snapshots send at up to 10 Hz when state changes and at least once per second while idle. Guest ship edits are coalesced to 10 Hz and acknowledged; pending local edits remain visible until confirmed, avoiding stale snapshot snapback. A pending move is flushed before a firing request. TCP provides ordered reliable frames; no interpolation or lag compensation for tactical combat is needed because board placement and combat remain manual.

## Disconnect and reconnect

Disconnect retains the last table and returns that instance to full offline umpire control. If a guest leaves, the host keeps listening for one opponent. Rejoining with the correct code receives the current host table and the same complementary ship assignment. Extra simultaneous connections are closed. There is no host migration, automatic reconnect or discovery lobby. A host shutdown ends the session; the remaining player can continue the retained table offline or host a new session.

## Protocol and validation

The protocol uses versioned JSON inside a 4-byte big-endian length-prefixed TCP frame, maximum 65,536 bytes. Only the main Unity thread parses/applies commands and modifies scene state; background reader/writer threads perform socket I/O. Bounded input/output queues, a connection timeout, heartbeat timeout and message rate limit prevent unbounded queuing. The server validates guest ownership, sequence, board coordinates, six-way facing, engine-load bounds and battery selection. Guest-authored full-world state is rejected. These controls are not an anti-cheat or encrypted transport guarantee against an untrusted host.

## Build and verify

Use Unity 6000.2.12f1 with `Ironclads.Editor.ProjectBuilder.DeliverMultiplayer` to build `Builds/Windows-TCP`.

```powershell
& .\.tools\verify-network.ps1
& .\.tools\verify-player.ps1 -BuildDirectory 'Builds\Windows-TCP'
```

The network helper runs two separate headless standalone game processes over a loopback-only ephemeral TCP port. It checks framing with fragmented/oversized frames, wrong codes, ship assignment/ownership, weather/turn replication, moves/headings/engine orders, pending edit acknowledgements, both players' salvos, forged opponent edits and world snapshots, third-player rejection, reconnect and offline recovery. It does not open a public listener or modify firewall rules. Results go to unique directories under `Artifacts/Network`. The ordinary player verification separately exercises graphics, offline controls and effects and captures the Multiplayer panel. LAN/internet reachability still depends on the players' networks and has not been established by loopback testing.
