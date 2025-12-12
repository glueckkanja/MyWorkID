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

export const loadCustomCss = (customCssUrl?: string) => {
  if (!customCssUrl || customCssUrl.trim() === "") {
    return;
  }

  const link = document.createElement("link");
  link.rel = "stylesheet";
  link.type = "text/css";
  link.href = customCssUrl;
  link.id = "custom-css";
  
  document.head.appendChild(link);
};
