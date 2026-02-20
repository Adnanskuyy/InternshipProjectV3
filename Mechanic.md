Act as a Senior Unity Architect. We are building a Detective Game in Unity 6.3 (WebGL) using UI Toolkit. 

### Game Logic Update:
- **The Pool:** There is a master pool of Suspect ScriptableObjects (e.g., 5+ suspects).
- **The Selection:** Each game session, the Manager must pick exactly 4 suspects to display.
- **The Constraint:** Out of the 4 picked, exactly ONE must have the `isUser` boolean set to true in its ScriptableObject.
- **Data Responsibility:** The "True Identity" (isUser) is defined within the ScriptableObject, NOT hardcoded in the Manager.

### Technical Architecture (SOLID):
1. **SuspectData (SO):** Add `public bool isUser;` to the class.
2. **SuspectSelector (Service):** Create a standalone class/method responsible for the shuffle logic:
   - Filter the master list into two lists: `Users` and `NonUsers`.
   - Pick 1 from `Users`.
   - Pick 3 from `NonUsers`.
   - Shuffle the resulting 4 to randomize their UI positions.
3. **UI Toolkit Integration:** Use a `VisualElement` container that dynamically clones a `SuspectTemplate.uxml` for each of the 4 selected suspects.

### Your Task:
1. Define the C# `SuspectData` ScriptableObject with the `isUser` property.
2. Outline the `SuspectSelector` logic using LINQ for clean, readable filtering.
3. Describe how the `GameManager` should initialize the UI by passing the filtered list to the UI Toolkit `UIDocument`.
4. Ensure the 'Urine Test' logic only reveals the `isUser` status for the specific suspect clicked.

Analyze the folder structure and provide the C# architecture for this selection system.