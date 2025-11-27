const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5100/api/v1';

export const api = {
    auth: {
        login: async (username: string, password: string) => {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password }),
            });
            return response.json();
        },
        register: async (username: string, password: string) => {
            const response = await fetch(`${API_BASE_URL}/auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password }),
            });
            return response.json();
        },
    },
    tenants: {
        list: async () => {
            const response = await fetch(`${API_BASE_URL}/tenants`);
            return response.json();
        },
    },
    vault: {
        list: async (tenantId: string) => {
            const response = await fetch(`${API_BASE_URL}/tenants/${tenantId}/vault`);
            return response.json();
        },
    },
};
