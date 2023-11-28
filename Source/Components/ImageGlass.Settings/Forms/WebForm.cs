﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;

public partial class WebForm : ThemedForm
{
    // Public events
    #region Public events

    /// <summary>
    /// Occurs when <see cref="Web2"/> is ready to use.
    /// </summary>
    public event EventHandler<EventArgs> Web2Ready;

    /// <summary>
    /// Occurs when <see cref="Web2"/> navigation is completed
    /// </summary>
    public event EventHandler<EventArgs> Web2NavigationCompleted;

    /// <summary>
    /// Occurs when <see cref="Web2"/> receives a message from web view.
    /// </summary>
    public event EventHandler<Web2MessageReceivedEventArgs> Web2MessageReceived;

    #endregion // Public events


    public WebForm()
    {
        InitializeComponent();
        Web2.Web2Ready += Web2_Web2Ready;
        Web2.Web2NavigationCompleted += Web2_Web2NavigationCompleted;
        Web2.Web2MessageReceived += Web2_Web2MessageReceived;
        Web2.Web2KeyDown += Web2_Web2KeyDown;

        // hide the control by default
        // we will show it when the navigation is completed
        Web2.Visible = false;
        Web2.EnableDebug = Config.EnableDebug;

        _ = Web2.EnsureWeb2Async();
    }


    // Protected / override methods
    #region Protected / override methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        _ = Config.UpdateFormIcon(this);
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        if (DesignMode) return;

        if (!EnableTransparent)
        {
            BackColor = Config.Theme.ColorPalatte.AppBg;
        }

        Web2.DarkMode = darkMode;
        Web2.AccentColor = Config.Theme.ColorPalatte.Accent;

        base.ApplyTheme(darkMode, style);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        base.OnRequestUpdatingColorMode(e);

        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        base.OnSystemAccentColorChanged(e);
        _ = Web2.SetWeb2AccentColorAsync(e.AccentColor);
    }


    protected override void OnRequestUpdatingTheme(RequestUpdatingThemeEventArgs e)
    {
        base.OnRequestUpdatingTheme(e);
        _ = Web2.SetWeb2DarkModeAsync(e.Theme.Settings.IsDarkMode);
        _ = SetThemeAsync();
    }


    protected override void OnRequestUpdatingLanguage()
    {
        base.OnRequestUpdatingLanguage();

        _ = LoadLanguageAsync(true);
    }


    protected override void OnWindowStateChanging(WindowStateChangedEventArgs e)
    {
        base.OnWindowStateChanging(e);

        if (e.State == FormWindowState.Minimized)
        {
            _ = Web2.TrySuspendWeb2Async();
        }
    }


    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _ = Web2.SetWeb2WindowFocusAsync(true);
    }


    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        _ = Web2.SetWeb2WindowFocusAsync(false);
    }


    #endregion // Protected / override methods


    // Virtual methods
    #region Virtual methods

    /// <summary>
    /// Occurs when the <see cref="Web2"/> control is ready.
    /// </summary>
    protected virtual async Task OnWeb2ReadyAsync()
    {
        Web2Ready?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Occurs when the <see cref="Web2"/> navigation is completed.
    /// </summary>
    protected virtual async Task OnWeb2NavigationCompleted()
    {
        await SetThemeAsync();
        await LoadLanguageAsync(false);

        Web2NavigationCompleted?.Invoke(this, EventArgs.Empty);

        // show the control when the navigation is completed
        Web2.Visible = true;
    }


    /// <summary>
    /// Triggers <see cref="Web2MessageReceived"/> event.
    /// </summary>
    protected virtual async Task OnWeb2MessageReceivedAsync(Web2MessageReceivedEventArgs e)
    {
        Web2MessageReceived?.Invoke(this, e);
        await Task.CompletedTask;
    }

    #endregion // Virtual methods


    // Web2 events
    #region Web2 events

    private void Web2_Web2Ready(object? sender, EventArgs e)
    {
        _ = OnWeb2ReadyAsync();
    }

    private void Web2_Web2NavigationCompleted(object? sender, EventArgs e)
    {
        _ = OnWeb2NavigationCompleted();
    }

    private void Web2_Web2MessageReceived(object? sender, Web2MessageReceivedEventArgs e)
    {
        _ = OnWeb2MessageReceivedAsync(e);
    }

    private void Web2_Web2KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyData == CloseFormHotkey)
        {
            CloseFormByKeys();
        }
        else
        {
            this.OnKeyDown(e);
        }
    }

    #endregion // Web2 events


    /// <summary>
    /// Updates language
    /// </summary>
    public async Task LoadLanguageAsync(bool forced)
    {
        // get language as json string
        var langJson = BHelper.ToJson(Config.Language);

        await Web2.ExecuteScriptAsync($"""
            window._page.lang = {langJson};
            window._page.loadLanguage();
        """);
    }


    /// <summary>
    /// Sets <c>_page.theme</c> to the <see cref="Config.Theme"/> value.
    /// </summary>
    public async Task SetThemeAsync()
    {
        await Web2.ExecuteScriptAsync($"""
            window._page.theme = '{Config.Theme.FolderName}';
        """);
    }

}