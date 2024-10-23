import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import { generateTAP } from "../../../services/ApiService";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

type TAPDisplay = {
  visible: boolean;
  value: string;
  loading: boolean;
};

const svgIcon = (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="52"
    height="44"
    viewBox="0 0 52 44"
    fill="none"
  >
    <path
      d="M37.375 42.0416C47.7838 42.0411 54.2896 30.7562 49.0847 21.7286C46.6692 17.539 42.2055 14.9583 37.375 14.9583C26.9661 14.9583 20.4606 26.243 25.665 35.2707C28.0805 39.4608 32.544 42.0418 37.375 42.0416Z"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M43.3333 24.7083L37.0801 32.7524C36.4895 33.5094 35.342 33.5887 34.6455 32.9206L31.4167 29.8051"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M14.6175 20.3749H5.32246C2.9812 20.3749 1.08333 18.5426 1.08333 16.2823V6.05084C1.08333 3.79052 2.9812 1.95825 5.32246 1.95825H45.5942C47.9355 1.95825 49.8333 3.79052 49.8333 6.05084V7.86253"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M21.2343 11.2802C20.8359 11.2802 20.5868 10.8489 20.786 10.5039C20.8785 10.3438 21.0494 10.2451 21.2343 10.2451"
      stroke="white"
      stroke-width="2"
      stroke-linecap="round"
    />
    <path
      d="M21.2346 11.2802C21.6331 11.2802 21.8821 10.8489 21.6829 10.5039C21.5904 10.3438 21.4195 10.2451 21.2346 10.2451"
      stroke="white"
      stroke-width="2"
      stroke-linecap="round"
    />
    <path
      d="M11.3939 11.2802C10.9955 11.2802 10.7464 10.8489 10.9456 10.5039C11.0382 10.3438 11.2091 10.2451 11.3939 10.2451"
      stroke="white"
      stroke-width="2"
      stroke-linecap="round"
    />
    <path
      d="M11.3942 11.2802C11.7926 11.2802 12.0417 10.8489 11.8425 10.5039C11.75 10.3438 11.5791 10.2451 11.3942 10.2451"
      stroke="white"
      stroke-width="2"
      stroke-linecap="round"
    />
  </svg>
);

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
      {/* <Button
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
      </div> */}
      {!tapDisplay.visible ? (
        <Card
          style={{
            backgroundColor: "#0072C6",
            color: "white",
            width: "164px",
            borderRadius: "24px"
          }}
          onClick={() => {
            createTAP;
          }}
        >
          <CardHeader>
            <CardTitle>{svgIcon}</CardTitle>
          </CardHeader>
          <CardFooter>
            <p>Create Temporary Access Password</p>
          </CardFooter>
        </Card>
      ) : (
        <Card
          style={{
            backgroundColor: "white",
            color: "black",
            width: "164px",
            borderRadius: "24px"
          }}
        >
          <CardContent>{tapDisplay.value}</CardContent>
        </Card>
      )}
    </div>
  );
};
