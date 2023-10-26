import Button from "@mui/material/Button";
import { useState } from "react";
import { ActionResultProps } from "../../../types";
import TextField from "@mui/material/TextField";

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
      visible: true,
      value: "test",
      loading: true,
    });
  };

  return (
    <div>
      <Button variant="contained" onClick={createTAP}>
        Create Temporary Access Password
      </Button>
      <TextField className={tapDisplay.visible ? undefined : "hidden_element"} sx={{width: "100%"}} label="TAP" variant="filled" value={tapDisplay.value} />
    </div>
  );
};
