import FunctionPlane from "./components/FunctionPlane/function-plane";
import { Header } from "./components/main-header";
import { Footer } from "./components/main-footer";
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
