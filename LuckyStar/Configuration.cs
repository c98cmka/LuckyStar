using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace LuckyStar;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool PoZhiJia_1_check = true;
    public bool PoZhiJia_2_check = true;
    public bool PoZhiJia_3_check = true;

    public bool TanXiZhiWu_1_check = true;
    public bool TanXiZhiWu_2_check = true;
    public bool TanXiZhiWu_3_check = true;

    public bool YiXiuDa_1_check = true;
    public bool YiXiuDa_2_check = true;
    public bool YiXiuDa_3_check = true;

    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}
