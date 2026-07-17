import type { ComponentPropsWithoutRef } from "react";

type IdentityVerificationIconProps = ComponentPropsWithoutRef<"svg">;

export const IdentityVerificationIcon = ({
  width = 20,
  height = 20,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 1.8,
  strokeLinecap = "round",
  strokeLinejoin = "round",
  ...svgProperties
}: IdentityVerificationIconProps) => {
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
      <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  );
};
