import FunctionPlane from "./components/FunctionPlane/FunctionPlane";
import { ToggleColorModeProvider } from "./contexts/ToggleColorModeProvider";
import { Header } from "./components/Header";
import { Footer } from "./components/Footer";

export const App = () => {
  return (
    <ToggleColorModeProvider>
      <Header />
      <FunctionPlane />
      <Footer />
    </ToggleColorModeProvider>
  );
};

export default App;
