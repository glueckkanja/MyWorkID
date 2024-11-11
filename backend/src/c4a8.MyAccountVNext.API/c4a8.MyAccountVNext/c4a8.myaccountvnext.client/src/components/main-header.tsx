import { useTheme } from "./use-theme";

const headerLogo = (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="25"
    height="48"
    viewBox="0 0 25 48"
    fill="none"
  >
    <path
      d="M22.1483 2C15.8583 2 9.60492 3.15673 3.79224 5.57548C3.26225 5.79976 2.80994 6.17416 2.49135 6.65228C2.17277 7.13041 2.00193 7.69122 2 8.26522V22.8751C2.05638 27.7027 3.59954 32.3963 6.4205 36.3204C9.24145 40.2445 13.2039 43.2095 17.7713 44.8139L19.9224 45.6059C20.6668 45.8785 21.4017 46 22.2299 46"
      stroke="url(#paint0_linear_2224_212)"
      stroke-width="3.5658"
      stroke-linecap="square"
      stroke-linejoin="round"
    />
    <defs>
      <linearGradient
        id="paint0_linear_2224_212"
        x1="12.1149"
        y1="1.9979"
        x2="12.1149"
        y2="46.0005"
        gradientUnits="userSpaceOnUse"
      >
        <stop stop-color="#5CBBFF" />
        <stop offset="1" stop-color="#0072C6" />
      </linearGradient>
    </defs>
  </svg>
);
const darkModeIcon = (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="38"
    height="38"
    viewBox="0 0 38 38"
    fill="none"
  >
    <path
      d="M9.18182 19C9.18182 21.6039 10.2162 24.1012 12.0575 25.9425C13.8988 27.7838 16.3961 28.8182 19 28.8182M9.18182 19C9.18182 16.3961 10.2162 13.8988 12.0575 12.0575C13.8988 10.2162 16.3961 9.18182 19 9.18182M9.18182 19H1M9.18182 19H5.90909M19 28.8182C21.6039 28.8182 24.1012 27.7838 25.9425 25.9425C27.7838 24.1012 28.8182 21.6039 28.8182 19M19 28.8182V37M28.8182 19C28.8182 16.3961 27.7838 13.8988 25.9425 12.0575C24.1012 10.2162 21.6039 9.18182 19 9.18182M28.8182 19H37M19 9.18182V1M16.48 9.50909C15.7779 11.2003 15.5627 13.0541 15.8587 14.8612C16.1548 16.6682 16.9503 18.3565 18.1554 19.7352C19.3605 21.1139 20.9272 22.1281 22.6784 22.6633C24.4296 23.1984 26.2955 23.2332 28.0655 22.7636M31.7309 6.26909L25.9382 12.0618M31.7309 31.7309L25.9382 25.9382M6.26909 31.7309L12.0618 25.9382M6.26909 6.26909L12.0618 12.0618"
      stroke="url(#paint0_linear_2123_2054)"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    />
    <defs>
      <linearGradient
        id="paint0_linear_2123_2054"
        x1="19.1282"
        y1="0.0286983"
        x2="19.1283"
        y2="38.0062"
        gradientUnits="userSpaceOnUse"
      >
        <stop stop-color="#5CBBFF" />
        <stop offset="1" stop-color="#0072C6" />
      </linearGradient>
    </defs>
  </svg>
);
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
      {headerLogo}
      <div className="header__title">
        <span className="header__title__text-myWork">myWork</span>
        <span className="header__title__text-ID">ID</span>
      </div>
      <div className="header__dark-mode-toggle" onMouseDown={() => setColorTheme()}>
        {darkModeIcon}
      </div>
    </div>
  );
};
