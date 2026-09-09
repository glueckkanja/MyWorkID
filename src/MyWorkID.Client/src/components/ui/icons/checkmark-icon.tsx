import { SvgIconProps } from "../../../types";

export const CheckmarkIcon = ({
  width = 14,
  height = 14,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 2,
  strokeLinecap = "round",
  ...svgProperties
}: SvgIconProps) => {
  return (
    <svg
      width={width}
      height={height}
      viewBox={viewBox}
      fill={fill}
      stroke={stroke}
      strokeWidth={strokeWidth}
      strokeLinecap={strokeLinecap}
      {...svgProperties}
    >
      <path d="M20 6L9 17l-5-5" />
    </svg>
  );
};
