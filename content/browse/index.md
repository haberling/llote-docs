# Browse

```corner-tag
text: "Browse"
```

`llote browse` is a pageable TUI over the default file (`llote browse notes.llote` for a named one). Start menu and a double-clicked `.llote` file land here too.

```table
a: Tab
b: Key
c: What it does
rows:
  - a: Add
    b: Ctrl+A
    c: type a note and press Enter
  - a: Find
    b: Ctrl+F
    c: search by meaning; a handful of best matches, top one highlighted
  - a: Rank
    b: Ctrl+R
    c: same search, every candidate listed by relevance, nothing pre-selected
  - a: New
    b: Ctrl+N
    c: create a new .llote file
  - a: Load
    b: Ctrl+L
    c: open an existing file
  - a: Settings
    b: Ctrl+F2
    c: splash, default file, embedding model, search index, docs
  - a: Delete
    b: Ctrl+D
    c: remove the highlighted note, with a Yes/No confirm
```

`llote settings` opens browse already on the Settings tab. Esc leaves Settings, Load, or New and returns to Add; Esc from Add quits.
