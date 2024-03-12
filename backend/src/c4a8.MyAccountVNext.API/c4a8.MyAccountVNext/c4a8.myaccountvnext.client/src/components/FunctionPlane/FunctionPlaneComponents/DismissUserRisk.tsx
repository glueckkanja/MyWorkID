import { dismissUserRisk } from "../../../services/ApiService";
import Button from "@mui/material/Button";
import { ActionResultProps } from "../../../types";
import CircularProgress from "@mui/material/CircularProgress";
import { useEffect, useState } from "react";

export const DismissUserRisk = (props: ActionResultProps<any>) => {
  const [loading, setLoading] = useState<boolean>(false);

  useEffect(() => {
    if (props.result) {
      switch (props.result.status) {
        case "pending":
          setLoading(true);
          break;
        // @ts-expect-error - Fall through is intentional here
        case "error":
          console.error("Could not dismiss status", props.result.data);
        // Fall through
        case "success":
        // Fall through
        default:
          setLoading(false);
      }
    }
  }, [props.result]);

  const triggerDismissUserRisk = () => {
    setLoading(true);
    dismissUserRisk()
      .then(() => setLoading(false))
      .catch(() => setLoading(false));
  };

  return (
    <div>
      <Button
        className="function_plane__function_component__action"
        variant="contained"
        onClick={triggerDismissUserRisk}
        disabled={loading}
      >
        Dismiss User Risk
      </Button>
      <div>
        <div
          className={
            loading
              ? "function_plane__function_component__loading_spinner__container"
              : "hidden_element"
          }
        >
          <CircularProgress />
        </div>
      </div>
    </div>
  );
};
