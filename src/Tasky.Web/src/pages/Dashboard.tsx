import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Title, Grid, Card, Text, Button, Group, Modal, TextInput, Textarea, Stack, Paper } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm } from '@mantine/form';
import { IconPlus, IconFolder } from '@tabler/icons-react';
import { useNavigate } from 'react-router-dom';
import { projectService } from '../api/projectService';

export function Dashboard() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [opened, { open, close }] = useDisclosure(false);

    const { data: projects, isLoading } = useQuery({
        queryKey: ['projects'],
        queryFn: projectService.getAll
    });

    const form = useForm({
        initialValues: {
            name: '',
            description: '',
        },
        validate: {
            name: (value) => (value.length < 3 ? 'Name must be at least 3 characters' : null),
        },
    });

    const createMutation = useMutation({
        mutationFn: projectService.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['projects'] });
            form.reset();
            close();
        },
    });

    return (
        <>
            <Group justify="space-between" mb="xl">
                <Title order={2}>Your Projects</Title>
                <Button leftSection={<IconPlus size={16} />} onClick={open}>
                    New Project
                </Button>
            </Group>

            {isLoading ? (
                <Text>Loading projects...</Text>
            ) : (
                <Grid>
                    {projects?.map((project) => (
                        <Grid.Col key={project.id} span={{ base: 12, sm: 6, lg: 4 }}>
                            <Card shadow="sm" padding="lg" radius="md" withBorder
                                style={{ cursor: 'pointer', transition: 'transform 0.2s' }}
                                onClick={() => navigate(`/projects/${project.id}`)}>
                                <Group mb="xs">
                                    <IconFolder size={24} color="var(--mantine-color-blue-6)" />
                                    <Text fw={500}>{project.name}</Text>
                                </Group>
                                <Text size="sm" c="dimmed" lineClamp={2}>
                                    {project.description || 'No description provided.'}
                                </Text>
                                <Button variant="light" color="blue" fullWidth mt="md" radius="md">
                                    View Boards
                                </Button>
                            </Card>
                        </Grid.Col>
                    ))}
                    {projects?.length === 0 && (
                        <Grid.Col span={12}>
                            <Paper p="xl" withBorder pos="relative">
                                <Text ta="center" c="dimmed">You don't have any projects yet. Create one to get started!</Text>
                            </Paper>
                        </Grid.Col>
                    )}
                </Grid>
            )}

            <Modal opened={opened} onClose={close} title="Create New Project" centered>
                <form onSubmit={form.onSubmit((values) => createMutation.mutate(values))}>
                    <Stack>
                        <TextInput label="Project Name" placeholder="Enter project name" required {...form.getInputProps('name')} />
                        <Textarea label="Description" placeholder="Optional project description" {...form.getInputProps('description')} />
                        <Button type="submit" loading={createMutation.isPending}>
                            Create Project
                        </Button>
                    </Stack>
                </form>
            </Modal>
        </>
    );
}
