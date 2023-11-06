import { PasswordReset } from "./FunctionPlaneComponents/PasswordReset";
import { CreateTAP } from "./FunctionPlaneComponents/CreateTAP";
import { DismissUserRisk } from "./FunctionPlaneComponents/DismissUserRisk";
import { useEffect, useState } from "react";
import { getPendingAction, handleActionAuthRedirect, handleRedirectPromise } from "../../services/MsalService";
import {
  ActionResultProps,
  EApiFunctionTypes,
  TFunctionResult,
} from "../../types";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import { UserDisplay } from "./UserDisplay";

const FUNCTION_PLANE_COMPONENTS: {
  element: (props: ActionResultProps<any>) => JSX.Element;
  functionType: EApiFunctionTypes;
}[] = [
  { element: PasswordReset, functionType: EApiFunctionTypes.PASSWORD_RESET },
  { element: CreateTAP, functionType: EApiFunctionTypes.CREATE_TAP },
  {
    element: DismissUserRisk,
    functionType: EApiFunctionTypes.DISMISS_USER_RISK,
  },
];

const FunctionPlane = () => {
  var [actionResult, setActionResult] = useState<TFunctionResult<any>>();

  useEffect(() => {
    handleRedirectPromise().then((authenticationResult) => {
      if(authenticationResult){
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
      <Stack className="function_plane__function_component__wrapper" direction={{ xs: "column", md: "row" }} spacing={2}>
        {FUNCTION_PLANE_COMPONENTS.map((functionComponent) => {
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
        })}
      </Stack>
    </Container>
  );
};

export default FunctionPlane;
