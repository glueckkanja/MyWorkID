import Button from "@mui/material/Button";
import { useState } from "react";
import { ActionResultProps } from "../../../types";
import TextField from "@mui/material/TextField";
import CircularProgress from "@mui/material/CircularProgress";

type TAPDisplay = {
  visible: boolean;
  value: string;
  loading: boolean;
};

export const CreateTAP = (props: ActionResultProps<any>) => {
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
    setTimeout(() => {
      setTapDisplay({
        visible: true,
        value: "jkwshjfjosgs51g8",
        loading: false,
      });
    }, 2000);
  };

  return (
    <div>
      <Button
        className="function_plane__function_component__action"
        variant="contained"
        onClick={createTAP}
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
        />
        <div
          className={tapDisplay.loading ? "function_plane__function_component__loading_spinner__container" : "hidden_element"}
        >
          <CircularProgress />
        </div>
      </div>
    </div>
  );
};
