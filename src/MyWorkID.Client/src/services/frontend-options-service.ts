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
  if (!frontendOptionsCache) {
    throw new Error("Frontend options not found");
  }
  return frontendOptionsCache;
};

export const loadCustomCss = (customCssUrl?: string) => {
  if (!customCssUrl || customCssUrl.trim() === "") {
    return;
  }

  // Validate URL format and protocol
  try {
    const url = new URL(customCssUrl);

    // Only allow HTTPS
    if (url.protocol !== "https:") {
      return;
    }

    // Ensure it's a CSS file
    if (!url.pathname.endsWith(".css")) {
      return;
    }

    // Check if custom CSS link already exists
    const existingLink = document.getElementById("custom-css");
    if (existingLink) {
      // If the URL is the same, skip adding it again
      if (existingLink instanceof HTMLLinkElement && existingLink.href === url.href) {
        return;
      }
      // If the URL is different, remove the old one
      existingLink.remove();
    }

    const link = document.createElement("link");
    link.rel = "stylesheet";
    link.type = "text/css";
    link.href = url.href;
    link.id = "custom-css";

    document.head.appendChild(link);
  } catch (error) {
    console.error(
      `Custom CSS URL rejected: Invalid URL format '${customCssUrl}'`,
      error
    );
  }
};
