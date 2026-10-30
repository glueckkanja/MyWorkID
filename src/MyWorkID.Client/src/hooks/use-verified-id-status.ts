import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { getVerifiedIdStatus } from "../services/api-service";
import { TVerifiedIdStatusResponse, VerifiedIdStatus } from "../types";
import {
  isVerificationValidAt,
  parseTimestamp,
} from "@/lib/verified-id-status";

const VERIFIED_ID_STATUS_POLL_INTERVAL_MILLISECONDS = 60000;
// Entra ID can take a few seconds until a freshly written security attribute is readable.
const POST_VERIFICATION_RETRY_INTERVAL_MILLISECONDS = 2000;
const POST_VERIFICATION_MAXIMUM_ATTEMPTS = 10;
// setTimeout cannot schedule further ahead than a signed 32-bit integer of milliseconds.
const MAXIMUM_TIMEOUT_MILLISECONDS = 2147483647;

/**
 * Loads whether sensitive actions currently require a recent Verified ID verification and, if so,
 * the verification state of the signed-in user. The state is stored server-side (custom security
 * attribute on the user), so it survives page reloads and is shared across sessions and devices.
 *
 * The hook re-evaluates the expiry on the client as time passes, using the server's clock (the
 * offset to the device clock is taken from `serverTime`), so the UI flips to "expired" at the
 * right moment even without a server round trip or with a skewed device clock, and it polls the server
 * periodically to pick up verifications completed elsewhere.
 */
export const useVerifiedIdStatus = (): VerifiedIdStatus => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [response, setResponse] = useState<TVerifiedIdStatusResponse>();
  // Current time in milliseconds on the server's clock, so a skewed device clock cannot make a
  // valid verification look expired (or an expired one look valid).
  const [nowInMilliseconds, setNowInMilliseconds] = useState(() => Date.now());

  // Difference between the server clock and the device clock, measured when a response arrives.
  const clockOffsetInMillisecondsRef = useRef(0);

  const postVerificationTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  const latestRequestNumberRef = useRef(0);

  // Timestamp (in milliseconds) of the last verification seen by the most recently applied response.
  const lastVerifiedAtInMillisecondsRef = useRef<number | undefined>(undefined);

  // Never rejects: failures are reported through the error state and resolve to undefined.
  // Only the most recently started request may update state, so an older response (or error)
  // that finishes late cannot overwrite a newer result. Superseded requests resolve to undefined.
  const loadStatus = useCallback((): Promise<TVerifiedIdStatusResponse | undefined> => {
    latestRequestNumberRef.current += 1;
    const requestNumber = latestRequestNumberRef.current;
    const isSuperseded = () => requestNumber !== latestRequestNumberRef.current;
    let appliedResult: TVerifiedIdStatusResponse | undefined;

    return getVerifiedIdStatus()
      .then((result) => {
        if (isSuperseded()) {
          return;
        }
        const serverTimeInMilliseconds = parseTimestamp(result.serverTime)?.getTime();
        clockOffsetInMillisecondsRef.current =
          serverTimeInMilliseconds === undefined ? 0 : serverTimeInMilliseconds - Date.now();
        setResponse(result);
        setError(false);
        setLoading(false);
        setNowInMilliseconds(Date.now() + clockOffsetInMillisecondsRef.current);
        lastVerifiedAtInMillisecondsRef.current = parseTimestamp(result.lastVerifiedAt)?.getTime();
        appliedResult = result;
      })
      .catch((requestError) => {
        if (isSuperseded()) {
          return;
        }
        console.error("Could not get Verified ID status", requestError);
        setError(true);
        setLoading(false);
      })
      .then(() => appliedResult);
  }, []);

  const refresh = useCallback(() => {
    void loadStatus();
  }, [loadStatus]);

  // Tries to update the verification status after a successful verification attempt. A user may
  // re-verify while a previous verification is still valid, so retries continue until a
  // verification newer than the one known at the start becomes visible.
  const refreshAfterVerification = useCallback(() => {
    clearTimeout(postVerificationTimeoutRef.current);
    const previousLastVerifiedAtInMilliseconds = lastVerifiedAtInMillisecondsRef.current;
    const attempt = async (attemptNumber: number) => {
      const result = await loadStatus();
      const lastVerifiedAtInMilliseconds = parseTimestamp(result?.lastVerifiedAt)?.getTime();
      const isVerified = result?.isVerified === true;
      const hasLastVerifiedAt = lastVerifiedAtInMilliseconds !== undefined;
      const hadNoPreviousVerification = previousLastVerifiedAtInMilliseconds === undefined;
      const isNewerThanPreviousVerification =
        hasLastVerifiedAt &&
        (hadNoPreviousVerification ||
          lastVerifiedAtInMilliseconds > previousLastVerifiedAtInMilliseconds);
      const isNewVerificationVisible = isVerified && isNewerThanPreviousVerification;
      if (isNewVerificationVisible || attemptNumber >= POST_VERIFICATION_MAXIMUM_ATTEMPTS) {
        return;
      }
      postVerificationTimeoutRef.current = setTimeout(
        () => void attempt(attemptNumber + 1),
        POST_VERIFICATION_RETRY_INTERVAL_MILLISECONDS,
      );
    };
    void attempt(1);
  }, [loadStatus]);

  useEffect(() => () => clearTimeout(postVerificationTimeoutRef.current), []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const required = response?.required ?? false;

  // Poll while the requirement is active, and keep retrying while the status is unknown after a
  // failed request. Otherwise the status never changes.
  useEffect(() => {
    if (!required && !error) {
      return;
    }
    const intervalId = setInterval(
      refresh,
      VERIFIED_ID_STATUS_POLL_INTERVAL_MILLISECONDS,
    );
    return () => clearInterval(intervalId);
  }, [refresh, required, error]);

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
      () => setNowInMilliseconds(Date.now() + clockOffsetInMillisecondsRef.current),
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
      refreshAfterVerification,
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
      refreshAfterVerification,
    ],
  );
};
