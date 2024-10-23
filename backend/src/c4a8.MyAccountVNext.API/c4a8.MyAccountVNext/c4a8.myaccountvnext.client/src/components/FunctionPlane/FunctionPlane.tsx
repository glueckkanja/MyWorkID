import { PasswordReset } from "./FunctionPlaneComponents/PasswordReset";
import { CreateTAP } from "./FunctionPlaneComponents/CreateTAP";
import { DismissUserRisk } from "./FunctionPlaneComponents/DismissUserRisk";
import { useEffect, useState } from "react";
import {
  getPendingAction,
  handleRedirectPromise,
} from "../../services/MsalService";
import {
  EApiFunctionTypes,
  TFunctionProps,
} from "../../types";
import Container from "@mui/material/Container";
import { UserDisplay } from "./UserDisplay";
import { useSignedInUser } from "../../contexts/SignedInUserProvider";
import { Role } from "../../services/RolesService";
import { ValidateIdentity } from "./FunctionPlaneComponents/ValidateIdentity";

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
    permissionRoleRequired: Role.ALLOW_VALIDATE_IDENTITY, // TODO Change to own role
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
    <Container maxWidth="xl" style={{backgroundColor : "#F5F5F5"}}>
      <UserDisplay />
      {/* <Stack
        className="function_plane__function_component__wrapper"
        direction={{ xs: "column", md: "row" }}
        spacing={2}
      > */}
      <div style={{display:"flex",}}>
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
                comingFromRedirect = {actionResult == functionComponent.functionType}
              />
            );
          } else {
            return undefined;
          }
        })}
      {/* </Stack> */}
      </div>
    </Container>
  );
};

export default FunctionPlane;
