import { useEffect } from "react";
import {
  authenticateRequest,
  getMsalInstance,
} from "../../services/MsalService";
import { REQUEST_TYPE } from "../../types";
import { useNavigate } from "react-router-dom";

export const DismissUserRiskRedirector = () => {
  const navigate = useNavigate();

  useEffect(() => {
    authenticateRequest(
      "https://localhost:7093/DismissUserRisk",
      REQUEST_TYPE.PUT
    ).then(() => {
      navigate("/");
    }).catch((err) => {
      navigate("/error");
    });
  }, [navigate]);

  return (
    <div>
      <h1>DismissUserRiskRedirector</h1>
    </div>
  );
};
