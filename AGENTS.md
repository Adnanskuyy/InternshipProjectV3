# AGENTS.md

This file provides context and instructions for AI agents working on the `InternshipProjectV3` repository.

## 1. Build, Lint, and Test Commands

### Testing
This project uses the **Unity Test Framework** (based on NUnit).
Tests are located in `Assets/Tests` (or similar, depending on future structure).

*   **Run All Tests (Editor Mode):**
    ```bash
    # Note: Requires Unity Editor path to be in your environment variables or specified fully.
    # Example for Windows:
    "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -runTests -batchmode -projectPath . -testPlatform EditMode -resultsFile test_results.xml
    ```
*   **Run All Tests (Play Mode):**
    ```bash
    "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -runTests -batchmode -projectPath . -testPlatform PlayMode -resultsFile test_results.xml
    ```
*   **Run a Single Test (or Filter):**
    *   Append `-testFilter "TestNamespace.TestClass.TestMethod"` to the command.
    *   Example:
        ```bash
        ... -runTests -testFilter "MovementTests.PlayerMovement" ...
        ```

### Building
*   **Build Command (Windows Standalone):**
    ```bash
    # Requires a build script (e.g., BuildScript.cs) in Editor folder.
    "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -quit -batchmode -projectPath . -executeMethod BuildScript.BuildWindows
    ```
    *(Note: Since no build script exists yet, agents should create one if automated builds are required.)*

## 2. Code Style & Conventions

### General
*   **Language:** C# (Latest version supported by Unity 6).
*   **Frameworks:** Unity 6 (6000.3.8f1), URP, Input System.
*   **Formatting:** Allman style (opening braces on a new line).
    ```csharp
    // Good
    void MyMethod()
    {
        if (condition)
        {
            DoSomething();
        }
    }
    ```

### Naming Conventions
*   **Classes/Structs/Enums:** `PascalCase` (e.g., `PlayerController`, `GameState`).
*   **Methods:** `PascalCase` (e.g., `CalculateDamage`, `StartGame`).
*   **Public Fields/Properties:** `PascalCase` (e.g., `Health`, `IsAlive`).
*   **Private/Protected Fields:** `camelCase` (no underscore prefix).
    ```csharp
    private float health; // Correct
    private float _health; // Avoid
    private float m_health; // Avoid
    ```
*   **Parameters/Local Variables:** `camelCase` (e.g., `playerSpeed`, `damageAmount`).
*   **Interfaces:** Prefix with `I` (e.g., `IDamageable`).

### Unity Specifics
*   **Serialized Fields:** Use `[SerializeField] private` instead of `public` for inspector variables unless they truly need to be accessed by other classes.
    ```csharp
    [SerializeField] private float moveSpeed = 5f;
    ```
*   **Attributes:** Place attributes above the method/class, or inline for fields if short.
*   **Vector Math:** Use `Vector3` / `Vector2` struct methods (e.g., `Vector3.Distance(a, b)`) rather than `(a - b).magnitude` for readability.

### Error Handling
*   Use `Debug.LogWarning` or `Debug.LogError` for recoverable issues.
*   Use exceptions (`throw new System.Exception(...)`) only for critical failures that should stop execution or invalid state.
*   Validate `GetComponent` calls if the component is required.
    ```csharp
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D missing on " + gameObject.name);
    }
    ```

## 3. Cursor & Copilot Rules

### Core Behavior
*   **Context:** Always check existing code before generating new files to maintain consistency.
*   **Unity API:** Prefer `TryGetComponent` over `GetComponent` in hot paths (Update/FixedUpdate).
*   **Input System:** Use the new Input System (`UnityEngine.InputSystem`) instead of the legacy `Input.GetAxis`.
*   **Performance:**
    *   Avoid `FindObjectOfType`, `GameObject.Find`, or `GetComponent` in `Update`. Cache references in `Awake` or `Start`.
    *   Use `StringBuilder` for frequent string concatenations.
    *   Use object pooling for frequently spawned/destroyed objects.

### Testing
*   Write **EditMode** tests for pure logic (no MonoBehaviours or simple data classes).
*   Write **PlayMode** tests for physics, component interactions, and game loops.
*   Mock dependencies where possible (though difficult with MonoBehaviours, use interfaces/wrappers if architectural complexity warrants it).
