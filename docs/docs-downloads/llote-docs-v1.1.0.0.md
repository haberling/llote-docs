# llote docs v1.1.0.0

## Install

llote is a Windows command-line tool and TUI, available from two places:

Once installed, you can run llote from the command line (`llote`) or from the Start menu. A Start menu launch or a double-clicked `.llote` file opens in Windows Terminal when it's installed.

Your notes live in Documents, not with the app. Uninstalling leaves them in place. The Store download still removes the cached model, search index, and settings with the app; the installer from consoland.net asks first.

## Quick Start

```
llote -- bought milk
llote
llote ask -- what did I write about the dentist?
llote browse
```

Notes are single lines, timestamped automatically the moment you write them.
`ask` searches by meaning, not exact wording — and only ever returns a line
you already wrote, verbatim.

## Browse

`llote browse` is a pageable TUI over the default file (`llote browse notes.llote` for a named one). Start menu and a double-clicked `.llote` file land here too.

| Tab | Key | What it does |
| --- | --- | --- |
| Add | Ctrl+A | type a note and press Enter |
| Find | Ctrl+F | search by meaning; a handful of best matches, top one highlighted |
| Rank | Ctrl+R | same search, every candidate listed by relevance, nothing pre-selected |
| New | Ctrl+N | create a new .llote file |
| Load | Ctrl+L | open an existing file |
| Settings | Ctrl+F2 | splash, default file, embedding model, search index, docs |
| Delete | Ctrl+D | remove the highlighted note, with a Yes/No confirm |

`llote settings` opens browse already on the Settings tab. Esc leaves Settings, Load, or New and returns to Add; Esc from Add quits.

## Search

Searching is done by Find and Rank in browse, and by `ask` from the command line. All three find notes by meaning, not exact wording — they never generate text; they only ever return lines you already wrote, verbatim. As your file grows, `ask` and Find will return more alternate results, up to 10. Rank lists every candidate.

Each note is embedded (turned into a vector) by a local model. Your question gets embedded the same way, and llote ranks every note by cosine similarity to it — closest meaning wins, not closest spelling.

The default model is BGE Small EN v1.5. Settings can switch you to a quantized (smaller/faster) or larger BGE model; changing models rebuilds the search index. `llote setup` downloads the current choice ahead of time; otherwise the first `ask` or browse search does it for you, which needs internet access once.

Embeddings are cached. Each note's embedding is stored in an index file in app data, keyed by a hash of its timestamp and text, which cuts down on `ask` times. Switching models, or clearing the index from Settings, starts that cache over.

## Writing Searchable Notes

Short notes with one idea are easier to find. `bought milk` is a single thought; a short question like "milk" or "what groceries do I need?" can land on it.

A long note that mixes several ideas — groceries, the dentist, and a reminder to call the bank — is one embedding covering all of that. A short question about the dentist has to compete with the rest of the line. To find that note, the search text needs to be longer and richer, closer to what you actually wrote.

When you can, split: one note per idea. When you can't, search with more of the note, not less.

## Time Windowing

`ask` recognizes a small, fixed set of time expressions and uses them to narrow which notes are even considered, before any meaning-based matching happens. **The expression must be at the end of the question** — `ask "what did I write about the dentist last week"` works; putting `last week` first does not.

Recognized phrases: `today`, `yesterday`, `this week`, `last week`, `this month`, `last month`, `last # [days/weeks/months/years]` — plus an explicit date, `2026-09-01` or the short form `26-09-01`. Prefix any of those with `before`: `before last 8 days` searches everything older than 8 days ago. The time phrase is stripped before the rest gets embedded, so the dentist example searches for "what did I write about the dentist", scoped to last week's notes only.

If the whole question is just a time phrase, llote skips meaning-based ranking and returns matching notes by recency, newest first.

## Getting the Most Out of Search

Write notes the way you'd actually search for them. Search is meaning-based, so wording doesn't have to match exactly — but the words you use still shape the embedding. "milk" and "bought milk at the store" won't rank the same against a question like "what groceries do I need?"

Combine a time phrase with an actual question to cut a big file down fast: `ask "dentist last week"` scopes the search before ranking even starts, instead of paging through everything.

A question that's only a time phrase — `ask "yesterday"` — gets you a quick recency-ordered recap instead of a ranked search, since there's no question text left to rank by.

`ask` and browse's Find tab cap at 10 even in a large file. If you need more than that, use Rank (Ctrl+R) in browse, or narrow with a time phrase first rather than expecting one `ask` to surface everything.

## Commands

| Command | What it does |
| --- | --- |
| llote [<file.llote>] | list notes |
| llote [<file.llote>] "text" | add a note |
| llote [<file.llote>] -- text | add a note, no quoting needed (joins the rest of the line) |
| llote new [<file.llote>] | create ./log.llote, or a named file |
| llote setup | download the embedding model ahead of time |
| llote pop [<file.llote>] | remove the last note |
| llote ask [<file.llote>] "question" | semantic search |
| llote ask [<file.llote>] -- question | same, no quoting needed |
| llote browse [<file.llote>] | pageable TUI |
| llote settings | same, opened straight to the Settings tab |
| llote docs | print this site's URL |
| llote help | print the command structure |

`[<file.llote>]` is optional — omit it to use the default file.

## CLI Tips

`llote` with no arguments dumps the whole file. On a long one, pipe it: `llote | more`. `head` works too — llote treats a closed pipe as done, not an error. Same idea for a quick filter: `llote | findstr /i dentist`.

`--` joins everything after it into one note or question, so you don't have to quote: `llote -- bought milk`, `llote ask -- what did I write about the dentist?`. Quotes still work (`llote "bought milk"`). Without either, `llote bought milk` is unrecognized.

`llote ask <file.llote> -- question` searches a named file the same way `llote <file.llote> -- text` writes to one. `llote` lists; `llote browse` is the TUI.

## Footguns

A first argument ending in `.llote` is always a file path, never note text. `llote called-the-dentist.llote` tries to list that file. The match is case-sensitive, so `log.LLOTE` would actually get logged as a note. To log something that looks like a filename, use `--`: `llote -- called-the-dentist.llote`.

`new`, `setup`, `pop`, `ask`, `browse`, `settings`, `docs`, and `help` are reserved as the first word. `llote pop` does not write a note that says "pop" — it offers to delete your last one (y/N, default no). To log a reserved word, use `--`: `llote -- pop`. Passing it after a file still works too: `llote log.llote pop`.

Extra words after a command are an error — `llote browse notes` without the `.llote` suffix is unrecognized.

## Default File

llote picks which file to read from and write to when you don't name one explicitly. It also determines which file `llote browse` opens when no file is named. First match wins:

1. **A local `.\log.llote`** — used automatically if one exists in the current directory (and "always use global notes" is off).
2. **The default file** — a custom default file, if you've set one in Settings; otherwise the global notes file at `%USERPROFILE%\Documents\llote\global.llote`.

`llote new` creates that local `.\log.llote` for you; once it exists,
llote prefers it over the default file with no setting required — a
custom default file only ever replaces the global fallback; it doesn't
override a local file that's already there.

`Ctrl+F2` opens settings in the TUI (`llote settings` from the command line). From there you can set a custom default file and/or set "always use global notes" to skip over a local `.\log.llote`.

## Settings

`Ctrl+F2` in browse, or `llote settings` from the command line, opens the Settings tab. Enter toggles or runs the highlighted row; Esc returns to Add.

| Section | What it does |
| --- | --- |
| Splash | show the intro animation when browse starts |
| Default File | always use global notes; pick or enable a custom default file |
| Embedding | switch the local search model; delete unused downloaded models |
| Search Index | clear the cache, or clean index data left over from deleted files |
| Documentation | open this site |

Changing models rebuilds the search index. `llote setup` downloads the current choice ahead of time. How the default file is chosen is on [Default File](/default-file/).

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
`ask` or `setup` (or when you switch models in Settings). Your notes are never sent anywhere during writing or searching. Try turning your Wi-Fi off; the program runs all the same. The [Privacy Policy](/privacy/) spells this out.

**Why "llote"?**
There are only two hard problems in software engineering: concurrency and naming things. Its name needed to be short and unique. I just smashed LLM and note together to denote its AI search feature alongside the note-taking core.

## Privacy Policy

Last updated: 15 September 2026.

This policy covers the llote application. This documentation site is separate.

**The application.** Habersoft does not collect, receive, or store your notes. llote has no account, analytics, or crash reporting.

Notes you write are stored on your device as plain text in a `.llote` file: `.\log.llote` if one exists, a path you name, or `%USERPROFILE%\Documents\llote\global.llote`. Search embeddings are stored in `%APPDATA%\Habersoft\llote\index`. Settings and downloaded models are stored in `%APPDATA%\Habersoft\llote`. Those files are used only to run llote on that device. They are not sent to Habersoft.

You can read, edit, or delete them in Explorer. Uninstall can remove the search index, model, and settings. Your notes in Documents, and any `.llote` files in your own folders, are not removed.

**Network.** `llote setup`, the first `ask`, and a model switch in Settings download the embedding model from Hugging Face, with a GitHub Releases mirror as fallback. Notes are not included in that request. After those files are on disk, llote does not use the network. `llote docs` and the Jump List open this site in your browser.

**This site.** llote.consoland.net is a static site hosted on GitHub Pages. Requests for these pages are subject to GitHub's privacy statement. Microsoft Store has its own privacy statement for Store acquisition and updates.

**Contact.** Habersoft, a trade name registered in North Dakota, via the Microsoft Store listing for llote, or https://consoland.net.

## License

Copyright 2026 Habersoft. All rights reserved. Habersoft is a trade name registered in North Dakota.

llote is free for use in any environment, including personal, academic, commercial, government, business, non-profit, and for-profit. "Free" means there is no cost or charge associated with the installation and use of llote.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software (the "Software"), to use the Software.

You may not modify, adapt, rent, lease, loan, sell, redistribute, or create derivative works based upon the Software or any part thereof.

This license does not apply to the contents of `.llote` files.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

The embedding models (BGE English v1.5, several sizes) are downloaded separately under BAAI's MIT license. Arvo, used on this site, is under the SIL Open Font License.
