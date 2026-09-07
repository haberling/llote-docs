# CLI Tips

```corner-tag
text: "Commands - CLI Tips"
```

`llote` with no arguments dumps the whole file. On a long one, pipe it: `llote | more`. `head` works too — llote treats a closed pipe as done, not an error. Same idea for a quick filter: `llote | findstr /i dentist`.

A note with spaces has to be one argument. `llote bought milk` is unrecognized; `llote "bought milk"` is the note. Same for questions: `llote ask "what did I write about the dentist?"`.

`llote ask <file.llote> "question"` searches a named file the same way `llote <file.llote> "text"` writes to one. `llote` lists; `llote browse` is the TUI.
