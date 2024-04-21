using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using LuckyStar.Managers;
using ECommons.Automation;
using ECommons;
using Dalamud.Plugin;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Threading;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Microsoft.VisualBasic;
using Dalamud.Game.ClientState.Objects;
using ECommons.DalamudServices.Legacy;
using ECommons.DalamudServices;
using LuckyStar.Data;
using ECommons.GameFunctions;
using ECommons.Configuration;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace LuckyStar.Windows;

public unsafe class MainWindow : Window, IDisposable
{
    Plugin _plugin;
    private Chat _chat;

    private List<Fate> fateList = new List<Fate>();
    private List<GameObject> fateEnemyList = new List<GameObject>();
    private Fate? currentFate = null;

    private bool needToTakeOff = false;

    private int HuntEnemyId = 0;
    private int DataIndex = 0;
    private bool readyToTheNextpos = true;
    private bool isVnavWorking = false;
    private bool isRuning = false;

    private bool PoZhiJia_1 = true;
    private bool PoZhiJia_2 = true;
    private bool PoZhiJia_3 = true;

    private bool TanXiZhiWu_1 = true;
    private bool TanXiZhiWu_2 = true;
    private bool TanXiZhiWu_3 = true;


    public MainWindow(Plugin plugin, DalamudPluginInterface pluginInterface)
        : base("LuckyStar", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        _chat = new();
        _plugin = plugin;
        Service.Framework.Update += OnUpdate;
    }
    public void OnUpdate(IFramework framework)
    {
        if (HuntEnemyId != 0)
        {
            if (needToTakeOff)
            {
                if (!Service.Condition[ConditionFlag.Mounted])
                {
                    Mount();
                }
                else
                {
                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        Takeoff();
                    }
                    else
                    {
                        needToTakeOff = false;
                    }
                }
            }
            if (HuntEnemyId == 13524 || HuntEnemyId == 13526 || HuntEnemyId == 13529) //萨岛
            {
                if (PoZhiJia_1 && PoZhiJia_2 && PoZhiJia_3)
                {
                    AShuTuoLoop();
                }
                if (!PoZhiJia_1 && PoZhiJia_2 && PoZhiJia_3)
                {
                    BiSheZheLoop();
                }
                if (!PoZhiJia_1 && !PoZhiJia_2 && PoZhiJia_3)
                {
                    JingangweiLoop();
                }
            }
            else if (HuntEnemyId == 13365 || HuntEnemyId == 13366 || HuntEnemyId == 13367) //月球
            {
                if (TanXiZhiWu_1 && TanXiZhiWu_2 && TanXiZhiWu_3)
                {
                    SiKaoLoop();
                }
                if (!TanXiZhiWu_1 && TanXiZhiWu_2 && TanXiZhiWu_3)
                {
                    PangHuangLoop();
                }
                if (!TanXiZhiWu_1 && !TanXiZhiWu_2 && TanXiZhiWu_3)
                {
                    TanXiLoop();
                }
            }
            else if (HuntEnemyId == 10276 || HuntEnemyId == 10277 || HuntEnemyId == 10280) //大森林
            {
                YiXiuDaLoop();
            }
        }




        if (Service.FateTable != null && Service.FateTable.Length > 0)
        {
            fateList = Service.FateTable.ToList();
        }
        else
        {
            fateList = new List<Fate>();
        }

        if (currentFate is not null)
        {
            if (needToTakeOff)
            {
                if (!Service.Condition[ConditionFlag.Mounted])
                {
                    Mount();
                }
                else
                {
                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        Takeoff();
                    }
                    else
                    {
                        needToTakeOff = false;
                    }
                }
            }

            fateEnemyList = new List<GameObject>();

            foreach (var o in Service.ObjectTable)
            {
                if (o != null)
                {
                    uint objectFateId = o.Struct()->FateId;
                    if (currentFate != null && objectFateId == currentFate.FateId && _plugin.CanAttack(142, o.Address) == 1 && o.ObjectId != 13372)
                    {
                        fateEnemyList.Add(o);
                    }
                }
            }
            FateLoop();
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("tabbar");
        if (ImGui.BeginTabItem("农怪"))
        {
            //if (ImGui.Button("当前坐标"))
            //{
            //    var playerX = Service.ClientState.LocalPlayer.Position.X;
            //    var playerY = Service.ClientState.LocalPlayer.Position.Y;
            //    var playerZ = Service.ClientState.LocalPlayer.Position.Z;

            //    Service.Chat.Print(playerX.ToString() + ", " + playerY + ", " + playerZ + "," + Service.ClientState.TerritoryType);
            //}
            //ImGui.SameLine();
            //if (ImGui.Button("目标ID"))
            //{
            //    if (Service.ClientState.LocalPlayer.TargetObject != null)
            //    {
            //        Service.Chat.Print(Service.ClientState.LocalPlayer.TargetObject.DataId.ToString());
            //        //Service.Chat.Print("ismounted:" + (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]).ToString());
            //        //ActionManager.Instance()->UseAction(ActionType.Action, 31, Service.ClientState.LocalPlayer.TargetObject.ObjectId);
            //    }

            //}

            ImGui.Text("萨维奈岛-颇胝迦 (护法村)");
            ImGui.SameLine();
            ImGui.BeginDisabled(isRuning);
            if (ImGui.Checkbox("阿输陀花", ref Service.Config.PoZhiJia_1_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("毕舍遮", ref Service.Config.PoZhiJia_2_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("金刚尾", ref Service.Config.PoZhiJia_3_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Go##PoZhiJia"))
            {
                DataIndex = 0;
                readyToTheNextpos = true;
                isVnavWorking = false;

                HuntEnemyId = 13529;
                //HuntEnemyId = 13524;
                //HuntEnemyId = 13526;
                needToTakeOff = true;

                PoZhiJia_1 = true;
                PoZhiJia_2 = true;
                PoZhiJia_3 = true;

                TurnOnRS();

                isRuning = true;
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Stop##PoZhiJia"))
            {
                isRuning = false;

                HuntEnemyId = 0;
                needToTakeOff = false;

                PoZhiJia_1 = false;
                PoZhiJia_2 = false;
                PoZhiJia_3 = false;

                DataIndex = 0;
                readyToTheNextpos = true;
                isVnavWorking = false;

                Stop();
            }
            ImGui.Text("叹息海-沉思之物 (最佳威兔洞)");
            ImGui.SameLine();
            ImGui.BeginDisabled(isRuning);
            if (ImGui.Checkbox("思考之物", ref Service.Config.TanXiZhiWu_1_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("彷徨之物", ref Service.Config.TanXiZhiWu_2_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("叹息之物", ref Service.Config.TanXiZhiWu_3_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Go##Tanxi"))
            {
                DataIndex = 0;
                readyToTheNextpos = true;
                isVnavWorking = false;

                HuntEnemyId = 13367;
                //HuntEnemyId = 13365;
                //HuntEnemyId = 13366;
                needToTakeOff = true;

                TanXiZhiWu_1 = true;
                TanXiZhiWu_2 = true;
                TanXiZhiWu_3 = true;

                TurnOnRS();

                isRuning = true;
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Stop##Tanxi"))
            {
                isRuning = false;

                HuntEnemyId = 0;
                needToTakeOff = false;

                TanXiZhiWu_1 = false;
                TanXiZhiWu_2 = false;
                TanXiZhiWu_3 = false;

                DataIndex = 0;
                readyToTheNextpos = true;
                isVnavWorking = false;

                Stop();
            }
            ImGui.Text("拉凯提亚大森林-伊休妲 (法诺村)");
            ImGui.SameLine();
            ImGui.BeginDisabled(isRuning);
            if (ImGui.Checkbox("人偶", ref Service.Config.YiXiuDa_1_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("石蒺藜", ref Service.Config.YiXiuDa_2_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("器皿", ref Service.Config.YiXiuDa_3_check))
            {
                Service.Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Go##YiXiuDa"))
            {
                DataIndex = 0;
                readyToTheNextpos = true;
                isVnavWorking = false;

                if (Service.Config.YiXiuDa_1_check || Service.Config.YiXiuDa_2_check || Service.Config.YiXiuDa_3_check)
                {
                    HuntEnemyId = 10276;
                }
                TurnOnRS();
                needToTakeOff = true;

                isRuning = true;
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Stop##YiXiuDa"))
            {
                isRuning = false;

                HuntEnemyId = 0;
                needToTakeOff = false;

                DataIndex = 0;
                readyToTheNextpos = true;
                isVnavWorking = false;

                Stop();
            }
            ImGui.EndTabItem();

        }
        if (ImGui.BeginTabItem("Fate"))
        {
            //if (ImGui.Button("Test"))
            //{
            //    SyncFate(FateManager.Instance()->CurrentFate->FateId);
            //    if (Service.ClientState.LocalPlayer.TargetObject != null)
            //    {
            //        Service.Chat.Print(Service.ClientState.LocalPlayer.TargetObject.DataId.ToString());
            //        if (_plugin.CanAttack(142, Service.ClientState.LocalPlayer.TargetObject.Address) == 1)
            //        {
            //            Service.Chat.Print("111");
            //        }
            //        //Service.Chat.Print("ismounted:" + (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]).ToString());
            //        //ActionManager.Instance()->UseAction(ActionType.Action, 31, Service.ClientState.LocalPlayer.TargetObject.ObjectId);
            //    }
            //}
            foreach (Fate fateTemp in fateList)
            {
                if ("Running".Equals(fateTemp.State.ToString()))
                {
                    ImGui.Text(fateTemp.Name.ToString() + " (" + fateTemp.Progress.ToString() + "%%) ");
                    ImGui.SameLine();
                    //if (ImGui.Button($"Walk##{fateTemp.FateId}"))
                    //{
                    //    currentFate = fateTemp;
                    //    needToTakeOff = true;
                    //    readyToTheNextpos = true;
                    //    isVnavWorking = false;
                    //    walktowithmount(fateTemp.Position.X, fateTemp.Position.Y, fateTemp.Position.Z);
                    //}
                    //ImGui.SameLine();
                    //if (ImGui.Button($"Fly##{fateTemp.FateId}"))
                    //{
                    //    currentFate = fateTemp;
                    //    needToTakeOff = true;
                    //    readyToTheNextpos = true;
                    //    isVnavWorking = false;
                    //}
                    //ImGui.BeginDisabled(isRuning);
                    if (ImGui.Button($"Go##{fateTemp.FateId}"))
                    {
                        currentFate = fateTemp;
                        needToTakeOff = true;
                        readyToTheNextpos = true;
                        isVnavWorking = false;

                        TurnOnRS();

                        //isRuning = true;
                    }
                    //ImGui.EndDisabled();
                    ImGui.SameLine();
                    if (ImGui.Button($"Stop##{fateTemp.FateId}"))
                    {
                        currentFate = null;
                        needToTakeOff = false;
                        readyToTheNextpos = true;
                        isVnavWorking = false;
                        //isRuning = false;

                        Stop();
                    }
                }
            }
            ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }

    public void FateLoop()
    {
        lock (fateEnemyList)
        {
            var targetX = currentFate.Position.X;
            var targetZ = currentFate.Position.Z;

            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;

            var Posdistance = Math.Sqrt(Math.Pow(targetX - playerX, 2) + Math.Pow(targetZ - playerZ, 2));

            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                isVnavWorking = true;
                readyToTheNextpos = false;
                flyto(currentFate.Position.X, currentFate.Position.Y, currentFate.Position.Z);
            }

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                SyncFate(currentFate.FateId);

                var minDistance = 1000000;
                GameObject? closedObject = null;
                foreach (var temp in fateEnemyList)
                {
                    var distance = (int)Math.Sqrt(Math.Pow(temp.Position.X - playerX, 2) + Math.Pow(temp.Position.Z - playerZ, 2));
                    if (!temp.IsDead && distance < minDistance)
                    {
                        minDistance = distance;
                        closedObject = temp;
                    }
                }

                if (closedObject != null && !closedObject.IsDead)
                {
                    if (minDistance <= 25)
                    {
                        var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);

                        //Service.Chat.Print("distance:" + distance);
                        if (distance > 3)
                        {
                            walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                        }
                        attackObjectForFate(closedObject);
                    }
                    else
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                        isVnavWorking = true;
                        readyToTheNextpos = false;

                    }
                }
            }
        }
    }

    public void AShuTuoLoop()
    {
        if (Service.Config.PoZhiJia_1_check)
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                if (DataIndex == PoZhiJiaData.AShuTuoPosData.Count())
                {
                    HuntEnemyId = 13524;
                    PoZhiJia_1 = false;
                    reset();
                }
                else
                {
                    flyto(PoZhiJiaData.AShuTuoPosData[DataIndex].X, PoZhiJiaData.AShuTuoPosData[DataIndex].Y, PoZhiJiaData.AShuTuoPosData[DataIndex].Z);
                    isVnavWorking = true;
                    readyToTheNextpos = false;
                }
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(PoZhiJiaData.AShuTuoPosData[DataIndex].X - playerX, 2) + Math.Pow(PoZhiJiaData.AShuTuoPosData[DataIndex].Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);
                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;

                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            HuntEnemyId = 13524;
            PoZhiJia_1 = false;
            reset();
        }
    }

    public void BiSheZheLoop()
    {
        if (Service.Config.PoZhiJia_2_check)
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                if (DataIndex == PoZhiJiaData.BiSheZhePosData.Count())
                {
                    HuntEnemyId = 13526;
                    PoZhiJia_2 = false;
                    reset();
                }
                else
                {
                    flyto(PoZhiJiaData.BiSheZhePosData[DataIndex].X, PoZhiJiaData.BiSheZhePosData[DataIndex].Y, PoZhiJiaData.BiSheZhePosData[DataIndex].Z);
                    isVnavWorking = true;
                    readyToTheNextpos = false;
                }
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(PoZhiJiaData.BiSheZhePosData[DataIndex].X - playerX, 2) + Math.Pow(PoZhiJiaData.BiSheZhePosData[DataIndex].Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);
                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;

                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            HuntEnemyId = 13526;
            PoZhiJia_2 = false;
            reset();
        }
    }

    public void JingangweiLoop()
    {
        if (Service.Config.PoZhiJia_3_check)
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                if (DataIndex == PoZhiJiaData.JingangweiPosData.Count())
                {
                    HuntEnemyId = 13529;
                    PoZhiJia_1 = true;
                    PoZhiJia_2 = true;
                    PoZhiJia_3 = true;
                    reset();
                }
                else
                {
                    flyto(PoZhiJiaData.JingangweiPosData[DataIndex].X, PoZhiJiaData.JingangweiPosData[DataIndex].Y, PoZhiJiaData.JingangweiPosData[DataIndex].Z);
                    isVnavWorking = true;
                    readyToTheNextpos = false;
                }
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(PoZhiJiaData.JingangweiPosData[DataIndex].X - playerX, 2) + Math.Pow(PoZhiJiaData.JingangweiPosData[DataIndex].Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);

                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;

                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            HuntEnemyId = 13529;
            PoZhiJia_1 = true;
            PoZhiJia_2 = true;
            PoZhiJia_3 = true;
            reset();
        }
    }

    public void SiKaoLoop()
    {
        if (Service.Config.TanXiZhiWu_1_check)
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                if (DataIndex == TanXiZhiWu.SiKaoPosData.Count())
                {
                    HuntEnemyId = 13365;
                    TanXiZhiWu_1 = false;
                    reset();
                }
                else
                {
                    flyto(TanXiZhiWu.SiKaoPosData[DataIndex].X, TanXiZhiWu.SiKaoPosData[DataIndex].Y, TanXiZhiWu.SiKaoPosData[DataIndex].Z);
                    isVnavWorking = true;
                    readyToTheNextpos = false;
                }
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(TanXiZhiWu.SiKaoPosData[DataIndex].X - playerX, 2) + Math.Pow(TanXiZhiWu.SiKaoPosData[DataIndex].Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);

                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;

                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            HuntEnemyId = 13365;
            TanXiZhiWu_1 = false;
            reset();
        }
    }

    public void PangHuangLoop()
    {
        if (Service.Config.TanXiZhiWu_2_check)
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                if (DataIndex == TanXiZhiWu.PangHuangPosData.Count())
                {
                    HuntEnemyId = 13366;
                    TanXiZhiWu_2 = false;
                    reset();
                }
                else
                {
                    flyto(TanXiZhiWu.PangHuangPosData[DataIndex].X, TanXiZhiWu.PangHuangPosData[DataIndex].Y, TanXiZhiWu.PangHuangPosData[DataIndex].Z);
                    isVnavWorking = true;
                    readyToTheNextpos = false;
                }
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(TanXiZhiWu.PangHuangPosData[DataIndex].X - playerX, 2) + Math.Pow(TanXiZhiWu.PangHuangPosData[DataIndex].Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);

                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;

                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            HuntEnemyId = 13366;
            TanXiZhiWu_2 = false;
            reset();
        }
    }

    public void TanXiLoop()
    {
        if (Service.Config.TanXiZhiWu_3_check)
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                if (DataIndex == TanXiZhiWu.TanXiPosData.Count())
                {
                    HuntEnemyId = 13367;
                    TanXiZhiWu_1 = true;
                    TanXiZhiWu_2 = true;
                    TanXiZhiWu_3 = true;
                    reset();
                }
                else
                {
                    flyto(TanXiZhiWu.TanXiPosData[DataIndex].X, TanXiZhiWu.TanXiPosData[DataIndex].Y, TanXiZhiWu.TanXiPosData[DataIndex].Z);
                    isVnavWorking = true;
                    readyToTheNextpos = false;
                }
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(TanXiZhiWu.TanXiPosData[DataIndex].X - playerX, 2) + Math.Pow(TanXiZhiWu.TanXiPosData[DataIndex].Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }

            if (readyToTheNextpos == false && !(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);

                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;

                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            HuntEnemyId = 13367;
            TanXiZhiWu_1 = true;
            TanXiZhiWu_2 = true;
            TanXiZhiWu_3 = true;
            reset();
        }
    }

    public void YiXiuDaLoop()
    {
        if (DataIndex == YiXiuDa.AllPosData.Count())
        {
            DataIndex = 0;
            reset();
        }
        if ((Service.Config.YiXiuDa_1_check && YiXiuDa.AllPosData[DataIndex].huntId == 10276) ||
        (Service.Config.YiXiuDa_2_check && YiXiuDa.AllPosData[DataIndex].huntId == 10277) ||
        (Service.Config.YiXiuDa_3_check && YiXiuDa.AllPosData[DataIndex].huntId == 10280))
        {
            if (Service.Condition[ConditionFlag.InFlight] && AgentMap.Instance()->IsPlayerMoving != 1 && readyToTheNextpos == true)
            {
                flyto(YiXiuDa.AllPosData[DataIndex].Item2.X, YiXiuDa.AllPosData[DataIndex].Item2.Y, YiXiuDa.AllPosData[DataIndex].Item2.Z);
                HuntEnemyId = YiXiuDa.AllPosData[DataIndex].huntId;
                isVnavWorking = true;
                readyToTheNextpos = false;
            }
            var playerX = Service.ClientState.LocalPlayer.Position.X;
            var playerZ = Service.ClientState.LocalPlayer.Position.Z;
            var Posdistance = Math.Sqrt(Math.Pow(YiXiuDa.AllPosData[DataIndex].Item2.X - playerX, 2) + Math.Pow(YiXiuDa.AllPosData[DataIndex].Item2.Z - playerZ, 2));

            if (readyToTheNextpos == false && AgentMap.Instance()->IsPlayerMoving != 1 && (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]) && Posdistance < 3 && isVnavWorking)
            {
                Stop();
                Dismount();
            }
            if (!(Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]))
            {
                isVnavWorking = false;
            }
            //Service.Chat.Print("readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString() + ", HuntEnemyId:" + HuntEnemyId.ToString() + ", isVnavWorking:" + isVnavWorking.ToString() + ", ismounted:" + (Service.Condition[ConditionFlag.Mounted] || Service.Condition[ConditionFlag.Mounted2]).ToString());
            if (readyToTheNextpos == false && !isVnavWorking)
            {
                var minDistance = 1000000;
                GameObject closedObject = null;
                List<(int distance, GameObject enemy)> enemyList = getEnemyListByObjectId(HuntEnemyId);
                foreach (var temp in enemyList)
                {
                    if (!temp.enemy.IsDead && temp.distance < minDistance)
                    {
                        minDistance = temp.distance;
                        closedObject = temp.enemy;
                    }
                }

                if (minDistance <= 20)
                {
                    //if (!Service.Condition[ConditionFlag.InCombat])
                    //{
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, closedObject.Position);
                    if (distance > 5 && !(Service.Condition[ConditionFlag.Casting] || Service.Condition[ConditionFlag.Casting87]))
                    {
                        walkto(closedObject.Position.X, closedObject.Position.Y, closedObject.Position.Z);
                    }
                    attackObject(closedObject);
                    //}
                }
                else
                {
                    //Service.Chat.Print("minDistance:" + minDistance.ToString() + ", readyToTheNextpos:" + readyToTheNextpos.ToString() + ", needToTakeOff:" + needToTakeOff.ToString() + ", DataIndex:" + DataIndex.ToString());
                    needToTakeOff = true;
                    if (!Service.Condition[ConditionFlag.InFlight])
                    {
                        DataIndex++;
                        readyToTheNextpos = true;
                    }
                }
            }
        }
        else
        {
            DataIndex++;
            readyToTheNextpos = true;
            isVnavWorking = false;
        }
    }

    public List<(int distance, GameObject enemy)> getEnemyListByObjectId(int dataId)
    {
        if (Service.ObjectTable != null && Service.ObjectTable.Length > 0)
        {
            List<(int distance, GameObject enemy)> listTemp = new List<(int distance, GameObject enemy)>();
            foreach (var temp in Service.ObjectTable)
            {
                //Service.Chat.Print(temp.Name + ":" + temp.DataId);
                if (temp.DataId == dataId)
                {
                    var distance = Vector3.Distance(Service.ClientState.LocalPlayer?.Position ?? Vector3.Zero, temp.Position);

                    listTemp.Add(((int)distance, temp));
                }
            }
            return listTemp;
        }
        else
        {
            Service.Chat.Print("No Object Around");
            return new List<(int distance, GameObject enemy)>();
        }
    }
    public void attackObject(GameObject o)
    {
        if (o is null || o.IsDead) return;
        Service.TargetManager.SetTarget(o);
        BattleChara b = o as BattleChara;
        uint jobId = Service.ClientState.LocalPlayer.ClassJob.Id;
        if (b.CurrentHp == b.MaxHp)
        {
            if (jobId == 21)
            {
                ActionManager.Instance()->UseAction(ActionType.Action, 31, o.ObjectId); //飞斧
            }
            else if (jobId == 27)
            {
                ActionManager.Instance()->UseAction(ActionType.Action, 3579, o.ObjectId); //毁荡
            }
            else if (jobId == 31)
            {
                ActionManager.Instance()->UseAction(ActionType.Action, 7411, o.ObjectId); //热分裂弹
            }
        }
    }
    public void attackObjectForFate(GameObject o)
    {
        if (o is null || o.IsDead) return;
        Service.TargetManager.SetTarget(o);

        BattleChara b = o as BattleChara;
        uint jobId = Service.ClientState.LocalPlayer.ClassJob.Id;
        if (b.CurrentHp == b.MaxHp)
        {
            if (jobId == 21)
            {
                ActionManager.Instance()->UseAction(ActionType.Action, 31, o.ObjectId); //飞斧
            }
            else if (jobId == 27)
            {
                ActionManager.Instance()->UseAction(ActionType.Action, 3579, o.ObjectId); //毁荡
            }
            else if (jobId == 31)
            {
                ActionManager.Instance()->UseAction(ActionType.Action, 7411, o.ObjectId); //热分裂弹
            }
        }
    }
    public void reset()
    {
        DataIndex = 0;
        readyToTheNextpos = true;
        isVnavWorking = false;

        fateEnemyList = new List<GameObject>();
    }

    public void walkto(float x, float y, float z)
    {
        _chat.ExecuteCommand($"/vnavmesh moveto {x} {y} {z}");
    }

    public void walktowithmount(float x, float y, float z)
    {
        if (!Service.Condition[ConditionFlag.Mounted])
        {
            Mount();
        }
        else
        {
            _chat.ExecuteCommand($"/vnavmesh moveto {x} {y} {z}");
        }
    }

    public void flyto(float x, float y, float z)
    {
        if (!Service.Condition[ConditionFlag.InFlight])
        {
            needToTakeOff = true;
        }
        else
        {
            _chat.ExecuteCommand($"/vnavmesh flyto {x} {y} {z}");
        }
    }

    public void Mount()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9); //上坐骑
    }
    public void Dismount()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 23); //下坐骑
    }
    public void Takeoff()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 2); //起飞
        needToTakeOff = false;
    }
    public void TurnOnRS()
    {
        _chat.ExecuteCommand($"/rotation off");
        _chat.ExecuteCommand($"/rotation Auto");
    }
    public void Stop()
    {
        _chat.ExecuteCommand($"/vnavmesh stop");
    }

    public class Configs
    {
        public bool ExcludeARR = false;

        public bool ExcludeHW = false;

        public bool ExcludeSB = false;

        public bool ExcludeShB = false;

        public bool ExcludeEW = false;

        public bool ExcludeCombat = false;
    }
    public Configs Config { get; private set; }
    public void SyncFate(ushort value)
    {
        if (value != 0)
        {
            //var zone = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>().Where(x => x.RowId == Svc.ClientState.TerritoryType).First();
            //if (zone.ExVersion.Row == 0 && Config.ExcludeARR) return;
            //if (zone.ExVersion.Row == 1 && Config.ExcludeHW) return;
            //if (zone.ExVersion.Row == 2 && Config.ExcludeSB) return;
            //if (zone.ExVersion.Row == 3 && Config.ExcludeShB) return;
            //if (zone.ExVersion.Row == 4 && Config.ExcludeEW) return;
            if (Service.Condition[ConditionFlag.InCombat]) return;

            var FateMaxLevel = FateManager.Instance()->CurrentFate->MaxLevel;
            if (Svc.ClientState.LocalPlayer.Level > FateMaxLevel)
                Chat.Instance.SendMessage("/lsync");
        }
    }
}
