using CUE4Parse.Utils;

using Microsoft.AspNetCore.Mvc;

using Mirror.Resources.Framework.Base;

/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
/* Core Cloud Controller: Folder                                                                                                    */
/*                                                                                                                                  */
/* Lists every asset path under a folder of the mounted game files. Used by Reflection to reflect a whole folder without anything   */
/* in the project pointing at it first.                                                                                             */
/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

namespace Mirror.Cloud.Controllers;

public partial class CloudApiController
{
    /* The paths that read like what was asked for, capped, for finding an asset without pulling a
     * folder listing that runs to tens of megabytes. Searches the build the path named, or the
     * main one, rather than every build at once. */
    [HttpGet("folder/search")]
    public ActionResult SearchFolderPaths(string? query, int limit = 50, string? under = null)
    {
        var profile = RouteBuild(out var routeError) ?? MainProfile;

        if (routeError is not null) return routeError;
        if (profile is null || !profile.IsInitialized) return NotInitializedResponse;

        if (string.IsNullOrWhiteSpace(query)) return BadRequest(new
        {
            errorCode = "cloud.folder.no_query",
            errorMessage = "No query supplied",
            numericErrorCode = 1007
        });

        limit = Math.Clamp(limit, 1, 500);

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var scope = string.IsNullOrWhiteSpace(under) ? null : ToGameFolderPath(under, profile.Provider);

        var hits = new List<string>();
        var total = 0;

        foreach (var file in profile.Provider.Files.Keys)
        {
            if (!file.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase) &&
                !file.EndsWith(".umap", StringComparison.OrdinalIgnoreCase)) continue;

            if (scope is not null && !file.StartsWith(scope, StringComparison.OrdinalIgnoreCase)) continue;

            /* Every term has to read somewhere in the path, so "pickaxe assassin" finds the one
             * asset rather than everything either word appears in */
            var matched = true;

            foreach (var term in terms)
            {
                if (file.Contains(term, StringComparison.OrdinalIgnoreCase)) continue;

                matched = false;

                break;
            }

            if (!matched) continue;

            total++;

            if (hits.Count < limit) hits.Add(file.SubstringBeforeLast('.'));
        }

        return new JsonResult(new
        {
            build = profile.Name,
            query,
            total,
            returned = hits.Count,
            truncated = total > hits.Count,
            paths = hits
        });
    }

    /* Every asset path under a folder, in the same form the export endpoint takes back */
    [HttpGet("folder/paths")]
    public ActionResult GetFolderPaths(string? path)
    {
        if (!IsBaseProfileReady || MainProfile is null) return NotInitializedResponse;

        if (string.IsNullOrWhiteSpace(path)) return BadRequest(new
        {
            errorCode = "cloud.folder.no_path",
            errorMessage = "No folder supplied",
            numericErrorCode = 1003
        });

        var folder = ToGameFolderPath(path, MainProfile.Provider);
        if (folder is null) return NotFoundResponse;

        var paths = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        CollectFolderPaths(MainProfile.Provider, folder, paths, seen);

        /* A folder can be split across profiles, the same way a single path can be */
        foreach (var profile in SecondaryBaseProfiles)
        {
            if (!profile.IsInitialized) continue;

            CollectFolderPaths(profile.Provider, ToGameFolderPath(path, profile.Provider) ?? folder, paths, seen);
        }

        if (paths.Count == 0) return NotFoundResponse;

        paths.Sort(StringComparer.OrdinalIgnoreCase);

        return new JsonResult(new
        {
            path = folder,
            paths
        });
    }

    /* Everything mounted below Folder, subfolders included */
    private static void CollectFolderPaths(BaseProvider provider, string folder, List<string> paths, HashSet<string> seen)
    {
        var prefix = folder + "/";

        foreach (var file in provider.Files.Values)
        {
            if (file is null || !file.IsUePackage) continue;

            var packagePath = file.PathWithoutExtension;

            /* Editor-only side files ("Foo.o.uasset") are not assets of their own */
            if (packagePath.EndsWith(".o", StringComparison.OrdinalIgnoreCase)) continue;
            if (!packagePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            /* One package is several mounted files, so the path is what gets deduplicated */
            if (seen.Add(packagePath)) paths.Add(packagePath);
        }
    }

    /* Takes the folder however it was typed, editor form or the way the mounted files spell it,
     * and hands back the latter. Null when it names a root nothing is mounted under. */
    private static string? ToGameFolderPath(string rawFolder, BaseProvider provider)
    {
        var folder = rawFolder.Trim().Replace('\\', '/').Trim('/');
        if (folder.Length == 0) return null;

        var root = folder.SubstringBefore('/');
        var remainder = folder.Length > root.Length ? folder[(root.Length + 1)..] : string.Empty;

        /* Already a path into the mounted files, plugin folders and all */
        if (root.Equals(provider.ProjectName, StringComparison.OrdinalIgnoreCase)) return folder;

        if (root.Equals("Engine", StringComparison.OrdinalIgnoreCase))
        {
            return remainder.StartsWith("Content", StringComparison.OrdinalIgnoreCase)
                ? folder
                : CombineFolder("Engine/Content", remainder);
        }

        /* The editor's own root for the project's content */
        if (root.Equals("Game", StringComparison.OrdinalIgnoreCase))
        {
            return CombineFolder(provider.ProjectName + "/Content", remainder);
        }

        /* Anything else is a mount named after the plugin it came from */
        if (provider.VirtualPaths.TryGetValue(root, out var virtualPath))
        {
            return CombineFolder(virtualPath.Replace('\\', '/').Trim('/') + "/Content", remainder);
        }

        return null;
    }

    private static string CombineFolder(string root, string remainder)
    {
        return remainder.Length == 0 ? root : root + "/" + remainder;
    }
}
