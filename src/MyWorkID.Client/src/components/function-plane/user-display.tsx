import { UserProfile, VerifiedIdStatus } from "../../types";
import { RiskLevel } from "@/lib/risk-level";
import { formatDateTime } from "@/lib/verified-id-status";

const getInitials = (displayName: string | undefined): string => {
  if (!displayName) {
    return "";
  }
  const parts = displayName.trim().split(/\s+/);
  if (parts.length === 1) {
    return parts[0].substring(0, 2).toUpperCase();
  }
  return (parts[0][0] + parts.at(-1)![0]).toUpperCase();
};

const getBadgeClass = (riskLevel: RiskLevel, riskLoading: boolean): string => {
  if (riskLoading) {
    return "risk-badge risk-badge--loading";
  }
  if (riskLevel === RiskLevel.High) {
    return "risk-badge risk-badge--high";
  }
  if (riskLevel === RiskLevel.Medium) {
    return "risk-badge risk-badge--medium";
  }
  if (riskLevel === RiskLevel.Unknown) {
    return "risk-badge risk-badge--unknown";
  }
  return "risk-badge risk-badge--dismissed";
};

const getVerificationBadge = (
  verifiedIdStatus: VerifiedIdStatus,
): { className: string; label: string; title?: string } | undefined => {
  if (verifiedIdStatus.loading) {
    return {
      className: "verification-badge verification-badge--loading",
      label: "Checking verification…",
    };
  }
  if (verifiedIdStatus.isVerified) {
    return undefined;
  }
  if (verifiedIdStatus.lastVerifiedAt) {
    return {
      className: "verification-badge verification-badge--expired",
      label: "Identity verification expired",
      title: `Last verified ${formatDateTime(verifiedIdStatus.lastVerifiedAt)}`,
    };
  }
  return {
    className: "verification-badge verification-badge--expired",
    label: "Identity verification required",
  };
};

type UserDisplayProps = {
  profile: UserProfile;
  verifiedIdStatus?: VerifiedIdStatus;
};

export const UserDisplay = ({ profile, verifiedIdStatus }: UserDisplayProps) => {
  const { user, userImage, riskLoading, riskLevel, riskLabel } = profile;
  const displayName = user?.displayName;
  const email = user?.mail ?? user?.userPrincipalName;
  const initials = getInitials(displayName);
  const showVerificationBadge = !!verifiedIdStatus?.required;
  const verificationBadge = showVerificationBadge
    ? getVerificationBadge(verifiedIdStatus)
    : undefined;

  return (
    <div className="user-card">
      <div className="user-card__top">
        <div className="user-card__avatar" aria-hidden="true">
          {userImage ? (
            <img src={userImage} alt="" />
          ) : (
            <span>{initials || "?"}</span>
          )}
        </div>
        <div>
          <h2 className="user-card__name">{displayName ?? " "}</h2>
          {email && <p className="user-card__email">{email}</p>}
        </div>
      </div>
      <div className="user-card__badges">
        <div className={getBadgeClass(riskLevel, riskLoading)}>
          <span className="risk-badge__dot" />
          {riskLoading ? "Checking risk…" : riskLabel}
        </div>
        {verificationBadge && (
          <div
            className={verificationBadge.className}
            title={verificationBadge.title}
            role="status"
          >
            <span className="risk-badge__dot" />
            {verificationBadge.label}
          </div>
        )}
      </div>
    </div>
  );
};
