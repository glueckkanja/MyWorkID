import FunctionPlane from "./components/function-plane/function-plane";
import { Header } from "./components/header";
import { Footer } from "./components/footer";
import { ThemeProvider } from "./components/theme-provider";
import type { AppProps } from "./types";

export const App = ({ maxDismissibleRiskLevel }: AppProps) => {
  return (
    <ThemeProvider defaultTheme="system" storageKey="vite-ui-theme">
      <div className="app-shell">
        <Header />
        <FunctionPlane maxDismissibleRiskLevel={maxDismissibleRiskLevel} />
        <Footer />
      </div>
    </ThemeProvider>
  );
};
export default App;
