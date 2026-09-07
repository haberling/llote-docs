# Time Windowing

```corner-tag
text: "Search - Time Windowing"
```

`ask` recognizes a small, fixed set of time expressions and uses them to narrow which notes are even considered, before any meaning-based matching happens. **The expression must be at the end of the question** — `ask "what did I write about the dentist last week"` works; putting `last week` first does not.

Recognized phrases: `today`, `yesterday`, `this week`, `last week`, `this month`, `last month`, `last # [days/weeks/months/years]` — plus an explicit date, `2026-09-01` or the short form `26-09-01`. Prefix any of those with `before`: `before last 8 days` searches everything older than 8 days ago. The time phrase is stripped before the rest gets embedded, so the dentist example searches for "what did I write about the dentist", scoped to last week's notes only.

If the whole question is just a time phrase, llote skips meaning-based ranking and returns matching notes by recency, newest first.
