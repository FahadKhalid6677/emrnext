import React from 'react';
import { useParams } from 'react-router-dom';

const PatientDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <div>
      <h1>Patient Details</h1>
      <p>Details for Patient ID: {id}</p>
    </div>
  );
};

export default PatientDetailPage;
