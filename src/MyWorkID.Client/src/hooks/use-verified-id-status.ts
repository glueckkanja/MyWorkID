import { useCallback, useEffect, useMemo, useState } from "react";
import { getVerifiedIdStatus } from "../services/api-service";
import { TVerifiedIdStatusResponse, VerifiedIdStatus } from "../types";
import {
  isVerificationValidAt,
  parseTimestamp,
} from "@/lib/verified-id-status";

const VERIFIED_ID_STATUS_POLL_INTERVAL_MILLISECONDS = 60000;
// setTimeout cannot schedule further ahead than a signed 32-bit integer of milliseconds.
const MAXIMUM_TIMEOUT_MILLISECONDS = 2147483647;

/**
 * Loads whether sensitive actions currently require a recent Verified ID verification and, if so,
 * the verification state of the signed-in user. The state is stored server-side (custom security
 * attribute on the user), so it survives page reloads and is shared across sessions and devices.
 *
 * The hook re-evaluates the expiry on the client as time passes, so the UI flips to
 * "expired" at the right moment even without a server round trip, and it polls the server
 * periodically to pick up verifications completed elsewhere.
 */
export const useVerifiedIdStatus = (): VerifiedIdStatus => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [response, setResponse] = useState<TVerifiedIdStatusResponse>();
  const [nowInMilliseconds, setNowInMilliseconds] = useState(() => Date.now());

  const refresh = useCallback(() => {
    getVerifiedIdStatus()
      .then((result) => {
        setResponse(result);
        setError(false);
        setLoading(false);
        setNowInMilliseconds(Date.now());
      })
      .catch((requestError) => {
        console.error("Could not get Verified ID status", requestError);
        setError(true);
        setLoading(false);
      });
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const required = response?.required ?? false;

  // Poll only while the requirement is active - otherwise the status never changes.
  useEffect(() => {
    if (!required) {
      return;
    }
    const intervalId = setInterval(
      refresh,
      VERIFIED_ID_STATUS_POLL_INTERVAL_MILLISECONDS,
    );
    return () => clearInterval(intervalId);
  }, [refresh, required]);

  const lastVerifiedAt = useMemo(
    () => parseTimestamp(response?.lastVerifiedAt),
    [response?.lastVerifiedAt],
  );
  const validUntil = useMemo(
    () => parseTimestamp(response?.validUntil),
    [response?.validUntil],
  );

  const isVerified = isVerificationValidAt(
    response?.isVerified ?? false,
    validUntil,
    nowInMilliseconds,
  );

  // Wake up exactly when the verification expires so the UI updates without user interaction.
  useEffect(() => {
    if (!required || !isVerified || !validUntil) {
      return;
    }
    const remainingMilliseconds = validUntil.getTime() - nowInMilliseconds;
    const timeoutId = setTimeout(
      () => setNowInMilliseconds(Date.now()),
      Math.max(0, Math.min(remainingMilliseconds, MAXIMUM_TIMEOUT_MILLISECONDS)),
    );
    return () => clearTimeout(timeoutId);
  }, [required, isVerified, validUntil, nowInMilliseconds]);

  return useMemo(
    () => ({
      loading,
      error,
      required,
      isVerified,
      lastVerifiedAt,
      validUntil,
      validityInMinutes: response?.validityInMinutes ?? 0,
      refresh,
    }),
    [
      loading,
      error,
      required,
      isVerified,
      lastVerifiedAt,
      validUntil,
      response?.validityInMinutes,
      refresh,
    ],
  );
};
