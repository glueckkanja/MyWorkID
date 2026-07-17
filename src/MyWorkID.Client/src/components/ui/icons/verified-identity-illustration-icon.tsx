import type { ComponentPropsWithoutRef } from "react";

type VerifiedIdentityIllustrationIconProps = ComponentPropsWithoutRef<"svg">;

export const VerifiedIdentityIllustrationIcon = ({
  width = 80,
  height = 80,
  viewBox = "0 0 80 80",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 1.5,
  strokeLinecap = "round",
  ...svgProperties
}: VerifiedIdentityIllustrationIconProps) => {
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
      <rect x="8" y="8" width="64" height="64" rx="16" />
      <circle cx="40" cy="34" r="10" />
      <path d="M24 62c0-8.837 7.163-16 16-16s16 7.163 16 16" />
      <path d="M58 16l6-6M16 16l-6-6M58 64l6 6M16 64l-6 6" />
    </svg>
  );
};