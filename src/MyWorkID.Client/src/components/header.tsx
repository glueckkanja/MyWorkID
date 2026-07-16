import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { useToast } from "@/hooks/use-toast";
import { getFrontendOptions } from "@/services/frontend-options-service";
import { useTheme } from "./use-theme";
import HeaderLogoSvg from "../assets/svg/header-logo.svg";

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
    () => (localStorage.getItem(STORAGE_KEY) as ColorTheme) === ColorTheme.Dark
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
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="round"
            >
              <circle cx="12" cy="12" r="10" />
              <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" />
              <path d="M12 17h.01" />
            </svg>
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
          <svg
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
          >
            <circle cx="12" cy="12" r="3" />
            <path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 01-2.83 2.83l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z" />
          </svg>
        </button>

        {settingsOpen && (
          <>
            <div
              className="settings-backdrop"
              onClick={() => setSettingsOpen(false)}
              role="presentation"
              aria-hidden="true"
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
                      <svg
                        width="16"
                        height="16"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.8"
                        strokeLinecap="round"
                      >
                        <path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z" />
                      </svg>
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
                  <svg
                    width="16"
                    height="16"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                  >
                    <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4" />
                    <polyline points="16 17 21 12 16 7" />
                    <line x1="21" y1="12" x2="9" y2="12" />
                  </svg>
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
