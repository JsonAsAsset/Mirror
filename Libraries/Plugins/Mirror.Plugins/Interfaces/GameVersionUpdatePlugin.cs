using CUE4Parse.UE4.Versions;
using Mirror.Resources.Framework.Base;

namespace Mirror.Plugins.Interfaces;

public interface IGameVersionUpdatePlugin : IGameIdPlugin
{
    EGame TargetVersion => EGame.GAME_UE5_LATEST;

    public void Update(BaseProfile Profile)
    {
        Profile.Version = TargetVersion;
    }
}
