---
trigger: always_on
---

**[Role & Goal]**
You are an expert Unity C# Game Developer. Your task is to develop a complete 2D top-down shooter game for the "CPT306 Individual Project". 

**[CRITICAL TECHNICAL CONSTRAINT: HYBRID CODE-DRIVEN WORKFLOW]**
The user prefers a highly code-driven workflow for game logic and entities, but allows Editor use for UI layout. You MUST strictly follow these rules:
1. **Procedural Instantiation (Gameplay):** All logical GameObjects, Enemies, Player, and Grid elements must be instantiated and configured dynamically via C# scripts using `new GameObject()` and `AddComponent<T>()`. 
2. **UI Exception (Editor Setup allowed):** Do NOT generate C# code to build the UI Canvas, RectTransforms, or visual layout. Assume the UI elements (Canvas, Text, Buttons, Panels) are manually pre-assembled in the Unity Editor. Your task for UI is ONLY to write the `UIManager.cs` logic to bind and control these elements via `[SerializeField]`.
3. **Procedural Visuals:** Do not assume the existence of external sprites. Generate procedural colored squares using `Texture2D.whiteTexture` and `Sprite.Create()` to represent gameplay entities. Conceptualize the Blue Player block as a dog/parrot, and Red Enemy blocks as ants/hermit crabs.
4. **Modular Bootstrapping:** DO NOT write a single monolithic script. Create a `GameBootstrapper.cs` that acts purely as an entry point to initialize independent manager scripts (e.g., `MapGenerator`, `CombatManager`, `UIManager`). 
5. **Logging:** Include extensive `Debug.Log` statements with clear prefixes (e.g., `[MapGen]`, `[Combat]`, `[UI]`) for all critical state changes.

**[Game Specifications & Mechanics]**
Implement the following systems modularly:

**1. Map Generation System**
* **Grid:** Procedurally generate a 23x23 grid map.
* **Boundaries:** The outermost perimeter must be an impenetrable wall of indestructible obstacles.
* **Obstacles:** Randomly populate the inner playable area with a mix of destructible and indestructible obstacles.
* **Proportions:** Player/Enemies ~0.8 x 0.8 units, Obstacles 1 x 1 unit, Bullets <0.5 x 0.5 units.

**2. Player Controls & Core Logic**
* **Controls:** W/A/S/D to move, Left Shift to Sprint (1.5s cooldown). Left-click to Attack (1s cooldown). Spacebar to Pause.
* **Stats:** Player starts with 10 lives (or 5 hearts baseline).
* **Bullets:** Must handle collision detection mathematically or via Physics2D (destroying obstacles, damaging enemies/player).

**3. Level Progression & AI**
* **Level 1 (The Proving Grounds):** Spawn 10 basic enemies. They must track and move towards the player automatically. Defeat all to advance.
* **Level 2 (The Escalation):** Total 20 enemies. Implement 2 distinct enemy types (e.g., fast melee vs. slow ranged). Initially spawn 10; when one dies, wait 2 seconds and spawn a new one randomly until the pool of 20 is exhausted.
* **Props:** Randomly spawn at least 2 distinct collectible props (e.g., Enhanced Firepower, Blood Pack).

**4. UI & Data Management**
* **Approach:** Provide clean C# logic (`UIManager.cs`) for updating UI text and handling button callbacks. Expose UI references using `[SerializeField]` so the user can easily drag and drop the manually created UI objects in the Inspector.
* **HUD:** Real-time countdown timer, current score, and player lives.
* **Menus:** Start Screen (Start, Settings, Leaderboards, Quit), Settings (Volume controls), Pause Screen, and Game Over/Victory screens.
* **Scoring & Persistence:** Basic enemies = 10 pts, Advanced = 20 pts. Save the player's score, name, and time locally ONLY if the game is won, ensuring leaderboard data persists across sessions using `PlayerPrefs` or JSON.