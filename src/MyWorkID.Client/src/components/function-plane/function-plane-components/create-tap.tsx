import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import { generateTAP } from "../../../services/api-service";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import CreateTapSvgIcon from "@/assets/svg/create-tap.svg";
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
  const { toastException, toastError } = useToast();
  const createTAP = async () => {
    setTapDisplay({
      visible: true,
      value: "",
      loading: true,
    });
    generateTAP()
      .then((result) => {
        if (
          result.status === "success" &&
          !!result.data?.temporaryAccessPassword &&
          result.data?.temporaryAccessPassword.trim().length > 0
        ) {
          setTapDisplay({
            visible: true,
            value: result.data?.temporaryAccessPassword,
            loading: false,
          });
        } else {
          setTapDisplay({
            visible: false,
            value: "",
            loading: false,
          });
          toastError();
        }
      })
      .catch((error) => {
        toastException(error);
        setTapDisplay({
          visible: false,
          value: "",
          loading: false,
        });
      });
  };

  useEffect(() => {
    if (props.comingFromRedirect) {
      createTAP();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.comingFromRedirect]);

  return (
    <div>
      {!tapDisplay.visible ? (
        <Card
          className="action-card"
          onClick={() => {
            createTAP();
          }}
        >
          <CardHeader>
            <CardTitle>
              <img src={CreateTapSvgIcon} alt="CreateTapIcon" />
            </CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Create Temporary Access Pass
          </CardFooter>
        </Card>
      ) : tapDisplay.loading ? (
        <Card className="action-card__container__loading">
          <CardContent>
            <div className="action-card__loading">
              <Spinner />
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card className="action-card__tap">
          <CardContent className="action-card__tap_content text-select">
            {tapDisplay.value}
          </CardContent>
        </Card>
      )}
    </div>
  );
};
