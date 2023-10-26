import { dismissUserRisk } from "../../../services/ApiService";
import Button from "@mui/material/Button";
import { ActionResultProps } from "../../../types";

export const DismissUserRisk = (props: ActionResultProps<any>) => {
  return (
    <div>
      <Button className="function_plane__function_component__action" variant="contained" onClick={dismissUserRisk}>
        Dismiss User Risk
      </Button>
    </div>
  );
};
