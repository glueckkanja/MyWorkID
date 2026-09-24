import { SvgIconProps } from "../../../types";

export const AlertErrorIcon = ({
  width = 16,
  height = 16,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 2,
  strokeLinecap = "round",
  strokeLinejoin = "round",
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
      strokeLinejoin={strokeLinejoin}
      {...svgProperties}
    >
      <path d="M7.86 2h8.28L22 7.86v8.28L16.14 22H7.86L2 16.14V7.86L7.86 2Z" />
      <path d="M12 8v4" />
      <path d="M12 16h.01" />
    </svg>
  );
};
