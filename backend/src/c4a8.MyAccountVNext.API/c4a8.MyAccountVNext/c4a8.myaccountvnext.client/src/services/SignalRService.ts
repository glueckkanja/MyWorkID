import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Mutex } from "async-mutex";
import { getMsalInfo } from "./MsalService";

const getVerifiedIdConnectionMutex = new Mutex();
let verifiedIdConnectionCache: HubConnection | undefined = undefined;

export const getVerifiedIdConnection = async (): Promise<HubConnection> => {
  return await getVerifiedIdConnectionMutex.runExclusive(async () => {
    if (verifiedIdConnectionCache) {
      return verifiedIdConnectionCache;
    }

    let msalInfo = await getMsalInfo();
    let signedInUser = msalInfo.msalInstance.getAllAccounts()[0].localAccountId;

    verifiedIdConnectionCache = new HubConnectionBuilder()
      .withUrl("/hubs/verifiedId", { accessTokenFactory: () => signedInUser })
      .build();

    return verifiedIdConnectionCache;
  });
};
