import FunctionPlane from "./components/FunctionPlane/FunctionPlane";
import { Header } from "./components/Header";
import { Footer } from "./components/Footer";
import { ThemeProvider } from "./components/theme-provider";

export const App = () => {
  return (
    <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
      <Header />
      <FunctionPlane />
      <Footer />
    </ThemeProvider>
  );
};
export default App;
