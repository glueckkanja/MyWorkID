import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import { BrowserRouter } from "react-router-dom";
import { AppAutentication } from "./app-authentication";
import App from "./app-main";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

root.render(
  <BrowserRouter>
    <React.StrictMode>
      <AppAutentication>
        <App />
      </AppAutentication>
    </React.StrictMode>
  </BrowserRouter>
);
