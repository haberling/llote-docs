# Settings

```corner-tag
text: "Settings"
```

`Ctrl+F2` in browse, or `llote settings` from the command line, opens the Settings tab. Enter toggles or runs the highlighted row; Esc returns to Add.

```table
a: Section
c: What it does
rows:
  - a: Splash
    c: show the intro animation when browse starts
  - a: Default File
    c: always use global notes; pick or enable a custom default file
  - a: Embedding
    c: switch the local search model; delete unused downloaded models
  - a: Search Index
    c: clear the cache, or clean index data left over from deleted files
  - a: Documentation
    c: open this site
```

Changing models rebuilds the search index. `llote setup` downloads the current choice ahead of time. How the default file is chosen is on [Default File](/default-file/).
