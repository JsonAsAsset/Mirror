using System.Threading.Tasks;
using System.Linq;

using Avalonia.Threading;

using FluentAvalonia.UI.Controls;

using Mirror.Cloud;
using Mirror.Models.Profiles;
using Mirror.Models;
using Mirror.Cloud.Controllers;
using Mirror.Framework;

namespace Mirror.Services;

public class CloudService : IService
{
    public CloudWebAPI? API;
    
    public void Initialize()
    {
        if (API is null)
        {
            API = new CloudWebAPI();
            
            API.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(API.IsRunning))
                {
                    MainWM.UpdateAPIServiceStatusColor();
                }
            };
        
            API.OnError += OnError;
            API.OnInitialized += OnInitialized;
        }

        RegisterBuilds();

        if (Settings.Cloud.RunHostedAPI)
        {
            Task.Run(() => API.Run());
        }
    }

    /* Loading and unloading live in the app, and Mirror.Cloud cannot see it, so it is handed the
     * three things it needs once. Everything on disk is offered rather than only what is open,
     * since a reader asking for a build by name is usually asking for one that is not. */
    /* How many profiles were on disk when the list was last read */
    private static int KnownProfileCount;

    private static void RegisterBuilds()
    {
        CloudApiController.EnumerateBuilds = () =>
        {
            /* Re-read when the folder has gained or lost one, so a build saved while Mirror is open
             * can be asked for without restarting it. Counting files is cheap next to parsing
             * them, and the loaded ones are kept as they are: re-reading would hand back fresh
             * objects and drop every provider already mounted. */
            var onDisk = Mirror.Resources.Globals.ProfilesFolder.Exists
                ? Mirror.Resources.Globals.ProfilesFolder.GetFiles("*.json").Length
                : 0;

            if (GameDetection.LoadedProfiles.Count == 0 || onDisk != KnownProfileCount)
            {
                var loaded = GameDetection.LoadedProfiles.Where(it => it.IsInitialized).ToList();

                GameDetection.LoadAllAsync().GetAwaiter().GetResult();

                /* Whatever was already open stays the object that is open */
                for (var index = 0; index < GameDetection.LoadedProfiles.Count; index++)
                {
                    var fresh = GameDetection.LoadedProfiles[index];
                    var already = loaded.FirstOrDefault(it => it.FileName == fresh.FileName);

                    if (already is not null)
                    {
                        GameDetection.LoadedProfiles[index] = already;
                    }
                }

                KnownProfileCount = onDisk;
            }

            return GameDetection.LoadedProfiles;
        };

        CloudApiController.StartBuildAsync = async (profile, token) =>
        {
            if (profile is Profile known)
            {
                await known.Initialize(token);
            }
        };

        CloudApiController.StopBuild = profile =>
        {
            if (profile is Profile known)
            {
                known.DisposeProvider();
            }
        };

        CloudApiController.StartIdleSweep();
    }

    private void OnError(string Title, string Header, string Content)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Info.Message(Title, Content, InfoBarSeverity.Error, buttonCommand: Restart, buttonTitle: "Restart API", useButton: true);
        });
    }

    private void OnInitialized(string URL)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Info.Message("Started Cloud", URL, InfoBarSeverity.Success, closeTime: 0.5f);
        });
    }

    public void Restart()
    {
        Stop();
        Initialize();
    }

    public void Stop()
    {
        if (API is null)
        {
            return;
        }

        _ = API.StopAsync();
    }
}
