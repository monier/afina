import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { loginApi, type ApiError } from "../../services/api/login";
import { hash } from "../../services/passwordService";
import { LanguageSelector } from "../../components/shared/LanguageSelector";

export default function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      // Hash the password client-side before sending
      const authHash = await hash(password);
      await loginApi.invoke({ username, authHash });
      navigate("/dashboard");
    } catch (err) {
      console.error("Login failed", err);
      const apiError = err as ApiError;
      if (apiError.code) {
        setError(t(`errors.${apiError.code}`));
      } else {
        setError(t("errors.INVALID_CREDENTIALS"));
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // Language changes handled by LanguageSelector component

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 to-slate-800 p-4">
      <div className="bg-slate-800/50 backdrop-blur-xl p-8 rounded-2xl shadow-2xl w-full max-w-md border border-slate-700">
        {/* Language Selector */}
        <LanguageSelector className="flex justify-end mb-4" />

        <h1 className="text-3xl font-bold text-white mb-8 text-center">
          {t("auth.login.title")}
        </h1>

        {/* Error Message */}
        {error && (
          <div className="mb-6 p-4 bg-red-500/20 border border-red-500 rounded-lg">
            <p className="text-red-400 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              {t("auth.login.username")}
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder={t("auth.login.usernamePlaceholder")}
              disabled={isSubmitting}
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              {t("auth.login.password")}
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder={t("auth.login.passwordPlaceholder")}
              disabled={isSubmitting}
              required
            />
          </div>
          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-gradient-to-r from-blue-600 to-blue-700 text-white py-3 rounded-lg font-semibold hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-lg hover:shadow-blue-500/50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting ? t("common.loading") : t("auth.login.signInButton")}
          </button>
          <p className="text-center text-slate-400 text-sm">
            {t("auth.login.noAccount")}{" "}
            <Link
              to="/register"
              className="text-blue-400 hover:text-blue-300 transition"
            >
              {t("auth.login.signUpLink")}
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
