# LoupixDeck.Plugin.MoonPhase

LoupixDeck plugin that shows the current lunar phase on a touch button.

## Commands

| Command | What it does |
|---|---|
| `Moon.Image` — "Moon phase (image)" | Draws the current moon (photographic) |
| `Moon.Phase` — "Moon phase (text)" | Phase name + illumination % as text |

## Moon images

The 30 frames in `images/` are NASA **Dial-A-Moon** images (Moon Phase and
Libration, 2026 — SVS ID 5587), which are public domain (US government work).
They are embedded in the DLL as resources.

To regenerate the frames from NASA:

1. Download 30 hourly frames spanning one synodic month (29.53 days) from
   `https://svs.gsfc.nasa.gov/vis/a000000/a005500/a005587/frames/730x730_1x1_30p/moon.NNNN.jpg`
   (frame 1 = 2026-01-01 00:00 UTC; a new moon is at frame 424).
2. For each frame, crop the moon (680x680+24+24), resize to 140x140, key out
   the black background, and pad to 180x180 transparent:

   ```bash
   magick frame.jpg -crop 680x680+24+24 +repage -resize 140x140 \
     -fuzz 1% -transparent black -gravity center -background none -extent 180x180 \
     moon_XX.png
   ```

## Build & install

```bash
dotnet build -c Release
cp bin/Release/LoupixDeck.Plugin.MoonPhase.dll \
   bin/Release/LoupixDeck.Plugin.MoonPhase.deps.json \
   bin/Release/plugin.json \
   ~/.config/LoupixDeck/plugins/moonphase/
```

Then restart LoupixDeck.

## License

- **Code**: MIT — see [LICENSE](LICENSE).
- **Moon images** (`images/`): NASA **Dial-A-Moon** (Moon Phase and Libration,
  2026, SVS ID 5587), public domain as US government work. Credit to NASA's
  Scientific Visualization Studio is appreciated but not required.
