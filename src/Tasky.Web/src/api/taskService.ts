import api from './api';

export interface CommentDto {
    id: number;
    text: string;
    authorName: string;
    createdAt: string;
}

export interface TaskDetailDto {
    id: number;
    columnId: number;
    title: string;
    description: string;
    priority: string;
    assigneeId?: number;
    assigneeName?: string;
    dueDate?: string;
    estimate?: string;
    comments: CommentDto[];
}

export const taskService = {
    getById: async (id: number) => {
        const { data } = await api.get<TaskDetailDto>(`/Tasks/${id}`);
        return data;
    },
    create: async (task: any) => {
        const { data } = await api.post<number>('/Tasks', task);
        return data;
    },
    update: async (id: number, task: any) => {
        await api.put(`/Tasks/${id}`, { taskId: id, ...task });
    },
    addComment: async (taskId: number, text: string) => {
        const { data } = await api.post<number>(`/Tasks/${taskId}/comments`, { taskId, text });
        return data;
    }
};
