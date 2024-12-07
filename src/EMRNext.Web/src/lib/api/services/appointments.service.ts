import { apiClient } from '../client';

export interface Appointment {
  id: number;
  patientId: number;
  patientName: string;
  date: string;
  time: string;
  type: string;
  status: string;
}

export interface CreateAppointmentDto {
  patientId: number;
  date: string;
  time: string;
  type: string;
}

export interface UpdateAppointmentDto extends Partial<CreateAppointmentDto> {
  status?: string;
}

export const appointmentsService = {
  getAll: async () => {
    const response = await apiClient.get<Appointment[]>('/api/appointments');
    return response.data;
  },

  getById: async (id: number) => {
    const response = await apiClient.get<Appointment>(`/api/appointments/${id}`);
    return response.data;
  },

  create: async (data: CreateAppointmentDto) => {
    const response = await apiClient.post<Appointment>('/api/appointments', data);
    return response.data;
  },

  update: async (id: number, data: UpdateAppointmentDto) => {
    const response = await apiClient.put<Appointment>(
      `/api/appointments/${id}`,
      data
    );
    return response.data;
  },

  delete: async (id: number) => {
    await apiClient.delete(`/api/appointments/${id}`);
  },
};
