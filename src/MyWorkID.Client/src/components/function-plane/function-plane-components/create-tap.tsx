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
import  CreateTapSvgIcon  from "@/assets/svg/create-tap.svg";
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
  const { toast } = useToast();
  const createTAP = async () => {
    generateTAP()
      .then((result) => {
        setTapDisplay({
          visible: true,
          value: result.data?.temporaryAccessPassword ?? "ERROR",
          loading: false,
        });
      })
      .catch((error) => {
        toast({
          variant: "destructive",
          title: "Something went wrong during TAP generation.",
          description: error.response.statusText,
        });
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
  }, []);

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
            <CardTitle><img src={CreateTapSvgIcon} alt="CreateTapIcon" /></CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Create Temporary Access Password
          </CardFooter>
        </Card>
      ) : (
        <Card className="action-card__tap">
          <CardContent className="action-card__tap_content">
            {tapDisplay.value}
          </CardContent>
        </Card>
      )}
    </div>
  );
};
