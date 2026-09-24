import React from "react";
import ReactDOM from "react-dom/client";
import "./assets/css/main.css";
import "./assets/css/custom.css";
import { BrowserRouter } from "react-router-dom";
import { AppAuthentication } from "./app-authentication";
import App from "./app";
import { Toaster } from "./components/ui/toaster";
import {
  getFrontendOptions,
  updateDocumentHead,
} from "./services/frontend-options-service";
import { getMaximumDismissibleRiskLevel, RiskLevel } from "./lib/risk-level";

let maxDismissibleRiskLevel: RiskLevel = RiskLevel.Low;
try {
  const frontendOptions = await getFrontendOptions();
  updateDocumentHead(frontendOptions);
  maxDismissibleRiskLevel = getMaximumDismissibleRiskLevel(
    frontendOptions.maxDismissibleRiskLevel,
  );
} catch (error) {
  console.debug(
    "Failed to load frontend options; falling back to maxDismissibleRiskLevel Low",
    error,
  );
}

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement,
);

root.render(
  <BrowserRouter>
    <React.StrictMode>
      <AppAuthentication>
        <App maxDismissibleRiskLevel={maxDismissibleRiskLevel} />
        <Toaster />
      </AppAuthentication>
    </React.StrictMode>
  </BrowserRouter>,
);
