import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Mutex } from "async-mutex";
import { getMsalInfo } from "./msal-service";

const getVerifiedIdConnectionMutex = new Mutex();
let verifiedIdConnectionCache: HubConnection | undefined = undefined;

export const getVerifiedIdConnection = async (): Promise<HubConnection> => {
  return await getVerifiedIdConnectionMutex.runExclusive(async () => {
    if (verifiedIdConnectionCache) {
      return verifiedIdConnectionCache;
    }

    const msalInfo = await getMsalInfo();
    const signedInUser = msalInfo.msalInstance.getAllAccounts()[0].localAccountId;

    verifiedIdConnectionCache = new HubConnectionBuilder()
      .withUrl("/hubs/verifiedId", { accessTokenFactory: () => signedInUser })
      .build();

    return verifiedIdConnectionCache;
  });
};
