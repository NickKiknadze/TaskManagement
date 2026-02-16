import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Title, Grid, Card, Text, Button, Group, Modal, TextInput, Stack, Breadcrumbs, Anchor } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm } from '@mantine/form';
import { IconPlus, IconLayoutBoard } from '@tabler/icons-react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { projectService } from '../api/projectService';

export function ProjectDetail() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [opened, { open, close }] = useDisclosure(false);

    const { data: project, isLoading } = useQuery({
        queryKey: ['projects', id],
        queryFn: () => projectService.getById(Number(id)),
        enabled: !!id
    });

    const form = useForm({
        initialValues: {
            name: '',
        },
        validate: {
            name: (value) => (value.length < 3 ? 'Name must be at least 3 characters' : null),
        },
    });

    const createBoardMutation = useMutation({
        mutationFn: (name: string) => projectService.createBoard(Number(id), name),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['projects', id] });
            form.reset();
            close();
        },
    });

    if (isLoading) return <Text>Loading project details...</Text>;
    if (!project) return <Text>Project not found.</Text>;

    const items = [
        { title: 'Dashboard', href: '/' },
        { title: project.name, href: `/projects/${project.id}` },
    ].map((item, index) => (
        <Anchor component={Link} to={item.href} key={index}>
            {item.title}
        </Anchor>
    ));

    return (
        <>
            <Breadcrumbs mb="xl">{items}</Breadcrumbs>

            <Group justify="space-between" mb="xl">
                <Stack gap={0}>
                    <Title order={2}>{project.name}</Title>
                    <Text c="dimmed" size="sm">{project.description}</Text>
                </Stack>
                <Button leftSection={<IconPlus size={16} />} onClick={open}>
                    New Board
                </Button>
            </Group>

            <Grid>
                {project.boards.map((board) => (
                    <Grid.Col key={board.id} span={{ base: 12, sm: 6, lg: 4 }}>
                        <Card shadow="sm" padding="lg" radius="md" withBorder
                            style={{ cursor: 'pointer', transition: 'transform 0.2s' }}
                            onClick={() => navigate(`/boards/${board.id}`)}>
                            <Group mb="xs">
                                <IconLayoutBoard size={24} color="var(--mantine-color-green-6)" />
                                <Text fw={500}>{board.name}</Text>
                            </Group>
                            <Button variant="light" color="green" fullWidth mt="md" radius="md">
                                Open Board
                            </Button>
                        </Card>
                    </Grid.Col>
                ))}
                {project.boards.length === 0 && (
                    <Grid.Col span={12}>
                        <Text ta="center" c="dimmed">This project has no boards yet.</Text>
                    </Grid.Col>
                )}
            </Grid>

            <Modal opened={opened} onClose={close} title="Create New Board" centered>
                <form onSubmit={form.onSubmit((values) => createBoardMutation.mutate(values.name))}>
                    <Stack>
                        <TextInput label="Board Name" placeholder="Enter board name" required {...form.getInputProps('name')} />
                        <Button type="submit" loading={createBoardMutation.isPending}>
                            Create Board
                        </Button>
                    </Stack>
                </form>
            </Modal>
        </>
    );
}
