import React from 'react';
import {
  Box,
  Container,
  Typography,
  Paper,
  Grid,
  Button,
  Card,
  CardContent,
  IconButton,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import { withAuth } from '@/lib/auth/AuthContext';
import { Layout } from '@/components/layout/Layout';

// This would typically come from an API
const mockAppointments = [
  {
    id: 1,
    patientName: 'John Doe',
    date: '2023-12-15',
    time: '10:00 AM',
    type: 'Check-up',
    status: 'Scheduled',
  },
  // Add more mock data as needed
];

const AppointmentsPage: React.FC = () => {
  const handleAddAppointment = () => {
    // Implementation for adding new appointment
  };

  const handleEditAppointment = (id: number) => {
    // Implementation for editing appointment
  };

  const handleDeleteAppointment = (id: number) => {
    // Implementation for deleting appointment
  };

  return (
    <Layout>
      <Container maxWidth="lg">
        <Box sx={{ mt: 4, mb: 4 }}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
              <Typography variant="h4" component="h1">
                Appointments
              </Typography>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleAddAppointment}
              >
                New Appointment
              </Button>
            </Box>

            <Grid container spacing={3}>
              {mockAppointments.map((appointment) => (
                <Grid item xs={12} sm={6} md={4} key={appointment.id}>
                  <Card>
                    <CardContent>
                      <Box
                        sx={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'flex-start',
                        }}
                      >
                        <Box>
                          <Typography variant="h6" gutterBottom>
                            {appointment.patientName}
                          </Typography>
                          <Typography
                            variant="body2"
                            color="text.secondary"
                            gutterBottom
                          >
                            {appointment.type}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {appointment.date} at {appointment.time}
                          </Typography>
                          <Typography
                            variant="body2"
                            sx={{
                              mt: 1,
                              color:
                                appointment.status === 'Scheduled'
                                  ? 'success.main'
                                  : 'warning.main',
                            }}
                          >
                            {appointment.status}
                          </Typography>
                        </Box>
                        <Box>
                          <IconButton
                            onClick={() => handleEditAppointment(appointment.id)}
                            color="primary"
                            size="small"
                          >
                            <EditIcon />
                          </IconButton>
                          <IconButton
                            onClick={() => handleDeleteAppointment(appointment.id)}
                            color="error"
                            size="small"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Box>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          </Paper>
        </Box>
      </Container>
    </Layout>
  );
};

export default withAuth(AppointmentsPage, { requireAuth: true });
