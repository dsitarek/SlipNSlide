using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SlipNSlide.Windows;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System.Numerics;
using System.Drawing;
using System;

namespace SlipNSlide;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/pmycommand";

    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private MarkerWindow MarkerWindow { get; init; }
    private IAddonLifecycle AddonLifecycle { get; init; }
    private CharacterData Char { get; init; }
    private IClientState ClientState { get; init; }
    private IChatGui ChatGui { get; init; }
    private ImDrawListPtr imDrawList { get; init; }

    private unsafe void CastbarListener(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        var state = PlayerState.Instance();
        var character = ClientState.LocalPlayer;
        var actionManager = ActionManager.Instance();
        if(character != null)
        {
            var recast = actionManager->GetRecastTime((ActionType)character.CastActionType, character.CastActionId);
            var recastPercentage = character.AdjustedTotalCastTime / recast;
            var node = addon->GetNodeById(9);
            var barX = node->ScreenX;
            var barY = node->ScreenY;
            var width = node->Width * addon->Scale;
            var height = node->Height * addon->Scale;
            var v1 = new Vector2(barX  + (width * recastPercentage), barY);
            var v2 = new Vector2(barX + (width - (width * recastPercentage) -5)  + (width * recastPercentage), barY + height/5);
            MarkerWindow.Position = new Vector2(barX, barY);
            MarkerWindow.Size = new Vector2(width, height);
            MarkerWindow.v1 = v1;
            MarkerWindow.v2 = v2;
            if (character.IsCasting && recastPercentage < 1) MarkerWindow.IsOpen = true;
            else MarkerWindow.IsOpen = false;
        }
    }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] ITextureProvider textureProvider, IAddonLifecycle addonLifecycle, IClientState clientState)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        AddonLifecycle = addonLifecycle;
        ClientState = clientState;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        // you might normally want to embed resources and load them from the manifest stream
        var file = new FileInfo(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png"));

        // ITextureProvider takes care of the image caching and dispose
        var goatImage = textureProvider.GetTextureFromFile(file);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImage);
        MarkerWindow = new MarkerWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(MarkerWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        addonLifecycle.RegisterListener(Dalamud.Game.Addon.Lifecycle.AddonEvent.PreDraw, "_CastBar", CastbarListener);
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
