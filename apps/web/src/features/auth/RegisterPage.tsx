import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../../services/api';

export default function RegisterPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await api.auth.register(username, password);
            navigate('/dashboard');
        } catch (error) {
            console.error('Registration failed', error);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 to-slate-800">
            <div className="bg-slate-800/50 backdrop-blur-xl p-8 rounded-2xl shadow-2xl w-full max-w-md border border-slate-700">
                <h1 className="text-3xl font-bold text-white mb-8 text-center">Create Account</h1>
                <form onSubmit={handleSubmit} className="space-y-6">
                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-2">
                            Username
                        </label>
                        <input
                            type="text"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
                            placeholder="Choose a username"
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-300 mb-2">
                            Password
                        </label>
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            className="w-full px-4 py-3 bg-slate-700/50 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
                            placeholder="Create a strong password"
                            required
                        />
                    </div>
                    <button
                        type="submit"
                        className="w-full bg-gradient-to-r from-blue-600 to-blue-700 text-white py-3 rounded-lg font-semibold hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-lg hover:shadow-blue-500/50"
                    >
                        Create Account
                    </button>
                    <p className="text-center text-slate-400 text-sm">
                        Already have an account?{' '}
                        <a href="/login" className="text-blue-400 hover:text-blue-300 transition">
                            Sign in
                        </a>
                    </p>
                </form>
            </div>
        </div>
    );
}
