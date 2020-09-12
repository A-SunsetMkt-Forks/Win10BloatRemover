﻿using Microsoft.Win32;

namespace Win10BloatRemover.Operations
{
    public class TipsDisabler : IOperation
    {
        private readonly IUserInterface ui;
        public TipsDisabler(IUserInterface ui) => this.ui = ui;

        public void Run()
        {
            DisableTips();
            DisableFeedbackRequests();
        }

        private void DisableTips()
        {
            ui.PrintHeading("Disabling Tips and Spotlight via Registry edits...");

            // These two policies work only on Education and Enterprise editions
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableSoftLanding", 1);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1);

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightFeatures", 1);
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\CloudContent",
                "DisableTailoredExperiencesWithDiagnosticData", 1
            );

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
            {
                // System -> Notifications & actions -> Show the Windows welcome experience...
                key.SetValue("SubscribedContent-310093Enabled", 0);
                // System -> Notifications & actions -> Get tips, tricks, and suggestions as you use Windows
                key.SetValue("SubscribedContent-338389Enabled", 0);
                // Personalization -> Start -> Show suggestions occasionally in Start
                key.SetValue("SubscribedContent-338388Enabled", 0);
                // Privacy -> General -> Show suggested content in Settings app
                key.SetValue("SubscribedContent-338393Enabled", 0);
                key.SetValue("SubscribedContent-353694Enabled", 0);
                key.SetValue("SubscribedContent-353696Enabled", 0);
            }

            Registry.SetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsInkWorkspace",
                "AllowSuggestedAppsInWindowsInkWorkspace", 0
            );
        }

        private void DisableFeedbackRequests()
        {
            ui.PrintHeading("Disabling feedback requests via Registry edits...");
            Registry.SetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                "DoNotShowFeedbackNotifications", 1
            );
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0);

            ui.PrintHeading("Disabling feedback-related scheduled tasks...");
            new ScheduledTasksDisabler(new[] {
                @"\Microsoft\Windows\Feedback\Siuf\DmClient",
                @"\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload"
            }, ui).Run();
        }
    }
}
