import React, { ReactNode, useEffect } from "react";
import { MSAL_INFO } from "../services/MsalService";
import { parseRoles } from "../services/RolesService";

export type TSignedInUser = {
  roles: string[];
};

const SignedInUserContext = React.createContext(
  undefined as TSignedInUser | undefined
);
export const useSignedInUser = () => React.useContext(SignedInUserContext);

export type SignedInUserProviderProps = {
  children: ReactNode;
};

export const SignedInUserProvider = (props: SignedInUserProviderProps) => {
  const [signedInUser, setSignedInUser] = React.useState<TSignedInUser>();

  useEffect(() => {
    const accounts = MSAL_INFO.msalInstance.getAllAccounts();

    if (accounts.length === 0) {
      throw new Error(
        "User not signed in. SignedInUserProvider is only allowed to be used inside of an Authenticated context"
      );
    }

    const request = {
      scopes: [`api://${MSAL_INFO.backendClientId}/Access`],
      account: accounts[0],
    };

    MSAL_INFO?.msalInstance.acquireTokenSilent(request).then((result) => {
      setSignedInUser({roles: parseRoles(result.accessToken)})
    });
  }, []);

  return (
    <SignedInUserContext.Provider value={signedInUser}>
      {props.children}
    </SignedInUserContext.Provider>
  );
};
