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

export type AppAuthenticationProps = {
  children: ReactNode;
};

export const AppAuthentication = (props: AppAuthenticationProps) => {
  const [msalInfo, setMsalInfo] = useState<TMsalInfo>();

  const initializeMsalInfo = async (): Promise<TMsalInfo> => {
    const info = await getMsalInfo();
    await handleRedirectPromise();
    await getActiveMsalAccount();

    return info;
  };

  // Ensures an active account is set when accounts are already cached.
  // If multiple accounts are cached without an active one, this triggers
  // a loginRedirect and the page navigates away before setMsalInfo runs.
  useEffect(() => {
    initializeMsalInfo().then((resolvedMsalInfo) => {
      setMsalInfo(resolvedMsalInfo);
    });
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
