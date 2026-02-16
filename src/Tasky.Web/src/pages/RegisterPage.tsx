import { useState } from 'react';
import { TextInput, PasswordInput, Button, Paper, Title, Container, Text, Anchor, Stack } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useNavigate, Link } from 'react-router-dom';
import api from '../api/api';

export function RegisterPage() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const form = useForm({
        initialValues: {
            username: '',
            password: '',
            confirmPassword: '',
        },
        validate: {
            username: (value) => (value.length < 3 ? 'Username must have at least 3 characters' : null),
            password: (value) => (value.length < 6 ? 'Password must have at least 6 characters' : null),
            confirmPassword: (value, values) => (value !== values.password ? 'Passwords do not match' : null),
        },
    });

    const handleSubmit = async (values: typeof form.values) => {
        setLoading(true);
        setError(null);
        try {
            await api.post('/Auth/register', { username: values.username, password: values.password });
            navigate('/login');
        } catch (err: any) {
            setError(err.response?.data || 'Registration failed.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Container size={420} my={40}>
            <Title ta="center" fw={900}>
                Create account
            </Title>
            <Text c="dimmed" size="sm" ta="center" mt={5}>
                Already have an account?{' '}
                <Anchor size="sm" component={Link} to="/login">
                    Login
                </Anchor>
            </Text>

            <Paper withBorder shadow="md" p={30} mt={30} radius="md">
                <form onSubmit={form.onSubmit(handleSubmit)}>
                    <Stack>
                        <TextInput label="Username" placeholder="Your username" required {...form.getInputProps('username')} />
                        <PasswordInput label="Password" placeholder="Your password" required {...form.getInputProps('password')} />
                        <PasswordInput label="Confirm Password" placeholder="Confirm your password" required {...form.getInputProps('confirmPassword')} />
                        {error && <Text color="red" size="sm">{error}</Text>}
                        <Button fullWidth mt="xl" type="submit" loading={loading}>
                            Register
                        </Button>
                    </Stack>
                </form>
            </Paper>
        </Container>
    );
}
