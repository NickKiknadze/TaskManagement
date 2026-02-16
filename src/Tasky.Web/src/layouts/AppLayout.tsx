import { AppShell, Burger, Group, NavLink, Text, Button } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { IconDashboard, IconLogout, IconLayoutKanban } from '@tabler/icons-react';
import { useAuth } from '../contexts/AuthContext';
import { useSignalR } from '../contexts/SignalRContext';

export function AppLayout() {
    useSignalR();
    const [opened, { toggle }] = useDisclosure();
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <AppShell
            header={{ height: 60 }}
            navbar={{
                width: 300,
                breakpoint: 'sm',
                collapsed: { mobile: !opened },
            }}
            padding="md"
        >
            <AppShell.Header>
                <Group h="100%" px="md" justify="space-between">
                    <Group>
                        <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
                        <Text fw={700} size="xl">Tasky</Text>
                    </Group>
                    <Group>
                        <Text size="sm">{user?.username}</Text>
                        <Button variant="subtle" color="red" leftSection={<IconLogout size={16} />} onClick={handleLogout}>
                            Logout
                        </Button>
                    </Group>
                </Group>
            </AppShell.Header>

            <AppShell.Navbar p="md">
                <NavLink
                    label="Dashboard"
                    leftSection={<IconDashboard size={16} stroke={1.5} />}
                    onClick={() => navigate('/')}
                    active={location.pathname === '/'}
                />
                <NavLink
                    label="Projects"
                    leftSection={<IconLayoutKanban size={16} stroke={1.5} />}
                    onClick={() => navigate('/projects')}
                    active={location.pathname.startsWith('/projects')}
                />
            </AppShell.Navbar>

            <AppShell.Main>
                <Outlet />
            </AppShell.Main>
        </AppShell>
    );
}
