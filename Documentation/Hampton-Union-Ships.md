# Hampton Roads: Minnesota, Congress and Cumberland

Development version **1.2.1-dev** adds all three Union ships to the existing March 9, 1862 scene. Build with `Ironclads.Editor.ProjectBuilder.DeliverUnionShips`, producing `Builds/Windows-HamptonRoads-Union/Ironclads.exe`. The earlier build remains separate for existing open sessions. Select **Union ships / Minnesota & wrecks** to inspect the models and control Minnesota.

## Minnesota

The new procedural model represents an approximately 264 ft 8½ in wooden screw frigate, 51 ft 4 in beam, with three masts, a single funnel, a lower gun deck, spar-deck battery, bowsprit, boats and anchor cables. Hull lines, port spacing, fittings and the slight grounded list are approximate. It stays stationary on the shoals as turns advance, rather than bobbing like the maneuvering ships. The host/umpire may adjust its grounded position, heading and engine-smoke settings; automatic refloating and ordinary movement controls are not provided for this ship.

The user's Minnesota sheet supplies fourteen broadside sections, each with two nine-inch and one eight-inch smoothbore, plus a ten-inch bow pivot. This model therefore carries **28 IX-inch + 14 VIII-inch + 1 X-inch** nominal heavy guns. The [NHHC ship record](https://www.history.navy.mil/research/histories/ship-histories/danfs/m/minnesota-i.html) lists the earlier 26/14/1 outfit; the [1916 naval ship catalogue](https://www.usni.org/magazines/proceedings/1916/march/ships-united-states-navy) records two additional nine-inch guns in May 1861. Small boat/howitzer weapons and later rifle refits are not included. Gun profiles/carriages and individual placement remain approximate.

The supplied Iron & Fire Hampton Roads scenario directs the umpire to remove half the port guns. The default availability is **11 port / 21 starboard / 1 bow**, rounding half of 21 up. Availability sliders are umpire controls, independent of the visible nominal battery; retained guns are selected in station order rather than reconstructing documented individual damage. You can reduce either broadside to zero or disable the pivot. Salvos originate at the available modeled guns with the existing loud reports and drifting powder smoke. Loading, arcs, hits, damage and eligibility remain manual. Minnesota's single funnel has its own banked engine exhaust.

The custom gunnery guide can measure from Minnesota toward Virginia. In Iron & Fire it uses actual positions; in hex mode it uses occupied hex centers. The distance/bearing readout in the support panel always measures actual positions. No verified Minnesota sectional broadside template is asserted.

## Congress and Cumberland

Congress burned and exploded around midnight after the March 8 action ([NHHC Virginia history](https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/virginia.html)). It is represented as a torn, charred hull with collapsed deck beams and mast stumps, with no usable guns or intact sailing rig. There is no large continuing inferno in the morning scene.

Cumberland is represented by a submerged hull, exposed leaning masts, surviving rigging and limited visible timbers. The [Mariners' Museum account](https://www.marinersmuseum.org/2021/10/uss-cumberland-sink-before-surrender/) confirms the exposed masts. A [naval museum account of early salvage reports](https://www.history.navy.mil/content/dam/museums/hrnm/Daybook/DaybookReloadsV1-2021/PDF%20Daybook%20Reload-Volume%2011-Issue%204-PDF%20Reload.pdf) describes leaning masts and a bow visible above water. The model is an interpretation, not an archaeological reconstruction of the wreck's exact March 9 condition.

Wreck positions are approximate geographic placements off Newport News; they move with the reconstructed geographic shoreline when the sea board is relocated. Minnesota's staged local position remains part of the shared table. Wrecks are manual obstacles/scenery: they do not enforce collision, line of sight or grounding, and have no fire controls.

## Saves and multiplayer

Scenario saves and host snapshots include Minnesota's grounded position, engine state and battery availability. Older saves receive a default supporting-ship setup. Both wrecks are deterministic scenery. In TCP play the **Monitor/Union player** commands Minnesota's guns, while the host sets its placement and availability. Cosmetic salvos synchronize on both machines. The two maneuvering ship assignments remain Monitor and Virginia; no third player is needed. Protocol **4** requires both players to use this build.

Verification includes the 43-gun nominal model, available groups, broadside discharge count, engine attachment, grounded turn behavior, persistence, gunnery origin, both wreck geometries and shared Minnesota fire over TCP in hex and Iron & Fire modes. Ferrol regression checks remain available in the same player.
