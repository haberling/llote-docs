# Default File

```corner-tag
text: "Default File"
```

llote picks which file to read from and write to when you don't name one explicitly. It also determines which file `llote browse` opens when no file is named. First match wins:

1. **A local `.\log.llote`** — used automatically if one exists in the current directory (and "always use global notes" is off).
2. **The default file** — a custom default file, if you've set one in Settings; otherwise the global notes file at `%APPDATA%\Habersoft\llote\log.llote`.

`llote new` creates that local `.\log.llote` for you; once it exists,
llote prefers it over the default file with no setting required — a
custom default file only ever replaces the global fallback; it doesn't
override a local file that's already there.

`Ctrl+F2` opens settings in the TUI. From there you can set a custom default file and/or set "always use global notes" to skip over a local `.\log.llote`.
