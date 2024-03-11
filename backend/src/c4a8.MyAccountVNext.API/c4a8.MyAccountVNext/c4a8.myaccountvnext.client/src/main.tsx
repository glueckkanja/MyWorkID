import React from "react";
import ReactDOM from "react-dom/client";
import "./main.scss";
import { BrowserRouter } from "react-router-dom";
import { AppAutentication } from "./AppAuthentication";
import App from "./App";
import { TFrontendOptions } from "./types";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

declare global {
  interface Window {
    settings: TFrontendOptions;
  }
}

root.render(
  <BrowserRouter>
    <React.StrictMode>
      <AppAutentication>
        <App />
      </AppAutentication>
    </React.StrictMode>
  </BrowserRouter>
);
