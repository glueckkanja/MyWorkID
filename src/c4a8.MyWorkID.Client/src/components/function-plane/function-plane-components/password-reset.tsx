import { TFunctionProps } from "../../../types";
import { useEffect, useState } from "react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { callResetPassword } from "../../../services/api-service";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
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
const svgIcon = (
  <svg
    width="40"
    height="43"
    viewBox="0 0 40 43"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
  >
    <g id="Group 24">
      <path
        id="Vector"
        d="M2.62903 1.62024V13.1833H14.3865"
        stroke="white"
        strokeWidth="2"
        strokeLinecap="square"
      />
      <path
        id="Vector_2"
        d="M3.64105 11.6177C7.10735 7.72748 11.9237 5.37118 17.0445 5.06028C22.1654 4.74938 27.1773 6.50899 30.9922 9.9571C33.2483 12.0078 34.9891 14.5801 36.0719 17.4629C37.1548 20.3458 37.5487 23.4569 37.2214 26.541C36.8941 29.6252 35.8548 32.5944 34.189 35.2049C32.5231 37.8155 30.2782 39.9929 27.6383 41.5586"
        stroke="white"
        strokeWidth="2"
        strokeLinecap="square"
      />
    </g>
  </svg>
);
const eyeIcon = (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="20"
    height="12"
    viewBox="0 0 20 12"
    fill="none"
  >
    <path
      d="M7.46298 6.00005C7.46298 6.71409 7.74664 7.39889 8.25154 7.9038C8.75645 8.4087 9.44125 8.69236 10.1553 8.69236C10.8693 8.69236 11.5541 8.4087 12.059 7.9038C12.5639 7.39889 12.8476 6.71409 12.8476 6.00005C12.8476 5.286 12.5639 4.6012 12.059 4.0963C11.5541 3.59139 10.8693 3.30774 10.1553 3.30774C9.44125 3.30774 8.75645 3.59139 8.25154 4.0963C7.74664 4.6012 7.46298 5.286 7.46298 6.00005Z"
      stroke="#7F7F7F"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M18.9091 5.74615C18.0091 4.73077 14.4322 1 10.1553 1C5.87837 1 2.30145 4.73077 1.40145 5.74615C1.34269 5.8177 1.31058 5.90742 1.31058 6C1.31058 6.09258 1.34269 6.1823 1.40145 6.25385C2.30145 7.26923 5.87837 11 10.1553 11C14.4322 11 18.0091 7.26923 18.9091 6.25385C18.9679 6.1823 19 6.09258 19 6C19 5.90742 18.9679 5.8177 18.9091 5.74615Z"
      stroke="#7F7F7F"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);
const closeEyeIcon = (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="20"
    height="12"
    viewBox="0 0 20 12"
    fill="none"
  >
    <path
      d="M7.46298 6.00005C7.46298 6.71409 7.74664 7.39889 8.25154 7.9038C8.75645 8.4087 9.44125 8.69236 10.1553 8.69236C10.8693 8.69236 11.5541 8.4087 12.059 7.9038C12.5639 7.39889 12.8476 6.71409 12.8476 6.00005C12.8476 5.286 12.5639 4.6012 12.059 4.0963C11.5541 3.59139 10.8693 3.30774 10.1553 3.30774C9.44125 3.30774 8.75645 3.59139 8.25154 4.0963C7.74664 4.6012 7.46298 5.286 7.46298 6.00005Z"
      stroke="#7F7F7F"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M18.9091 5.74615C18.0091 4.73077 14.4322 1 10.1553 1C5.87837 1 2.30145 4.73077 1.40145 5.74615C1.34269 5.8177 1.31058 5.90742 1.31058 6C1.31058 6.09258 1.34269 6.1823 1.40145 6.25385C2.30145 7.26923 5.87837 11 10.1553 11C14.4322 11 18.0091 7.26923 18.9091 6.25385C18.9679 6.1823 19 6.09258 19 6C19 5.90742 18.9679 5.8177 18.9091 5.74615Z"
      stroke="#7F7F7F"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <line
      x1="14.0303"
      y1="2.03033"
      x2="6.03033"
      y2="10.0303"
      stroke="#7F7F7F"
      strokeWidth="1.5"
    />
  </svg>
);
const formSchema = z.object({
  password: z.string().min(2).max(50),
  confirmPassword: z.string().min(2).max(50),
});
export const PasswordReset = (props: TFunctionProps) => {
  const [passwordDisplay, setPasswordDisplay] = useState<PasswordDisplay>({
    visible: false,
    value: "",
    showValue: false,
    valueConfirm: "",
    showValueConfirm: false,
    loading: false,
  });
  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      password: "",
      confirmPassword: "",
    },
  });
  const { toast } = useToast();
  const [enableSubmitButton, setEnableSubmitButton] = useState(true);
  // Note this is the claim challenge check - if successfull comes back the password reset UI should be accasible - if not we have to close the menu again
  useEffect(() => {
    if (props.comingFromRedirect) {
      resetPassword();
      togglePasswordResetUI();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const resetPassword = () => {
    const passwordToSet = passwordDisplay.value;
    if (!getIsPasswordValid(passwordToSet)) {
      toast({
        variant: "destructive",
        title: "Something went wrong during password reset",
        description: PASSWORD_MUST_CONTAIN_ERROR_TEXT,
      });
    } else if (passwordToSet !== passwordDisplay.valueConfirm) {
      toast({
        variant: "destructive",
        title: "Something went wrong during password reset",
        description: PASSWORD_MUST_BE_SAME_ERROR_TEXT,
      });
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
          toast({
            variant: "destructive",
            title: "Something went wrong during Password reset generation.",
            description: error.response.statusText,
          });
        });
    }
  };

  const togglePasswordResetUI = () => {
    if (!passwordDisplay.visible) {
      setPasswordDisplay({
        visible: true,
        value: "",
        showValue: false,
        valueConfirm: "",
        showValueConfirm: false,
        loading: false,
      });
    } else {
      setPasswordDisplay({
        visible: false,
        value: "",
        showValue: false,
        valueConfirm: "",
        showValueConfirm: false,
        loading: false,
      });
    }
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
            <CardTitle>{svgIcon}</CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Reset Password
          </CardFooter>
        </Card>
      )}
      {passwordDisplay.visible && (
        <Form {...form}>
          <form className="space-y-2">
            <FormField
              control={form.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <div className="action-card__pasword-reser__input-container">
                      <Input
                        type={passwordDisplay.showValue ? "text" : "password"}
                        className="action-card__pasword-reset__input"
                        placeholder="Enter Password"
                        {...field}
                        onChange={(e) =>
                          handleInput(PasswordInputType.MAIN, e.target.value)
                        }
                        value={passwordDisplay.value}
                      />
                      <button
                        tabIndex={0}
                        className="action-card__pasword-reset__input-container__icon"
                        onClick={() =>
                          handleClickShowPassword(PasswordInputType.MAIN)
                        }
                        onKeyDown={(e) => {
                          if (e.key === "Enter" || e.key === " ") {
                            handleClickShowPassword(PasswordInputType.MAIN);
                          }
                        }}
                      >
                        {passwordDisplay.showValue ? closeEyeIcon : eyeIcon}
                      </button>
                    </div>
                  </FormControl>
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="confirmPassword"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <div className="action-card__pasword-reser__input-container">
                      <Input
                        className="action-card__pasword-reset__input"
                        type={passwordDisplay.showValue ? "text" : "password"}
                        placeholder="Confirm Password"
                        {...field}
                        onChange={(e) =>
                          handleInput(PasswordInputType.CONFIRM, e.target.value)
                        }
                        value={passwordDisplay.valueConfirm}
                      />
                      <button
                        className="action-card__pasword-reset__input-container__icon"
                        tabIndex={0}
                        onClick={() =>
                          handleClickShowPassword(PasswordInputType.MAIN)
                        }
                        onKeyDown={(e) => {
                          if (e.key === "Enter" || e.key === " ") {
                            handleClickShowPassword(PasswordInputType.MAIN);
                          }
                        }}
                      >
                        {passwordDisplay.showValue ? closeEyeIcon : eyeIcon}
                      </button>
                    </div>
                  </FormControl>
                </FormItem>
              )}
            />
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
          </form>
        </Form>
      )}
    </div>
  );
};
