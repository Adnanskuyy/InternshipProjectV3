Act as a Senior Unity UI Developer. We are in the final polish phase.

### Task 1: Persistent Urine Test
- In the `SuspectDetailController`, check the `SuspectData` for a `bool hasBeenTested`.
- If true, hide the 'Test Button' and show a 'Result Stamp' element.
- Ensure this state persists even when switching between suspects.

### Task 2: Tutorial Highlight Fix & Juice
- Refactor the Tutorial Highlighter to use `worldBound` to ensure it never misses the target box.
- Create a 'Breathing' animation in C# using `target.experimental.animation.Scale(1.1f, 1000)` or via USS transitions to make the highlight glow and pulse.
- Add a `box-shadow` or `border-color` glow effect to make it pop on a projector.

### Task 3: Implementation
- Provide the updated `TutorialManager.cs` snippet that handles the real-time positioning.
- Provide the USS for the `.tutorial-glow` class.