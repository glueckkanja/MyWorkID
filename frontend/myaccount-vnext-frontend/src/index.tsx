import React from "react";
import ReactDOM from "react-dom/client";
import "./main.scss";
import App from "./pages/App";
import { AppAutentication } from "./AppAuthentication";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);
root.render(
  <React.StrictMode>
    <AppAutentication>
      <App />
    </AppAutentication>
  </React.StrictMode>
);
