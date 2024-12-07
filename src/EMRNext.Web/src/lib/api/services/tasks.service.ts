import { apiClient } from '../client';

export interface Task {
  id: number;
  title: string;
  priority: string;
  dueDate: string;
  completed: boolean;
}

export interface CreateTaskDto {
  title: string;
  priority: string;
  dueDate: string;
}

export interface UpdateTaskDto extends Partial<CreateTaskDto> {
  completed?: boolean;
}

export const tasksService = {
  getAll: async () => {
    const response = await apiClient.get<Task[]>('/api/tasks');
    return response.data;
  },

  getById: async (id: number) => {
    const response = await apiClient.get<Task>(`/api/tasks/${id}`);
    return response.data;
  },

  create: async (data: CreateTaskDto) => {
    const response = await apiClient.post<Task>('/api/tasks', data);
    return response.data;
  },

  update: async (id: number, data: UpdateTaskDto) => {
    const response = await apiClient.put<Task>(`/api/tasks/${id}`, data);
    return response.data;
  },

  delete: async (id: number) => {
    await apiClient.delete(`/api/tasks/${id}`);
  },
};
