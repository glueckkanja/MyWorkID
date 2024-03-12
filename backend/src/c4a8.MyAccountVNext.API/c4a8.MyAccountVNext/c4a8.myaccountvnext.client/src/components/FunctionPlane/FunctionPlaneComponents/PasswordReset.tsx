import Button from "@mui/material/Button";
import { ActionResultProps } from "../../../types";

// eslint-disable-next-line @typescript-eslint/no-unused-vars -- This is a placeholder component
export const PasswordReset = (props: ActionResultProps<any>) => {
  return (
    <div>
      <Button className="function_plane__function_component__action" variant="contained" onClick={() => {console.log(props)}}>Password Reset</Button>
    </div>
  );
};
