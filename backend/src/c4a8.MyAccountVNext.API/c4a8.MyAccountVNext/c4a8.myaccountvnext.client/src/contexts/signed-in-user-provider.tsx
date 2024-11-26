import React, { ReactNode, useEffect } from "react";
import { parseRoles } from "../services/roles-service";
import { getMsalInfo } from "../services/msal-service";

export type TSignedInUser = {
  roles: string[];
};

const SignedInUserContext = React.createContext(
  undefined as TSignedInUser | undefined
);
// eslint-disable-next-line react-refresh/only-export-components
export const useSignedInUser = () => React.useContext(SignedInUserContext);

export type SignedInUserProviderProps = {
  children: ReactNode;
};

export const SignedInUserProvider = (props: SignedInUserProviderProps) => {
  const [signedInUser, setSignedInUser] = React.useState<TSignedInUser>();

  useEffect(() => {
    getMsalInfo().then((msalInfo) => {
      const accounts = msalInfo.msalInstance.getAllAccounts();

      if (accounts.length === 0) {
        throw new Error(
          "User not signed in. SignedInUserProvider is only allowed to be used inside of an Authenticated context"
        );
      }

      const request = {
        scopes: [`api://${msalInfo.backendClientId}/Access`],
        account: accounts[0],
      };

      msalInfo?.msalInstance.acquireTokenSilent(request).then((result) => {
        setSignedInUser({ roles: parseRoles(result.accessToken) });
      });
    });
  }, []);

  return (
    <SignedInUserContext.Provider value={signedInUser}>
      {props.children}
    </SignedInUserContext.Provider>
  );
};
