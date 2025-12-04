import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";

import enUS from "./locales/en-US.json";
import frFR from "./locales/fr-FR.json";

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      "en-US": { translation: enUS },
      "fr-FR": { translation: frFR },
    },
    fallbackLng: "en-US",
    supportedLngs: ["en-US", "fr-FR"],
    interpolation: {
      escapeValue: false,
    },
  });

// Custom language resolution:
// 1) URL query param `lang` overrides
// 2) Else use localStorage `afina_lang`
// 3) Else keep i18next's detected language
(() => {
  try {
    const params = new URLSearchParams(window.location.search);
    const urlLang = params.get("lang");
    const supported = ["en-US", "fr-FR"];
    if (urlLang && supported.includes(urlLang)) {
      i18n.changeLanguage(urlLang);
      localStorage.setItem("afina_lang", urlLang);
      return;
    }

    const saved = localStorage.getItem("afina_lang");
    if (saved && supported.includes(saved)) {
      i18n.changeLanguage(saved);
      return;
    }

    // Persist current detected language if supported
    const current = i18n.language;
    if (supported.includes(current)) {
      localStorage.setItem("afina_lang", current);
    } else {
      // enforce fallback
      i18n.changeLanguage("en-US");
      localStorage.setItem("afina_lang", "en-US");
    }
  } catch {
    // no-op on environments without window/localStorage
  }
})();

export default i18n;
