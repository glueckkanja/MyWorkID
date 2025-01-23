import { dismissUserRisk } from "../../../services/api-service";
import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import CircularProgress from "@mui/material/CircularProgress/CircularProgress";
import { CardContent } from "@mui/material";
import { useToast } from "@/hooks/use-toast";
import DismissUserRiskSvg from "@/assets/svg/dismiss-user-risk.svg";
import SuccessSvg from "../../../assets/svg/success.svg";

export const DismissUserRisk = (props: TFunctionProps) => {
  const [loading, setLoading] = useState<boolean>(false);
  const [success, setSuccess] = useState<boolean>();
  const { toast } = useToast();

  useEffect(() => {
    if (props.comingFromRedirect) {
      dismissUserRisk();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const triggerDismissUserRisk = () => {
    setLoading(true);
    dismissUserRisk()
      .then(() => {
        setLoading(false);
        setSuccess(true);
      })
      .catch((error) => {
        setLoading(false);
        setSuccess(false);
        toast({
          variant: "destructive",
          title: "Something went wrong trying to dismiss User risk.",
          description: error.response.statusText,
        });
      });
  };
  return (
    <div>
      {!loading && (success === undefined || success === false) && (
        <Card
          className="action-card"
          onClick={() => {
            triggerDismissUserRisk();
          }}
        >
          <CardHeader>
            <CardTitle><img src={DismissUserRiskSvg} alt="DismissUserRiskIcon" /></CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Dismiss User Risk
          </CardFooter>
        </Card>
      )}
      {loading && success === undefined && (
        <Card className="action-card__validate-id-success">
          <CardContent>
            <CircularProgress />
          </CardContent>
        </Card>
      )}
      {!loading && success === true && (
        <Card className="action-card__validate-id-success">
          <CardHeader>
            <CardTitle>
              <img src={SuccessSvg} alt="SuccessIcon" />
            </CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer__validate-id-success">
            User Risk Dismissed.
          </CardFooter>
        </Card>
      )}
    </div>
  );
};
