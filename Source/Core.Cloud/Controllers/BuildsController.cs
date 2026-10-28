using System.Collections.Concurrent;

using Microsoft.AspNetCore.Mvc;

using Serilog;

using Core.Resources.Framework.Base;

/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
/* Core Cloud Builds */
/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

namespace Core.Cloud.Controllers;

public partial class CloudApiController
{
    /* The app owns loading and unloading, and this assembly cannot see it, so it hands these in
     * once at startup. Unset, the builds endpoints report that nothing can be served. */
    public static Func<IReadOnlyList<BaseProfile>>? EnumerateBuilds;
    public static Func<BaseProfile, CancellationToken, Task>? StartBuildAsync;
    public static Action<BaseProfile>? StopBuild;

    /* When each build was last asked for something, so one nobody is reading can be let go */
    private static readonly ConcurrentDictionary<string, DateTime> BuildLastUsed = new(StringComparer.OrdinalIgnoreCase);

    /* A build sits loaded this long without a request before it is dropped. Zero leaves them all
     * loaded, which is what a reader working out of one build wants. */
    public static TimeSpan BuildIdleTimeout = TimeSpan.FromMinutes(30);

    private static Timer? IdleSweep;

    /* One load at a time per build. Two readers asking for the same one at once would otherwise
     * both find it unloaded and start it twice, mounting every archive a second time. */
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> BuildLoadLocks = new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonResult NoBuildsResponse =
        new(new
        {
            errorCode = "cloud.builds.unavailable",
            errorMessage = "Builds cannot be listed or loaded from here",
            numericErrorCode = 1002
        })
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };

    private static readonly JsonResult AmbiguousBuildResponse =
        new(new
        {
            errorCode = "cloud.builds.ambiguous",
            errorMessage = "More than one build answers to that name, ask for it by file id",
            numericErrorCode = 1003
        })
        {
            StatusCode = StatusCodes.Status409Conflict
        };

    /* A build is named by what the reader calls it or by the file it is kept in. Names are the
     * reader's own and two can read the same, so the file id is the one that always lands. */
    private static bool AnswersTo(BaseProfile profile, string named) =>
        string.Equals(profile.FileName, named, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Path.GetFileNameWithoutExtension(profile.FileName), named, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(profile.Name, named, StringComparison.OrdinalIgnoreCase);

    private static List<BaseProfile> BuildsNamed(string named) =>
        EnumerateBuilds?.Invoke().Where(it => AnswersTo(it, named)).ToList() ?? [];

    /* Noted against the build rather than against the request, so a reader working through one
     * build keeps every build it touches alive and lets go of the rest */
    private static void TouchBuild(BaseProfile? profile)
    {
        if (profile is null) return;

        BuildLastUsed[profile.FileName] = DateTime.UtcNow;
    }

    public static void StartIdleSweep()
    {
        IdleSweep ??= new Timer(_ => SweepIdleBuilds(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /* Everything loaded that nobody has asked about for a while, except the one the app is showing,
     * which is the reader's own and theirs to close */
    private static void SweepIdleBuilds()
    {
        if (BuildIdleTimeout <= TimeSpan.Zero || StopBuild is null || EnumerateBuilds is null) return;

        try
        {
            foreach (var profile in EnumerateBuilds())
            {
                if (!profile.IsInitialized) continue;
                if (ReferenceEquals(profile, MainProfile)) continue;

                if (!BuildLastUsed.TryGetValue(profile.FileName, out var last)) continue;
                if (DateTime.UtcNow - last < BuildIdleTimeout) continue;

                Log.Information($"[Core.Cloud]: Letting go of {profile.Name}, nothing has asked it for {BuildIdleTimeout.TotalMinutes:0} minute(s)");

                StopBuild(profile);

                BuildLastUsed.TryRemove(profile.FileName, out _);
            }
        }
        catch (Exception exception)
        {
            Log.Warning($"[Core.Cloud]: Could not sweep idle builds: {exception.Message}");
        }
    }

    /* Everything the reader could ask for, loaded or not */
    [HttpGet("builds")]
    public ActionResult GetBuilds()
    {
        if (EnumerateBuilds is null) return NoBuildsResponse;

        var builds = EnumerateBuilds().Select(profile => new
        {
            name = profile.Name,
            id = Path.GetFileNameWithoutExtension(profile.FileName),
            fileName = profile.FileName,
            loaded = profile.IsInitialized,
            version = (int) profile.Version,
            versionName = profile.Version.ToString(),
            archiveDirectory = profile.ArchiveDirectory,
            mappings = profile.LoadedMappingsFile,
            files = profile.IsInitialized ? profile.Provider?.Files.Count ?? 0 : 0,
            isMain = ReferenceEquals(profile, MainProfile),
            lastUsed = BuildLastUsed.TryGetValue(profile.FileName, out var used) ? used : (DateTime?) null
        });

        return new JsonResult(new
        {
            idleTimeoutMinutes = BuildIdleTimeout.TotalMinutes,
            builds
        });
    }

    /* Loads one, and answers once it can be read rather than once it has been asked for */
    [HttpPost("builds/{named}/start")]
    public async Task<ActionResult> StartBuild(string named, CancellationToken cancellationToken)
    {
        if (EnumerateBuilds is null || StartBuildAsync is null) return NoBuildsResponse;

        var matches = BuildsNamed(named);

        if (matches.Count == 0) return NotFoundResponse;
        if (matches.Count > 1) return AmbiguousBuildResponse;

        var profile = matches[0];

        TouchBuild(profile);

        if (profile.IsInitialized)
        {
            return new JsonResult(new { name = profile.Name, loaded = true, alreadyLoaded = true });
        }

        var loadLock = BuildLoadLocks.GetOrAdd(profile.FileName, _ => new SemaphoreSlim(1, 1));

        await loadLock.WaitAsync(cancellationToken);

        try
        {
            /* Whoever held the lock may have been loading this very build */
            if (profile.IsInitialized)
            {
                return new JsonResult(new { name = profile.Name, loaded = true, alreadyLoaded = true });
            }

            await StartBuildAsync(profile, cancellationToken);
        }
        catch (Exception exception)
        {
            Log.Warning($"[Core.Cloud]: Could not load {profile.Name}: {exception.Message}");

            return new JsonResult(new
            {
                errorCode = "cloud.builds.failed",
                errorMessage = exception.Message,
                numericErrorCode = 1004
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        finally
        {
            loadLock.Release();
        }

        StartIdleSweep();

        return new JsonResult(new
        {
            name = profile.Name,
            loaded = profile.IsInitialized,
            files = profile.Provider?.Files.Count ?? 0
        });
    }

    /* Lets one go. The one the app is showing stays, since closing it out from under the reader
     * leaves the window pointed at nothing. */
    [HttpPost("builds/{named}/stop")]
    public ActionResult StopBuildNamed(string named)
    {
        if (EnumerateBuilds is null || StopBuild is null) return NoBuildsResponse;

        var matches = BuildsNamed(named);

        if (matches.Count == 0) return NotFoundResponse;
        if (matches.Count > 1) return AmbiguousBuildResponse;

        var profile = matches[0];

        if (ReferenceEquals(profile, MainProfile))
        {
            return new JsonResult(new
            {
                errorCode = "cloud.builds.in_use",
                errorMessage = "That build is the one open in Core, close it there instead",
                numericErrorCode = 1005
            })
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }

        StopBuild(profile);

        BuildLastUsed.TryRemove(profile.FileName, out _);

        return new JsonResult(new { name = profile.Name, loaded = profile.IsInitialized });
    }
}
