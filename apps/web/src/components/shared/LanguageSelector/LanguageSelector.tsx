import { useTranslation } from "react-i18next";

type Props = {
  className?: string;
};

const options = [
  { value: "en-US", label: "English 🇺🇸" },
  { value: "fr-FR", label: "Français 🇫🇷" },
];

export function LanguageSelector({ className }: Props) {
  const { i18n } = useTranslation();

  return (
    <div className={className ?? ""}>
      <select
        value={i18n.language}
        onChange={(e) => {
          const value = e.target.value;
          i18n.changeLanguage(value);
          try {
            localStorage.setItem("afina_lang", value);
          } catch {
            // Ignore localStorage errors
          }
        }}
        className="px-3 py-2 text-xs rounded bg-slate-700 text-slate-200 border border-slate-600"
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  );
}
