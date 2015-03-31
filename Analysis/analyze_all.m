% dir_experiments_1_2_3_6 = '../Data/Experiments_1_2_3_6/';
% dir_experiments_4 = '../Data/Experiments_4/';
% 
% raw_data_table = table();
% 
% % loading data
% disp('Loading data...');
% tic;
% raw_data_table = readData(dir_experiments_1_2_3_6, raw_data_table);
% raw_data_table = readData(dir_experiments_4, raw_data_table);
% time = toc;
% fprintf('Loading data...Done!!!, time=%.2f\n',time);
% 
% % % % 
% disp('Cleaning data...');
% tic;
% data_table = cleanData(raw_data_table);
% time = toc;
% fprintf('Cleaning data...Done!!!, time=%.2f\n',time);
% 
% % % % 
% disp('Creating difference table...');
% tic;
% [difference_table, joints_difference_types] = getJointDifferenceTable(data_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% % % % 
% disp('Creating time average table...');
% tic;
% [time_average_table, joints_average_types] = getTimeAverageTable(difference_table, joints_difference_types);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);

% % % 
disp('Creating joints average study table...');
tic;
joints_average_study_table = getJointsAverageStudyTable(time_average_table, joints_average_types);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% % % 
disp('Creating all joints average Kinect Configuration table...');
tic;
joints_average_kinect_config_table = getJointsAverageKinectConfigTable(joints_average_study_table, joints_average_types);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% % %  
disp('Creating all joints average Scenario table...');
tic;
joints_average_scenario_table = getJointsAverageScenarioTable(joints_average_study_table, joints_average_types);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% % %  
disp('Creating coordinates average study table...');
tic;
[coordinates_average_study_table, coordinates_average_types] = getCoordinatesAverageStudyTable(joints_average_study_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% % %  
disp('Creating coordinates Kinect Configuration table...');
tic;
coordinates_average_kinect_config_table = getCoordinatesAverageKinectConfigTable(coordinates_average_study_table, coordinates_average_types);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% % %  
disp('Creating coordinates Scenario table...');
tic;
coordinates_average_scenario_table = getCoordinatesAverageScenarioTable(coordinates_average_study_table, coordinates_average_types);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

