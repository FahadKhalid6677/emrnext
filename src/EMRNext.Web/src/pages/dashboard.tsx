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
  CardActions,
} from '@mui/material';
import { useAuth } from '@/lib/auth/AuthContext';
import { withAuth } from '@/lib/auth/AuthContext';

const DashboardPage: React.FC = () => {
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Paper sx={{ p: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="h4" component="h1" gutterBottom>
                  Welcome, {user?.firstName} {user?.lastName}
                </Typography>
                <Typography variant="subtitle1" color="text.secondary">
                  {user?.email}
                </Typography>
              </Box>
              <Button variant="outlined" color="primary" onClick={handleLogout}>
                Logout
              </Button>
            </Paper>
          </Grid>

          <Grid item xs={12} md={6} lg={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" component="h2" gutterBottom>
                  Recent Patients
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  View and manage your recent patient interactions
                </Typography>
              </CardContent>
              <CardActions>
                <Button size="small" color="primary">
                  View All
                </Button>
              </CardActions>
            </Card>
          </Grid>

          <Grid item xs={12} md={6} lg={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" component="h2" gutterBottom>
                  Appointments
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Manage your upcoming appointments
                </Typography>
              </CardContent>
              <CardActions>
                <Button size="small" color="primary">
                  View Schedule
                </Button>
              </CardActions>
            </Card>
          </Grid>

          <Grid item xs={12} md={6} lg={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" component="h2" gutterBottom>
                  Tasks
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  View and manage your pending tasks
                </Typography>
              </CardContent>
              <CardActions>
                <Button size="small" color="primary">
                  View Tasks
                </Button>
              </CardActions>
            </Card>
          </Grid>
        </Grid>
      </Box>
    </Container>
  );
};

export default withAuth(DashboardPage, { requireAuth: true });
