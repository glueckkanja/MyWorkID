import { PasswordReset } from "./FunctionPlaneComponents/PasswordReset";
import { CreateTAP } from "./FunctionPlaneComponents/CreateTAP";
import { DismissUserRisk } from "./FunctionPlaneComponents/DismissUserRisk";
import Grid from "@mui/material/Unstable_Grid2";
import { useEffect, useState } from "react";
import { handleActionAuthRedirect } from "../../services/MsalService";
import {
  ActionResultProps,
  EApiFunctionTypes,
  TFunctionResult,
} from "../../types";
import Container from "@mui/material/Container";
import Box from "@mui/material/Box";
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
    handleActionAuthRedirect().then((result) => {
      setActionResult(result);
    });
  }, []);

  return (
    <>
      <UserDisplay />
      <Stack className="function_plane__function_component_wrapper" direction={{ xs: "column", md: "row" }} spacing={2}>
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
    </>
  );
};

export default FunctionPlane;
