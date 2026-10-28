using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.Utils;

using Microsoft.AspNetCore.Mvc;

/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
/* Mirror Cloud Controller: Mesh Readability                                                                                          */
/*                                                                                                                                  */
/* Whether this build's meshes can be read at all, answered by reading some. A mesh hands back no geometry for several reasons and   */
/* they look alike from the far end: the build may carry no cooked render data, or carry it in a form this parser does not know.     */
/* Asked here before anything is imported, the answer is one line instead of an import that fails a mesh at a time.                  */
/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

namespace Mirror.Cloud.Controllers;

public partial class CloudApiController
{
    /* What one mesh answered with, so a build that reads none can be told from one that reads most */
    private sealed record MeshRead(string Path, bool Readable, int Lods, int Vertices, bool Nanite, string? Reason);

    [HttpGet("can/mesh")]
    public ActionResult GetMeshReadability(string? path, int? sample)
    {
        if (!IsBaseProfileReady || MainProfile is null) return NotInitializedResponse;

        var provider = MainProfile.Provider;

        /* The one asked about, or a spread of whatever the build has.
         *
         * Named, the answer is about that mesh and says why it came back empty. Unnamed, it is
         * about the build, and one mesh cannot say that: a single mesh with nothing cooked into it
         * reads the same as a parser that understands none of them. */
        List<string> looking = [];

        if (!string.IsNullOrWhiteSpace(path))
        {
            looking.Add(path.SubstringBefore('.'));
        }
        else
        {
            var wanted = Math.Clamp(sample ?? 8, 1, 64);

            /* Spread through the listing rather than taken off the front, since what sits together
             * was cooked together and would answer alike */
            var meshes = provider.Files.Values
                .Select(file => file?.PathWithoutExtension)
                .Where(found => found is not null && found.Contains("/Meshes/"))
                .Distinct()
                .ToList();

            if (meshes.Count == 0)
            {
                meshes = provider.Files.Values
                    .Select(file => file?.PathWithoutExtension)
                    .Where(found => found is not null)
                    .Distinct()
                    .ToList();
            }

            var step = Math.Max(1, meshes.Count / wanted);

            for (var at = 0; at < meshes.Count && looking.Count < wanted; at += step)
            {
                if (meshes[at] is { } found) looking.Add(found);
            }
        }

        var read = new List<MeshRead>();

        foreach (var one in looking)
        {
            UStaticMesh? mesh = null;
            string? reason = null;

            /* A path that is not a mesh at all is not a failure to read one, so it is said apart
             * from a mesh that came back without geometry */
            try
            {
                mesh = LoadExportOfType<UStaticMesh>(provider, one);

                if (mesh is null) reason = "nothing of that name is a static mesh";
            }
            catch (Exception caught)
            {
                reason = caught.GetType().Name + ": " + caught.Message;
            }

            if (mesh is null)
            {
                /* Only counted where it was asked for by name. A sample that landed on something
                 * that is not a mesh says nothing about whether meshes read. */
                if (!string.IsNullOrWhiteSpace(path)) read.Add(new MeshRead(one, false, 0, 0, false, reason));

                continue;
            }

            if (mesh.RenderData is not { } renderData)
            {
                read.Add(new MeshRead(one, false, 0, 0, false,
                    reason ?? "the mesh carries no cooked render data this build could read"));

                continue;
            }

            var vertices = 0;

            /* Counted off the position buffer, which is the one the geometry is actually read from */
            foreach (var lod in renderData.LODs)
            {
                if (lod?.PositionVertexBuffer is { } positions) vertices += positions.Verts.Length;
            }

            read.Add(new MeshRead(one, true, renderData.LODs.Length, vertices,
                renderData.NaniteResources is { PageStreamingStates.Length: > 0 }, null));
        }

        var readable = read.Count(one => one.Readable);

        return new JsonResult(new
        {
            /* The one line somebody wants: whether this build's meshes can be read at all */
            can_read_meshes = readable > 0,
            tried = read.Count,
            readable,
            game = MainProfile.Version.ToString(),
            meshes = read
        });
    }
}
