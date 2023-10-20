import {
  MsalProvider,
  MsalAuthenticationTemplate,
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import { ReactNode, useEffect, useState } from "react";
import { MSAL_INFO } from "./services/MsalService";

export type AppAutenticationProps = {
  children: ReactNode;
};

export const AppAutentication = (props: AppAutenticationProps) => {
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    MSAL_INFO.msalInstance.initialize().then(() => {
      setLoading(false);
    });
  }, []);

  if (loading) {
    return <div>Loading</div>;
  } else if (MSAL_INFO?.msalInstance) {
    return (
      <MsalProvider instance={MSAL_INFO.msalInstance}>
        <MsalAuthenticationTemplate
          interactionType={InteractionType.Redirect}
          authenticationRequest={{
            scopes: [`api://${MSAL_INFO.backendClientId}/Access`],
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
