import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axios from 'axios';

export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  email: string;
  phoneNumber: string;
}

interface PatientState {
  patients: Patient[];
  currentPatient: Patient | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: PatientState = {
  patients: [],
  currentPatient: null,
  isLoading: false,
  error: null
};

export const fetchPatients = createAsyncThunk(
  'patients/fetchPatients',
  async (_, { rejectWithValue }) => {
    try {
      const response = await axios.get('/api/patients');
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to fetch patients');
    }
  }
);

export const fetchPatientById = createAsyncThunk(
  'patients/fetchPatientById',
  async (patientId: string, { rejectWithValue }) => {
    try {
      const response = await axios.get(`/api/patients/${patientId}`);
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to fetch patient details');
    }
  }
);

export const createPatient = createAsyncThunk(
  'patients/createPatient',
  async (patientData: Omit<Patient, 'id'>, { rejectWithValue }) => {
    try {
      const response = await axios.post('/api/patients', patientData);
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to create patient');
    }
  }
);

export const updatePatient = createAsyncThunk(
  'patients/updatePatient',
  async ({ id, patientData }: { id: string, patientData: Partial<Patient> }, { rejectWithValue }) => {
    try {
      const response = await axios.put(`/api/patients/${id}`, patientData);
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to update patient');
    }
  }
);

const patientSlice = createSlice({
  name: 'patients',
  initialState,
  reducers: {
    clearCurrentPatient: (state) => {
      state.currentPatient = null;
    }
  },
  extraReducers: (builder) => {
    // Fetch Patients
    builder.addCase(fetchPatients.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(fetchPatients.fulfilled, (state, action) => {
      state.isLoading = false;
      state.patients = action.payload;
    });
    builder.addCase(fetchPatients.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Fetch Patient By ID
    builder.addCase(fetchPatientById.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(fetchPatientById.fulfilled, (state, action) => {
      state.isLoading = false;
      state.currentPatient = action.payload;
    });
    builder.addCase(fetchPatientById.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Create Patient
    builder.addCase(createPatient.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(createPatient.fulfilled, (state, action) => {
      state.isLoading = false;
      state.patients.push(action.payload);
    });
    builder.addCase(createPatient.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Update Patient
    builder.addCase(updatePatient.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(updatePatient.fulfilled, (state, action) => {
      state.isLoading = false;
      const index = state.patients.findIndex(p => p.id === action.payload.id);
      if (index !== -1) {
        state.patients[index] = action.payload;
      }
      state.currentPatient = action.payload;
    });
    builder.addCase(updatePatient.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });
  }
});

export const { clearCurrentPatient } = patientSlice.actions;
export default patientSlice.reducer;
