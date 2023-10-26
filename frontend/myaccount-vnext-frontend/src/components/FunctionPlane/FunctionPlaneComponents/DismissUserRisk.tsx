import { dismissUserRisk } from "../../../services/ApiService";
import Button from "@mui/material/Button";
import { ActionResultProps } from "../../../types";

export const DismissUserRisk = (props: ActionResultProps<any>) => {
  return (
    <Button variant="contained" onClick={dismissUserRisk}>
      Dismiss User Risk
    </Button>
  );
};
