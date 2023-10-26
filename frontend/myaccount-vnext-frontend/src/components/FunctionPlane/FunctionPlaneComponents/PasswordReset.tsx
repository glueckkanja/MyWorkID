import Button from "@mui/material/Button";
import { ActionResultProps } from "../../../types";

export const PasswordReset = (props: ActionResultProps<any>) => {
  return (
    <div>
      <Button className="function_plane__function_component__action" variant="contained">Password Reset</Button>
    </div>
  );
};
