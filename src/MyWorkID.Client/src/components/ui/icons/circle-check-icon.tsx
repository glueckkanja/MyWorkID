import type { ComponentPropsWithoutRef } from "react";

type CircleCheckIconProps = ComponentPropsWithoutRef<"svg">;

export const CircleCheckIcon = ({
  width = 18,
  height = 18,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 2,
  strokeLinecap = "round",
  strokeLinejoin = "round",
  ...svgProperties
}: CircleCheckIconProps) => {
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
      <path d="M9 12l2 2 4-4" />
      <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z" />
    </svg>
  );
};