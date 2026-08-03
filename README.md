# AI Usage Notes

This file tracks gameplay-system changes that were implemented with AI assistance.

## Personal Note

Math-heavy logic is not my strongest area, so I use AI/LLMs to help design and validate systems with heavier math components, such as the enemy separation system.

## Enemy Namespace

### EnemySeparationSystem
- Enemy push separation logic was created, refactored, and rewritten with AI assistance.
- Added expected-capacity settings and pre-sized runtime collections in `Awake`.
- Reworked grid clearing to clear only active cells, with periodic stale-cell pruning for long sessions.
- Debug push tracking now runs only when debug and push-vector drawing are enabled.

### EnemyPushRegistry
- Pre-sized `Active` list and added `EnsureCapacity(int)` to reduce resize spikes when many enemies register.

## Player Namespace

### PlayerController
- Added configurable cast buffer capacity and pre-sized `_hits` list in `Awake` to avoid growth spikes.

## Relic and Player Stats Foundation

- The initial foundation and direction for the relic and player stats system were laid out by me.
- The implementation was then expanded with AI assistance into concrete runtime/data structures and player stat wiring.