# Project Overview
- Game Title: MotoTerrores
- High-Level Concept: Horror game where the player must avoid a monster called Mikilo.
- Players: Single player.
- Inspiration / Reference Games: Amnesia, Outlast.
- Tone / Art Direction: Dark, suspenseful.
- Target Platform: PC (StandaloneOSX).
- Screen Orientation / Resolution: Landscape.
- Render Pipeline: URP (PC_RPAsset).

# Game Mechanics
## Core Gameplay Loop
The player explores the environment to reach an objective while avoiding the Mikilo monster. The monster patrols the area and can detect the player through vision (raycasts) and sound (noise levels).

## Controls and Input Methods
- Movement: WASD / Gamepad (via StarterAssets ThirdPersonController).
- Noise: Movement speed determines the noise level emitted via `GameEvents`.

# UI
- Lives Counter: Displayed via `LivesSystem` (UI Text and Slider).
- Game Over Screen: Loaded when lives reach 0.

# Key Asset & Context
- `EnemyMonster.cs`: The main AI script to be refactored.
- `PatrolRoute.cs`: Holds the points for patrolling.
- `LivesSystem.cs`: Handles player damage/death.
- `GameEvents.cs`: Decouples noise emission from detection.

# Implementation Steps
1. **Refactor EnemyMonster.cs with State Machine**
   - Implement `EnemyState` enum: `Patrolling`, `Chasing`, `Investigating`, `Stunned`.
   - Add a `PatrolRoute` field to assign via inspector.
   - Use a `switch` statement in `Update` to handle behaviors.
   - **Dependencies**: None.

2. **Improve Vision Detection**
   - Optimize vision by checking every 0.1s (Coroutine or Timer) instead of every frame.
   - Implement Field of View (FOV) logic using `Vector3.Angle` and `Physics.Raycast`.
   - **Dependencies**: Step 1.

3. **Improve Sound Detection (Investigating)**
   - When `OnNoiseChanged` is triggered and the player is within `soundRange`, set state to `Investigating`.
   - Store the noise origin as the `investigationTarget`.
   - **Dependencies**: Step 1.

4. **Implement Patrolling Logic**
   - Cycle through `PatrolRoute` points.
   - Use `_agent.remainingDistance` to trigger moving to the next point.
   - **Dependencies**: Step 1.

5. **Implement Kill/Attack Logic**
   - In `Chasing` state, if distance to player < `killDistance`, call `LivesSystem.I.LoseLife()`.
   - Add a small cooldown or "Attack" animation trigger to avoid frame-perfect life depletion.
   - **Dependencies**: Step 1.

6. **Optimization & Performance**
   - Use `sqrMagnitude` for distance checks.
   - Minimize `NavMeshAgent.SetDestination` calls (only call when target moves significantly).
   - **Dependencies**: All previous steps.

# Verification & Testing
- **Patrol Test**: Verify Mikilo moves between points assigned in `PatrolRoute`.
- **Vision Test**: Hide behind an obstacle and verify Mikilo doesn't chase. Step out and verify detection.
- **Sound Test**: Run near Mikilo to trigger the `Investigating` state.
- **Kill Test**: Let Mikilo catch the player and verify the lives decrease and the Game Over scene loads at 0 lives.
- **Stun Test**: Apply light stun and verify the monster stops moving.
