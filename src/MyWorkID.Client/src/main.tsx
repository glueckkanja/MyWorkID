import React from "react";
import ReactDOM from "react-dom/client";
import "./assets/css/main.css";
import "./assets/css/custom.css";
import { BrowserRouter } from "react-router-dom";
import { AppAutentication } from "./app-authentication";
import App from "./app";
import { Toaster } from "./components/ui/toaster";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

root.render(
  <BrowserRouter>
    <React.StrictMode>
      <AppAutentication>
        <App />
        <Toaster />
      </AppAutentication>
    </React.StrictMode>
  </BrowserRouter>
);
