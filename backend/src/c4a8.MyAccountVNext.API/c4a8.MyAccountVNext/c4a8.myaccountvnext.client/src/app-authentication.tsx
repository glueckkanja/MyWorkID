import {
  MsalProvider,
  MsalAuthenticationTemplate,
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import { ReactNode, useEffect, useState } from "react";
import { SignedInUserProvider } from "./contexts/SignedInUserProvider";
import { TMsalInfo, getMsalInfo } from "./services/msal-service";

export type AppAutenticationProps = {
  children: ReactNode;
};

export const AppAutentication = (props: AppAutenticationProps) => {
  const [msalInfo, setMsalInfo] = useState<TMsalInfo>();

  useEffect(() => {
    getMsalInfo().then((_msalInfo) => {
      setMsalInfo(_msalInfo);
    });
  }, []);

  if (!msalInfo) {
    return <div>Loading</div>;
  } else if(msalInfo.msalInstance) {
    return (
      <MsalProvider instance={msalInfo.msalInstance}>
        <MsalAuthenticationTemplate
          interactionType={InteractionType.Redirect}
          authenticationRequest={{
            scopes: [`api://${msalInfo.backendClientId}/Access`],
          }}
        >
          <AuthenticatedTemplate>
            <SignedInUserProvider>{props.children}</SignedInUserProvider>
          </AuthenticatedTemplate>
          <UnauthenticatedTemplate>
            <p>You are not signed in! Please sign in.</p>
          </UnauthenticatedTemplate>
        </MsalAuthenticationTemplate>
      </MsalProvider>
    );
  } else {
    return <div>Something went wrong</div>;
  }
};
