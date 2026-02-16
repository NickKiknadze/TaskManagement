import api from './api';

export interface ProjectDto {
    id: number;
    name: string;
    description: string;
}

export interface BoardDto {
    id: number;
    name: string;
}

export interface ProjectDetailDto extends ProjectDto {
    boards: BoardDto[];
}

export interface CreateProjectRequest {
    name: string;
    description: string;
}

export const projectService = {
    getAll: async () => {
        const { data } = await api.get<ProjectDto[]>('/Projects');
        return data;
    },
    getById: async (id: number) => {
        const { data } = await api.get<ProjectDetailDto>(`/Projects/${id}`);
        return data;
    },
    create: async (project: CreateProjectRequest) => {
        const { data } = await api.post<number>('/Projects', project);
        return data;
    },
    createBoard: async (projectId: number, name: string) => {
        const { data } = await api.post<number>(`/Projects/${projectId}/boards`, { projectId, name });
        return data;
    }
};

export interface TaskDto {
    id: number;
    title: string;
    description: string;
    priority: string;
    assigneeId?: number;
    assigneeName?: string;
}

export interface ColumnDto {
    id: number;
    name: string;
    order: number;
    tasks: TaskDto[];
}

export interface BoardDetailDto {
    id: number;
    projectId: number;
    name: string;
    columns: ColumnDto[];
}

export const boardService = {
    getById: async (id: number) => {
        const { data } = await api.get<BoardDetailDto>(`/Boards/${id}`);
        return data;
    },
    addColumn: async (boardId: number, name: string, order: number) => {
        const { data } = await api.post<number>(`/Boards/${boardId}/columns`, { boardId, name, order });
        return data;
    }
};
