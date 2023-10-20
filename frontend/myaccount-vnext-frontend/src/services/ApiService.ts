import { authenticateRequest } from "./MsalService";
import { REQUEST_TYPE } from "../types";
import { Strings } from "../Strings";

export const dismissUserRisk = async (): Promise<void> => {
  return authenticateRequest(
    `${window.settings.backendApiUrl}/DismissUserRisk`,
    REQUEST_TYPE.PUT,
    Strings.DISMISS_USER_RISK
  );
};
