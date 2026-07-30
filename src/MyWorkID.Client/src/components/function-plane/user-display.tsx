import { RiskLevel, UserProfile } from "../../types";

const getInitials = (displayName: string | undefined): string => {
  if (!displayName) {
    return "";
  }
  const parts = displayName.trim().split(/\s+/);
  if (parts.length === 1) {
    return parts[0].substring(0, 2).toUpperCase();
  }
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
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

type UserDisplayProps = {
  profile: UserProfile;
};

export const UserDisplay = ({ profile }: UserDisplayProps) => {
  const { user, userImage, riskLoading, riskLevel, riskLabel } = profile;
  const displayName = user?.displayName;
  const email = user?.mail ?? user?.userPrincipalName;
  const initials = getInitials(displayName);

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
          <h2 className="user-card__name">{displayName ?? " "}</h2>
          {email && <p className="user-card__email">{email}</p>}
        </div>
      </div>
      <div className={getBadgeClass(riskLevel, riskLoading)}>
        <span className="risk-badge__dot" />
        {riskLoading ? "Checking risk…" : riskLabel}
      </div>
    </div>
  );
};
