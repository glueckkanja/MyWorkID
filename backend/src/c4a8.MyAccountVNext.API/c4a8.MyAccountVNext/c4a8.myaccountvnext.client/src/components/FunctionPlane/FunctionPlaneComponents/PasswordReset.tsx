import { TFunctionProps } from "../../../types";
import { useEffect, useState } from "react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { callResetPassword } from "../../../services/ApiService";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
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
        stroke-width="2"
        stroke-linecap="square"
      />
      <path
        id="Vector_2"
        d="M3.64105 11.6177C7.10735 7.72748 11.9237 5.37118 17.0445 5.06028C22.1654 4.74938 27.1773 6.50899 30.9922 9.9571C33.2483 12.0078 34.9891 14.5801 36.0719 17.4629C37.1548 20.3458 37.5487 23.4569 37.2214 26.541C36.8941 29.6252 35.8548 32.5944 34.189 35.2049C32.5231 37.8155 30.2782 39.9929 27.6383 41.5586"
        stroke="white"
        stroke-width="2"
        stroke-linecap="square"
      />
    </g>
  </svg>
);

const formSchema = z.object({
  password: z.string().min(2).max(50),
  confirmPassword: z.string().min(2).max(50),
});
// eslint-disable-next-line @typescript-eslint/no-unused-vars -- This is a placeholder component
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
  // Note this is the claim challenge check - if successfull comes back the password reset UI should be accasible - if not we have to close the menu again
  useEffect(() => {
    if (props.comingFromRedirect) {
      resetPassword();
    }
  }, []);

  const resetPassword = () => {
    const passwordToSet = passwordDisplay.value;
    if (!getIsPasswordValid(passwordToSet)) {
      return;
    }
    setPasswordDisplay({
      visible: false,
      value: "",
      showValue: false,
      valueConfirm: "",
      showValueConfirm: false,
      loading: true,
    });

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
        console.error(
          "Something went wrong during Password reset generation.",
          error
        );
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
      });
  };

  const togglePasswordResetUI = () => {
    // if (!passwordDisplay.visible) {
    //   checkResetPasswordClaim();
    // }
    // setPasswordDisplay({
    //   visible: !passwordDisplay.visible,
    //   value: "",
    //   showValue: false,
    //   valueConfirm: "",
    //   showValueConfirm: false,
    //   loading: false,
    // });
    setPasswordDisplay({
      visible: true,
      value: "",
      showValue: false,
      valueConfirm: "",
      showValueConfirm: false,
      loading: false,
    });
  };

  // const handleClickShowPassword = (inputType: PasswordInputType) => {
  //   setPasswordDisplay((oldValues) => {
  //     let propertyToSet = "showValue";
  //     let valueToSet = !oldValues.showValue;

  //     if (inputType == PasswordInputType.CONFIRM) {
  //       propertyToSet = "showValueConfirm";
  //       valueToSet = !oldValues.showValueConfirm;
  //     }

  //     return { ...oldValues, [propertyToSet]: valueToSet };
  //   });
  // };

  const getIsPasswordValid = (password: string) => {
    if (password.length < 8 || password.length > 255) {
      return false;
    }

    const passwordRequirements: RegExp[] = [
      new RegExp("[A-Z]"),
      new RegExp("[a-z]"),
      new RegExp("[0-9]"),
      new RegExp(
        "[@#%\\^&\\*\\-_\\!\\+=\\[\\]{}\\|\\\\:',\\.\\?\\/`~\"\\(\\);<> ]"
      ),
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
  function onSubmit(values: z.infer<typeof formSchema>) {
    // Do something with the form values.
    // ✅ This will be type-safe and validated.
   handleInput( PasswordInputType.CONFIRM, values.password);
  }
  // const handleMouseDownPassword = (
  //   event: React.MouseEvent<HTMLButtonElement>
  // ) => {
  //   event.preventDefault();
  // };

  return (
    <div>
      {/* <Button
        className="function_plane__function_component__action"
        variant="contained"
        onClick={togglePasswordResetUI}
      >
        Reset Password
      </Button> */}
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
        <>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-2">
              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormControl>
                      <Input
                        type="password"
                        className="action-card__pasword-reset__input"
                        placeholder="Enter Password"
                        {...field}
                      />
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
                      <Input
                        className="action-card__pasword-reset__input"
                        type="password"
                        placeholder="Confirm Password"
                        {...field}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />
              <Button
                className="action-card__pasword-reset__submit-button"
                type="submit"
              >
                Submit
              </Button>
            </form>
          </Form>
        </>
      )}
      {/* {passwordDisplay.visible && (
        <div>
          <FormControl
            sx={{ mt: 2, width: "100%" }}
            variant="outlined"
            error={passwordDisplay.errorValue ? true : false}
          >
            <InputLabel htmlFor="outlined-adornment-password">
              Password
            </InputLabel>
            <OutlinedInput
              id="outlined-adornment-password"
              type={passwordDisplay.showValue ? "text" : "password"}
              onChange={(e) =>
                handleInput(PasswordInputType.MAIN, e.target.value)
              }
              aria-describedby="component-error-text"
              value={passwordDisplay.value}
              endAdornment={
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle password visibility"
                    onClick={() =>
                      handleClickShowPassword(PasswordInputType.MAIN)
                    }
                    onMouseDown={handleMouseDownPassword}
                    edge="end"
                  >
                    {passwordDisplay.showValue ? (
                      <VisibilityOff />
                    ) : (
                      <Visibility />
                    )}
                  </IconButton>
                </InputAdornment>
              }
              label="Password"
            />
            <FormHelperText id="component-error-text">
              {passwordDisplay.errorValue}
            </FormHelperText>
          </FormControl>
          <FormControl
            sx={{ mt: 1, width: "100%" }}
            variant="outlined"
            error={passwordDisplay.errorValueConfirm ? true : false}
          >
            <InputLabel htmlFor="outlined-adornment-password">
              Password
            </InputLabel>
            <OutlinedInput
              id="outlined-adornment-password"
              type={passwordDisplay.showValueConfirm ? "text" : "password"}
              onChange={(e) =>
                handleInput(PasswordInputType.CONFIRM, e.target.value)
              }
              aria-describedby="component-error-text"
              value={passwordDisplay.valueConfirm}
              endAdornment={
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle password visibility"
                    onClick={() =>
                      handleClickShowPassword(PasswordInputType.CONFIRM)
                    }
                    onMouseDown={handleMouseDownPassword}
                    edge="end"
                  >
                    {passwordDisplay.showValueConfirm ? (
                      <VisibilityOff />
                    ) : (
                      <Visibility />
                    )}
                  </IconButton>
                </InputAdornment>
              }
              label="Confirm Password"
            />
            <FormHelperText id="component-error-text">
              {passwordDisplay.errorValueConfirm}
            </FormHelperText>
          </FormControl>
          <Box textAlign="center">
            <Button
              sx={{ mt: 1 }}
              variant="contained"
              onClick={resetPassword}
              disabled={
                passwordDisplay.loading ||
                passwordDisplay.errorValue ||
                passwordDisplay.errorValueConfirm ||
                passwordDisplay.value === "" ||
                passwordDisplay.valueConfirm === ""
                  ? true
                  : false
              }
            >
              Apply
            </Button>
          </Box>
        </div>
      )}
      <div
        className={
          passwordDisplay.loading
            ? "function_plane__function_component__loading_spinner__container"
            : "hidden_element"
        }
      >
        <CircularProgress />
      </div> */}
      {/* {passwordDisplay.visible && ()}
      <div
        className={
          passwordDisplay.loading
            ? "function_plane__function_component__loading_spinner__container"
            : "hidden_element"
        }
      >
        <CircularProgress />
      </div> */}
    </div>
  );
};
