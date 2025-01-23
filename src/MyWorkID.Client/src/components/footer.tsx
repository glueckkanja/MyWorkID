import BrandingSvg from "../assets/svg/branding.svg";
import BrandingSvgDark from "../assets/svg/branding-dark.svg";

export const Footer = () => {
  return (
    <div className="footer__container">
      <span className="footer__text">powered by</span>
      <div className="footer_svg-icon">
        <img
          className="footer_svg-icon-light"
          src={BrandingSvg}
          alt="BrandingLogo"
        />
        <img
          className="footer_svg-icon-dark"
          src={BrandingSvgDark}
          alt="BrandingLogo"
        />
      </div>
    </div>
  );
};
