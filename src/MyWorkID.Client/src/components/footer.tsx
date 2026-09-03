import BrandingSvg from "../assets/svg/branding.svg";
import BrandingDarkSvg from "../assets/svg/branding-dark.svg";

export const Footer = () => {
  return (
    <footer className="app-footer">
      <div className="app-footer__row">
        <span className="app-footer__text">powered by</span>
        <a
          href="https://www.glueckkanja.com"
          target="_blank"
          rel="noopener noreferrer"
          aria-label="glueckkanja"
          className="app-footer__logo-link"
        >
          <img
            className="app-footer__logo app-footer__logo--light"
            src={BrandingSvg}
            alt="glueckkanja"
          />
          <img
            className="app-footer__logo app-footer__logo--dark"
            src={BrandingDarkSvg}
            alt="glueckkanja"
          />
        </a>
      </div>
      <span className="app-footer__version">{__APP_VERSION__}</span>
    </footer>
  );
};
