using Mirror.Resources.Framework.Base;

namespace Mirror.Plugins.OnDemand;

public interface IOnDemandPlugin : IPlugin
{
    public Task SetupProvider(BaseProfile Profile)
    {
        return Task.CompletedTask;
    }
    
    public async Task PreInitialize(BaseProfile Profile)
    {
        return;
    }

    public async Task Initialize(BaseProfile Profile)
    {
        return;
    }
    
    public async Task InitializeStreaming(BaseProfile Profile)
    {
        return;
    }
}
