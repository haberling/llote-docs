// Concatenates notebook pages into one markdown download, same
// depth-first order as root-copy/js/notebook.js flattenNav() and
// tools/page-number.cs, skipping the cover, Contents, and this Docs
// Archive page. Writes root-copy/docs-downloads/llote-docs-v{ver}.md
// (copied to docs/ by Canary), regenerates that folder's index.html
// from whatever version files are already there -- old dumps stay --
// and appends an archive-link widget listing those files onto this page.
//
// Applied on content/docs-archive/. A different CANARY_ROUTE_PATH
// passes stdin through unchanged so the tool is safe if someone adds
// it to another directory by mistake.
//
// Reads source index.md files from disk, not toolchain stdout, so
// injected page-number / toc-list fences never appear. Widget fences
// are deleted except `code`, which is unspun into a bare markdown
// fence. Remaining ATX headings are demoted one level so the dump's
// own H1 stays unique.
//
// Version comes from the cover-plate fence in content/index.md.
// Each dump's SHA-256 and last-changed date live in
// docs-downloads/manifest.json. The markdown file is rewritten and
// the date bumped only when that hash changes, so a no-op rebuild
// keeps the previous "updated" stamp on the archive-link.
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);

var source = Console.In.ReadToEnd();
var routePath = Environment.GetEnvironmentVariable("CANARY_ROUTE_PATH") ?? "";
if (routePath != "docs-archive")
{
    Console.Out.Write(source);
    return;
}

var manifestPath = Environment.GetEnvironmentVariable("CANARY_MANIFEST_PATH")
    ?? throw new InvalidOperationException("CANARY_MANIFEST_PATH not set");

var contentRoot = Path.GetDirectoryName(manifestPath)
    ?? throw new InvalidOperationException("manifest path has no directory");
var siteRoot = Path.GetDirectoryName(contentRoot)
    ?? throw new InvalidOperationException("content root has no parent");
var outDir = Path.Combine(siteRoot, "root-copy", "docs-downloads");
Directory.CreateDirectory(outDir);

var coverPath = Path.Combine(contentRoot, "index.md");
var version = ReadCoverVersion(coverPath)
    ?? throw new InvalidOperationException($"no cover-plate version in {coverPath}");
if (!Regex.IsMatch(version, @"^[\w.\-]+$"))
    throw new InvalidOperationException($"refusing dump filename version '{version}'");

var manifestJson = File.ReadAllText(manifestPath);
var root = JsonNode.Parse(manifestJson)
    ?? throw new InvalidOperationException($"failed to parse manifest at {manifestPath}");
var navArray = root["nav"]?.AsArray()
    ?? throw new InvalidOperationException("manifest has no 'nav' array");

var dump = new StringBuilder();
dump.Append("# llote docs v").Append(version).Append("\n\n");

foreach (var path in FlattenNav(navArray))
{
    if (path is "" or "contents" or "docs-archive") continue;
    var pageFile = PageFile(contentRoot, path);
    if (!File.Exists(pageFile))
        throw new InvalidOperationException($"missing page source {pageFile} (nav path '{path}')");
    var body = TransformPage(File.ReadAllText(pageFile));
    if (body.Length == 0) continue;
    dump.Append(body).Append("\n\n");
}

var markdown = CollapseBlankLines(dump.ToString()).TrimEnd() + "\n";
var dumpName = $"llote-docs-v{version}.md";
var hash = Sha256Hex(markdown);
var metaPath = Path.Combine(outDir, "manifest.json");
var versions = LoadVersions(metaPath);
UpsertCurrentDump(outDir, versions, dumpName, markdown, hash);
EnsureEntriesForExistingDumps(outDir, versions);
WriteVersions(metaPath, versions);
var dumps = ListDumpFiles(outDir, dumpName, versions);
WriteArchiveIndex(outDir, dumps);
Console.Out.Write(source.TrimEnd() + "\n\n" + ArchiveLinkFence(dumps));

static List<string> FlattenNav(JsonArray items)
{
    var paths = new List<string>();
    foreach (var itemNode in items)
    {
        var item = itemNode!.AsObject();
        paths.Add(item["path"]?.GetValue<string>() ?? "");
        if (item["children"] is JsonArray children && children.Count > 0)
        {
            paths.AddRange(FlattenNav(children));
        }
    }
    return paths;
}

static string PageFile(string contentRoot, string path)
{
    if (path == "") return Path.Combine(contentRoot, "index.md");
    var parts = new List<string> { contentRoot };
    parts.AddRange(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
    parts.Add("index.md");
    return Path.Combine(parts.ToArray());
}

static string? ReadCoverVersion(string coverPath)
{
    var text = File.ReadAllText(coverPath);
    var fence = Regex.Match(
        text,
        @"^```cover-plate\s*\n(.*?)^```",
        RegexOptions.Multiline | RegexOptions.Singleline);
    if (!fence.Success) return null;
    var ver = Regex.Match(fence.Groups[1].Value, @"^\s*version:\s*(.+?)\s*$", RegexOptions.Multiline);
    if (!ver.Success) return null;
    return Unquote(ver.Groups[1].Value.Trim());
}

static string TransformPage(string markdown)
{
    var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    var output = new List<string>();
    var i = 0;
    while (i < lines.Length)
    {
        var line = lines[i];
        if (line.StartsWith("```"))
        {
            var name = line[3..].Trim();
            var space = name.IndexOfAny([' ', '\t']);
            if (space >= 0) name = name[..space];
            var body = new List<string>();
            i++;
            while (i < lines.Length && !lines[i].StartsWith("```"))
            {
                body.Add(lines[i]);
                i++;
            }
            if (i < lines.Length) i++; // closing fence
            if (name == "code")
            {
                output.AddRange(CodeWidgetToFence(body));
            }
            continue;
        }

        output.Add(DemoteHeading(line));
        i++;
    }

    return CollapseBlankLines(string.Join('\n', output)).Trim();
}

static IEnumerable<string> CodeWidgetToFence(List<string> yaml)
{
    var texts = new List<string>();
    foreach (var raw in yaml)
    {
        var textMatch = Regex.Match(raw.TrimEnd(), @"^\s*- text:\s*(.*)$");
        if (textMatch.Success)
        {
            texts.Add(Unquote(textMatch.Groups[1].Value.Trim()));
        }
    }
    if (texts.Count == 0) yield break;
    yield return "```";
    foreach (var t in texts) yield return t;
    yield return "```";
}

static string DemoteHeading(string line)
{
    if (line.Length == 0 || line[0] != '#') return line;
    var n = 0;
    while (n < line.Length && line[n] == '#') n++;
    if (n == 0 || n >= 6) return line;
    if (n == line.Length || line[n] != ' ') return line;
    return "#" + line;
}

static string Unquote(string value)
{
    if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        return value[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\");
    if (value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
        return value[1..^1].Replace("''", "'");
    return value;
}

static string CollapseBlankLines(string text)
{
    return Regex.Replace(text.Replace("\r\n", "\n"), @"\n{3,}", "\n\n");
}

static string Sha256Hex(string text)
{
    var bytes = SHA256.HashData(new UTF8Encoding(false).GetBytes(text));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

static JsonArray LoadVersions(string path)
{
    if (!File.Exists(path)) return new JsonArray();
    var node = JsonNode.Parse(File.ReadAllText(path));
    var arr = node?["versions"]?.AsArray();
    if (arr is null) return new JsonArray();
    return (JsonArray)arr.DeepClone();
}

static void WriteVersions(string path, JsonArray versions)
{
    var root = new JsonObject { ["versions"] = versions };
    var json = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, json + "\n", new UTF8Encoding(false));
}

static JsonObject? FindVersion(JsonArray versions, string file)
{
    foreach (var node in versions)
    {
        var obj = node as JsonObject;
        if (obj is null) continue;
        if ((obj["file"]?.GetValue<string>() ?? "") == file) return obj;
    }
    return null;
}

static void UpsertCurrentDump(string outDir, JsonArray versions, string dumpName, string markdown, string hash)
{
    var entry = FindVersion(versions, dumpName);
    var previousHash = entry?["hash"]?.GetValue<string>();
    if (previousHash == hash && File.Exists(Path.Combine(outDir, dumpName)))
    {
        return;
    }

    File.WriteAllText(Path.Combine(outDir, dumpName), markdown, new UTF8Encoding(false));
    var updated = DateTime.UtcNow.ToString("yyyy-MM-dd");
    if (entry is null)
    {
        versions.Insert(0, new JsonObject
        {
            ["file"] = dumpName,
            ["hash"] = hash,
            ["updated"] = updated,
        });
        return;
    }
    entry["hash"] = hash;
    entry["updated"] = updated;
}

static void EnsureEntriesForExistingDumps(string outDir, JsonArray versions)
{
    foreach (var path in Directory.GetFiles(outDir, "llote-docs-v*.md"))
    {
        var name = Path.GetFileName(path);
        if (FindVersion(versions, name) is not null) continue;
        var hash = Sha256Hex(File.ReadAllText(path).Replace("\r\n", "\n"));
        var updated = File.GetLastWriteTimeUtc(path).ToString("yyyy-MM-dd");
        versions.Add(new JsonObject
        {
            ["file"] = name,
            ["hash"] = hash,
            ["updated"] = updated,
        });
    }
}

static List<(string File, string Updated)> ListDumpFiles(string outDir, string currentName, JsonArray versions)
{
    var files = Directory.GetFiles(outDir, "llote-docs-v*.md")
        .Select(Path.GetFileName)
        .Where(n => n != null)
        .Cast<string>()
        .ToList();

    files.Sort((a, b) =>
    {
        if (a == currentName && b != currentName) return -1;
        if (b == currentName && a != currentName) return 1;
        return CompareVersionNames(b, a);
    });

    return files.Select(name =>
    {
        var updated = FindVersion(versions, name)?["updated"]?.GetValue<string>() ?? "";
        return (name, updated);
    }).ToList();
}

static string DumpLabel(string filename)
{
    return filename.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
        ? filename[..^3].Replace("llote-docs-v", "llote docs v", StringComparison.Ordinal)
        : filename;
}

static string ArchiveLinkFence(List<(string File, string Updated)> files)
{
    var items = new StringBuilder();
    foreach (var entry in files)
    {
        items.Append("  - label: \"").Append(DumpLabel(entry.File)).Append("\"\n");
        items.Append("    url: \"/docs-downloads/").Append(entry.File).Append("\"\n");
        items.Append("    updated: \"").Append(entry.Updated).Append("\"\n");
    }
    return "```archive-link\nitems:\n" + items + "```\n";
}

static void WriteArchiveIndex(string outDir, List<(string File, string Updated)> files)
{
    var items = new StringBuilder();
    foreach (var entry in files)
    {
        items.Append("    <li><a href=\"")
            .Append(WebUtility.HtmlEncode(entry.File))
            .Append("\">")
            .Append(WebUtility.HtmlEncode(DumpLabel(entry.File)))
            .Append("</a>");
        if (entry.Updated.Length > 0)
        {
            items.Append(" <time datetime=\"")
                .Append(WebUtility.HtmlEncode(entry.Updated))
                .Append("\">")
                .Append(WebUtility.HtmlEncode(entry.Updated))
                .Append("</time>");
        }
        items.Append("</li>\n");
    }

    var html =
        "<!doctype html>\n<html lang=\"en\">\n<head>\n" +
        "  <meta charset=\"utf-8\">\n" +
        "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n" +
        "  <title>llote docs downloads</title>\n" +
        "</head>\n<body>\n" +
        "  <h1>llote docs downloads</h1>\n" +
        "  <ul>\n" + items +
        "  </ul>\n" +
        "</body>\n</html>\n";

    File.WriteAllText(Path.Combine(outDir, "index.html"), html, new UTF8Encoding(false));
}

static int CompareVersionNames(string a, string b)
{
    var va = VersionParts(a);
    var vb = VersionParts(b);
    var n = Math.Max(va.Length, vb.Length);
    for (var i = 0; i < n; i++)
    {
        var ai = i < va.Length ? va[i] : 0;
        var bi = i < vb.Length ? vb[i] : 0;
        var cmp = ai.CompareTo(bi);
        if (cmp != 0) return cmp;
    }
    return string.CompareOrdinal(a, b);
}

static int[] VersionParts(string filename)
{
    var m = Regex.Match(filename, @"llote-docs-v(.+)\.md$", RegexOptions.IgnoreCase);
    if (!m.Success) return [];
    return m.Groups[1].Value
        .Split('.', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => int.TryParse(p, out var n) ? n : 0)
        .ToArray();
}
