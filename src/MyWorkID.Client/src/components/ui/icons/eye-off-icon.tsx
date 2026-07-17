import type { ComponentPropsWithoutRef } from "react";

type EyeOffIconProps = ComponentPropsWithoutRef<"svg">;

export const EyeOffIcon = ({
  width = 18,
  height = 18,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 1.8,
  strokeLinecap = "round",
  ...svgProperties
}: EyeOffIconProps) => {
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
      <path d="M17.94 17.94A10.94 10.94 0 0112 20c-7 0-11-8-11-8a19.77 19.77 0 015.06-5.94" />
      <path d="M9.9 4.24A10.94 10.94 0 0112 4c7 0 11 8 11 8a19.85 19.85 0 01-4.19 5.19" />
      <line x1="1" y1="1" x2="23" y2="23" />
    </svg>
  );
};
