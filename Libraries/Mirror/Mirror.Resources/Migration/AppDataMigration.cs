using System;
using System.IO;
using Mirror.Resources.Framework.Base;

namespace Mirror.Resources.Migration;

public static class AppDataMigration
{
    /* Every name this application has kept its data under, oldest first. A rename copies the
     * folder forward rather than starting empty beside it, and the paths profiles hold into it
     * are rewritten to match. */
    private static readonly string[] PreviousNames = { "vj0", "Core" };

    /* Folders worth nothing once copied: downloaded chunks, exports and the native libraries,
     * all of which this build fetches again by itself. Carrying them forward turns a megabyte
     * of settings and profiles into several gigabytes. */
    private static readonly string[] SkippedFolders = { ".runtime" };

    public static void Apply()
    {
        foreach (var previous in PreviousNames)
        {
            MigrateDirectory(previous, Globals.CODENAME, skipRuntime: true);
        }

        MigrateDirectory(Path.Combine(Globals.CODENAME, "Profiles"), Path.Combine(Globals.CODENAME, ".profiles"));
    }
    
    public static void MigrateDirectory(string oldRelativePath, string newRelativePath, bool skipRuntime = false)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        var oldPath = Path.Combine(appData, oldRelativePath);
        var newPath = Path.Combine(appData, newRelativePath);

        if (!Directory.Exists(oldPath) || Directory.Exists(newPath))
        {
            return;
        }

        /* Copied rather than moved, so what the previous name held is still there if this one
         * turns out to read it wrongly. Nothing is deleted: the old folder is the reader's to
         * remove once they are satisfied. */
        CopyDirectory(oldPath, newPath, skipRuntime);
    }

    public static void ApplyToProfile(BaseProfile profile)
    {
        profile.MappingsContainer.Path = Rehome(profile.MappingsContainer.Path);
        profile.LoadedMappingsFile = Rehome(profile.LoadedMappingsFile);
    }

    /* A path under one of the previous names, pointed at the current one. Compared with the
     * separators levelled, since a path typed by hand or written by another tool uses forward
     * slashes and would otherwise never be recognised as ours. */
    private static string? Rehome(string? path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var levelled = path.Replace('/', Path.DirectorySeparatorChar);

        foreach (var previous in PreviousNames)
        {
            var oldPath = Path.Combine(appData, previous);

            if (!levelled.StartsWith(oldPath, StringComparison.OrdinalIgnoreCase)) continue;

            var rest = levelled.Substring(oldPath.Length).TrimStart(Path.DirectorySeparatorChar);

            return Path.Combine(appData, Globals.CODENAME, rest);
        }

        return path;
    }

    private static void CopyDirectory(string source, string destination, bool isRoot = false)
    {
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source))
        {
            var dest = Path.Combine(destination, Path.GetFileName(file));
            File.Copy(file, dest, true);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            var name = Path.GetFileName(dir);

            if (isRoot && Array.Exists(SkippedFolders, it => string.Equals(it, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            CopyDirectory(dir, Path.Combine(destination, name));
        }
    }
}