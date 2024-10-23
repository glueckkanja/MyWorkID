import axios from "axios";
import { TFrontendOptions } from "../types";

let frontendOptionsCache: TFrontendOptions | undefined = undefined;

export const getFrontendOptions = async () => {
  if (frontendOptionsCache) {
    return frontendOptionsCache;
  }

  const apiUrl: string = `${window.location.protocol}//${window.location.host}/api/config/frontend`;
  const frontendOptionsResponse = await axios.get<TFrontendOptions>(apiUrl);
  frontendOptionsCache = frontendOptionsResponse.data;
  if(!frontendOptionsCache){
    throw new Error("Frontend options not found");
  }
  return frontendOptionsCache;
};
