import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { LanguageSelector } from "../../components/shared/LanguageSelector";
import { registerApi, type ApiError } from "../../services/api/register";
import { hash, validateStrength } from "../../services/passwordService";

export default function RegisterPage() {
  const { t } = useTranslation();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordHint, setPasswordHint] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");

    // Client-side validation
    if (!username.trim()) {
      setError(t("errors.USERNAME_REQUIRED"));
      return;
    }

    if (!password) {
      setError(t("errors.PASSWORD_REQUIRED"));
      return;
    }

    if (password !== confirmPassword) {
      setError(t("errors.PASSWORD_MISMATCH"));
      return;
    }

    const passwordValidation = validateStrength(password);
    if (!passwordValidation.isValid) {
      setError(t(`errors.${passwordValidation.errors[0]}`));
      return;
    }

    setIsSubmitting(true);

    try {
      // Hash the password on the client-side
      const passwordHash = await hash(password);

      // Send registration request
      const response = await registerApi.invoke({
        username: username.trim(),
        passwordHash,
      });

      if (response.userId) {
        setSuccess(true);
      }
    } catch (err) {
      const apiError = err as ApiError;
      if (apiError.code) {
        setError(t(`errors.${apiError.code}`));
      } else {
        setError(t("errors.NETWORK_ERROR"));
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
          {t("auth.register.title")}
        </h1>

        {/* Success Message */}
        {success && (
          <div className="mb-6 p-4 bg-green-500/20 border border-green-500 rounded-lg">
            <p className="text-green-400 text-sm font-medium">
              {t("auth.register.successMessage")}
            </p>
            <p className="text-green-300 text-xs mt-2">
              <Link to="/login" className="text-blue-300 underline">
                {t("auth.register.signInLink")}
              </Link>
            </p>
          </div>
        )}

        {/* Error Message */}
        {error && !success && (
          <div className="mb-6 p-4 bg-red-500/20 border border-red-500 rounded-lg">
            <p className="text-red-400 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Username */}
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              {t("auth.register.username")}
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder={t("auth.register.usernamePlaceholder")}
              disabled={isSubmitting || success}
              required
            />
            <p className="mt-1 text-xs text-slate-400">
              {t("auth.register.usernameHelp")}
            </p>
          </div>

          {/* Password */}
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              {t("auth.register.password")}
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder={t("auth.register.passwordPlaceholder")}
              disabled={isSubmitting || success}
              required
            />
            <p className="mt-1 text-xs text-slate-400">
              {t("auth.register.passwordHelp")}
            </p>
            <div className="mt-2 p-2 bg-yellow-500/10 border border-yellow-500/30 rounded text-xs text-yellow-300">
              {t("auth.register.passwordWarning")}
            </div>
          </div>

          {/* Confirm Password */}
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              {t("auth.register.confirmPassword")}
            </label>
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder={t("auth.register.confirmPasswordPlaceholder")}
              disabled={isSubmitting || success}
              required
            />
          </div>

          {/* Password Hint */}
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">
              {t("auth.register.passwordHint")}
            </label>
            <input
              type="text"
              value={passwordHint}
              onChange={(e) => setPasswordHint(e.target.value)}
              className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder={t("auth.register.passwordHintPlaceholder")}
              disabled={isSubmitting || success}
            />
            <p className="mt-1 text-xs text-slate-400">
              {t("auth.register.passwordHintHelp")}
            </p>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={isSubmitting || success}
            className="w-full bg-gradient-to-r from-blue-600 to-blue-700 text-white py-3 rounded-lg font-semibold hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-lg hover:shadow-blue-500/50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting
              ? t("common.loading")
              : t("auth.register.registerButton")}
          </button>

          {/* Login Link */}
          <p className="text-center text-slate-400 text-sm">
            {t("auth.register.hasAccount")}{" "}
            <Link
              to="/login"
              className="text-blue-400 hover:text-blue-300 transition"
            >
              {t("auth.register.signInLink")}
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
