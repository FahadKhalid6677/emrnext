-- Create test provider
INSERT INTO Providers (Id, FirstName, LastName, Specialty, Email, Username)
VALUES 
('1', 'John', 'Doe', 'Internal Medicine', 'john.doe@test.com', 'jdoe'),
('2', 'Jane', 'Smith', 'Family Medicine', 'jane.smith@test.com', 'jsmith');

-- Create test patients
INSERT INTO Patients (Id, FirstName, LastName, DateOfBirth, Gender, MRN)
VALUES 
('1', 'Alice', 'Johnson', '1980-05-15', 'F', 'MRN001'),
('2', 'Bob', 'Williams', '1975-08-22', 'M', 'MRN002'),
('3', 'Carol', 'Davis', '1990-03-10', 'F', 'MRN003');

-- Create test problems
INSERT INTO Problems (Id, PatientId, Code, Description, Status, OnsetDate, Severity)
VALUES 
('1', '1', 'I10', 'Essential Hypertension', 'active', '2020-01-15', 'moderate'),
('2', '1', 'E11.9', 'Type 2 Diabetes', 'active', '2019-11-20', 'moderate'),
('3', '2', 'J45.909', 'Asthma', 'active', '2018-05-10', 'mild');

-- Create test vital signs
INSERT INTO VitalSigns (Id, PatientId, Type, Value, Unit, Timestamp, IsAbnormal)
VALUES 
('1', '1', 'blood_pressure_systolic', 138, 'mmHg', '2023-01-15 10:30:00', 0),
('2', '1', 'blood_pressure_diastolic', 88, 'mmHg', '2023-01-15 10:30:00', 0),
('3', '1', 'heart_rate', 72, 'bpm', '2023-01-15 10:30:00', 0),
('4', '1', 'temperature', 37.2, 'C', '2023-01-15 10:30:00', 0);

-- Create order templates
INSERT INTO OrderTemplates (Id, Name, Category, DefaultDetails)
VALUES 
('1', 'Basic Metabolic Panel', 'lab', 'Routine morning collection'),
('2', 'Chest X-Ray PA and Lateral', 'imaging', 'Standard views'),
('3', 'Complete Blood Count', 'lab', 'With differential');

-- Create test orders
INSERT INTO Orders (Id, PatientId, ProviderId, Type, Name, Priority, Status, OrderedAt)
VALUES 
('1', '1', '1', 'lab', 'Basic Metabolic Panel', 'routine', 'completed', '2023-01-15 09:00:00'),
('2', '1', '1', 'imaging', 'Chest X-Ray', 'urgent', 'completed', '2023-01-15 10:00:00');

-- Create test results
INSERT INTO Results (Id, OrderId, Status, CollectedAt, ReportedAt, PerformedBy)
VALUES 
('1', '1', 'final', '2023-01-15 10:00:00', '2023-01-15 11:30:00', 'Lab Tech'),
('2', '2', 'final', '2023-01-15 11:00:00', '2023-01-15 12:30:00', 'Radiology');

-- Create test result values
INSERT INTO ResultValues (Id, ResultId, Name, Value, Unit, ReferenceRange, IsAbnormal)
VALUES 
('1', '1', 'Sodium', '140', 'mEq/L', '135-145', 0),
('2', '1', 'Potassium', '4.2', 'mEq/L', '3.5-5.0', 0),
('3', '1', 'Glucose', '110', 'mg/dL', '70-100', 1);

-- Create macro templates
INSERT INTO MacroTemplates (Id, Name, Category, Content, CreatedBy)
VALUES 
('1', 'Normal Physical Exam', 'physical_exam', 'Constitutional: Alert and oriented, no acute distress\nCardiovascular: Regular rate and rhythm, normal S1/S2\nRespiratory: Clear to auscultation bilaterally', '1'),
('2', 'Normal Review of Systems', 'ros', 'Constitutional: No fever, chills, or weight changes\nCardiovascular: No chest pain or palpitations\nRespiratory: No shortness of breath or cough', '1'),
('3', 'Diabetes Follow-up', 'assessment_plan', 'Assessment: Type 2 Diabetes, [control]\nPlan:\n1. Continue current medications\n2. Check HbA1c in 3 months\n3. Return to clinic in 3 months', '1');
