import { useEffect, useMemo, useState } from "react";
import { ThemeProviderContext } from "./theme-provider-context";
import type {
  Theme,
  ResolvedTheme,
  ThemeProviderProps,
} from "../types";

const SYSTEM_DARK_MEDIA_QUERY = "(prefers-color-scheme: dark)";

const isValidTheme = (value: unknown): value is Theme =>
  value === "dark" || value === "light" || value === "system";

const getSystemTheme = (): ResolvedTheme => {
  if (
    typeof window === "undefined" ||
    typeof window.matchMedia !== "function"
  ) {
    return "light";
  }
  return window.matchMedia(SYSTEM_DARK_MEDIA_QUERY).matches ? "dark" : "light";
};

export function ThemeProvider({
  children,
  defaultTheme = "system",
  storageKey = "vite-ui-theme",
  ...props
}: Readonly<ThemeProviderProps>) {
  const [theme, setTheme] = useState<Theme>(() => {
    const stored = sessionStorage.getItem(storageKey);
    return isValidTheme(stored) ? stored : defaultTheme;
  });
  const [systemTheme, setSystemTheme] = useState<ResolvedTheme>(() =>
    getSystemTheme(),
  );

  useEffect(() => {
    if (
      typeof window === "undefined" ||
      typeof window.matchMedia !== "function"
    ) {
      return;
    }
    const mediaQuery = window.matchMedia(SYSTEM_DARK_MEDIA_QUERY);
    const handleChange = (event: MediaQueryListEvent) => {
      setSystemTheme(event.matches ? "dark" : "light");
    };
    mediaQuery.addEventListener("change", handleChange);
    return () => {
      mediaQuery.removeEventListener("change", handleChange);
    };
  }, []);

  const resolvedTheme: ResolvedTheme = theme === "system" ? systemTheme : theme;

  useEffect(() => {
    const root = window.document.documentElement;
    root.classList.remove("light", "dark");
    root.classList.add(resolvedTheme);
  }, [resolvedTheme]);

  const value = useMemo(
    () => ({
      theme,
      resolvedTheme,
      setTheme: (nextTheme: Theme) => {
        sessionStorage.setItem(storageKey, nextTheme);
        setTheme(nextTheme);
      },
    }),
    [theme, resolvedTheme, storageKey],
  );

  return (
    <ThemeProviderContext.Provider {...props} value={value}>
      {children}
    </ThemeProviderContext.Provider>
  );
}
