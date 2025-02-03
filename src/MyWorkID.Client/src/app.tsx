import FunctionPlane from "./components/function-plane/function-plane";
import { Header } from "./components/header";
import { Footer } from "./components/footer";
import { ThemeProvider } from "./components/theme-provider";
import { TooltipProvider } from "./components/ui/tooltip";

export const App = () => {
  return (
    <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
      <TooltipProvider delayDuration={100}>
        <Header />
        <FunctionPlane />
        <Footer />
      </TooltipProvider>
    </ThemeProvider>
  );
};
export default App;
