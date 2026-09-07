# File Format

```corner-tag
text: "File Format"
```

Plain text, append-only, human-editable.

```code
lang: text
lines:
  - text: "#llote-id:3fae1c2e-9d7b-4b3a-8c1e-2a1e6f9d1234"
  - text: "2026-09-03T14:02:11-05:00	bought milk"
  - text: "2026-09-03T14:07:44-05:00	call dentist tomorrow"
```

Line one is a header llote uses to key its own search cache in app data —
never a sidecar file next to your notes. Every other line is a timestamp
and your text, tab-separated. As plain text, it plays nicely with Git and AI assistants.

Be careful editing a `.llote` file by hand. Damaging the `llote-id` can make the file unopenable (deleting the ID line outright will fix this). Lines without a properly formatted date may be harder for llote to find during an `ask`.
