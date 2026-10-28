using System;
using System.IO;

using CUE4Parse.FileProvider.Vfs;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;

namespace Mirror.Resources.Framework.Base;

public class BaseProvider : AbstractVfsFileProvider
{
    protected readonly DirectoryInfo WorkingDirectory;
    protected static readonly EnumerationOptions EnumerationOptions = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true
    };

    public BaseProvider(string directory, VersionContainer? version = null) : base(version, StringComparer.OrdinalIgnoreCase)
    {
        WorkingDirectory = new DirectoryInfo(directory);
        SkipReferencedTextures = true;
    }

    public override void Initialize()
    {
        if (!WorkingDirectory.Exists) throw new DirectoryNotFoundException($"Profile installation folder doesn't exist: {WorkingDirectory.FullName}");
        
        RegisterFiles(WorkingDirectory);
    }

    public string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        var candidate = path.Replace('\\', '/').TrimStart('/');
        var root = candidate.SubstringBefore('/');

        if (root.Length == 0 || !root.Equals(ProjectName, StringComparison.OrdinalIgnoreCase)) return path;

        var tree = candidate.SubstringAfter('/');
        if (tree.Length == 0) return path;

        /* The three the mount actually carries, and the ones FixPath itself treats as already placed */
        var next = tree.SubstringBefore('/');

        if (next.Equals("Content", StringComparison.OrdinalIgnoreCase) ||
            next.Equals("Config", StringComparison.OrdinalIgnoreCase) ||
            next.Equals("Plugins", StringComparison.OrdinalIgnoreCase)) return path;

        return string.Concat(root, "/Content/", tree);
    }

    private void RegisterFiles(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles("*.*", EnumerationOptions))
        {
            RegisterVfs(file.FullName, [ file.OpenRead() ], it => new FStreamArchive(it, File.OpenRead(it), Versions));
        }
    }
}
