# Search

```corner-tag
text: "Search"
```

`ask` finds notes by meaning, not exact wording — it never generates text; it only ever returns lines you already wrote, verbatim. As your file grows, `ask` will return more alternate results (max 10).

Each note is embedded (turned into a vector) by a small local model. Your question gets embedded the same way, and llote ranks every note by cosine similarity to it — closest meaning wins, not closest spelling.

Everything runs locally once the model's on disk. `llote setup` downloads it ahead of time; otherwise the first `ask` or `browse` search does it for you, which needs internet access once.

Embeddings are cached. Each note's embedding is stored in an index file in app data, keyed by a hash of its timestamp and text, which cuts down on `ask` times.


