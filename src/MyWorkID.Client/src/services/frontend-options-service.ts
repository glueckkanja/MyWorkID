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
    link.id = "custom-css";
    link.rel = "stylesheet";
    link.href = url.href;
    document.head.appendChild(link);
  } catch {
    console.error("Invalid custom CSS URL:", customCssUrl);
  }
};

export const updateDocumentHead = (options: TFrontendOptions) => {
  // Update page title
  if (options.appTitle && options.appTitle.trim() !== "") {
    document.title = options.appTitle;
  }

  // Update favicon
  if (options.faviconUrl && options.faviconUrl.trim() !== "") {
    const existingFavicon = document.querySelector("link[rel='icon']");
    if (existingFavicon) {
      existingFavicon.setAttribute("href", options.faviconUrl);
    } else {
      const link = document.createElement("link");
      link.rel = "icon";
      link.href = options.faviconUrl;
      document.head.appendChild(link);
    }
  }

  // Load custom CSS if provided
  loadCustomCss(options.customCssUrl);
};
