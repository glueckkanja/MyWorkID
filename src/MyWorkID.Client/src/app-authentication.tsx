import {
  MsalProvider,
  MsalAuthenticationTemplate,
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import { ReactNode, useEffect, useState } from "react";
import { SignedInUserProvider } from "./contexts/signed-in-user-provider";
import {
  TMsalInfo,
  getActiveMsalAccount,
  getMsalInfo,
  handleRedirectPromise,
} from "./services/msal-service";

export type AppAutenticationProps = {
  children: ReactNode;
};

export const AppAutentication = (props: AppAutenticationProps) => {
  const [msalInfo, setMsalInfo] = useState<TMsalInfo>();

  // Ensures an active account is set when accounts are already cached.
  // If multiple accounts are cached without an active one, this triggers
  // a loginRedirect and the page navigates away before setMsalInfo runs.
  useEffect(() => {
    const initialize = async () => {
      const info = await getMsalInfo();
      // Process any pending redirect first so the active account gets set
      await handleRedirectPromise();
      try {
        await getActiveMsalAccount();
      } catch (error) {
        if (import.meta.env.DEV) {
          console.debug("No active account resolved during app init", error);
        }
      }
      setMsalInfo(info);
    };
    initialize();
  }, []);

  if (!msalInfo) {
    return <div>Loading</div>;
  } else if (msalInfo.msalInstance) {
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
