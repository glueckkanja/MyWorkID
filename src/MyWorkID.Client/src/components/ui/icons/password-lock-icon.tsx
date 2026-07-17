import type { ComponentPropsWithoutRef } from "react";

type PasswordLockIconProps = ComponentPropsWithoutRef<"svg">;

export const PasswordLockIcon = ({
  width = 20,
  height = 20,
  viewBox = "0 0 24 24",
  fill = "none",
  stroke = "currentColor",
  strokeWidth = 1.8,
  strokeLinecap = "round",
  strokeLinejoin = "round",
  ...svgProperties
}: PasswordLockIconProps) => {
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
      <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
      <path d="M7 11V7a5 5 0 0110 0v4" />
      <circle cx="12" cy="16" r="1" />
    </svg>
  );
};