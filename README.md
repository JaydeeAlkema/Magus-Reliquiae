# AI Usage Notes

## Enemy namespace
- **EnemyPushRegistry**: Pre-sized `Active` list and added `EnsureCapacity(int)` to reduce resize spikes when many enemies register.
- **EnemySeparationSystem**: Added expected-capacity settings and pre-sized runtime collections in `Awake`.
- **EnemySeparationSystem**: Reworked grid clearing to clear only active cells, plus periodic stale-cell pruning for long sessions.
- **EnemySeparationSystem**: Debug push tracking now runs only when debug + push-vector drawing is enabled.

## Player namespace
- **PlayerController**: Added configurable cast buffer capacity and pre-sized `_hits` list in `Awake` to avoid growth spikes.