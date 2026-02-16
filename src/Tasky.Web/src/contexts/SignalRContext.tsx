import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';
import { useQueryClient } from '@tanstack/react-query';

interface SignalRContextType {
    connection: signalR.HubConnection | null;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const { token } = useAuth();
    const queryClient = useQueryClient();
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    useEffect(() => {
        if (!token) {
            if (connectionRef.current) {
                connectionRef.current.stop();
                connectionRef.current = null;
                setConnection(null);
            }
            return;
        }

        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/realtime', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        newConnection.on('TaskUpdated', (taskId: number) => {
            queryClient.invalidateQueries({ queryKey: ['tasks', taskId] });
            queryClient.invalidateQueries({ queryKey: ['boards'] });
        });

        newConnection.on('CommentAdded', (data: any) => {
            queryClient.invalidateQueries({ queryKey: ['tasks', data.taskId] });
        });

        newConnection.start()
            .then(() => {
                console.log('SignalR Connected');
                connectionRef.current = newConnection;
                setConnection(newConnection);
            })
            .catch(err => console.log('SignalR Connection Error: ', err));

        return () => {
            newConnection.stop();
            connectionRef.current = null;
            setConnection(null);
        };
    }, [token, queryClient]);

    return (
        <SignalRContext.Provider value={{ connection }}>
            {children}
        </SignalRContext.Provider>
    );
};

export const useSignalR = () => {
    const context = useContext(SignalRContext);
    if (context === undefined) {
        throw new Error('useSignalR must be used within a SignalRProvider');
    }
    return context;
};
