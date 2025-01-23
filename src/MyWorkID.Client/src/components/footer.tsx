import BrandingSvg from "../assets/svg/branding.svg";

export const Footer = () => {
  return (
    <div className="footer__container">
      <span className="footer__text">powered by</span>
      <div className="footer_svg-icon">
        <img src={BrandingSvg} alt="BrandingLogo" />
      </div>
    </div>
  );
};
