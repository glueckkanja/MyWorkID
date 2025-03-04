import { TFunctionProps } from "../../../types";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  callResetPassword,
  checkResetPasswordClaim,
} from "../../../services/api-service";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
import PasswordResetSvg from "../../../assets/svg/password-reset.svg";
import EyeOpenSvg from "../../../assets/svg/eye-open.svg";
import EyeClosedSvg from "../../../assets/svg/eye-closed.svg";
import { Spinner } from "@/components/ui/spinner";

type PasswordDisplay = {
  visible: boolean;
  value: string;
  showValue: boolean;
  errorValue?: string;
  valueConfirm: string;
  showValueConfirm: boolean;
  errorValueConfirm?: string;
  loading: boolean;
};

enum PasswordInputType {
  MAIN,
  CONFIRM,
}

const PASSWORD_MUST_CONTAIN_ERROR_TEXT =
  "Please use characters from at least 3 of these groups: lowercase, uppercase, digits, special symbols.";
const PASSWORD_MUST_BE_SAME_ERROR_TEXT =
  "Password must be the same in both fields.";

export const PasswordReset = (props: TFunctionProps) => {
  const [passwordDisplay, setPasswordDisplay] = useState<PasswordDisplay>({
    visible: false,
    value: "",
    showValue: false,
    valueConfirm: "",
    showValueConfirm: false,
    loading: false,
  });
  const { toastError, toastException, toastSuccess } = useToast();
  const [enableSubmitButton, setEnableSubmitButton] = useState(true);
  // Note this is the claim challenge check - if successfull comes back the password reset UI should be accasible - if not we have to close the menu again
  useEffect(() => {
    if (props.comingFromRedirect) {
      togglePasswordResetUI();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.comingFromRedirect]);

  const resetPassword = () => {
    const passwordToSet = passwordDisplay.value;
    if (!getIsPasswordValid(passwordToSet)) {
      toastError(PASSWORD_MUST_CONTAIN_ERROR_TEXT);
    } else if (passwordToSet !== passwordDisplay.valueConfirm) {
      toastError(PASSWORD_MUST_BE_SAME_ERROR_TEXT);
    } else {
      // Call the API to reset the password
      callResetPassword(passwordToSet)
        .then(() => {
          setPasswordDisplay({
            visible: false,
            value: "",
            showValue: false,
            valueConfirm: "",
            showValueConfirm: false,
            loading: false,
          });
          toastSuccess(
            "Reset Password",
            "Your password has been sucessfully set."
          );
        })
        .catch((error) => {
          setPasswordDisplay({
            visible: true,
            value: "",
            showValue: false,
            valueConfirm: "",
            showValueConfirm: false,
            loading: false,
            errorValue: "ERROR",
            errorValueConfirm: "ERROR",
          });
          toastException(error);
        });
    }
  };

  const togglePasswordResetUI = () => {
    if (!passwordDisplay.visible) {
      checkResetPasswordClaim();
    }

    setPasswordDisplay({
      visible: !passwordDisplay.visible,
      value: "",
      showValue: false,
      valueConfirm: "",
      showValueConfirm: false,
      loading: false,
    });
  };

  const handleClickShowPassword = (inputType: PasswordInputType) => {
    setPasswordDisplay((oldValues) => {
      let propertyToSet = "showValue";
      let valueToSet = !oldValues.showValue;

      if (inputType == PasswordInputType.CONFIRM) {
        propertyToSet = "showValueConfirm";
        valueToSet = !oldValues.showValueConfirm;
      }

      return { ...oldValues, [propertyToSet]: valueToSet };
    });
  };

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

    let satisfiedRequirements = 0;
    for (const requirement of passwordRequirements) {
      if (requirement.test(password)) {
        satisfiedRequirements++;
      }
    }

    return satisfiedRequirements >= 3;
  };

  const handleInput = (inputType: PasswordInputType, value: string) => {
    setPasswordDisplay((oldValues) => {
      let propertyToSet = "value";
      if (value.length > 0) {
        setEnableSubmitButton(false);
      } else {
        setEnableSubmitButton(true);
      }
      if (inputType == PasswordInputType.CONFIRM) {
        propertyToSet = "valueConfirm";
      }

      const newValues = {
        ...oldValues,
        [propertyToSet]: value,
      };

      let errorValue = undefined;
      let errorValueConfirm = undefined;

      if (newValues.value !== newValues.valueConfirm) {
        errorValueConfirm = PASSWORD_MUST_BE_SAME_ERROR_TEXT;
      }

      if (!getIsPasswordValid(newValues.value)) {
        errorValue = PASSWORD_MUST_CONTAIN_ERROR_TEXT;
      }

      return { ...newValues, errorValueConfirm, errorValue };
    });
  };
  function onSubmit() {
    resetPassword();
  }

  return (
    <div>
      {!passwordDisplay.visible && (
        <Card
          className="action-card"
          onClick={() => {
            togglePasswordResetUI();
          }}
        >
          <CardHeader>
            <CardTitle>
              <img src={PasswordResetSvg} alt="PasswordResetIcon" />
            </CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Reset Password
          </CardFooter>
        </Card>
      )}
      {passwordDisplay.visible &&
        (passwordDisplay.loading ? (
          <Card className="action-card">
            <CardContent>
              <div className="action-card__loading">
                <Spinner />
              </div>
            </CardContent>
          </Card>
        ) : (
          <div className="space-y-2">
            <div className="action-card__pasword-reser__input-container">
              <Input
                type={passwordDisplay.showValue ? "text" : "password"}
                className="action-card__pasword-reset__input"
                placeholder="Enter Password"
                onChange={(e) =>
                  handleInput(PasswordInputType.MAIN, e.target.value)
                }
                value={passwordDisplay.value}
              />
              <button
                tabIndex={0}
                className="action-card__pasword-reset__input-container__icon"
                onClick={() => handleClickShowPassword(PasswordInputType.MAIN)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") {
                    handleClickShowPassword(PasswordInputType.MAIN);
                  }
                }}
              >
                <img
                  src={passwordDisplay.showValue ? EyeClosedSvg : EyeOpenSvg}
                  alt="ShowPasswordIcon"
                />
              </button>
            </div>
            <div className="action-card__pasword-reser__input-container">
              <Input
                className="action-card__pasword-reset__input"
                type={passwordDisplay.showValue ? "text" : "password"}
                placeholder="Confirm Password"
                onChange={(e) =>
                  handleInput(PasswordInputType.CONFIRM, e.target.value)
                }
                value={passwordDisplay.valueConfirm}
              />
              <button
                className="action-card__pasword-reset__input-container__icon"
                tabIndex={0}
                onClick={() => handleClickShowPassword(PasswordInputType.MAIN)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") {
                    handleClickShowPassword(PasswordInputType.MAIN);
                  }
                }}
              >
                <img
                  src={passwordDisplay.showValue ? EyeClosedSvg : EyeOpenSvg}
                  alt="ShowPasswordIcon"
                />
              </button>
            </div>
            <Button
              className="action-card__pasword-reset__submit-button"
              type="submit"
              disabled={enableSubmitButton}
              onClick={() => {
                onSubmit();
              }}
            >
              Submit
            </Button>
          </div>
        ))}
    </div>
  );
};
