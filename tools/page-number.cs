// Computes this page's position within the notebook's own page-turn order
// and appends a "page-number" widget fence carrying it -- see
// widgets/page-number.html/.css for the widget itself, and
// root-copy/js/notebook.js's flattenNav() for the client-side ordering this
// mirrors: a section's own page, then all of its children recursively,
// before the next top-level sibling. The walk order here MUST match
// flattenNav() exactly, or the printed number would disagree with the
// indicator dock text the client renders for the same page.
//
// Registered as "page-number" in canary.jsonc's tools registry; add
// "page-number" to a content directory's own .toolchain.json to apply it
// there. Same stdin/stdout contract and same two environment variables as
// tools/curtain.cs and tools/toc.ps1 -- see curtain.cs if this is
// unfamiliar.
//
// JsonNode (a DOM, not a typed POCO deserializer) on purpose: this SDK's
// `dotnet run` file-based-app mode has reflection-based JsonSerializer
// disabled by default (source-generator serialization only), which a plain
// JsonSerializer.Deserialize<T> call hits immediately. JsonNode sidesteps
// that entirely -- no source generator, no reflection, just a document walk.
using System.Text.Json.Nodes;

Console.InputEncoding = new System.Text.UTF8Encoding(false);
Console.OutputEncoding = new System.Text.UTF8Encoding(false);

var routePath = Environment.GetEnvironmentVariable("CANARY_ROUTE_PATH") ?? "";
var manifestPath = Environment.GetEnvironmentVariable("CANARY_MANIFEST_PATH")
    ?? throw new InvalidOperationException("CANARY_MANIFEST_PATH not set");

var manifestJson = File.ReadAllText(manifestPath);
var root = JsonNode.Parse(manifestJson)
    ?? throw new InvalidOperationException($"failed to parse manifest at {manifestPath}");
var navArray = root["nav"]?.AsArray()
    ?? throw new InvalidOperationException("manifest has no 'nav' array");

var counter = 0;
var current = FindPosition(navArray, routePath, ref counter)
    ?? throw new InvalidOperationException($"route '{routePath}' not found in manifest nav");

var source = Console.In.ReadToEnd();
var fence = $"```page-number\ncurrent: {current}\n```\n";
Console.Out.Write(source.TrimEnd() + "\n\n" + fence);

// Depth-first walk matching flattenNav()'s order, counting as it goes and
// stopping the instant it finds routePath -- no need to walk the rest of
// the tree just to know a total that's no longer printed. Returns the
// page's 1-based position, or null if the whole tree turned up no match.
static int? FindPosition(JsonArray items, string routePath, ref int counter)
{
    foreach (var itemNode in items)
    {
        var item = itemNode!.AsObject();
        counter++;
        if ((item["path"]?.GetValue<string>() ?? "") == routePath)
        {
            return counter;
        }
        if (item["children"] is JsonArray children && children.Count > 0)
        {
            var found = FindPosition(children, routePath, ref counter);
            if (found is not null)
            {
                return found;
            }
        }
    }
    return null;
}
