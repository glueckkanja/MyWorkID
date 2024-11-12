import { PasswordReset } from "./function-plane-components/password-reset";
import { CreateTAP } from "./function-plane-components/create-tap";
import { DismissUserRisk } from "./function-plane-components/dismiss-userisk";
import { useEffect, useState } from "react";
import {
  getPendingAction,
  handleRedirectPromise,
} from "../../services/msal-service";
import { EApiFunctionTypes, TFunctionProps } from "../../types";
import { UserDisplay } from "./user-display";
import { useSignedInUser } from "../../contexts/signed-in-user-provider";
import { Role } from "../../services/roles-service";
import { ValidateIdentity } from "./function-plane-components/validate-identity";

const FUNCTION_PLANE_COMPONENTS: {
  element: (props: TFunctionProps) => JSX.Element;
  functionType: EApiFunctionTypes;
  permissionRoleRequired?: string;
}[] = [
  {
    element: PasswordReset,
    functionType: EApiFunctionTypes.PASSWORD_RESET,
    permissionRoleRequired: Role.ALLOW_CHANGE_PASSWORD,
  },
  {
    element: CreateTAP,
    functionType: EApiFunctionTypes.CREATE_TAP,
    permissionRoleRequired: Role.ALLOW_CREATE_TAP,
  },
  {
    element: DismissUserRisk,
    functionType: EApiFunctionTypes.DISMISS_USER_RISK,
    permissionRoleRequired: Role.ALLOW_DISMISS_USER_RISK,
  },
  {
    element: ValidateIdentity,
    functionType: EApiFunctionTypes.VALIDATE_IDENTITY,
    permissionRoleRequired: Role.ALLOW_VALIDATE_IDENTITY,
  },
];

const FunctionPlane = () => {
  const [actionResult, setActionResult] = useState<EApiFunctionTypes>();

  const signedInUserInfo = useSignedInUser();

  useEffect(() => {
    handleRedirectPromise().then((authenticationResult) => {
      if (authenticationResult) {
        setActionResult(getPendingAction(authenticationResult));
      }
    });
  }, []);

  return (
    <div>
      <UserDisplay />
      <div className="function-plane__container">
        {FUNCTION_PLANE_COMPONENTS.map((functionComponent) => {
          if (
            !functionComponent.permissionRoleRequired ||
            (signedInUserInfo &&
              signedInUserInfo.roles.includes(
                functionComponent.permissionRoleRequired
              ))
          ) {
            return (
              <functionComponent.element
                key={functionComponent.functionType}
                comingFromRedirect={
                  actionResult == functionComponent.functionType
                }
              />
            );
          } else {
            return undefined;
          }
        })}
      </div>
    </div>
  );
};

export default FunctionPlane;
