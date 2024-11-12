import { dismissUserRisk } from "../../../services/api-service";
import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import CircularProgress from "@mui/material/CircularProgress/CircularProgress";
import { CardContent } from "@mui/material";
import { useToast } from "@/hooks/use-toast";

export const DismissUserRisk = (props: TFunctionProps) => {
  const [loading, setLoading] = useState<boolean>(false);
  const [success, setSuccess] = useState<boolean>();
  const svgIcon = (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="36"
      height="46"
      viewBox="0 0 36 46"
      fill="none"
    >
      <path
        d="M16.1158 9.14827V5.10132H12.3477V9.14827C10.9875 9.55381 9.79442 10.3874 8.94583 11.5252C8.09724 12.663 7.63843 14.0442 7.63757 15.4636V42.7824C7.63757 43.2821 7.83607 43.7613 8.1894 44.1146C8.54273 44.468 9.02195 44.6665 9.52163 44.6665H18.9419C19.4416 44.6665 19.9208 44.468 20.2741 44.1146C20.6275 43.7613 20.826 43.2821 20.826 42.7824V15.4636C20.8251 14.0442 20.3663 12.663 19.5177 11.5252C18.6691 10.3874 17.476 9.55381 16.1158 9.14827Z"
        stroke="white"
        stroke-width="2"
        stroke-linecap="square"
      />
      <path
        d="M3.86953 1.33325L12.3478 5.10136C9.59952 5.10136 6.96383 6.1931 5.02052 8.13641C3.07721 10.0797 1.98547 12.7154 1.98547 15.4637V39.0144"
        stroke="white"
        stroke-width="2"
        stroke-linecap="square"
      />
      <path
        d="M32.9481 1.34272C33.081 1.32501 33.2162 1.3359 33.3446 1.37465C33.473 1.41339 33.5916 1.47911 33.6926 1.5674C33.7935 1.65569 33.8745 1.76451 33.93 1.8866C33.9855 2.00868 34.0143 2.14121 34.0144 2.27532V7.92749C34.0156 8.06239 33.9878 8.19598 33.9329 8.3192C33.878 8.44243 33.7972 8.55242 33.6961 8.64174C33.595 8.73106 33.4759 8.79762 33.3469 8.83691C33.2178 8.87621 33.0818 8.88733 32.9481 8.86951L21.5194 7.34531C21.0669 7.28505 20.6516 7.06253 20.3508 6.71911C20.05 6.37569 19.8841 5.93474 19.884 5.47822V4.73401C19.8837 4.27717 20.0494 3.83576 20.3502 3.49194C20.651 3.14813 21.0665 2.92534 21.5194 2.86503L32.9481 1.34272Z"
        stroke="white"
        stroke-width="2"
        stroke-linecap="square"
      />
      <path
        d="M16.1158 5.10132H19.884"
        stroke="white"
        stroke-width="2"
        stroke-linecap="square"
      />
      <path
        d="M20.826 39.0145H16.1159C15.866 39.0145 15.6264 38.9153 15.4497 38.7386C15.2731 38.5619 15.1738 38.3223 15.1738 38.0725V19.2319C15.1738 18.9821 15.2731 18.7425 15.4497 18.5658C15.6264 18.3892 15.866 18.2899 16.1159 18.2899H20.826"
        stroke="white"
        stroke-width="2"
        stroke-linecap="square"
      />
    </svg>
  );
  const svgIconSuccess = (
    <svg
      width="42"
      height="42"
      viewBox="0 0 42 42"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <g id="Group 33">
        <path
          id="Vector"
          d="M21.0002 41.0003C36.3734 40.9996 45.982 24.3325 38.2947 10.9994C34.7272 4.81159 28.1345 1 21.0002 1C5.62697 1 -3.98128 17.6668 3.70532 31.0002C7.27283 37.1887 13.8651 41.0007 21.0002 41.0003Z"
          stroke="#ACD653"
          stroke-width="2"
          stroke-linecap="square"
        />
        <path
          id="Vector_2"
          d="M29.8001 15.4001L20.5645 27.2808C19.6922 28.3989 17.9975 28.5159 16.9688 27.5293L12.2 22.9279"
          stroke="#ACD653"
          stroke-width="2"
          stroke-linecap="square"
        />
      </g>
    </svg>
  );
  const { toast } = useToast();

  useEffect(() => {
    if (props.comingFromRedirect) {
      dismissUserRisk();
    }
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
            <CardTitle>{svgIcon}</CardTitle>
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
            <CardTitle>{svgIconSuccess}</CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer__validate-id-success">
            User Risk Dismissed.
          </CardFooter>
        </Card>
      )}
    </div>
  );
};
