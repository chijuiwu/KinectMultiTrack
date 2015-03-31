% load('analysis_data.mat');

%
% First three scenarios
% 

% dd, tasks, joint types
% plotJointsTasks3d(joints_average_study_table);
% 

disp('Plotting joints average for Stationary task...');
tic;
plotJointsStationaryTask(joints_average_study_table, joints_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting joints average for Steps task...');
tic;
plotJointsStepsTask(joints_average_study_table, joints_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting joints average for Walk task...');
tic;
plotJointsWalkTask(joints_average_study_table, joints_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);


disp('Plotting joints average for Stationary, Steps, and Walk task for Parallel Kinects...');
tic;
plotJointsFirstThreeTasksParallel(joints_average_kinect_config_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting joints average for Stationary, Steps, and Walk task for 45 deg Kinects...');
tic;
plotJointsFirstThreeTasks45(joints_average_kinect_config_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting joints average for Stationary, Steps, and Walk task for 90 deg Kinects...');
tic;
plotJointsFirstThreeTasks90(joints_average_kinect_config_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% % % % % % 

disp('Plotting coordinates average for Stationary task...');
tic;
plotCoordinatesStationaryTask(coordinates_average_study_table, coordinates_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting coordinates average for Steps task...');
tic;
plotCoordinatesStepsTask(coordinates_average_study_table, coordinates_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting coordinates average for Walk task...');
tic;
plotCoordinatesWalkTask(coordinates_average_study_table, coordinates_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

disp('Plotting coordinates average for first three tasks...');
tic;
plotCoordinatesFirstThreeTasks(coordinates_average_scenario_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% 
% First three scenarios configurations
% 

disp('Plotting coordinates average for first three tasks over all kinect configs...');
tic;
plotCoordinatesFirstThreeTasksKinectConfig(coordinates_average_kinect_config_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

% 
disp('Plotting participant average over time...');
tic;
% plotParticipantAverageOverTime(difference_table);
% plotParticipantJointsOverTime(difference_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);
% 

% 
% Overall studies
% 
% disp('Plotting coordinates average for all scenarios & kinect configs...');
% tic;
% plotCoordinatesAllStudies(coordinates_merged_average_study_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
