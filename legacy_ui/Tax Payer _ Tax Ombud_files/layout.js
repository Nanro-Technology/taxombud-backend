/* eslint-disable */
// Restore layout attributes from sessionStorage without forcing reloads.
(function () {
  "use strict";
  const loadDefaultThemeColor = () => {
    try {
      const scriptUrl = document.currentScript && document.currentScript.src
        ? document.currentScript.src
        : window.location.href;
      const configUrl = new URL("../../config/ui-theme.json", scriptUrl);
      const xhr = new XMLHttpRequest();
      xhr.open("GET", configUrl.toString(), false);
      xhr.send(null);
      if (xhr.status >= 200 && xhr.status < 300) {
        const data = JSON.parse(xhr.responseText || "{}");
        if (data && typeof data.default_theme_color === "string" && data.default_theme_color.trim()) {
          return data.default_theme_color.trim();
        }
      }
    } catch (e) {}
    return "green";
  };
  const fallbackTheme = loadDefaultThemeColor();
  
  const allowedThemeColors = ["green", "purple", "blue", "deep-blue", "red"];
  const getStoredThemeColor = () => {
    try {
      return localStorage.getItem("data-theme-colors");
    } catch (e) {
      return null;
    }
  };
  const pageThemeColorsRaw = document.documentElement.getAttribute("data-theme-colors");
  const pageThemeColors = pageThemeColorsRaw === "default" ? fallbackTheme : pageThemeColorsRaw;
  if (!sessionStorage.getItem("defaultAttribute")) {
    const htmlEl = document.documentElement;
    const attrs = htmlEl.attributes;
    const initial = {};
    Object.entries(attrs).forEach(([, attr]) => {
      if (attr && attr.nodeName) {
        initial[attr.nodeName] = attr.nodeValue;
      }
    });
    if (!initial["data-theme-colors"] || initial["data-theme-colors"] === "default") {
      const stored = getStoredThemeColor();
      const validStored = stored && allowedThemeColors.includes(stored) ? stored : null;
      initial["data-theme-colors"] = validStored || pageThemeColors || fallbackTheme;
      htmlEl.setAttribute("data-theme-colors", initial["data-theme-colors"]);
      try {
        localStorage.setItem("data-theme-colors", initial["data-theme-colors"]);
      } catch (e) {}
    }
    if (!initial["data-preloader"] || initial["data-preloader"] === "disable") {
      htmlEl.setAttribute("data-preloader", "enable");
      initial["data-preloader"] = "enable";
    }
    sessionStorage.setItem("defaultAttribute", JSON.stringify(initial));
    Object.keys(initial).forEach((key) => {
      htmlEl.setAttribute(key, initial[key]);
    });
  }

  const htmlAttrs = document.documentElement.attributes;
  const current = {};
  Object.entries(htmlAttrs).forEach(([, attr]) => {
    if (attr && attr.nodeName) {
      current[attr.nodeName] = attr.nodeValue;
    }
  });

  const map = {
    "data-layout": sessionStorage.getItem("data-layout"),
    "data-sidebar-size": sessionStorage.getItem("data-sidebar-size"),
    "data-bs-theme": sessionStorage.getItem("data-bs-theme"),
    "data-layout-width": sessionStorage.getItem("data-layout-width"),
    "data-sidebar": sessionStorage.getItem("data-sidebar"),
    "data-sidebar-visibility": sessionStorage.getItem("data-sidebar-visibility"),
    "data-sidebar-image": sessionStorage.getItem("data-sidebar-image"),
    "data-layout-direction": sessionStorage.getItem("data-layout-direction"),
    "data-layout-position": sessionStorage.getItem("data-layout-position"),
    "data-layout-style": sessionStorage.getItem("data-layout-style"),
    "data-topbar": sessionStorage.getItem("data-topbar"),
    "data-preloader": sessionStorage.getItem("data-preloader"),
    "data-body-image": sessionStorage.getItem("data-body-image"),
    "data-theme": sessionStorage.getItem("data-theme"),
    "data-theme-colors": sessionStorage.getItem("data-theme-colors") || getStoredThemeColor(),
  };

  const defaults = (() => {
    try {
      return JSON.parse(sessionStorage.getItem("defaultAttribute") || "{}");
    } catch (e) {
      return {};
    }
  })();

  Object.keys(map).forEach((key) => {
    let storedVal = map[key];
    const fallbackVal = defaults[key];
    if (key === "data-theme-colors") {
      const localVal = getStoredThemeColor();
      const validLocal = localVal && allowedThemeColors.includes(localVal) ? localVal : null;
      if (storedVal === "default") {
        storedVal = validLocal || fallbackTheme;
      }
      if (!storedVal || !allowedThemeColors.includes(storedVal)) {
        storedVal = validLocal || pageThemeColors || fallbackTheme;
      }
      sessionStorage.setItem("data-theme-colors", storedVal);
      try {
        localStorage.setItem("data-theme-colors", storedVal);
      } catch (e) {}
      defaults["data-theme-colors"] = storedVal;
      sessionStorage.setItem("defaultAttribute", JSON.stringify(defaults));
    }
    const val = storedVal || fallbackVal || (key === "data-theme-colors" ? fallbackTheme : null);
    if (val) document.documentElement.setAttribute(key, val);
  });
})();