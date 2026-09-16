# CLI Tips

```corner-tag
text: "Commands - CLI Tips"
```

`llote` with no arguments dumps the whole file. On a long one, pipe it: `llote | more`. `head` works too — llote treats a closed pipe as done, not an error. Same idea for a quick filter: `llote | findstr /i dentist`.

`--` joins everything after it into one note or question, so you don't have to quote: `llote -- bought milk`, `llote ask -- what did I write about the dentist?`. Quotes still work (`llote "bought milk"`). Without either, `llote bought milk` is unrecognized.

`llote ask <file.llote> -- question` searches a named file the same way `llote <file.llote> -- text` writes to one. `llote` lists; `llote browse` is the TUI.
