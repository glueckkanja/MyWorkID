import CssBaseline from "@mui/material/CssBaseline";
import FunctionPlane from "./components/FunctionPlane";
import { HeaderBar } from "./components/HeaderBar";
import { ToggleColorModeProvider } from "./contexts/ToggleColorModeProvider";

export const App = () => {
  return (
    <ToggleColorModeProvider>
      <CssBaseline />
      <HeaderBar />
      <FunctionPlane />
    </ToggleColorModeProvider>
  );
};

export default App;
