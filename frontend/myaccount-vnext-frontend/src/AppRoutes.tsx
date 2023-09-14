import App from "./pages/App";
import { DismissUserRiskRedirector } from "./pages/FunctionRedirector/DismissUserRiskRedirector";

const AppRoutes = [
  {
    index: true,
    element: <App />,
  },
  {
    path: "/redirectors/dismissUserRisk",
    element: (
      <DismissUserRiskRedirector/>
    ),
  }
];

export default AppRoutes;
