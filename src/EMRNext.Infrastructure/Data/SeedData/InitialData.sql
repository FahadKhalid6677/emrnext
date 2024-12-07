-- Reference Data

-- Specialties
IF NOT EXISTS (SELECT * FROM Specialties)
BEGIN
    INSERT INTO Specialties (Name, Code, Description)
    VALUES 
    ('Family Medicine', 'FM', 'Primary care for all ages'),
    ('Internal Medicine', 'IM', 'Adult primary care'),
    ('Pediatrics', 'PED', 'Child and adolescent care'),
    ('Emergency Medicine', 'EM', 'Emergency and acute care')
END

-- Visit Types
IF NOT EXISTS (SELECT * FROM VisitTypes)
BEGIN
    INSERT INTO VisitTypes (Name, Duration, Description)
    VALUES 
    ('New Patient', 30, 'Initial consultation for new patients'),
    ('Follow-up', 15, 'Follow-up visit for existing patients'),
    ('Annual Physical', 45, 'Comprehensive annual examination'),
    ('Urgent Care', 20, 'Same-day urgent care visit')
END

-- Lab Tests
IF NOT EXISTS (SELECT * FROM LabTests)
BEGIN
    INSERT INTO LabTests (Name, Code, Category, Description)
    VALUES 
    ('Complete Blood Count', 'CBC', 'Hematology', 'Basic blood cell count and differentials'),
    ('Basic Metabolic Panel', 'BMP', 'Chemistry', 'Basic metabolic status'),
    ('Lipid Panel', 'LIPID', 'Chemistry', 'Cholesterol and triglycerides'),
    ('Hemoglobin A1C', 'HBA1C', 'Chemistry', 'Diabetes monitoring')
END

-- Imaging Studies
IF NOT EXISTS (SELECT * FROM ImagingStudies)
BEGIN
    INSERT INTO ImagingStudies (Name, Code, Modality, Description)
    VALUES 
    ('Chest X-Ray', 'CXR', 'XR', 'Standard chest radiograph'),
    ('Abdominal Ultrasound', 'ABUS', 'US', 'Abdominal ultrasound study'),
    ('Brain MRI', 'BMRI', 'MR', 'Brain magnetic resonance imaging'),
    ('CT Head', 'CTH', 'CT', 'Head computed tomography')
END

-- Medications
IF NOT EXISTS (SELECT * FROM Medications)
BEGIN
    INSERT INTO Medications (Name, GenericName, Strength, Form)
    VALUES 
    ('Lisinopril', 'Lisinopril', '10mg', 'Tablet'),
    ('Metformin', 'Metformin HCl', '500mg', 'Tablet'),
    ('Amoxicillin', 'Amoxicillin', '500mg', 'Capsule'),
    ('Ibuprofen', 'Ibuprofen', '400mg', 'Tablet')
END

-- Allergies
IF NOT EXISTS (SELECT * FROM Allergies)
BEGIN
    INSERT INTO Allergies (Name, Type, Severity, Description)
    VALUES 
    ('Penicillin', 'Medication', 'Severe', 'Penicillin and derivatives'),
    ('Sulfa', 'Medication', 'Moderate', 'Sulfa-based medications'),
    ('Latex', 'Environmental', 'Moderate', 'Latex products'),
    ('Peanuts', 'Food', 'Severe', 'Peanuts and peanut products')
END

-- Problem List Items
IF NOT EXISTS (SELECT * FROM Problems)
BEGIN
    INSERT INTO Problems (Name, ICD10, SNOMED, Description)
    VALUES 
    ('Essential Hypertension', 'I10', '59621000', 'Primary hypertension'),
    ('Type 2 Diabetes', 'E11.9', '44054006', 'Type 2 diabetes mellitus'),
    ('Major Depressive Disorder', 'F32.9', '370143000', 'Depression'),
    ('Asthma', 'J45.909', '195967001', 'Asthma, unspecified')
END

-- Clinical Templates
IF NOT EXISTS (SELECT * FROM DocumentTemplates)
BEGIN
    INSERT INTO DocumentTemplates (Name, Type, Content)
    VALUES 
    ('Progress Note', 'Note', 'SUBJECTIVE:\n\nOBJECTIVE:\n\nASSESSMENT:\n\nPLAN:\n'),
    ('H&P', 'Note', 'HISTORY OF PRESENT ILLNESS:\n\nPAST MEDICAL HISTORY:\n\nREVIEW OF SYSTEMS:\n\nPHYSICAL EXAM:\n\nASSESSMENT & PLAN:\n'),
    ('Procedure Note', 'Note', 'PROCEDURE:\n\nINDICATION:\n\nTECHNIQUE:\n\nFINDINGS:\n\nPLAN:\n'),
    ('Discharge Summary', 'Note', 'ADMISSION DATE:\nDISCHARGE DATE:\n\nPRINCIPAL DIAGNOSIS:\n\nHOSPITAL COURSE:\n\nDISCHARGE MEDICATIONS:\n\nFOLLOW-UP:\n')
END

-- Order Sets
IF NOT EXISTS (SELECT * FROM OrderSets)
BEGIN
    INSERT INTO OrderSets (Name, Category, Description)
    VALUES 
    ('Admission Orders', 'Inpatient', 'Standard admission order set'),
    ('Diabetes Management', 'Chronic Care', 'Diabetes care order set'),
    ('Chest Pain', 'Emergency', 'Emergency chest pain workup'),
    ('Annual Physical', 'Preventive', 'Annual physical exam orders')
END
