# Search

```corner-tag
text: "Search"
```

Searching is done by Find and Rank in browse, and by `ask` from the command line. All three find notes by meaning, not exact wording — they never generate text; they only ever return lines you already wrote, verbatim. As your file grows, `ask` and Find will return more alternate results, up to 10. Rank lists every candidate.

Each note is embedded (turned into a vector) by a local model. Your question gets embedded the same way, and llote ranks every note by cosine similarity to it — closest meaning wins, not closest spelling.

The default model is BGE Small EN v1.5. Settings can switch you to a quantized (smaller/faster) or larger BGE model; changing models rebuilds the search index. `llote setup` downloads the current choice ahead of time; otherwise the first `ask` or browse search does it for you, which needs internet access once.

Embeddings are cached. Each note's embedding is stored in an index file in app data, keyed by a hash of its timestamp and text, which cuts down on `ask` times. Switching models, or clearing the index from Settings, starts that cache over.


