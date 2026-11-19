import type { ReactNode } from "react";
import { InformationCircleIcon } from "@/components/ui/icons/information-circle-icon";

/**
 * Grey box with an information icon, used for hints inside action panels.
 */
export const InformationBox = ({
  children,
}: Readonly<{ children: ReactNode }>) => (
  <div className="confirm-box confirm-box--neutral">
    <InformationCircleIcon aria-hidden="true" />
    <span>{children}</span>
  </div>
);
