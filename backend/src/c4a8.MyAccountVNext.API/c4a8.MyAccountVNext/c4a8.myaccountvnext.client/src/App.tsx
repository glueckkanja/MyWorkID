import CssBaseline from "@mui/material/CssBaseline";
import FunctionPlane from "./components/FunctionPlane/FunctionPlane";
import { ToggleColorModeProvider } from "./contexts/ToggleColorModeProvider";
import { Header } from "./components/Header";

export const App = () => {
  return (
    <ToggleColorModeProvider>
      <CssBaseline />
      <Header />
      <FunctionPlane />
    </ToggleColorModeProvider>
  );
};

export default App;
