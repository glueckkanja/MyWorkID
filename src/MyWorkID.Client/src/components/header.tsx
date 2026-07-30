import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { useToast } from "@/hooks/use-toast";
import { getFrontendOptions } from "@/services/frontend-options-service";
import { useTheme } from "./use-theme";

// Import Icons
import HeaderLogoSvg from "../assets/svg/header-logo.svg";
import { CrescentMoonIcon } from "@/components/ui/icons/crescent-moon-icon";
import { HelpCircleIcon } from "@/components/ui/icons/help-circle-icon";
import { LogoutArrowIcon } from "@/components/ui/icons/logout-arrow-icon";
import { SettingsGearIcon } from "@/components/ui/icons/settings-gear-icon";

enum ColorTheme {
  Light = "light",
  Dark = "dark",
}

const STORAGE_KEY = "vite-ui-theme";

export const Header = () => {
  const { instance } = useMsal();
  const { setTheme } = useTheme();
  const { toastError } = useToast();
  const [helpUrl, setHelpUrl] = useState<string | undefined>(undefined);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [darkMode, setDarkMode] = useState(
    () => (localStorage.getItem(STORAGE_KEY) as ColorTheme) === ColorTheme.Dark,
  );

  useEffect(() => {
    let cancelled = false;
    void getFrontendOptions()
      .then((options) => {
        if (cancelled) {
          return;
        }
        const url = options.helpUrl?.trim();
        if (!url) {
          return;
        }
        try {
          const parsed = new URL(url);
          if (parsed.protocol !== "https:") {
            return;
          }
          setHelpUrl(parsed.href);
        } catch {
          console.error("Invalid help URL:", url);
        }
      })
      .catch(() => {
        // ignore - header should still render
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const handleDarkModeToggle = (enabled: boolean) => {
    setDarkMode(enabled);
    setTheme(enabled ? ColorTheme.Dark : ColorTheme.Light);
  };

  const handleLogoutClick = () => {
    setSettingsOpen(false);
    instance
      .logoutRedirect()
      .catch(() => toastError("Logout failed. Please try again."));
  };

  return (
    <header className="app-header">
      <div className="app-header__logo">
        <img
          className="app-header__logo-mark"
          src={HeaderLogoSvg}
          alt="MyWorkID Logo"
        />
        <span className="app-header__logo-text">
          <span className="app-header__logo-text--work">MyWork</span>
          <span className="app-header__logo-text--id">ID</span>
        </span>
      </div>

      <div className="app-header__actions">
        {helpUrl && (
          <a
            className="app-header__icon-button"
            href={helpUrl}
            target="_blank"
            rel="noopener noreferrer"
            aria-label="Help"
            title="Help"
          >
            <HelpCircleIcon />
          </a>
        )}

        <button
          type="button"
          className="app-header__icon-button"
          aria-label="Settings"
          aria-haspopup="menu"
          aria-expanded={settingsOpen}
          title="Settings"
          onClick={() => setSettingsOpen((open) => !open)}
        >
          <SettingsGearIcon />
        </button>

        {settingsOpen && (
          <>
            <button
              type="button"
              className="settings-backdrop"
              aria-label="Close settings menu"
              onClick={() => setSettingsOpen(false)}
            />
            <div className="settings-dropdown" role="menu">
              <div className="settings-dropdown__header">
                <h3 className="settings-dropdown__title">Settings</h3>
                <p className="settings-dropdown__subtitle">
                  Manage your preferences and account.
                </p>
              </div>
              <div className="settings-dropdown__body">
                <div className="settings-item">
                  <div className="settings-item__left">
                    <div className="settings-item__icon">
                      <CrescentMoonIcon />
                    </div>
                    <div>
                      <div className="settings-item__label">Dark Mode</div>
                      <div className="settings-item__desc">
                        Switch between light and dark theme
                      </div>
                    </div>
                  </div>
                  <label className="toggle-switch">
                    <input
                      type="checkbox"
                      checked={darkMode}
                      onChange={(event) =>
                        handleDarkModeToggle(event.target.checked)
                      }
                      aria-label="Toggle dark mode"
                    />
                    <span className="toggle-switch__slider" />
                  </label>
                </div>
                <button
                  type="button"
                  className="settings-logout"
                  onClick={handleLogoutClick}
                >
                  <LogoutArrowIcon />
                  Log Out
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </header>
  );
};
