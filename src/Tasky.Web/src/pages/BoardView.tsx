import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Title, Card, Text, Button, Group, Modal, TextInput, Stack, Breadcrumbs, Anchor, Paper, ScrollArea, Badge, Avatar, Tooltip, Select } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm } from '@mantine/form';
import { IconPlus, IconLayoutBoard } from '@tabler/icons-react';
import { useParams, Link } from 'react-router-dom';
import { boardService } from '../api/projectService';
import { taskService } from '../api/taskService';
import { DragDropContext, Droppable, Draggable } from '@hello-pangea/dnd';
import { TaskDetailModal } from '../components/TaskDetailModal';

export function BoardView() {
    const { id } = useParams<{ id: string }>();
    const queryClient = useQueryClient();
    const [opened, { open, close }] = useDisclosure(false);
    const [selectedTaskId, setSelectedTaskId] = useState<number | null>(null);
    const [taskModalOpened, { open: openTaskModal, close: closeTaskModal }] = useDisclosure(false);

    const [selectedColumnId, setSelectedColumnId] = useState<number | null>(null);
    const [taskCreateModalOpened, { open: openTaskCreateModal, close: closeTaskCreateModal }] = useDisclosure(false);

    const taskForm = useForm({
        initialValues: {
            title: '',
            description: '',
            priority: 0,
        },
        validate: {
            title: (val) => (val.length < 3 ? 'Title is too short' : null),
        }
    });

    const createTaskMutation = useMutation({
        mutationFn: (values: any) => taskService.create({ ...values, columnId: selectedColumnId }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['boards', id] });
            taskForm.reset();
            closeTaskCreateModal();
        }
    });

    const { data: board, isLoading } = useQuery({
        queryKey: ['boards', id],
        queryFn: () => boardService.getById(Number(id)),
        enabled: !!id
    });

    const form = useForm({
        initialValues: {
            name: '',
        },
    });

    const createColumnMutation = useMutation({
        mutationFn: (name: string) => boardService.addColumn(Number(id), name, (board?.columns.length || 0) + 1),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['boards', id] });
            form.reset();
            close();
        },
    });

    const onDragEnd = async (result: any) => {
        const { destination, source, draggableId } = result;

        if (!destination) return;

        if (
            destination.droppableId === source.droppableId &&
            destination.index === source.index
        ) {
            return;
        }

        const taskId = Number(draggableId);
        const targetColumnId = Number(destination.droppableId);

        queryClient.setQueryData(['boards', id], (old: any) => {
            if (!old) return old;

            const newColumns = [...old.columns];
            const sourceColIdx = newColumns.findIndex(c => c.id === Number(source.droppableId));
            const destColIdx = newColumns.findIndex(c => c.id === Number(destination.droppableId));

            const [movedTask] = newColumns[sourceColIdx].tasks.splice(source.index, 1);
            newColumns[destColIdx].tasks.splice(destination.index, 0, movedTask);

            return { ...old, columns: newColumns };
        });

        try {
            await taskService.update(taskId, { columnId: targetColumnId });
        } catch (err) {
            queryClient.invalidateQueries({ queryKey: ['boards', id] });
        }
    };

    if (isLoading) return <Text>Loading board...</Text>;
    if (!board) return <Text>Board not found.</Text>;

    const items = [
        { title: 'Dashboard', href: '/' },
        { title: 'Board', href: `/boards/${board.id}` },
    ].map((item, index) => (
        <Anchor component={Link} to={item.href} key={index}>
            {item.title}
        </Anchor>
    ));

    return (
        <div style={{ height: 'calc(100vh - 120px)', display: 'flex', flexDirection: 'column' }}>
            <Breadcrumbs mb="md">{items}</Breadcrumbs>

            <Group justify="space-between" mb="xl">
                <Title order={2}>{board.name}</Title>
                <Button leftSection={<IconPlus size={16} />} variant="light" onClick={open}>Add Column</Button>
            </Group>

            <ScrollArea offsetScrollbars style={{ flex: 1 }}>
                <DragDropContext onDragEnd={onDragEnd}>
                    <Group align="flex-start" wrap="nowrap" gap="md" pb="xl">
                        {board.columns.map((column) => (
                            <Droppable droppableId={column.id.toString()} key={column.id}>
                                {(provided) => (
                                    <Paper
                                        ref={provided.innerRef}
                                        {...provided.droppableProps}
                                        p="md"
                                        bg="var(--mantine-color-dark-8)"
                                        radius="md"
                                        style={{ width: 300, minHeight: 400 }}
                                    >
                                        <Group justify="space-between" mb="md">
                                            <Group gap="xs">
                                                <IconLayoutBoard size={16} />
                                                <Text fw={600} size="sm">{column.name.toUpperCase()}</Text>
                                            </Group>
                                            <Badge variant="filled" size="sm">{column.tasks.length}</Badge>
                                        </Group>

                                        <Stack gap="xs">
                                            {column.tasks.map((task, index) => (
                                                <Draggable key={task.id} draggableId={task.id.toString()} index={index}>
                                                    {(provided) => (
                                                        <Card
                                                            ref={provided.innerRef}
                                                            {...provided.draggableProps}
                                                            {...provided.dragHandleProps}
                                                            shadow="xs" p="sm" radius="md" withBorder
                                                            style={{ ...provided.draggableProps.style, cursor: 'pointer' }}
                                                            onClick={() => {
                                                                setSelectedTaskId(task.id);
                                                                openTaskModal();
                                                            }}
                                                        >
                                                            <Text size="sm" fw={500} mb={5}>{task.title}</Text>
                                                            <Group justify="space-between" mt="xs">
                                                                <Badge color={task.priority === 'High' ? 'red' : 'blue'} size="xs">{task.priority}</Badge>
                                                                {task.assigneeName && (
                                                                    <Tooltip label={task.assigneeName}>
                                                                        <Avatar size="xs" radius="xl">{task.assigneeName[0]}</Avatar>
                                                                    </Tooltip>
                                                                )}
                                                            </Group>
                                                        </Card>
                                                    )}
                                                </Draggable>
                                            ))}
                                            {provided.placeholder}
                                            <Button
                                                variant="subtle" fullWidth size="xs"
                                                leftSection={<IconPlus size={14} />}
                                                justify="flex-start" mt="xs"
                                                onClick={() => {
                                                    setSelectedColumnId(column.id);
                                                    openTaskCreateModal();
                                                }}
                                            >
                                                Add Task
                                            </Button>
                                        </Stack>
                                    </Paper>
                                )}
                            </Droppable>
                        ))}
                    </Group>
                </DragDropContext>
            </ScrollArea>

            <TaskDetailModal taskId={selectedTaskId} opened={taskModalOpened} onClose={closeTaskModal} />

            <Modal opened={taskCreateModalOpened} onClose={closeTaskCreateModal} title="Create Task" centered>
                <form onSubmit={taskForm.onSubmit((values) => createTaskMutation.mutate(values))}>
                    <Stack>
                        <TextInput label="Title" required {...taskForm.getInputProps('title')} />
                        <TextInput label="Description" {...taskForm.getInputProps('description')} />
                        <Select
                            label="Priority"
                            data={[
                                { value: '0', label: 'Low' },
                                { value: '1', label: 'Medium' },
                                { value: '2', label: 'High' }
                            ]}
                            {...taskForm.getInputProps('priority')}
                            onChange={(val) => taskForm.setFieldValue('priority', Number(val))}
                            value={taskForm.values.priority.toString()}
                        />
                        <Button type="submit" loading={createTaskMutation.isPending}>Create Task</Button>
                    </Stack>
                </form>
            </Modal>

            <Modal opened={opened} onClose={close} title="Add New Column" centered>
                <form onSubmit={form.onSubmit((values) => createColumnMutation.mutate(values.name))}>
                    <Stack>
                        <TextInput label="Column Name" placeholder="e.g. To Do, Done" required {...form.getInputProps('name')} />
                        <Button type="submit" loading={createColumnMutation.isPending}>Add Column</Button>
                    </Stack>
                </form>
            </Modal>
        </div>
    );
}
