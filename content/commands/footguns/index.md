# Footguns

```corner-tag
text: "Commands - Footguns"
```

A first argument ending in `.llote` is always a file path, never note text. `llote called-the-dentist.llote` tries to list that file. The match is case-sensitive, so `log.LLOTE` would actually get logged as a note.

`new`, `setup`, `pop`, `ask`, `browse`, `docs`, and `help` are reserved as the first word. `llote pop` does not write a note that says "pop" — it offers to delete your last one (y/N, default no). To log a reserved word, pass it as the second argument after a file: `llote log.llote pop`.

Extra words after a command are an error — `llote browse notes` without the `.llote` suffix is unrecognized.
