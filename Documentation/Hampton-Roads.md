# Hampton Roads reconstruction — 1.2.0 development build

Choose **Scenarios / shores & forts → Hampton Roads · Monitor vs Virginia**, or launch with `-hampton-roads`. The Ferrol pair remains available. The scenario starts March 9, 1862 at 14:00 UTC, near Newport News; ship positions and fair weather are staged, adjustable presets. This is a two-ship tabletop, not a recreation of the entire battle fleet.

Monitor has a separate moving armored turret with two XI-inch Dahlgrens, a low raft hull, forward rectangular pilothouse and low battle exhaust boxes. **Fire batteries** provides manual turret angles relative to the bow. Meshes, gun directions and smoke/sound origins turn together. Virginia (converted USS Merrimack) has a fixed sloping casemate, six IX-inch Dahlgrens, two 6.4-inch Brooke broadside rifles, and bow/stern 7-inch Brooke rifles. The ram is represented as a damaged stump for March 9. The nominal ten-gun battery remains available: the umpire decides which guns are usable after March 8 damage. Virginia's pivot guns are drawn at their center ports; individual pivot training is not implemented.

Sources and dimension conventions:

- [NHHC Monitor I](https://www.history.navy.mil/research/histories/ship-histories/danfs/m/monitor-i.html): 172 ft length, 41 ft 6 in beam, 10 ft 6 in draft, two XI-inch guns.
- [NOAA Monitor construction](https://monitor.noaa.gov/150th/construction.html), [NOAA configuration reconstructions](https://monitor.noaa.gov/science/technology-for-conservation/): March battle appearance distinguished from later tall ventilation/smoke arrangements. Outer turret diameter approximately 21.5 ft, height 9 ft; nominal eight-inch armor. The current gun carriage/fittings are approximate.
- [Mariners' Museum: Guns of CSS Virginia](https://www.marinersmuseum.org/2024/08/guns-of-css-virginia/), [Porter gun-deck drawing catalog](https://catalogs.marinersmuseum.org/object/ARI130037): as-built battery differs from the original all-Dahlgren proposal.
- [NHHC Virginia](https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/virginia.html): reported length 275 ft and beam 38 ft 6 in; other records use a 263 ft hull and approximately 51 ft overall armored width. This model uses the latter approximate hull/overall conventions, not a verified tracing of offsets.
- [NHHC battle history and March 8 damage](https://www.history.navy.mil/news-and-events/news/2024/030524.html).

The supplied Monitor sheet is relevant. `Virginia2.pdf` depicts CSS Virginia II and was excluded. No private scans were added to distributable assets.

Terrain is a hand-built geographic approximation: Hampton/Newport News, Sewell's Point, Craney Island, low woodland, beaches, wharves and fortifications. Fort Monroe's bastioned masonry walls, earth parapets and moat are representative; the unfinished Rip Raps fort is named Fort Calhoun for March 9 (renamed Fort Wool March 18). Batteries at Newport News and Sewell's Point are representative groupings, not surveyed gun placements. Coastlines, elevations, buildings, armor seams and rivets are interpretive. No modern bridge, reclaimed container terminal or contemporary city skyline is modeled.

- [Fort Monroe history](https://fortmonroe.org/plan-your-visit/about-fort-monroe/)
- [Army Historical Foundation: Fort Wool](https://armyhistory.org/fort-wool-virginia/)
- [Official orders, March 18–19, 1862](https://www.civilwar.com/battles/981-official-record/series/volume/campaign/supplements-part-i/253903-555-series-i-volume-li-i-serial-107-supplements-part-i.html)
- [NHHC battle accounts and shore batteries](https://www.history.navy.mil/research/library/online-reading-room/title-list-alphabetically/u/uss-monitor-versus-css-virginia-and-the-battle-for-hampton-roads.html)

Land does not enforce collision, depth, grounding or visibility. Fort guns are scenic; there is no shore-battery fire control yet. Measuring sectors are adjustable aids rather than verified Monitor/Virginia Yaquinto pattern cards. Eligibility, pilothouse obstruction, damage and all adjudication remain manual.

TCP sessions synchronize scenario identity, both ship states, Monitor turret angle and salvos. Only the host changes scenarios; either ship can be assigned to either player. Protocol 3 requires both players to use this build; the earlier TCP development player is not compatible. Saved tables retain scenario identity and turret angle; older Ferrol saves remain supported. The existing single save slot is shared: save elsewhere if preserving multiple tables. See [Iron & Fire support](Iron-and-Fire.md) for the additional continuous movement mode.
