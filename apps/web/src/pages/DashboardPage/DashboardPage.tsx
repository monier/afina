// import { useQuery } from '@tanstack/react-query';
// import { api } from '../../services/api';
import { Lock, FileText, Image, CreditCard } from 'lucide-react';

export default function DashboardPage() {
    // const { data: tenants } = useQuery({
    //   queryKey: ['tenants'],
    //   queryFn: api.tenants.list,
    // });

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-900 to-slate-800">
            <nav className="bg-slate-800/50 backdrop-blur-xl border-b border-slate-700">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
                    <div className="flex justify-between items-center">
                        <h1 className="text-2xl font-bold text-white">Afina</h1>
                        <div className="flex items-center space-x-4">
                            <span className="text-slate-300">Welcome back</span>
                            <button className="text-slate-400 hover:text-white transition">
                                Logout
                            </button>
                        </div>
                    </div>
                </div>
            </nav>

            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="mb-8">
                    <h2 className="text-3xl font-bold text-white mb-2">Your Vault</h2>
                    <p className="text-slate-400">Securely manage your private data</p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    <VaultItemCard
                        icon={<CreditCard className="w-6 h-6" />}
                        title="Banking Credentials"
                        type="Credential"
                        lastModified="2 hours ago"
                    />
                    <VaultItemCard
                        icon={<FileText className="w-6 h-6" />}
                        title="Personal Notes"
                        type="Note"
                        lastModified="1 day ago"
                    />
                    <VaultItemCard
                        icon={<Lock className="w-6 h-6" />}
                        title="SSH Keys"
                        type="Credential"
                        lastModified="3 days ago"
                    />
                    <VaultItemCard
                        icon={<Image className="w-6 h-6" />}
                        title="ID Documents"
                        type="Media"
                        lastModified="1 week ago"
                    />
                </div>

                <button className="mt-8 bg-gradient-to-r from-blue-600 to-blue-700 text-white px-6 py-3 rounded-lg font-semibold hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-lg hover:shadow-blue-500/50">
                    Add New Item
                </button>
            </main>
        </div>
    );
}

function VaultItemCard({
    icon,
    title,
    type,
    lastModified,
}: {
    icon: React.ReactNode;
    title: string;
    type: string;
    lastModified: string;
}) {
    return (
        <div className="bg-slate-800/50 backdrop-blur-xl p-6 rounded-xl border border-slate-700 hover:border-blue-500/50 transition-all duration-200 cursor-pointer group">
            <div className="flex items-start justify-between mb-4">
                <div className="p-3 bg-blue-500/10 rounded-lg text-blue-400 group-hover:bg-blue-500/20 transition">
                    {icon}
                </div>
                <span className="text-xs px-2 py-1 bg-slate-700 text-slate-300 rounded-full">
                    {type}
                </span>
            </div>
            <h3 className="text-white font-semibold mb-2">{title}</h3>
            <p className="text-sm text-slate-400">Modified {lastModified}</p>
        </div>
    );
}
