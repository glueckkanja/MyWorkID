import { PasswordReset } from "./FunctionPlaneComponents/PasswordReset";
import { CreateTAP } from "./FunctionPlaneComponents/CreateTAP";
import { DismissUserRisk } from "./FunctionPlaneComponents/DismissUserRisk";
import { useEffect, useState } from "react";
import {
  getPendingAction,
  handleActionAuthRedirect,
  handleRedirectPromise,
} from "../../services/MsalService";
import {
  ActionResultProps,
  EApiFunctionTypes,
  TFunctionResult,
} from "../../types";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import { UserDisplay } from "./UserDisplay";
import { useSignedInUser } from "../../contexts/SignedInUserProvider";
import { Role } from "../../services/RolesService";

const FUNCTION_PLANE_COMPONENTS: {
  element: (props: ActionResultProps<any>) => JSX.Element;
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
];

const FunctionPlane = () => {
  var [actionResult, setActionResult] = useState<TFunctionResult<any>>();

  var signedInUserInfo = useSignedInUser();

  useEffect(() => {
    handleRedirectPromise().then((authenticationResult) => {
      if (authenticationResult) {
        setActionResult(getPendingAction(authenticationResult));
        handleActionAuthRedirect(authenticationResult).then((result) => {
          setActionResult(result);
        });
      }
    });
  }, []);

  return (
    <Container maxWidth="xl">
      <UserDisplay />
      <Stack
        className="function_plane__function_component__wrapper"
        direction={{ xs: "column", md: "row" }}
        spacing={2}
      >
        {FUNCTION_PLANE_COMPONENTS.map((functionComponent) => {
          if (!functionComponent.permissionRoleRequired || (signedInUserInfo && signedInUserInfo.roles.includes(functionComponent.permissionRoleRequired))) {
            return (
              <functionComponent.element
                key={functionComponent.functionType}
                result={
                  actionResult &&
                  actionResult.dataType === functionComponent.functionType
                    ? actionResult
                    : undefined
                }
              />
            );
          } else {
            return undefined;
          }
        })}
      </Stack>
    </Container>
  );
};

export default FunctionPlane;
