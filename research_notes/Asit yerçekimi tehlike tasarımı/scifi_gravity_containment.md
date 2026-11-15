# Sci-fi and Real-Science Devices That Hold Liquid Aloft: visual catalogue for the acid gravity-device hazards

Scope: how fiction and real science show levitation, containment and gravity devices, judged against Rock The Game's needs. The hazards are moving circles and bars (linear ping-pong, circular, accelerating and rotating paths). The world look is a flat sky, orange ground with pink cream edges, plum pipes, halftone print platforms and a Bauhaus/brutalist UI. The fiction is acid held in the air by a gravity device. Each idea is mapped to a **circle hazard** (core plus acid disc) and a **bar hazard** (core bar plus acid wrap), with its readability risk at speed and its 2D cost in Unity URP.

Cost scale used below: **S** = sprite(s) plus a transform script; **SH** = one Shader Graph/SDF shader on a quad; **P** = ParticleSystem; **L** = LineRenderer. Low/Med/High is the rough effort for a solo developer.

---

## 1. How do iconic sci-fi works visualise gravity, levitation and containment tech?

### Takeaway
Most recognisable designs fall into a few families: **rotating rings around a sphere** (Event Horizon's gravity drive), **visible beams or cones** (1950s tractor beams), **coloured energy planes held between emitter posts with a status light** (Half-Life 2 Combine fields), **glowing crystal cores** (Laputa's levitation stone) and **fresnel/hex "shield" shells** (game VFX convention). For this game the ring-around-core and emitter-post families fit best, because they put mechanical parts *inside* or *at the ends of* the acid silhouette, which keeps the lethal outline clean.

### Cited Findings
- **Event Horizon (1997), gravity drive:** a central sphere with three rings spinning around it. When the rings line up they open a gateway. — [Wikipedia: Event Horizon (film)](https://en.Wikipedia.com/wiki/Event_Horizon_(film)); [RetroZap review](https://retrozap.com/event-horizon-1997-sci-fi-saturdays/). A 3D-print fan model of the drive can be rotated for reference: [CGTrader model](https://cgtrader.com/3d-print-models/miniatures/memorabilia/event-horizon-gravity-drive). (One other site describing it as a "centrifuge" looked AI-generated, so I left it out.)
- Event Horizon's production design was described as "techno-Medieval", a Gothic cathedral in space with a cruciform layout. — [American Cinematographer article](https://theasc.com/article/needs-pics-unearthly-terrors-event-horizon/)
- **Tractor beams:** E. E. Smith coined the term in *Spacehounds of IPC* (1931), updating his earlier "attractor beam". — [Wikipedia: Tractor beam](https://en.wikipedia.com/wiki/Tractor_beam)
- *This Island Earth* (1955) shows a beam you can see and hear pulling an aircraft into a saucer. *Spaceballs* (1987) parodies it as a "magnetic beam". — [Theme Ontology: tractor beam](https://themeontology.org/theme/tractor-beam.html); trope list: [All The Tropes: Tractor Beam](https://Allthetropes.org/wiki/Tractor_Beam)
- The Star Wars (1977) Death Star beam that pulls in the Millennium Falcon is the most-cited example. — [IOL](https://iol.co.za/news/2015-10-28-science-fictions-tractor-beam-now-real/)
- **Half-Life 2, Combine Force Field:** a blue energy field that blocks unregistered people. The E3 2003 demo version was orange-tinted. — [Combine OverWiki: Force Field](https://combineoverwiki.net/wiki/Forcefield)
- Combine fields are powered by generators. Late in the game, glowing **Energy Balls** power the fields, and the player pulls the orbs out with the Gravity Gun to switch the fields off. — [Half-Life Wiki: Combine Power Generator](https://half-life.fandom.com/wiki/Combine_Power_Generator); [Combine OverWiki: Energy Ball](https://combineoverwiki.net/wiki/Energy_Ball); [Gravity Gun](https://combineoverwiki.net/wiki/Gravity_Gun)
- Combine control interfaces show the field state. When the field is switched off, the indicator blinks from blue to red. Field bridges show emitter lights at both ends. — [Combine OverWiki: Combine Interface](https://combineoverwiki.net/wiki/Combine_Interface); [Powered bridge](https://combineoverwiki.net/wiki/Powered_bridge)
- **Laputa: Castle in the Sky (Ghibli):** a giant glowing blue levitation crystal powers the floating castle. A fan diorama of the scene where Muska finds it is covered here. — [grape Japan](https://grapeejapan.com/?p=199710). (Its name, "volucite" or "Aetherium", comes only from commentary sources. Not verified against official material.)
- **Game "force field" VFX conventions:** a fresnel rim drives both emission and alpha, so the edge glows brighter than the centre. Other common parts are intersection highlights, scrolling UVs, refraction and hex-cell patterns. — [godotshaders: Scifi shield](https://godotshaders.com/shader/scifi-shield/); [ForceFieldFX](https://awesome.ecosyste.ms/projects/github.com%2Fvux427%2FForceFieldFX); [Jurassic Snap shield in Unity Shader Graph](https://gamedeveloper.com/production/how-we-made-our-shield-fx-for-jurassic-snap-with-unity-shader-graph); [80.lv hex shield](https://80.lv/articles/how-to-make-a-glowing-hexagon-shield-animation-with-blender-s-geometry-nodes)
- A 2D hex force-field addon exposes hex size, line width, rim width, rim power, pulse speed and glow radius as parameters. These make a good starting list of knobs for a 2D shader. — [Construct 3 Force Field effect docs](https://www.construct.net/en/make-games/addons/1591/force-field-effect/documentation)
- **Pulp art:** Frank R. Paul largely set the look of 1920s science-fiction pulp covers and interiors (Amazing Stories, 38 covers from 1926 to 1929). — [Wikipedia: Frank R. Paul](https://en.wikipedia.org/wiki/Frank_R._Paul); image galleries: [Dangerous Minds on Frank R. Paul](https://dangerousminds.net/comments/the_far_out_work_of_frank_r._paul_the_father_of_science_fiction_art)

### Inferences (idea catalogue, sci-fi side)

**A. "Gravity drive" ring core (Event Horizon)**
- *Circle:* a small ink (#140C1C) core sphere, with 2–3 thin rings drawn as ellipses at different tilts that counter-rotate. The acid disc sits between the core and the outer ring. On rotating-path hazards, the rings can spin against the path direction, which shows a spinning hazard clearly.
- *Bar:* a "bobbin". Two ring assemblies sit at the ends of the core bar, and a sleeve of acid is held between them.
- *Readability risk:* medium. Rings that stick out past the acid make the hitbox unclear. Keep every ring **inside** the acid silhouette, or make the ring the lethal outline itself.
- *Cost:* S (2–3 ring sprites plus a rotation script), Low.

**B. Emitter-post field (Half-Life 2 Combine field, field bridge)**
- *Circle:* weaker fit. It works only as a hub with 3–4 short spokes or posts sitting inside the acid disc.
- *Bar:* **strong fit.** Two chunky end caps (emitters) with small lights, and a slab of acid held between them, drawn as a flat colour field. The end lights follow the Combine convention: a status colour that changes (yellow before it moves, red at full speed).
- *Readability risk:* low if the end caps are also lethal (everything inside the capsule kills). High if the caps are safe but the acid between them kills.
- *Cost:* S plus SH (scrolling noise on the acid slab), Low–Med.

**C. Power orb or crystal core (HL2 Energy Ball, Laputa crystal)**
- *Circle:* the core is a glowing orb or faceted crystal in a contrasting accent (yellow #FCAE04 or teal #289AA4 against the acid green). The acid is the field it projects.
- *Bar:* a row of 2–3 small orbs along the bar, like batteries in a holder.
- *Readability risk:* low. The core sits inside the hitbox.
- *Cost:* S plus a glow pulse (sprite alpha or URP 2D light), Low.

**D. Fresnel/hex shell (game shield convention)**
- In 2D, "fresnel" becomes **distance to the SDF edge**: brighter near the rim and clearer toward the centre. A faint hex or dot lattice inside the acid suggests that a field is shaping it.
- *Circle and bar:* the same shader. A circle or capsule SDF gives a rim band plus an inner pattern.
- *Readability risk:* **medium–high** if the glow spreads outside the hitbox. Keep the bloom *inside* the edge and add a hard outer ink outline. A soft outer halo makes the hitbox look bigger than it is, which is unfair.
- *Cost:* SH, Med.

**E. Visible tractor-beam cone or tether (This Island Earth)**
- *Circle and bar:* a pale dashed or striped cone or line runs from a fixed wall or ceiling emitter to the moving hazard, or along its path. This tells the player *what* holds the acid and also previews the route.
- *Readability risk:* high if the beam looks lethal. It must be clearly non-lethal: thin, ink or paper-coloured, dashed, low alpha, and never acid green.
- *Cost:* L, Low.

**F. Pulp-cover attitude (Frank R. Paul)**
- Large, naive, saturated machines on flat colour backgrounds already match the flat sky-blue world. Use it as a mood reference, not a specific design.

### Gaps
- I found no source describing how the Star Wars tractor beam *looks* on screen. My memory is that the beam is invisible in the 1977 film, but I did not verify that.
- No official concept-art or art-book pages were found for the Event Horizon gravity drive or Laputa's crystal. The developer should check the films directly or official art books.
- I did not research several other likely references: Mass Effect biotics/mass-effect fields, Remedy's *Control* (levitation), *Akira* (1988) containment capsules, Ghostbusters containment, Portal's emancipation grid, and Star Trek force fields. They are worth a look but are uncited here.

---

## 2. How does liquid actually behave when levitated, and which behaviours are striking and cheap to fake in 2D?

### Takeaway
Real levitated liquid gives five strong 2D motifs: **(1) the wobbling sphere** (ISS water balls, ripples moving through and across the surface), **(2) blobs shooting off and pulling back in** (Pettit's effervescent tablet), **(3) swirled colour bands** (Pettit's "Jupiter" ball), **(4) ferrofluid spikes** (Rosensweig instability; Kodama's spikes that climb and rotate) and **(5) blobs stacked at evenly spaced nodes** between two emitters (acoustic levitators). Spikes and node-stacking fit the core-and-acid concept best. Wobble is the cheapest way to make the hazard feel alive.

### Cited Findings
**ISS water spheres**
- On the ISS, water forms floating spheres held together by surface tension. NASA's Physics Demos show an astronaut drinking from a floating water ball. — [NASA (Pettit droplets)](https://www.nasa.gov/news-release/dancing-droplets-rock-out-on-space-station/); [Physics Demos NASA video](https://omb.report/videos/physics-demos-nasa)
- Pettit used sound from a speaker to make water droplets oscillate and fly off. In one episode the droplets "danced" to ZZ Top. — [NASA news release](https://www.nasa.gov/news-release/dancing-droplets-rock-out-on-space-station/); [phys.org PDF](https://phys.org/news/2012-05-droplets-space-station.pdf); video collection: [Open Culture, Pettit experiments](https://www.openculture.com/2012/08/watch_astronaut_don_pettit_conduct_cool_experiments_aboard_the_international_space_station_.html)
- Puffs of air aimed at a floating water mass set off two sets of ripples, one through the body and one across the surface. — [Boing Boing on a Pettit video](https://boingboing.net/2009/11/28/saturday-morning-sci-7.html)
- An effervescent tablet pushed into a water sphere turns it milky, makes it grow, and sends smaller fizzy globules shooting away from the main orb. — [SlashGear video](https://www.slashgear.com/watch-an-effervescent-tablet-fizz-inside-a-ball-of-water-in-space-28394568); [NASA image iss072e069828](https://www.nasa.gov/image-detail/amf-iss072e069828)
- Pettit made a "Jupiter-like" water sphere on the ISS (Oct 2024) by adding food colouring, which swirled into bands. — [Space.com](https://www.space.com/nasa-astronaut-don-pettit-jupiter-water-ball-iss); [NASA image iss072e742508](https://www.nasa.gov/image-detail/amf-iss072e742508)
- Astronauts sealed a GoPro inside a floating water orb, and the orb visibly holds its shape around the object. — [Popular Science](https://www.popsci.com/article/technology/iss-astronauts-encase-gopro-orb-floating-water)
- ISS droplet-merging research: larger drops would sag into puddles under gravity, but in orbit they stay spherical. — [Astronomy.com](https://www.astronomy.com/science/watch-iss-tests-how-water-droplets-merge-in-space/)
- (NASA image pages showed a federal-funding-lapse notice when searched, so availability may vary.)

**Acoustic levitation**
- A transducer and a reflector set up a standing wave. Droplets settle near its pressure nodes, so several droplets can float in a vertical stack, evenly spaced. — [PMC9500997](https://pmc.ncbi.nlm.nih.gov/articles/PMC9500997); [arXiv 2307.00400](https://arxiv.org/pdf/2307.00400)
- One experiment had nodes about 4.5 mm apart. The levitated 2 mm droplet **flattened into an oblate (squashed) shape** and sat slightly below the node. Another rig held droplets at the 2nd, 3rd and 4th of five nodes, and the number of droplets and their positions affected stability. — [arXiv 2307.00400](https://arxiv.org/pdf/2307.00400); [PMC9500997](https://pmc.ncbi.nlm.nih.gov/articles/PMC9500997); [LSBU IUS2023 proceedings](https://openresearch.lsbu.ac.uk/download/0e87207296252f9a6248cbd4040a72b2cf45c5416f6a51906d5df19214ebf2c8/518934/IUS2023_proceedings_Investigation%20of%20ultrasonically%20levitated%20droplets%20for%20sonochemistry%20using%20high%20speed%20camera%20imaging_v4.pdf)
- Neighbouring droplets push on each other through scattered sound, which matters when they merge. — [IEEE IUS 2019 paper](https://ewh.ieee.org/conf/ius/2019/media/files/0761.pdf)
- **TinyLev (Marzo, Barnes, Drinkwater, 2017):** a single-axis levitator built from two opposing arrays of cheap 40 kHz ultrasonic transducers at about 20 V. A later system injects, moves, merges and ejects droplets in mid-air. — [Open Electronics on TinyLev](https://www.open-electronics.org/?p=21501); [Andrade/Camargo/Marzo droplet paper](https://typeset.io/pdf/automatic-contactless-injection-transportation-merging-and-566nhflvhy.pdf)
- Bristol's acoustic tractor beam used fast-changing acoustic vortices to trap a polystyrene sphere. — [Lab Manager](https://www.labmanager.com/the-world-s-most-powerful-acoustic-tractor-beam-could-pave-the-way-for-levitating-humans-5349); [Scientific American](https://www.scientificamerican.com/article/real-life-tractor-beam-can-levitate-objects-using-sound-waves/); [Smithsonian](https://www.smithsonianmag.com/science-nature/acoustic-tractor-beam-can-levitate-small-objects-sound-180957060/)
- Watching droplets "dance" in midair under sound waves: [NBC News](https://www.nbcnews.com/id/wbna54154146)

**Ferrofluid**
- **Rosensweig instability (Cowley & Rosensweig, 1967):** once the magnetic field passes a critical value, a flat ferrofluid surface jumps into a **hexagonal array of spikes**. A stronger field can switch it to a square pattern. — [Europhysics News PDF](https://www.europhysicsnews.org/articles/epn/pdf/2011/03/epn2011423p17.pdf); [arXiv nlin/0102004](https://arxiv.org/pdf/nlin/0102004)
- Over a small magnet, spikes appear above it, **multiply as the magnet gets closer, and finally break off as separate droplets.** — [arXiv 1711.09942](https://arxiv.org/pdf/1711.09942)
- Interactive visualisation of hexagonal spike formation (it gives about 9–10 mm spacing for typical ferrofluid; secondary source): [elysiatools](https://elysiatools.com/en/visualizations/ferrofluid-rosensweig-instability)
- **Sachiko Kodama's ferrofluid sculptures** (Morpho Tower, 2006 onward): spikes form at the base, then **climb, tremble and rotate around the edge of an iron spiral**. Their size grows and shrinks with the electromagnet's current, and the spin speed is controlled without motors. *Morpho Tower – Sprout* (2019) has cactus- or urchin-like spikes appearing and vanishing around a metal cone. — [CACM: Dynamic Ferrofluid Sculpture](https://cacm.acm.org/magazines/2008/6/5384-dynamic-ferrofluid-sculpture/fulltext); [Digital Art Archive: Morpho Tower / Two Standing Spirals](https://digitalartarchive.at/database/work/2230); [Pellas Gallery: Sprout](https://www.pellasgallery.com/artworks/9723-sachiko-kodama-morpho-tower-sprout-2019); [Kodama: Breathing Chaos](https://www.sachikokodama.com/en/works/breathing-chaos); [Makezine](https://makezine.com/article/craft/morpho-towers-two-standin)

**Diamagnetic levitation**
- Water is diamagnetic, so a strong enough field pushes it up against gravity. A live frog was levitated inside the 32 mm bore of a ~16 T Bitter solenoid at Nijmegen. Pyrolytic graphite floats over ordinary rare-earth magnets. — [Wikipedia: Diamagnetism](https://en.wikipedia.org/wiki/Diamagnetism); [MRIquestions: how to levitate a frog](https://mriquestions.com/how-to-levitate-a-frog.html); [Physics World](https://physicsworld.com/a/floating-femtodroplets/)

### Inferences (idea catalogue, real-science side)

**G. Wobbling acid sphere (ISS water ball)**
- *Circle:* a polar SDF in the shader, `r(θ,t) = R + a·sin(2θ + ωt) + b·sin(3θ − ω₂t)`. Low mode numbers (2–3) read as a liquid wobbling under surface tension. Add a faint inner band offset for the "ripples through the body" look.
- *Bar:* a capsule SDF with a travelling sine along the long edges, so ripples run along the bar like a skin.
- *Readability risk:* low if the amplitude stays small (≤5–8% of radius) and the **collider uses the minimum radius**. The visual should never shrink below the collider.
- *Cost:* SH, Low–Med. This is the single best "alive" upgrade for the cost.

**H. Fizz globules that leave and return (Pettit's effervescent tablet)**
- *Circle:* 3–6 small acid droplets break off the rim, arc out a little and get pulled back in.
- *Bar:* droplets peel off the bar ends and snap back.
- *Readability risk:* **high for fairness.** Flying droplets look lethal. Either make them lethal with tiny colliders (costly, and noisy to read) or keep them very close and short-lived, so they stay inside a "danger halo" the player already learns to avoid. Use sparingly, perhaps only as a tell (see Q4).
- *Cost:* P (sub-emitter or simple orbit script), Low–Med.

**I. Swirled colour bands (Pettit's "Jupiter" ball)**
- *Circle:* two greens (bright acid plus a darker or yellower acid) swirl slowly in polar UV inside the disc, which hints at the field stirring the acid. The swirl direction can show which way a rotating hazard spins.
- *Bar:* bands flow along the bar axis (scrolling stripes). This fits the Bauhaus stripe language.
- *Readability risk:* low (it is all inside the silhouette). Too much contrast inside the acid could make the edge harder to see, so keep the bands low-contrast.
- *Cost:* SH (polar UV twist plus noise), Low.

**J. Ferrofluid spike crown (Rosensweig / Kodama)**
- *Circle:* the acid ring's outer edge forms evenly spaced spikes, `r = R + A·max(0,cos(nθ))^k`. The spikes **travel around the ring** like Kodama's climbing spikes, and **spike height pulses** with the "field strength". Kodama's sculpture form (a metal core with liquid climbing it) is close to the developer's "core plus acid ring" idea.
- *Bar:* spikes march along the bar edges like a sea urchin or caterpillar. On accelerating ping-pong hazards, spike height can rise with speed.
- *Readability risk:* **lowest of all.** Spikes are the universal "lethal" silhouette in platformers and read even without colour (see Q4). Watch for spike tips poking past the collider. Either include the tips in the hitbox or keep spikes short and blunt.
- *Cost:* SH, Low–Med.

**K. Node-stacked blobs between two emitters (acoustic levitator / TinyLev)**
- *Bar:* **strong fit.** Two flat transducer caps at the ends, drawn as a grid of small circles (looks like a speaker array). 3–5 slightly **flattened** acid blobs hang evenly spaced between them, each wobbling a little. Faint dashed "pressure band" lines mark the nodes. The silhouette reads as a "device holding liquid" in one glance.
- *Circle:* blobs at the nodes of a *ring-shaped* standing wave, i.e. a necklace of 6–8 acid beads orbiting the core. This could be a lighter variant for small hazards.
- *Readability risk:* medium. Gaps between the blobs make players wonder whether they can pass through. Either the gaps are truly safe (separate colliders: a deliberate "threading" design) or a continuous faint acid film joins the blobs so the hazard reads as solid.
- *Cost:* S plus SH (per-blob wobble) or several sprites, Low–Med.

**L. Coil bore with acid floating inside (diamagnetic frog)**
- *Bar:* a striped coil sleeve (Bauhaus bands of ink and paper) wrapped around a floating column of acid. The acid shows at both open ends and through slots. This is the *inverse* of the core-in-the-middle concept: device outside, acid inside.
- *Circle:* a torus coil ring with an acid blob floating at its centre.
- *Readability risk:* medium. If the coil is not lethal but the acid is, the hitbox becomes complicated. Simplest rule: the whole object kills.
- *Cost:* S, Low. Optional as a third hazard family.

### Gaps
- I found no primary NASA video specifically on *free-floating sphere oscillation modes* (the 2, 3 and 4 lobe shapes). The ripple description comes from a secondary blog. Searching the NASA video library or the ISS Research YouTube channel for "water sphere oscillation" would close this.
- Plasma toroids, ball lightning and tokamak plasma glow were planned but not researched for lack of budget.

---

## 3. What visual cues make an emitter look like it is "holding" something?

### Takeaway
Across the sources, "holding" reads through **(a) a visible source** (emitter caps, coils, a core), **(b) visible structure in the gap** (rings, nodes, pressure bands, spike patterns that follow the field), **(c) a reaction to the field** (spikes that grow and shrink with current, blobs flattened at nodes, swirls), and **(d) a status light** that tells you the field's state. A bare blob of liquid does not read as "held". The liquid needs to *respond* to the device.

### Cited Findings
- Kodama's spikes **grow and shrink with the electromagnet current, and rotate around the iron core.** The liquid's shape visibly follows the device. — [CACM](https://cacm.acm.org/magazines/2008/6/5384-dynamic-ferrofluid-sculpture/fulltext); [Digital Art Archive](https://digitalartarchive.at/database/work/2230)
- Ferrofluid spikes **multiply as the magnet gets closer** and break off when the field gets strong enough. The shape works as a readout of field strength. — [arXiv 1711.09942](https://arxiv.org/pdf/1711.09942)
- Acoustic levitation holds droplets at **evenly spaced pressure nodes between a transducer and a reflector**, and the droplets flatten in the field. — [PMC9500997](https://pmc.ncbi.nlm.nih.gov/articles/PMC9500997); [arXiv 2307.00400](https://arxiv.org/pdf/2307.00400)
- HL2 Combine field bridges show **emitter lights at both ends**. Control panels show field state with a colour change (blue to red when off). — [Combine OverWiki: Powered bridge](https://combineoverwiki.net/wiki/Powered_bridge); [Combine Interface](https://combineoverwiki.net/wiki/Combine_Interface)
- HL2 fields are tied to a visible power source (Energy Balls in generators). Removing the orb kills the field, which makes the cause and effect easy to read. — [Combine OverWiki: Energy Ball](https://combineoverwiki.net/wiki/Energy_Ball)
- Event Horizon's drive is defined by **rings orbiting a core**. Their alignment state is the visual event. — [Wikipedia: Event Horizon](https://en.Wikipedia.com/wiki/Event_Horizon_(film))
- Shield VFX use **rim glow (fresnel), pulsing and scrolling UVs** to show the field is active. — [godotshaders: Scifi shield](https://godotshaders.com/shader/scifi-shield/); [Construct force field docs](https://www.construct.net/en/make-games/addons/1591/force-field-effect/documentation)

### Inferences ("holding" cue kit, mix and match)
1. **Core visibly drives the acid:** a small spinning core with a slot or indicator, and the acid swirl or spike rotation *synced to the core's spin*. This is cheap (one shared time parameter) and is the clearest cause-and-effect.
2. **Spike pulse = field strength:** a single float drives both the core light brightness and the spike height. Cost: shared shader property.
3. **Dashed orbit ring inside the acid:** an ink dashed circle (or dashed rails along a bar) under a semi-opaque acid layer, like a printed diagram. This fits the Bauhaus/halftone print look. Cost: SH or sprite.
4. **Emitter caps on bars:** two chunky end pieces with a 2×3 grid of little circles (the transducer look) or coil stripes. The acid "starts" and "stops" at them, so the bar clearly reads as held between them.
5. **Status lamp:** one small lamp on the core. Paper/off when idle, yellow (#FCAE04) when about to move, red (#FC1E00) at full speed or when accelerating. This follows the HL2 status-light idea and doubles as a telegraph.
6. **Pressure bands:** thin rings or bars of slightly lighter acid that pulse outward from the core and fade at the rim, never crossing it. This suggests a field pushing outward to keep the acid in shape.
7. **Avoid outer glow halos.** All "field" cues stay inside the lethal silhouette. Anything outside it is either an ink, dashed, clearly non-acid element (tether/path line) or absent.

### Gaps
- I found no design-analysis source (for example a sci-fi interface or VFX design blog) that sets out a "holding" visual grammar directly. The list above is my synthesis from physics and game examples.

---

## 4. What failure and tension cues could add life and telegraph danger, and how do they affect readability at speed?

### Takeaway
The physics offers natural "stress" cues: **spikes multiplying and then breaking off into droplets** as the field strengthens, **blobs flattening or merging** under the field, and **fizzing globules escaping**. Game-readability sources agree that **shape beats colour and value contrast beats hue**. So danger telegraphs should change the *silhouette* (spikes growing, stretching) more than the tint, and must never make the visual bigger than the hitbox.

### Cited Findings
- Over a magnet, ferrofluid spikes **multiply and finally break off into separate droplets.** This is a real-world "overload" sequence. — [arXiv 1711.09942](https://arxiv.org/pdf/1711.09942)
- Ultrasonically levitated droplets can **atomise**, with many complex breakup effects shown in high-speed imaging. — [arXiv 2307.00400 (title: Atomisation of an acoustically levitated droplet)](https://arxiv.org/pdf/2307.00400)
- Effervescent fizz makes the water sphere **grow, turn milky and shed globules**. — [SlashGear](https://www.slashgear.com/watch-an-effervescent-tablet-fizz-inside-a-ball-of-water-in-space-28394568)
- HL2 shows field state changes with a **blue-to-red blink** on its interface. — [Combine OverWiki: Combine Interface](https://combineoverwiki.net/wiki/Combine_Interface)
- **Readability guidance:** every hazard should be recognisable as a filled shape, and tint is a weaker signal than shape. Most readability failures are value (lightness) failures, so check in greyscale. If everything draws attention, nothing does. — [Bugnet: Readable game art](https://bugnet.io/blog/readable-game-art-why-clarity-beats-detail); [Game Developer: reduce visual confusion](https://gamedeveloper.com/design/how-to-reduce-visual-confusion-in-your-game)
- One indie dev found red spikes did not contrast with a brown/orange cave, and red was already used for other harmful things. The fix was a darker grey body with the danger colour as cracks on it. — [Lost Caves devlog #10](https://adscomics.itch.io/lost-caves/devlog/173099/lost-caves-devlog-10-subtle-indicators)
- Playtesters complained when spike art and hitbox did not match (triangle art, square collider). — [GB Studio Central](https://gbstudiocentral.com/?p=2094); [itch.io feedback thread](https://itch.io/post/9877412)
- Red means danger in many Western contexts but is auspicious in parts of Southeast Asia. — [IGDA: Color stories in game design](https://igda.org/news-archive/color-stories-in-game-design/)

### Inferences (tension cues per path type)
- **Linear ping-pong / accelerating:** stretch the acid along the velocity (squash and stretch; the acoustic blobs' flattening gives a physical excuse). Spikes lengthen with speed, and a short droplet trail appears only above a speed threshold. *Risk:* stretch must stay inside the collider or the collider must stretch too. *Cost:* SH (velocity uniform from script), Low–Med.
- **Turnaround at path ends:** the field "strains". There is a brief rim flicker and the core lamp blinks yellow just before reversing, giving players a rhythmic cue. *Cost:* script plus shader property, Low.
- **Circular / rotating:** spike crown or swirl speed tied to angular velocity. The direction of travel can be read from the spike lean. *Cost:* SH, Low.
- **Idle "life":** slow wobble (G) plus an occasional drip that forms on the underside and is pulled back up (failed escape). It reads as alive without new lethal geometry. *Cost:* SH or tiny P, Low.
- **Cracks/overheat on the core (Toriyama-style machine):** hairline cracks or a little steam puff on the core when the hazard reaches top speed. This is purely decorative, inside the silhouette. *Cost:* S swap, Low.
- **Greyscale test:** the acid green against sky blue (#71BAEA) and orange ground may be close in *value*. Give the acid a dark ink outline (2–3 px at gameplay zoom). Then the hazard reads by outline plus spike shape, even for colour-blind players. The acid colour is already used for static acid strips, which is good: the same material means the same rule ("green = death") everywhere.
- **Do not reuse launch-pad red as the main hazard colour.** Red is already "good" (launch pads). Keep red only for small status lamps.

### Gaps
- No controlled studies on hazard readability at high speed were found. Guidance comes from dev blogs and articles.
- Exact value contrast between the acid green and the background could not be checked here (the exact acid hex is not given). It should be checked in-engine with a greyscale capture.

---

## 5. Taste references: Toriyama machines and Javier Rodríguez colour. How do they shape these devices? (Türkçe özet dahil)

### Takeaway
Toriyama's machines are **round, egg-shaped, chunky, with exaggerated proportions and charm over function**. They point to a stubby, friendly-looking core with obvious parts. That makes the lethal part (the acid) clearly separate from the cute machine. Rodríguez uses **colour as emotion, flat and clear compositions, pop-art (Lichtenstein, Warhol, Glaser) and surrealism (Magritte)**, and tunes surrounding colours so key colours look **fluorescent**. This supports making the acid the one "fluorescent" element against a calmer device, plus halftone/Ben-Day dots that match the platform print look.

### Cited Findings
- Toriyama's mechanical designs are known for **exaggerated proportions and an "egg" shape**, drawn in his rounded, deformed style but with real detail. He mixed real hardware (Reliant Scimitar, Horten Ho 229, P-51 Mustang on a Dr. Slump cover) with invented machines (Capsule Corp vehicles). *SandLand* vehicles borrow from WWII designs. — [Zimmerit: Unappreciated mechanical design of Akira Toriyama](https://www.zimmerit.moe/akira-toriyama-mechanical-designs/)
- His machines are said to "ooze charm and fun", and machines appear in nearly all his works. — [Gizmodo](https://gizmodo.com/akira-toriyama-mechanical-design-dragon-ball-sand-land-1851320681); [Jalopnik: Toriyama is a gearhead](https://jalopnik.com/the-creator-of-dragon-ball-is-an-unabashed-gearhead-1849136374); [Jalopnik: automotive creations](https://jalopnik.com/these-are-some-of-akira-toriyamas-best-automotive-creat-1851321944)
- Javier Rodríguez both draws and colours *Absolute Martian Manhunter*. Muted colours mark John's everyday FBI life and vivid colour marks the Martian. Colour follows mood rather than realism (a sad character turns blue). He wants pages clear and uncluttered. — [SKTCHD interview](https://sktchd.com/art-feature/javier-rodriguez-absolute-art-feature/)
- His influences include Warhol, Lichtenstein, Milton Glaser, Magritte (surrealism), Guy Peellaert and Jean-Claude Forest (*Barbarella*). He built the Martian's palette from the original green, red, blue and yellow plus a few extra colours, and **tuned the surrounding palette so the Martian's colours look fluorescent**. — [SKTCHD interview](https://sktchd.com/art-feature/javier-rodriguez-absolute-art-feature/)
- Reviewers call the art surreal and psychedelic, with 60s–70s sci-fi comics as a reference point, and note whole pages flooding red when blood flows. — [Popverse](https://www.thepopverse.com/comics-absolute-martian-mahunter-dc-comics-art-javier-rodriguez/); [GamesRadar](https://www.gamesradar.com/comics/marvel-comics/70-years-after-the-martian-manhunters-debut-the-justice-league-mainstay-has-been-reinvented-in-the-absolute-dc-universes-most-daring-and-compelling-series-yet); [DC blog](https://www.dc.com/blog/2025-12-05/absolute-martian-manhunter-shows-you-your-absolute-self); [Graphic Policy](https://graphicpolicy.com/2025/03/28/absolute-martian-manhunter-1-pushes-the-boundaries-of-the-human-mind-and-the-comic-book-medium)

### Inferences
- **Toriyama core recipe (inference; the article does not cover rivets or panel lines):** an egg- or capsule-shaped core in ink or plum with one paper-coloured highlight, 1–2 big round bolts, a stubby antenna or vent, and a stencilled number ("No.3"). It should look like a friendly gadget that happens to hold death, which matches the game's mix of children's-book NPCs over a hard world. The bar version is a stubby capsule-ended rod (like a Capsule Corp capsule stretched out).
- **Rodríguez colour rule:** keep the device in flat ink, paper and one accent, and make the acid the *only* near-fluorescent colour on screen apart from the player pink. Tune the surroundings (slightly desaturated device colours) so the acid "glows" without a bloom halo, which also keeps the hitbox honest.
- **Pop-art halftone:** Ben-Day or halftone dots inside the acid, in a darker green, match the platform halftone print look (smooth print, not pixel art). They can scroll with the swirl, which gives a stylistic link between hazards and platforms.
- **Surreal touch (Magritte):** a flat, diagram-like look. Dashed orbits, a number label and a printed shadow offset under the hazard make it read like an illustrated technical plate, which is cheap and stylish.

### Türkçe sonuç ve öneriler (final takeaways)
- **En güçlü üç fikir (önerilen kombinasyon):**
  1. **Ferrosıvı diken tacı (J) + sallanan küre (G):** Daire tehlikede mürekkep renkli çekirdek, çevresinde asit halkası. Halkanın dış kenarında halka boyunca kayan, "alan gücüyle" boyu atan kısa dikenler olsun. Diken siluet olarak "ölümcül" mesajını renkten bağımsız verir. Okunabilirlik en iyi, maliyeti tek bir SDF shader.
  2. **Akustik levitatör çubuğu (K) / Combine emitörü (B):** Dikdörtgen tehlikede iki uçta tombul emitör kapağı (hoparlör ızgarası ya da bobin şeritleri), arada düzgün aralıklı, hafif basık asit damlaları ya da sürekli asit kılıfı olsun. "Cihaz sıvıyı tutuyor" tek bakışta okunur. Damlalar arasındaki boşluklar ya gerçekten güvenli olmalı ya da ince bir asit filmiyle doldurulmalı.
  3. **Toriyama çekirdeği + durum lambası:** Yumurta/kapsül biçimli, cıvatalı, numaralı sevimli bir çekirdek. Üstündeki küçük lamba boşta kâğıt rengi, harekete geçmeden önce sarı, tam hızda kırmızı olsun. Hem "makine canlı" hissi verir hem de telegraf işi görür.
- **Adalet kuralları:** (a) Görsel hiçbir zaman çarpışma kutusundan büyük görünmesin; dış parıltı (glow/halo) yok, parıltı silüetin içinde kalsın. (b) Halkalar, kapaklar gibi mekanik parçalar ya asit silüetinin içinde ya da onlar da öldürücü olsun. (c) Asidin etrafında 2–3 px koyu mürekkep kontur olsun, gri tonlama testinde de okunsun. (d) Yol/ip çizgileri ince, kesikli, mürekkep ya da kâğıt renkli olsun, asla asit yeşili olmasın. (e) Kırmızı lansman pedlerinde "iyi" anlamında kullanıldığı için ana tehlike rengi olmasın, sadece küçük lambalarda kullanılsın.
- **Gerilim/telegraf:** Hızlanan tehlikelerde dikenler uzasın ve asit hız yönünde gerilsin. Ping-pong dönüş noktasında kısa bir kenar titremesi ve sarı lamba yanıp sönsün. Boşta yavaş dalgalanma olsun, alttan damla sarkıp geri çekilsin.
- **Renk:** Rodríguez gibi cihazı düz ve sakin tut (mürekkep, kâğıt, tek aksan), asit ekrandaki tek "floresan" renk olsun (oyuncu pembesi hariç). Asidin içinde koyu yeşil halftone/Ben-Day noktaları platformlarla aynı baskı dilini kurar.
- **Maliyet sırası (ucuzdan pahalıya):** Çekirdek sprite + dönen halka sprite'ları (S) → tek polar/kapsül SDF asit shader'ı (dalgalanma + diken + girdap + halftone, SH) → durum lambası scripti → isteğe bağlı damla parçacıkları (P) → ip/yol çizgisi (L). İlk prototip için **tek bir SDF shader + çekirdek sprite'ı** yeterli. Sıvının hissi oyun hissi kapsamında, son ayar geliştiricinin gamepad testine kalır.

### Gaps
- No source describes Toriyama's specific detailing vocabulary (rivets, panel lines, numbering). The core recipe above is inference from general descriptions and should be checked against actual Dr. Slump / Dragon Ball pages.
- The Rodríguez interview does not discuss flat colour or halftone explicitly. The halftone link comes from his stated Lichtenstein/pop-art influence and is my inference.
