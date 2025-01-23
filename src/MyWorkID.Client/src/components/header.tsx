import { useTheme } from "./use-theme";
import HeaderLogoSvg from "../assets/svg/header-logo.svg";
import DarkModeSvg from "../assets/svg/dark-mode-toggle.svg";

enum ColorTheme {
  Light = "light",
  Dark = "dark",
}
const STORAGEKEY = "vite-ui-theme";
type Theme = ColorTheme.Dark | ColorTheme.Light;
export const Header = () => {
  const { setTheme } = useTheme();
  const setColorTheme = () => {
    if ((localStorage.getItem(STORAGEKEY) as Theme) === "dark") {
      setTheme(ColorTheme.Light);
    } else {
      setTheme(ColorTheme.Dark);
    }
  };

  return (
    <div className="header__container">
      <img src={HeaderLogoSvg} alt="myWorkID Logo" />
      <div className="header__title">
        <span className="header__title__text-myWork">myWork</span>
        <span className="header__title__text-ID">ID</span>
      </div>
      <button
        className="header__dark-mode-toggle"
        tabIndex={0}
        onMouseDown={() => setColorTheme()}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            setColorTheme();
          }
        }}
      >
        <img src={DarkModeSvg} alt="DarkModeToggle" />
      </button>
    </div>
  );
};
