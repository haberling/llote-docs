# Commands

```corner-tag
text: "Commands"
```

`[<file.llote>]` is optional — omit it to use the default file.

```table
a: Command
c: What it does
code: true
rows:
  - a: "llote [<file.llote>]"
    c: list notes
  - a: 'llote [<file.llote>] "text"'
    c: add a note
  - a: "llote [<file.llote>] -- text"
    c: add a note, no quoting needed (joins the rest of the line)
  - a: "llote new [<file.llote>]"
    c: create ./log.llote, or a named file
  - a: llote setup
    c: download the embedding model ahead of time
  - a: "llote pop [<file.llote>]"
    c: remove the last note
  - a: 'llote ask [<file.llote>] "question"'
    c: semantic search
  - a: "llote ask [<file.llote>] -- question"
    c: same, no quoting needed
  - a: "llote browse [<file.llote>]"
    c: pageable TUI
  - a: llote settings
    c: same, opened straight to the Settings tab
  - a: llote docs
    c: print this site's URL
  - a: llote help
    c: print the command structure
```

