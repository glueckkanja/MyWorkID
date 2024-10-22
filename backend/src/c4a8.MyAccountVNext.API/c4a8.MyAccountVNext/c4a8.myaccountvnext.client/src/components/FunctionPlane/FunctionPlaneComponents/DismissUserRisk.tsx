import { dismissUserRisk } from "../../../services/ApiService";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";

export const DismissUserRisk = (props: TFunctionProps) => {
  const [loading, setLoading] = useState<boolean>(false);

  useEffect(() => {
    if (props.comingFromRedirect) {
      dismissUserRisk();
    }
  }, []);

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
