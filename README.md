# Ubongo Puzzle Prototype

A functional 2D puzzle prototype built in Unity within a **3-day development**. This project focuses on precise spatial positioning, custom snapping mechanics, and modular code architecture.

## 🚀 Key Features

* **Custom Spatial Logic:** Implemented a manual-coordinate slot system for flexible level design.
* **Half-Unit Snapping:** Specialized logic in `Piece.cs` for 'I' and 'O' shapes to handle pivot offsets and ensure perfect visual alignment.
* **Coordinate Validation:** Uses modulo-based checks as a gatekeeper to ensure pieces only snap to valid integer coordinates, preventing overlaps.
* **Juicy Game Feel:** Integrated Coroutines with `Vector3.Lerp` and `Quaternion.Lerp` for smooth animations, paired with Particle Systems and Audio management.

## 🛠️ Technical Highlights

### Precise Snapping & Validation
The core of the positioning system lies in **`Piece.cs`**. To handle different pivot offsets between shapes, I implemented specialized logic for **'I' and 'O' pieces**:
* **Half-Unit Rounding:** Since 'I' and 'O' shapes have even dimensions, their pivots naturally sit on half-unit offsets. I used a rounding calculation to find the closest valid half-unit:
  `float snappedX = Mathf.Round(pos.x * 2f) / 2f;`
* **Validation Gate:** To ensure the piece doesn't stay in a "half-unit" position (which would look misaligned on the grid), I used a modulo check as a gatekeeper:
  `if (snapPos.x % 1 != 0) return false;`

This ensures the game remains bug-free and visually precise by forcing pieces to snap perfectly to the grid or reset.

## 🛠️ Tech Stack & Tools
* **Engine:** Unity 6000.0.53f1 [LTS]
* **Scripting:** C#
* **Version Control:** Git
* **Architecture:** Singleton Pattern and Modular Object-Oriented Design.

## 🎮 How to Play
1. Drag the puzzle pieces into the board slots.
2. Press **'R'** to rotate the selected piece (smooth 90-degree rotation).
3. Fill all slots to trigger the win condition.
4. Press **'T'** to restart the level.
