import FunctionPlane from "./components/function-plane/function-plane";
import { Header } from "./components/header";
import { Footer } from "./components/footer";
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
