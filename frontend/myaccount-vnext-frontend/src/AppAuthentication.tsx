import {
  MsalProvider,
  MsalAuthenticationTemplate,
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { InteractionType, PublicClientApplication } from "@azure/msal-browser";
import { ReactNode, useEffect, useState } from "react";
import App from "./pages/App";
import { getMsalInstance } from "./services/MsalService";

export type AppAutenticationProps = {
  children: ReactNode;
};

export const AppAutentication = (props: AppAutenticationProps) => {
  const [msalInstance, setMsalInstance] = useState<
    PublicClientApplication | undefined
  >(undefined);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    getMsalInstance().then((res) => {
      setMsalInstance(res);
      setLoading(false);
    });
  }, []);

  if (loading) {
    return (<div>Loading</div>)
  } else if (msalInstance) {
    return (
      <MsalProvider instance={msalInstance}>
        <MsalAuthenticationTemplate interactionType={InteractionType.Redirect}>
          <AuthenticatedTemplate>
            {props.children}
          </AuthenticatedTemplate>
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
