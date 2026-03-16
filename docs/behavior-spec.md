# Flipqlo — Behavior Specification

## Visual Identity
A full-screen flip clock on a near-black background. The clock displays the
current time as four digits (HH MM) arranged in two groups separated by a colon.
Each digit sits on a dark rounded-rectangle card that is visually split into
a top half and a bottom half by a thin horizontal divider line.

## Flip Animation
When a digit changes value (e.g., 3 → 4):

1. **Top flap folds down (0 – 50% of duration):**
   - The top half of the OLD digit pivots downward around the horizontal center
     line of the card.
   - Visually simulated as a vertical scale from 1.0 → 0.0 anchored at the
     bottom edge of the top half.
   - Easing: ease-in (accelerating, like gravity pulling a flap).
   - As the flap folds, a darkening shadow grows on its face.
   - Behind the folding flap, the top half of the NEW digit is revealed (static).

2. **Bottom flap unfolds down (50% – 100% of duration):**
   - The bottom half of the NEW digit pivots from above the center line down
     into its resting position.
   - Visually simulated as a vertical scale from 0.0 → 1.0 anchored at the
     top edge of the bottom half.
   - Easing: ease-out (decelerating, like a flap settling).
   - As the flap unfolds, the shadow on its face fades away.
   - Behind the unfolding flap, the bottom half of the OLD digit is visible
     (static) until covered.

3. **Duration:** 600 ms total (300 ms per phase).

## Digit Layout
- Each digit card has an aspect ratio of 0.75 (width/height).
- Cards within the same group (HH or MM) are separated by a small gap (3% of
  card height).
- The two groups are separated by a wider gap (6% of card height) containing
  a colon.
- The colon is rendered as two small filled circles stacked vertically.
- The entire clock assembly is vertically centered on screen.
- Clock height is approximately 32% of screen height.

## Colors
- Background: #0A0A0A (near-black)
- Card face: #1C1C1C (dark gray)
- Digit text: #D8D8D8 (light gray, not pure white to reduce glare)
- Divider line: #0F0F0F (just darker than card face)
- Colon: #3A3A3A (subtle, not distracting)

## Modes (Windows only)
- **/s** — Full-screen screensaver on all monitors.
- **/p HWND** — Embedded preview in the Display Settings mini-window.
- **/c** — Configuration dialog (24h/12h, seconds toggle, screen selection).
- Exit on: any mouse move (beyond threshold), mouse click, or key press.
- Cursor is hidden during /s mode.

## Modes (Android)
- Runs as a DreamService (system screen saver / daydream).
- Full-screen, immersive, no system bars.
- Exit on any touch.
- Settings accessible via DreamService settings activity.

## Settings
| Setting        | Values         | Default |
|--------------- |--------------- |---------|
| use24Hour      | true / false   | true    |
| showSeconds    | true / false   | false   |
| primaryOnly    | true / false   | false   |

## Performance
- Rendering is idle (no repaints) when no digit is actively flipping.
- During a flip, repaint at display refresh rate (~60 fps) for smoothness.
- Timer polls current time at 250 ms intervals to detect changes.
- No GPU shaders required; all rendering uses 2D primitives (rects, text, clips, transforms).

## Audio
- None.
