﻿using Win10BloatRemover.Operations;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover.UI;

abstract class MenuEntry
{
    public abstract string FullName { get; }
    public virtual bool ShouldQuit => false;
    public abstract string GetExplanation();
    public abstract IOperation CreateNewOperation(IUserInterface ui);
}

class UWPAppRemovalEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Remove UWP apps";
    public override string GetExplanation()
    {
        string impactedUsers = configuration.UWPAppsRemovalMode == UwpAppRemovalMode.CurrentUser
            ? "the current user"
            : "all present and future users";
        string explanation = $"The following groups of UWP apps will be removed for {impactedUsers}:";
        foreach (UwpAppGroup app in configuration.UWPAppsToRemove)
            explanation += $"\n  {app}";

        if (configuration.UWPAppsRemovalMode == UwpAppRemovalMode.AllUsers)
            explanation += "\n\nServices, components and scheduled tasks used specifically by those apps will also " +
                           "be disabled or removed,\ntogether with any leftover data.";

        return explanation;
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new UwpAppGroupRemover(configuration.UWPAppsToRemove, configuration.UWPAppsRemovalMode,
                                  ui, new AppxRemover(ui), new ServiceRemover(ui));
}

class DefenderDisablingEntry : MenuEntry
{
    public override string FullName => "Disable Windows Defender antivirus";
    public override string GetExplanation() => """
        IMPORTANT: Before starting, disable Tamper protection in Windows Security app under Virus & threat protection settings.

        Windows Defender antimalware engine and SmartScreen feature will be disabled via Group Policies, and services
        related to those features will be removed.
        Furthermore, Windows Security app will be prevented from running automatically at system start-up.
        Windows Defender Firewall will continue to work as intended.

        Be aware that SmartScreen for Microsoft Edge and Store apps will be disabled only for the currently logged in user
        and for new users created after running this procedure.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new DefenderDisabler(ui, new ServiceRemover(ui));
}

class EdgeRemovalEntry : MenuEntry
{
    public override string FullName => "Remove Microsoft Edge";
    public override string GetExplanation() => """
        Both Edge Chromium and the legacy Edge browser will be uninstalled from the system.
        Be aware that Windows cumulative updates might reinstall Edge Chromium automatically.
        Make sure that Edge Chromium is not updating itself before proceeding.

        Note that Edge WebView2 runtime will NOT be removed if it's installed, as it may be required
        by other programs installed on this PC.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new EdgeRemover(ui, new AppxRemover(ui));
}

class OneDriveRemovalEntry : MenuEntry
{
    public override string FullName => "Remove OneDrive";
    public override string GetExplanation() => """
        OneDrive will be disabled using Group Policies and then uninstalled for the current user.
        Furthermore, it will be prevented from being installed when a new user logs in for the first time.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new OneDriveRemover(ui);
}

class ServicesRemovalEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Remove miscellaneous services";
    public override string GetExplanation()
    {
        string explanation = "All services whose name starts with the following names will be removed:\n";
        foreach (string service in configuration.ServicesToRemove)
            explanation += $"  {service}\n";
        return explanation + "\nServices will be backed up in the same folder as this program executable.";
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new ServiceRemovalOperation(configuration.ServicesToRemove, ui, new ServiceRemover(ui));
}

class WindowsFeaturesRemovalEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Remove Windows features";
    public override string GetExplanation()
    {
        string explanation = "The following features on demand will be removed:";
        foreach (string feature in configuration.WindowsFeaturesToRemove)
            explanation += $"\n  {feature}";
        return explanation;
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new FeaturesRemover(configuration.WindowsFeaturesToRemove, ui);
}

class PrivacySettingsTweakEntry : MenuEntry
{
    public override string FullName => "Tweak settings for privacy";
    public override string GetExplanation() => """
        Several default settings and policies will be changed to make Windows more respectful of users' privacy.
        These changes consist essentially of:
          - adjusting various options under Privacy section of Settings app (disable advertising ID, app launch tracking etc.)
          - preventing input data (inking/typing information, speech) from being sent to Microsoft to improve their services
          - preventing Edge from sending browsing history, favorites and other data to Microsoft in order to personalize ads,
            news and other services for your Microsoft account
          - denying access to sensitive data (location, documents, activities, account details, diagnostic info) to
            all UWP apps by default
          - denying location access to Windows search
          - disabling voice activation for voice assistants (so that they can't always be listening)
          - disabling cloud synchronization of sensitive data (user activities, clipboard, text messages, passwords
            and app data)

        Whereas almost all of these settings are applied for all users, some of them will only be changed for the current
        user and for new users created after running this procedure.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new PrivacySettingsTweaker(ui);
}

class TelemetryDisablingEntry : MenuEntry
{
    public override string FullName => "Disable telemetry";
    public override string GetExplanation() => """
        This procedure will disable scheduled tasks, services and features that are responsible for collecting and
        reporting data to Microsoft, including Compatibility Telemetry, Device Census, Customer Experience Improvement
        Program and Compatibility Assistant.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new TelemetryDisabler(ui, new ServiceRemover(ui));
}

class AutoUpdatesDisablingEntry : MenuEntry
{
    public override string FullName => "Disable automatic updates";
    public override string GetExplanation() => """
        Automatic updates for Windows, Store apps and speech models will be disabled using Group Policies.
        At least Windows 10 Pro edition is required to disable automatic Windows updates.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new AutoUpdatesDisabler(ui);
}

class ScheduledTasksDisablingEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Disable miscellaneous scheduled tasks";
    public override string GetExplanation()
    {
        string explanation = "The following scheduled tasks will be disabled:";
        foreach (string task in configuration.ScheduledTasksToDisable)
            explanation += $"\n  {task}";
        return explanation;
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new ScheduledTasksDisabler(configuration.ScheduledTasksToDisable, ui);
}

class ErrorReportingDisablingEntry : MenuEntry
{
    public override string FullName => "Disable Windows Error Reporting";
    public override string GetExplanation() => """
        Windows Error Reporting will disabled by editing Group Policies, as well as by removing its services (after
        backing them up).
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new ErrorReportingDisabler(ui, new ServiceRemover(ui));
}

class ConsumerFeaturesDisablingEntry : MenuEntry
{
    public override string FullName => "Disable consumer features";
    public override string GetExplanation() => """
        This procedure will disable the following cloud-powered features aimed at the consumer market:
          - Windows Spotlight (dynamic lock screen backgrounds)
          - Spotlight experiences and recommendations in Microsoft Edge
          - News and Interests
          - Search highlights
          - Bing search in Windows search bar
          - Skype's Meet Now icon in the taskbar
          - automatic installation of suggested apps
          - cloud optimized content in the taskbar

        Be aware that some of these features will be disabled only for the currently logged in user and for new users
        created after running this procedure.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new ConsumerFeaturesDisabler(ui);
}

class SuggestionsDisablingEntry : MenuEntry
{
    public override string FullName => "Disable suggestions and feedback requests";
    public override string GetExplanation() => """
        Feedback notifications and requests, apps suggestions, Windows and account-related tips will be turned off
        by changing Group Policies and system settings accordingly and by disabling some related scheduled tasks.

        If you are not using an Enterprise or Education edition of Windows, suggestions will be disabled only for the
        currently logged in user and for new users created after running this procedure.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new SuggestionsDisabler(ui);
}

class NewGitHubIssueEntry : MenuEntry
{
    public override string FullName => "Report an issue/Suggest a feature";
    public override string GetExplanation() => """
        You will now be brought to a web page where you can open a GitHub issue in order to report a bug or to suggest
        a new feature.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new BrowserOpener("https://github.com/Fs00/Win10BloatRemover/issues/new");
}

class AboutEntry : MenuEntry
{
    public override string FullName => "About this program";
    public override string GetExplanation()
    {
        Version programVersion = GetType().Assembly.GetName().Version!;
        return $"""
            Windows 10 Bloat Remover and Tweaker version {programVersion.Major}.{programVersion.Minor}
            Developed by Fs00
            Official GitHub repository: https://github.com/Fs00/Win10BloatRemover

            Originally based on Windows 10 de-botnet guide by Federico Dossena: https://fdossena.com
            Credits to all open source projects whose work has helped me to improve this software:
              - privacy.sexy website: https://privacy.sexy
              - Debloat Windows 10 scripts: https://github.com/W4RH4WK/Debloat-Windows-10
              - AveYo's Edge removal script: https://github.com/AveYo/fox

            This software is released under BSD 3-Clause Clear license (continue to read full text).
            """;
    }

    public override IOperation CreateNewOperation(IUserInterface ui) => new LicensePrinter(ui);
}

class QuitEntry(RebootRecommendedFlag rebootFlag) : MenuEntry
{
    public override string FullName => "Exit the application";
    public override bool ShouldQuit => true;
    public override string GetExplanation() => "Are you sure?";
    public override IOperation CreateNewOperation(IUserInterface ui) => new AskForRebootOperation(ui, rebootFlag);
}
