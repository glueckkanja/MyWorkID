import type { ComponentPropsWithoutRef } from "react";

type CrescentMoonIconProps = ComponentPropsWithoutRef<"svg">;

export const CrescentMoonIcon = ({
  width = 16,
  height = 16,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 1.8,
  strokeLinecap = "round",
  ...svgProperties
}: CrescentMoonIconProps) => {
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
      <path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z" />
    </svg>
  );
};
