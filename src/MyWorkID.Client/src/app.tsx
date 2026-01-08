import FunctionPlane from "./components/function-plane/function-plane";
import { Header } from "./components/header";
import { Footer } from "./components/footer";
import { ThemeProvider } from "./components/theme-provider";
import { useEffect } from "react";
import { getFrontendOptions, updateDocumentHead } from "./services/frontend-options-service";

export const App = () => {
  useEffect(() => {
    const loadConfig = async () => {
      try {
        const options = await getFrontendOptions();
        updateDocumentHead(options);
      } catch (error) {
        console.error("Failed to load frontend configuration:", error);
      }
    };
    void loadConfig();
  }, []);

  return (
    <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
      <Header />
      <FunctionPlane />
      <Footer />
    </ThemeProvider>
  );
};
export default App;
