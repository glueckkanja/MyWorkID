import { useEffect, useState } from "react";
import { Panel } from "../../panel";
import {
  callResetPassword,
  checkResetPasswordClaim,
} from "../../../services/api-service";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";

// Import Icons
import { EyeIcon } from "@/components/ui/icons/eye-icon";
import { EyeOffIcon } from "@/components/ui/icons/eye-off-icon";
import { InformationCircleIcon } from "@/components/ui/icons/information-circle-icon";

type PasswordResetPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
};

const PASSWORD_MUST_CONTAIN_ERROR_TEXT =
  "Please use characters from at least 3 of these groups: lowercase, uppercase, digits, special symbols.";
const PASSWORD_MUST_BE_SAME_ERROR_TEXT =
  "Password must be the same in both fields.";

const getIsPasswordValid = (password: string) => {
  if (password.length < 8 || password.length > 255) {
    return false;
  }

  const passwordRequirements: RegExp[] = [
    /[A-Z]/,
    /[a-z]/,
    /\d/,
    /[@#%^&*\-_!+=[\]{}|\\:',./`~"();<> ]/,
  ];

  let satisfied = 0;
  for (const requirement of passwordRequirements) {
    if (requirement.test(password)) {
      satisfied++;
    }
  }
  return satisfied >= 3;
};

export const PasswordResetPanel = ({
  open,
  onClose,
  comingFromRedirect,
}: PasswordResetPanelProps) => {
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const { toastError, toastException, toastSuccess } = useToast();

  useEffect(() => {
    if (open && comingFromRedirect) {
      // No-op: we arrived here already authenticated with the right claims.
    }
  }, [open, comingFromRedirect]);

  useEffect(() => {
    if (!open) {
      setPassword("");
      setConfirmPassword("");
      setShowPassword(false);
      setShowConfirmPassword(false);
      setSubmitting(false);
    }
  }, [open]);

  const handleOpen = () => {
    if (!comingFromRedirect) {
      checkResetPasswordClaim().catch(() => {
        // Redirect happens via authenticateRequest — no need to surface here.
      });
    }
  };

  useEffect(() => {
    if (open) {
      handleOpen();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const handleSubmit = () => {
    if (!getIsPasswordValid(password)) {
      toastError(PASSWORD_MUST_CONTAIN_ERROR_TEXT);
      return;
    }
    if (password !== confirmPassword) {
      toastError(PASSWORD_MUST_BE_SAME_ERROR_TEXT);
      return;
    }

    setSubmitting(true);
    callResetPassword(password)
      .then(() => {
        toastSuccess(
          "Password Changed",
          "Your new password is active. Use it the next time you sign in.",
        );
        onClose();
      })
      .catch((error) => {
        toastException(error);
      })
      .finally(() => {
        setSubmitting(false);
      });
  };

  const canSubmit =
    password.length > 0 && confirmPassword.length > 0 && !submitting;

  return (
    <Panel
      open={open}
      title="Reset Password"
      subtitle="Choose a strong password you haven't used before."
      onClose={onClose}
    >
      {submitting ? (
        <div className="panel-loading">
          <Spinner />
        </div>
      ) : (
        <>
          <div className="form-group">
            <label className="form-label" htmlFor="password-reset-new">
              New Password
            </label>
            <div className="form-input-wrapper">
              <input
                id="password-reset-new"
                type={showPassword ? "text" : "password"}
                className="form-input"
                placeholder="Enter new password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="new-password"
              />
              <button
                type="button"
                className="form-toggle-visibility"
                onClick={() => setShowPassword((value) => !value)}
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? <EyeOffIcon /> : <EyeIcon />}
              </button>
            </div>
          </div>
          <div className="form-group">
            <label className="form-label" htmlFor="password-reset-confirm">
              Confirm Password
            </label>
            <div className="form-input-wrapper">
              <input
                id="password-reset-confirm"
                type={showConfirmPassword ? "text" : "password"}
                className="form-input"
                placeholder="Repeat new password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                autoComplete="new-password"
              />
              <button
                type="button"
                className="form-toggle-visibility"
                onClick={() => setShowConfirmPassword((value) => !value)}
                aria-label={
                  showConfirmPassword ? "Hide password" : "Show password"
                }
              >
                {showConfirmPassword ? <EyeOffIcon /> : <EyeIcon />}
              </button>
            </div>
          </div>
          <div className="form-hint">
            <InformationCircleIcon />
            <span>
              At least 10 characters (24 for admin accounts) including uppercase
              and lowercase letters, a number, and a symbol.
            </span>
          </div>
          <button
            type="button"
            className="panel-primary-button"
            disabled={!canSubmit}
            onClick={handleSubmit}
          >
            Change Password
          </button>
        </>
      )}
    </Panel>
  );
};
