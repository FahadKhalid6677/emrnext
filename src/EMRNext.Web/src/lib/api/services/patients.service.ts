import { apiClient } from '../client';

export interface Patient {
  id: number;
  name: string;
  age: number;
  gender: string;
  lastVisit: string;
  status: string;
}

export interface CreatePatientDto {
  name: string;
  age: number;
  gender: string;
}

export interface UpdatePatientDto extends Partial<CreatePatientDto> {
  status?: string;
}

export const patientsService = {
  getAll: async () => {
    const response = await apiClient.get<Patient[]>('/api/patients');
    return response.data;
  },

  getById: async (id: number) => {
    const response = await apiClient.get<Patient>(`/api/patients/${id}`);
    return response.data;
  },

  create: async (data: CreatePatientDto) => {
    const response = await apiClient.post<Patient>('/api/patients', data);
    return response.data;
  },

  update: async (id: number, data: UpdatePatientDto) => {
    const response = await apiClient.put<Patient>(`/api/patients/${id}`, data);
    return response.data;
  },

  delete: async (id: number) => {
    await apiClient.delete(`/api/patients/${id}`);
  },
};
