import FunctionPlane from "./components/function-plane/function-plane";
import { Header } from "./components/header";
import { Footer } from "./components/footer";
import { ThemeProvider } from "./components/theme-provider";
import { SettingsProvider } from "./contexts/settings-provider";
import { RiskLevel } from "@/lib/risk-level";

type AppProps = {
  maxDismissibleRiskLevel: RiskLevel;
};

export const App = ({ maxDismissibleRiskLevel }: AppProps) => {
  return (
    <ThemeProvider defaultTheme="system" storageKey="vite-ui-theme">
      <div className="app-shell">
        <Header />
        <SettingsProvider maxDismissibleRiskLevel={maxDismissibleRiskLevel}>
          <FunctionPlane />
        </SettingsProvider>
        <Footer />
      </div>
    </ThemeProvider>
  );
};
export default App;
