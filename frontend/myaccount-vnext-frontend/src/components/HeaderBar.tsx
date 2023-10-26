import AppBar from "@mui/material/AppBar";
import Container from "@mui/material/Container";
import Toolbar from "@mui/material/Toolbar";
import InstallDesktopIcon from "@mui/icons-material/InstallDesktop";
import Typography from "@mui/material/Typography";
import { useTheme } from "@mui/material/styles";
import Brightness4Icon from "@mui/icons-material/Brightness4";
import Brightness7Icon from "@mui/icons-material/Brightness7";
import React from "react";
import IconButton from "@mui/material/IconButton";
import { useColorMode } from "../contexts/ToggleColorModeProvider";

export const HeaderBar = () => {
  const theme = useTheme();
  const colorMode = useColorMode();
  return (
    <AppBar position="static">
      <Toolbar disableGutters>
        <InstallDesktopIcon
          sx={{ display: "flex", mr: 1 }}
        />
        <Typography
          variant="h6"
          noWrap
          component="a"
          href="#app-bar-with-responsive-menu"
          sx={{
            mr: 2,
            display: "flex",
            flexGrow: 1,
            fontFamily: "monospace",
            fontWeight: 700,
            letterSpacing: ".3rem",
            color: "inherit",
            textDecoration: "none",
          }}
        >
          myAccount vNext
        </Typography>
        <IconButton
          size="large"
          aria-label="account of current user"
          aria-controls="menu-appbar"
          aria-haspopup="true"
          onClick={colorMode.toggleColorMode}
          color="inherit"
        >
          {theme.palette.mode === "dark" ? (
            <Brightness7Icon />
          ) : (
            <Brightness4Icon />
          )}
        </IconButton>
      </Toolbar>
    </AppBar>
  );
};
