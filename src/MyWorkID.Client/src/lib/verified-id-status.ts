const MINUTES_PER_HOUR = 60;
const MINUTES_PER_DAY = 24 * MINUTES_PER_HOUR;

/**
 * Parses an ISO 8601 timestamp coming from the API into a Date. Returns undefined for
 * missing or unparsable values so callers can treat "unknown" explicitly.
 */
export const parseTimestamp = (
  value: string | null | undefined,
): Date | undefined => {
  if (!value) {
    return undefined;
  }
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return undefined;
  }
  return parsed;
};

/**
 * Formats a validity window given in minutes as a human readable duration,
 * e.g. "90 minutes", "24 hours" or "7 days".
 */
export const formatValidityDuration = (validityInMinutes: number): string => {
  if (validityInMinutes >= MINUTES_PER_DAY && validityInMinutes % MINUTES_PER_DAY === 0) {
    const days = validityInMinutes / MINUTES_PER_DAY;
    return days === 1 ? "1 day" : `${days} days`;
  }
  if (validityInMinutes >= MINUTES_PER_HOUR && validityInMinutes % MINUTES_PER_HOUR === 0) {
    const hours = validityInMinutes / MINUTES_PER_HOUR;
    return hours === 1 ? "1 hour" : `${hours} hours`;
  }
  return validityInMinutes === 1 ? "1 minute" : `${validityInMinutes} minutes`;
};

/**
 * Formats a point in time for display in the UI using the browser locale.
 */
export const formatDateTime = (date: Date | undefined): string => {
  if (!date) {
    return "";
  }
  return date.toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
};

/**
 * Returns true if the verification reported by the server is still valid at the given point in time.
 * The server already evaluated the window, but the page may stay open past the expiry, so the
 * client re-checks against `validUntil`.
 */
export const isVerificationValidAt = (
  isVerifiedAccordingToServer: boolean,
  validUntil: Date | undefined,
  nowInMilliseconds: number,
): boolean => {
  if (!isVerifiedAccordingToServer) {
    return false;
  }
  if (!validUntil) {
    return true;
  }
  return validUntil.getTime() > nowInMilliseconds;
};
