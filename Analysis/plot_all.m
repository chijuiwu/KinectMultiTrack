% % load('analysis_data.mat');
% 
% %
% % First three scenarios
% % 
% 
% disp('Plotting joints average for the Stationary task...');
% tic;
% plotJointsStationaryTask(joints_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for the Steps task...');
% tic;
% plotJointsStepsTask(joints_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for the Walk task...');
% tic;
% plotJointsWalkTask(joints_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for the first three tasks...');
% tic;
% plotJointsThreeTaks(joints_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for the Parallel Kinects over the first three tasks...');
% tic;
% plotJointsKinectParallel(joints_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for the 45 deg Kinects over the first three tasks...');
% tic;
% plotJointsKinect45(joints_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for the 90 deg Kinects over the first three tasks...');
% tic;
% plotJointsKinect90(joints_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting joints average for all kinect configurations over the first three tasks...');
% tic;
% plotJointsKinectAll(joints_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% % % % % % % 
% 
% disp('Plotting coordinates average for Stationary task...');
% tic;
% plotCoordinatesStationaryTask(coordinates_average_study_merged_table, coordinates_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting coordinates average for Steps task...');
% tic;
% plotCoordinatesStepsTask(coordinates_average_study_merged_table, coordinates_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting coordinates average for Walk task...');
% tic;
% plotCoordinatesWalkTask(coordinates_average_study_merged_table, coordinates_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting coordinates average for the first three tasks...');
% tic;
% plotCoordinatesThreeTasks(coordinates_average_scenario_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% % 
% % First three scenarios configurations
% % 
% 
% disp('Plotting coordinates average for parallel kinects...');
% tic;
% plotCoordinatesKinectParallel(coordinates_average_study_merged_table, coordinates_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting coordinates average for 45-degrees kinects...');
% tic;
% plotCoordinatesKinect45(coordinates_average_study_merged_table, coordinates_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting coordinates average for 90-degrees kinects...');
% tic;
% plotCoordinatesKinect90(coordinates_average_study_merged_table, coordinates_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting coordinates average for all kinect configs...');
% tic;
% plotCoordinatesKinectAll(coordinates_average_kinect_config_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% Overall studies
% disp('Plotting coordinates average for all scenarios & kinect configs...');
% tic;
% plotCoordinatesAllStudies(coordinates_average_study_merged_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);
% 
% disp('Plotting participant average over time...');
% tic;
plotParticipantCoordinatesAveragesOverTime(difference_table);
% plotParticipantJointsOverTime(difference_table);
% plotParticipantJointsHeatMap(difference_table);
% time = toc;
% fprintf('Done!!!, time=%.2f\n',time);

