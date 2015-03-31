% load('analysis_data.mat');

disp('Plotting joints average for Stationary task...');
tic;
plotJointsStationaryTask(joints_average_study_table);
time = toc;
fprintf('Done!!!, time=%.2f\n',time);

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

% disp('Plotting participant joints average...');
% plotParticipantJointsAverage(time_average_table);
% fprintf('Done!!!\n');
% 
% disp('Plotting participant joints over time...');
% plotParticipantJointsOverTime(difference_table);
% fprintf('Done!!!\n');

% disp('Plotting participant average over time...');
% plotParticipantAverageOverTime(difference_table);
% fprintf('Done!!!\n');

% disp('Plotting studies average...');
% plotStudiesJointsAverage(study_average_table);
% fprintf('Done!!!\n');