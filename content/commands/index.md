# Commands

- `llote "text"` — add a note to the default file
- `llote` — list notes from the default file
- `llote <file.llote>` — list notes from that file
- `llote <file.llote> "text"` — add a note to that file
- `llote new` — create `./log.llote`
- `llote setup` — download the embedding model ahead of time
- `llote pop [file]` — remove the last note
- `llote ask "question"` — semantic search over your notes
- `llote browse [file]` — a pageable TUI note browser

A bare argument is a file path if it ends in `.llote`, otherwise it's note
text or a question — `new` and `ask` are the only reserved leading words.
