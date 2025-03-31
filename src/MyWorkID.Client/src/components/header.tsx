import { useTheme } from "./use-theme";
import HeaderLogoSvg from "../assets/svg/header-logo.svg";
import DarkModeSvg from "../assets/svg/dark-mode-toggle.svg";
import LogOutSvg from "../assets/svg/log-out.svg";
import { useMsal } from "@azure/msal-react";
import { useToast } from "@/hooks/use-toast";

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
        <button
          className="header__buttons__logout-button"
          onClick={handleLogoutRedirect}
        >
          <img src={LogOutSvg} alt="LogOut Button" />
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
          <img src={DarkModeSvg} alt="DarkMode Toggle" />
        </button>
      </div>
    </div>
  );
};
