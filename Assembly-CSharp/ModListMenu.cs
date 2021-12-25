using System;
using System.Collections.Generic;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;
using static Modding.ModLoader;
using Patch = Modding.Patches;

namespace Modding
{
    internal class ModListMenu
    {
        private MenuScreen screen;
        public static Dictionary<IMod, MenuScreen> ModScreens = new Dictionary<IMod, MenuScreen>();

        // Due to the lifecycle of the UIManager object, The `EditMenus` event has to be used to create custom menus.
        // This event is called every time a UIManager is created,
        // and will also call the added action if the UIManager has already started.
        internal void InitMenuCreation()
        {
            Patch.UIManager.EditMenus += () =>
            {
                ModScreens = new Dictionary<IMod, MenuScreen>();
                var builder = new MenuBuilder("ModListMenu");
                this.screen = builder.Screen;
                builder.CreateTitle("Mods", MenuTitleStyle.vanillaStyle)
                    .SetDefaultNavGraph(new ChainedNavGraph())
                    .CreateContentPane(RectTransformData.FromSizeAndPos(
                        new RelVector2(new Vector2(1920f, 903f)),
                        new AnchoredPosition(
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0f, -60f)
                        )
                    ))
                    .CreateControlPane(RectTransformData.FromSizeAndPos(
                        new RelVector2(new Vector2(1920f, 259f)),
                        new AnchoredPosition(
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0f, -502f)
                        )
                    ))
                    .AddContent(
                        new NullContentLayout(),
                        c => c.AddScrollPaneContent(
                            new ScrollbarConfig
                            {
                                CancelAction = _ => this.ApplyChanges(),
                                Navigation = new Navigation { mode = Navigation.Mode.Explicit },
                                Position = new AnchoredPosition
                                {
                                    ChildAnchor = new Vector2(0f, 1f),
                                    ParentAnchor = new Vector2(1f, 1f),
                                    Offset = new Vector2(-310f, 0f)
                                },
                                SelectionPadding = _ => (-60, 0)
                            },
                            new RelLength(0f),
                            RegularGridLayout.CreateVerticalLayout(105f),
                            c =>
                            {
                                foreach (var mod in ModLoader.LoadedMods)
                                {
                                    ModToggleDelegates? toggleDels = null;
                                    if (mod is ITogglableMod itmod)
                                    {
                                        try 
                                        {    
                                            if (
                                                mod is not (
                                                    IMenuMod { ToggleButtonInsideMenu: true } or
                                                    ICustomMenuMod { ToggleButtonInsideMenu: true }
                                                )
                                            )
                                            {
                                                var rt = c.ContentObject.GetComponent<RectTransform>();
                                                rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                                c.AddHorizontalOption(
                                                    mod.GetName(),
                                                    new HorizontalOptionConfig
                                                    {
                                                        ApplySetting = (self, ind) =>
                                                        {
                                                            if (ind == 1)
                                                            {
                                                                ModLoader.LoadMod(mod, true);
                                                            }
                                                            else
                                                            {
                                                                //cast shouldnt fail cuz its checked for above
                                                                ModLoader.UnloadMod((ITogglableMod)mod);
                                                            }
                                                        },
                                                        CancelAction = _ => this.ApplyChanges(),
                                                        Label = mod.GetName(),
                                                        Options = new string[] { "Off", "On" },
                                                        RefreshSetting = (self, apply) => self.optionList.SetOptionTo(
                                                            ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()] ? 1 : 0
                                                        ),
                                                        Style = HorizontalOptionStyle.VanillaStyle,
                                                        Description = new DescriptionInfo
                                                        {
                                                            Text = $"Version {mod.GetVersion()}"
                                                        }
                                                    },
                                                    out var opt
                                                );
                                                opt.menuSetting.RefreshValueFromGameSettings();
                                            }
                                            else
                                            {
                                                toggleDels = new ModToggleDelegates
                                                {
                                                    SetModEnabled = enabled =>
                                                    {
                                                        if (enabled)
                                                        {
                                                            ModLoader.LoadMod(mod, true);
                                                        }
                                                        else
                                                        {
                                                            //cast shouldnt fail cuz its checked for above
                                                            ModLoader.UnloadMod((ITogglableMod)mod);
                                                        }
                                                    },
                                                    GetModEnabled = () =>  ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()],
                                                    #pragma warning disable CS0618
                                                    // Kept for backwards compatability.
                                                    ApplyChange = () => {  }
                                                    #pragma warning restore CS0618
                                                };
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.APILogger.LogError(e);
                                        }
                                    }
                                    if (mod is IMenuMod)
                                    {
                                        try 
                                        {
                                            var menu = CreateModMenu(mod, toggleDels);
                                            var rt = c.ContentObject.GetComponent<RectTransform>();
                                            rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                            c.AddMenuButton(
                                                $"{mod.GetName()}_Settings",
                                                new MenuButtonConfig
                                                {
                                                    Style = MenuButtonStyle.VanillaStyle,
                                                    CancelAction = _ => this.ApplyChanges(),
                                                    Label = toggleDels == null ? $"{mod.GetName()} Settings" : mod.GetName(),
                                                    SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                        .UIGoToDynamicMenu(menu),
                                                    Proceed = true,
                                                    Description = new DescriptionInfo
                                                    {
                                                        Text = $"Version {mod.GetVersion()}"
                                                    }
                                                }
                                            );
                                            ModScreens[mod] = menu;
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.APILogger.LogError(e);
                                        }
                                    }
                                    else if (mod is ICustomMenuMod icmmod)
                                    {
                                        try {
                                            var menu = icmmod.GetMenuScreen(this.screen, toggleDels);
                                            var rt = c.ContentObject.GetComponent<RectTransform>();
                                            rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                            c.AddMenuButton(
                                                $"{mod.GetName()}_Settings",
                                                new MenuButtonConfig
                                                {
                                                    Style = MenuButtonStyle.VanillaStyle,
                                                    CancelAction = _ => this.ApplyChanges(),
                                                    Label = toggleDels == null ? $"{mod.GetName()} Settings" : mod.GetName(),
                                                    SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                        .UIGoToDynamicMenu(menu),
                                                    Proceed = true,
                                                    Description = new DescriptionInfo
                                                    {
                                                        Text = $"Version {mod.GetVersion()}"
                                                    }
                                                }
                                            );
                                            ModScreens[mod] = menu;
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.APILogger.LogError(e);
                                        }
                                    }
                                }
                            }
                        )
                    )
                    .AddControls(
                        new SingleContentLayout(new AnchoredPosition(
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0f, -64f)
                        )),
                        c => c.AddMenuButton(
                            "BackButton",
                            new MenuButtonConfig
                            {
                                Label = "Back",
                                CancelAction = _ => this.ApplyChanges(),
                                SubmitAction = _ => this.ApplyChanges(),
                                Proceed = true,
                                Style = MenuButtonStyle.VanillaStyle
                            }
                        )
                    )
                    .Build();

                var optScreen = UIManager.instance.optionsMenuScreen;
                var mbl = (Modding.Patches.MenuButtonList)optScreen.gameObject.GetComponent<MenuButtonList>();
                new ContentArea(optScreen.content.gameObject, new SingleContentLayout(new Vector2(0.5f, 0.5f)))
                    .AddWrappedItem(
                        "ModMenuButtonWrapper",
                        c =>
                        {
                            c.AddMenuButton(
                                "ModMenuButton",
                                new MenuButtonConfig
                                {
                                    CancelAction = self => UIManager.instance.UIGoToMainMenu(),
                                    Label = "Mods",
                                    SubmitAction = GoToModListMenu,
                                    Proceed = true,
                                    Style = MenuButtonStyle.VanillaStyle
                                },
                                out var modButton
                            );
                            mbl.AddSelectableEnd(modButton, 1);
                        }
                    );
                mbl.RecalculateNavigation();
            };
        }

        private void ApplyChanges()
        {
            ((Patch.UIManager)UIManager.instance).UILeaveDynamicMenu(
                UIManager.instance.optionsMenuScreen,
                Patch.MainMenuState.OPTIONS_MENU
            );
        }

        private MenuScreen CreateModMenu(IMod imod, ModToggleDelegates? toggleDelegates)
        {
            var mod = imod as IMenuMod;
            IMenuMod.MenuEntry? toggleEntry = toggleDelegates is ModToggleDelegates dels ? new IMenuMod.MenuEntry
            {
                Name = mod.GetName(),
                Values = new string[] { "Off", "On" },
                Saver = v => dels.SetModEnabled(v == 1),
                Loader = () => dels.GetModEnabled() ? 1 : 0,
            } : null;
            
            var name = mod.GetName();
            var entries = mod.GetMenuData(toggleEntry);
            return MenuUtils.CreateMenuScreen(name, entries, this.screen);
        }

        private void GoToModListMenu(object _) => GoToModListMenu();
        private void GoToModListMenu() => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen);

    }
}
