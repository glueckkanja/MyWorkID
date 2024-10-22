import Button from "@mui/material/Button";
import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import TextField from "@mui/material/TextField";
import CircularProgress from "@mui/material/CircularProgress";
import { generateTAP } from "../../../services/ApiService";

type TAPDisplay = {
  visible: boolean;
  value: string;
  loading: boolean;
};

export const CreateTAP = (props: TFunctionProps) => {
  const [tapDisplay, setTapDisplay] = useState<TAPDisplay>({
    visible: false,
    value: "",
    loading: false,
  });

  const createTAP = () => {
    setTapDisplay({
      visible: false,
      value: "",
      loading: true,
    });

    generateTAP()
      .then((result) => {
        setTapDisplay({
          visible: true,
          value: result.data?.temporaryAccessPassword || "ERROR",
          loading: false,
        });
      })
      .catch((error) => {
        console.error("Something went wrong during TAP generation.", error);
        setTapDisplay({
          visible: true,
          value: "ERROR",
          loading: false,
        });
      });
  };

  useEffect(() => {
    if (props.comingFromRedirect) {
      createTAP();
    }
  }, []);

  return (
    <div>
      <Button
        className="function_plane__function_component__action"
        variant="contained"
        onClick={createTAP}
        disabled={tapDisplay.loading}
      >
        Create Temporary Access Password
      </Button>
      <div>
        <TextField
          className={tapDisplay.visible ? undefined : "hidden_element"}
          sx={{ width: "100%" }}
          label="TAP"
          variant="filled"
          value={tapDisplay.value}
          inputProps={
            { readOnly: true, }
          }
        />
        <div
          className={
            tapDisplay.loading
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
