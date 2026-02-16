import { Modal, Stack, Title, Text, Group, Badge, Avatar, Divider, Textarea, Button, Paper } from '@mantine/core';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { taskService, type CommentDto } from '../api/taskService';
import { IconSend, IconClock } from '@tabler/icons-react';
import { useState, useEffect } from 'react';
import { useSignalR } from '../contexts/SignalRContext';

interface TaskDetailModalProps {
    taskId: number | null;
    opened: boolean;
    onClose: () => void;
}

export function TaskDetailModal({ taskId, opened, onClose }: TaskDetailModalProps) {
    const queryClient = useQueryClient();
    const { connection } = useSignalR();
    const [comment, setComment] = useState('');

    useEffect(() => {
        if (connection && opened && taskId) {
            connection.invoke('JoinTaskGroup', taskId);
            return () => {
                connection.invoke('LeaveTaskGroup', taskId);
            };
        }
    }, [connection, opened, taskId]);

    const { data: task, isLoading } = useQuery({
        queryKey: ['tasks', taskId],
        queryFn: () => taskService.getById(taskId!),
        enabled: !!taskId && opened,
    });

    const commentMutation = useMutation({
        mutationFn: (text: string) => taskService.addComment(taskId!, text),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['tasks', taskId] });
            setComment('');
        },
    });

    if (!taskId) return null;

    return (
        <Modal opened={opened} onClose={onClose} size="lg" title={<Badge>Task Details</Badge>} centered>
            {isLoading ? (
                <Text>Loading task details...</Text>
            ) : task ? (
                <Stack gap="md">
                    <Title order={3}>{task.title}</Title>
                    <Text size="sm">{task.description || 'No description provided.'}</Text>

                    <Group gap="xl">
                        <Stack gap={5}>
                            <Text size="xs" c="dimmed" fw={700}>PRIORITY</Text>
                            <Badge color={task.priority === 'High' ? 'red' : 'blue'}>{task.priority}</Badge>
                        </Stack>
                        <Stack gap={5}>
                            <Text size="xs" c="dimmed" fw={700}>ASSIGNEE</Text>
                            <Group gap="xs">
                                <Avatar size="sm" radius="xl">{task.assigneeName?.[0] || '?'}</Avatar>
                                <Text size="sm">{task.assigneeName || 'Unassigned'}</Text>
                            </Group>
                        </Stack>
                        {task.estimate && (
                            <Stack gap={5}>
                                <Text size="xs" c="dimmed" fw={700}>ESTIMATE</Text>
                                <Group gap={5}>
                                    <IconClock size={14} />
                                    <Text size="sm">{task.estimate}</Text>
                                </Group>
                            </Stack>
                        )}
                    </Group>

                    <Divider my="sm" label="Comments" labelPosition="center" />

                    <Stack gap="sm" style={{ maxHeight: 300, overflowY: 'auto' }}>
                        {task.comments.map((c: CommentDto) => (
                            <Paper key={c.id} withBorder p="xs" radius="sm">
                                <Group justify="space-between" mb={5}>
                                    <Text size="xs" fw={700}>{c.authorName}</Text>
                                    <Text size="xs" c="dimmed">{new Date(c.createdAt).toLocaleString()}</Text>
                                </Group>
                                <Text size="sm">{c.text}</Text>
                            </Paper>
                        ))}
                        {task.comments.length === 0 && <Text size="sm" c="dimmed" ta="center">No comments yet.</Text>}
                    </Stack>

                    <Group align="flex-end">
                        <Textarea
                            placeholder="Write a comment..."
                            style={{ flex: 1 }}
                            value={comment}
                            onChange={(e) => setComment(e.currentTarget.value)}
                            minRows={2}
                        />
                        <Button variant="light" onClick={() => commentMutation.mutate(comment)} loading={commentMutation.isPending}>
                            <IconSend size={18} />
                        </Button>
                    </Group>
                </Stack>
            ) : (
                <Text>Error loading task.</Text>
            )}
        </Modal>
    );
}
