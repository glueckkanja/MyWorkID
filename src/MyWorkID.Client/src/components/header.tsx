import { useTheme } from "./use-theme";
import HeaderLogoSvg from "../assets/svg/header-logo.svg";
import DarkModeSvg from "../assets/svg/dark-mode-toggle.svg";
import LogOutSvg from "../assets/svg/log-out.svg";
import HelpCircleSvg from "../assets/svg/help-circle.svg";
import { useMsal } from "@azure/msal-react";
import { useToast } from "@/hooks/use-toast";
import { useEffect, useState } from "react";
import { getFrontendOptions } from "@/services/frontend-options-service";

enum ColorTheme {
  Light = "light",
  Dark = "dark",
}
const STORAGEKEY = "vite-ui-theme";
type Theme = ColorTheme.Dark | ColorTheme.Light;
export const Header = () => {
  const { instance } = useMsal();
  const { setTheme } = useTheme();
  const { toastError } = useToast();
  const [helpUrl, setHelpUrl] = useState<string | undefined>(undefined);

  useEffect(() => {
    let cancelled = false;
    void getFrontendOptions()
      .then((options) => {
        if (cancelled) return;
        const url = options.helpUrl?.trim();
        if (!url) return;
        try {
          const parsed = new URL(url);
          if (parsed.protocol !== "https:") return;
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

  const setColorTheme = () => {
    if ((localStorage.getItem(STORAGEKEY) as Theme) === "dark") {
      setTheme(ColorTheme.Light);
    } else {
      setTheme(ColorTheme.Dark);
    }
  };
  const handleLogoutRedirect = () => {
    instance
      .logoutRedirect()
      .catch(() => toastError("Logout failed. Please try again."));
  };
  return (
    <div className="header__container no-select">
      <img src={HeaderLogoSvg} alt="myWorkID Logo" />
      <div className="header__title">
        <span className="header__title__text-myWork">MyWork</span>
        <span className="header__title__text-ID">ID</span>
      </div>
      <div className="header__buttons">
        {helpUrl && (
          <a
            className="header__buttons__help-link"
            href={helpUrl}
            target="_blank"
            rel="noopener noreferrer"
            aria-label="Help"
            title="Help"
          >
            <img src={HelpCircleSvg} alt="Help" className="header__buttons__help-link__icon" />
          </a>
        )}
        <button
          className="header__buttons__logout-button"
          onClick={handleLogoutRedirect}
        >
          <img src={LogOutSvg} alt="Logout Button" />
        </button>
        <button
          className="header__buttons__dark-mode-toggle"
          tabIndex={0}
          onMouseDown={() => setColorTheme()}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              setColorTheme();
            }
          }}
        >
          <img src={DarkModeSvg} alt="Darkmode Toggle" />
        </button>
      </div>
    </div>
  );
};
