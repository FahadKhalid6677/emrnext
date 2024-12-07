import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axios from 'axios';

export interface LabOrder {
  id: string;
  patientId: string;
  orderDate: string;
  status: 'Pending' | 'In Progress' | 'Completed' | 'Cancelled';
  testType: string;
}

interface LabOrderState {
  labOrders: LabOrder[];
  currentLabOrder: LabOrder | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: LabOrderState = {
  labOrders: [],
  currentLabOrder: null,
  isLoading: false,
  error: null
};

export const fetchLabOrders = createAsyncThunk(
  'labOrders/fetchLabOrders',
  async (_, { rejectWithValue }) => {
    try {
      const response = await axios.get('/api/laborders');
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to fetch lab orders');
    }
  }
);

export const fetchLabOrderById = createAsyncThunk(
  'labOrders/fetchLabOrderById',
  async (labOrderId: string, { rejectWithValue }) => {
    try {
      const response = await axios.get(`/api/laborders/${labOrderId}`);
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to fetch lab order details');
    }
  }
);

export const createLabOrder = createAsyncThunk(
  'labOrders/createLabOrder',
  async (labOrderData: Omit<LabOrder, 'id'>, { rejectWithValue }) => {
    try {
      const response = await axios.post('/api/laborders', labOrderData);
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to create lab order');
    }
  }
);

export const updateLabOrderStatus = createAsyncThunk(
  'labOrders/updateLabOrderStatus',
  async ({ id, status }: { id: string, status: LabOrder['status'] }, { rejectWithValue }) => {
    try {
      const response = await axios.patch(`/api/laborders/${id}/status`, { status });
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to update lab order status');
    }
  }
);

export const cancelLabOrder = createAsyncThunk(
  'labOrders/cancelLabOrder',
  async (labOrderId: string, { rejectWithValue }) => {
    try {
      const response = await axios.delete(`/api/laborders/${labOrderId}`);
      return response.data;
    } catch (error) {
      return rejectWithValue('Failed to cancel lab order');
    }
  }
);

const labOrderSlice = createSlice({
  name: 'labOrders',
  initialState,
  reducers: {
    clearCurrentLabOrder: (state) => {
      state.currentLabOrder = null;
    }
  },
  extraReducers: (builder) => {
    // Fetch Lab Orders
    builder.addCase(fetchLabOrders.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(fetchLabOrders.fulfilled, (state, action) => {
      state.isLoading = false;
      state.labOrders = action.payload;
    });
    builder.addCase(fetchLabOrders.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Fetch Lab Order By ID
    builder.addCase(fetchLabOrderById.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(fetchLabOrderById.fulfilled, (state, action) => {
      state.isLoading = false;
      state.currentLabOrder = action.payload;
    });
    builder.addCase(fetchLabOrderById.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Create Lab Order
    builder.addCase(createLabOrder.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(createLabOrder.fulfilled, (state, action) => {
      state.isLoading = false;
      state.labOrders.push(action.payload);
    });
    builder.addCase(createLabOrder.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Update Lab Order Status
    builder.addCase(updateLabOrderStatus.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(updateLabOrderStatus.fulfilled, (state, action) => {
      state.isLoading = false;
      const index = state.labOrders.findIndex(lo => lo.id === action.payload.id);
      if (index !== -1) {
        state.labOrders[index] = action.payload;
      }
      state.currentLabOrder = action.payload;
    });
    builder.addCase(updateLabOrderStatus.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Cancel Lab Order
    builder.addCase(cancelLabOrder.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(cancelLabOrder.fulfilled, (state, action) => {
      state.isLoading = false;
      state.labOrders = state.labOrders.filter(lo => lo.id !== action.payload.id);
      state.currentLabOrder = null;
    });
    builder.addCase(cancelLabOrder.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });
  }
});

export const { clearCurrentLabOrder } = labOrderSlice.actions;
export default labOrderSlice.reducer;
