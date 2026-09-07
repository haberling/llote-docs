# llote docs v1.0.0.0

## Install

llote is a Windows command-line tool and TUI, available from two places:

* The Store download gives you less fine-grained control over the uninstall process. *

Once installed, you can run llote from the command line (`llote`) or from the Start menu.

## Quick Start

```
llote "bought milk"
llote
llote ask "what did I write about the dentist?"
llote browse
```

Notes are single lines, timestamped automatically the moment you write them.
`ask` searches by meaning, not exact wording — and only ever returns a line
you already wrote, verbatim.

## Commands

- `llote` — list notes from the default file
- `llote <file.llote>` — list notes from that file
- `llote "text"` — add a note to the default file
- `llote <file.llote> "text"` — add a note to that file
- `llote new` — create `./log.llote`
- `llote new <file.llote>` — create that file
- `llote setup` — download the embedding model ahead of time
- `llote pop` — remove the last note from the default file
- `llote pop <file.llote>` — remove the last note from that file
- `llote ask "question"` — semantic search over your notes
- `llote browse` — pageable TUI over the default file
- `llote browse <file.llote>` — same, over that file
- `llote docs` — print this site's URL
- `llote help` — print the command structure

## CLI Tips

`llote` with no arguments dumps the whole file. On a long one, pipe it: `llote | more`. `head` works too — llote treats a closed pipe as done, not an error. Same idea for a quick filter: `llote | findstr /i dentist`.

A note with spaces has to be one argument. `llote bought milk` is unrecognized; `llote "bought milk"` is the note. Same for questions: `llote ask "what did I write about the dentist?"`.

`llote ask <file.llote> "question"` searches a named file the same way `llote <file.llote> "text"` writes to one. `llote` lists; `llote browse` is the TUI.

## Footguns

A first argument ending in `.llote` is always a file path, never note text. `llote called-the-dentist.llote` tries to list that file. The match is case-sensitive, so `log.LLOTE` would actually get logged as a note.

`new`, `setup`, `pop`, `ask`, `browse`, `docs`, and `help` are reserved as the first word. `llote pop` does not write a note that says "pop" — it offers to delete your last one (y/N, default no). To log a reserved word, pass it as the second argument after a file: `llote log.llote pop`.

Extra words after a command are an error — `llote browse notes` without the `.llote` suffix is unrecognized.

## Search

`ask` finds notes by meaning, not exact wording — it never generates text; it only ever returns lines you already wrote, verbatim. As your file grows, `ask` will return more alternate results (max 10).

Each note is embedded (turned into a vector) by a small local model. Your question gets embedded the same way, and llote ranks every note by cosine similarity to it — closest meaning wins, not closest spelling.

Everything runs locally once the model's on disk. `llote setup` downloads it ahead of time; otherwise the first `ask` or `browse` search does it for you, which needs internet access once.

Embeddings are cached. Each note's embedding is stored in an index file in app data, keyed by a hash of its timestamp and text, which cuts down on `ask` times.

## Time Windowing

`ask` recognizes a small, fixed set of time expressions and uses them to narrow which notes are even considered, before any meaning-based matching happens. **The expression must be at the end of the question** — `ask "what did I write about the dentist last week"` works; putting `last week` first does not.

Recognized phrases: `today`, `yesterday`, `this week`, `last week`, `this month`, `last month`, `last # [days/weeks/months/years]` — plus an explicit date, `2026-09-01` or the short form `26-09-01`. Prefix any of those with `before`: `before last 8 days` searches everything older than 8 days ago. The time phrase is stripped before the rest gets embedded, so the dentist example searches for "what did I write about the dentist", scoped to last week's notes only.

If the whole question is just a time phrase, llote skips meaning-based ranking and returns matching notes by recency, newest first.

## Getting the Most Out of Search

Write notes the way you'd actually search for them. Search is meaning-based, so wording doesn't have to match exactly — but the words you use still shape the embedding. "milk" and "bought milk at the store" won't rank the same against a question like "what groceries do I need?"

Combine a time phrase with an actual question to cut a big file down fast: `ask "dentist last week"` scopes the search before ranking even starts, instead of paging through everything.

A question that's only a time phrase — `ask "yesterday"` — gets you a quick recency-ordered recap instead of a ranked search, since there's no question text left to rank by.

Results cap at 10 even in a large file. If you need more than that, narrow with a time phrase first rather than expecting one `ask` to surface everything.

## Default File

llote picks which file to read from and write to when you don't name one explicitly. It also determines which file `llote browse` opens when no file is named. First match wins:

1. **A local `.\log.llote`** — used automatically if one exists in the current directory (and "always use global notes" is off).
2. **The default file** — a custom default file, if you've set one in Settings; otherwise the global notes file at `%APPDATA%\Habersoft\llote\log.llote`.

`llote new` creates that local `.\log.llote` for you; once it exists,
llote prefers it over the default file with no setting required — a
custom default file only ever replaces the global fallback; it doesn't
override a local file that's already there.

`Ctrl+F2` opens settings in the TUI. From there you can set a custom default file and/or set "always use global notes" to skip over a local `.\log.llote`.

## File Format

Plain text, append-only, human-editable.

```
#llote-id:3fae1c2e-9d7b-4b3a-8c1e-2a1e6f9d1234
2026-09-03T14:02:11-05:00	bought milk
2026-09-03T14:07:44-05:00	call dentist tomorrow
```

Line one is a header llote uses to key its own search cache in app data —
never a sidecar file next to your notes. Every other line is a timestamp
and your text, tab-separated. As plain text, it plays nicely with Git and AI assistants.

Be careful editing a `.llote` file by hand. Damaging the `llote-id` can make the file unopenable (deleting the ID line outright will fix this). Lines without a properly formatted date may be harder for llote to find during an `ask`.

## Philosophy & FAQ

**Does llote use AI to write anything?**
No. `ask` only ever returns a line you already wrote, verbatim. It can't generate text. No hallucinated notes.

**Does anything leave my machine?**
The search model is downloaded locally the first time you run
`ask` or `setup`. Your notes are never sent anywhere during writing or searching. Try turning your Wi-Fi off; the program runs all the same. The [Privacy Policy](/privacy/) spells this out.

**Why "llote"?**
There are only two hard problems in software engineering: concurrency and naming things. Its name needed to be short and unique. I just smashed LLM and note together to denote its AI search feature alongside the note-taking core.

## Privacy Policy

Last updated: 7 September 2026.

This policy covers the llote application. This documentation site is separate.

**The application.** Habersoft does not collect, receive, or store your notes. llote has no account, analytics, or crash reporting.

Notes you write are stored on your device as plain text in a `.llote` file: `.\log.llote` if one exists, a path you name, or `%APPDATA%\Habersoft\llote\log.llote`. Search embeddings are stored in `%APPDATA%\Habersoft\llote\index`. Settings are stored in `%APPDATA%\Habersoft\llote`. Those files are used only to run llote on that device. They are not sent to Habersoft.

You can read, edit, or delete them in Explorer. Uninstall can remove the app-data notes file, index, model, and settings. `.llote` files outside app data are not removed.

**Network.** `llote setup` and the first `ask` download the embedding model and ONNX Runtime from their publishers (Hugging Face and Microsoft). Notes are not included in that request. After those files are on disk, llote does not use the network. `llote docs` and the Jump List open this site in your browser.

**This site.** llote.consoland.net is a static site hosted on GitHub Pages. Requests for these pages are subject to GitHub's privacy statement. Microsoft Store has its own privacy statement for Store acquisition and updates.

**Contact.** Habersoft, a trade name registered in North Dakota, via the Microsoft Store listing for llote, or https://consoland.net.

## License

Copyright 2026 Habersoft. All rights reserved. Habersoft is a trade name registered in North Dakota.

llote is free for use in any environment, including personal, academic, commercial, government, business, non-profit, and for-profit. "Free" means there is no cost or charge associated with the installation and use of llote.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software (the "Software"), to use the Software.

You may not modify, adapt, rent, lease, loan, sell, redistribute, or create derivative works based upon the Software or any part thereof.

This license does not apply to the contents of `.llote` files.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

The embedding model (`BGE-small-en-v1.5`) is downloaded separately under BAAI's MIT license. Arvo, used on this site, is under the SIL Open Font License.
