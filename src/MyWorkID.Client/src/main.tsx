import React from "react";
import ReactDOM from "react-dom/client";
import "./assets/css/main.css";
import "./assets/css/custom.css";
import { BrowserRouter } from "react-router-dom";
import { AppAuthentication } from "./app-authentication";
import App from "./app";
import { Toaster } from "./components/ui/toaster";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement,
);

root.render(
  <BrowserRouter>
    <React.StrictMode>
      <AppAuthentication>
        <App />
        <Toaster />
      </AppAuthentication>
    </React.StrictMode>
  </BrowserRouter>,
);
