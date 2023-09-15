import {
  MsalProvider,
  MsalAuthenticationTemplate,
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { InteractionType, PublicClientApplication } from "@azure/msal-browser";
import { ReactNode, useEffect, useState } from "react";
import App from "./pages/App";
import { TMsalInfo, getMsalInfo } from "./services/MsalService";

export type AppAutenticationProps = {
  children: ReactNode;
};

export const AppAutentication = (props: AppAutenticationProps) => {
  const [msalInfo, setMsalInfo] = useState<
    TMsalInfo | undefined
  >(undefined);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    getMsalInfo().then((res) => {
      setMsalInfo(res);
      setLoading(false);
    });
  }, []);

  if (loading) {
    return <div>Loading</div>;
  } else if (msalInfo?.msalInstance) {
    return (
      <MsalProvider instance={msalInfo.msalInstance}>
        <MsalAuthenticationTemplate
          interactionType={InteractionType.Redirect}
          authenticationRequest={{
            scopes: [`api://${msalInfo.backendClientId}/Access`],
          }}
        >
          <AuthenticatedTemplate>{props.children}</AuthenticatedTemplate>
          <UnauthenticatedTemplate>
            <p>You are not signed in! Please sign in.</p>
          </UnauthenticatedTemplate>
        </MsalAuthenticationTemplate>
      </MsalProvider>
    );
  } else {
    return <></>;
  }
};
