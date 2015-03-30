% load('analysis_data.mat');

% disp('Plotting participant joints average...');
% plotParticipantJointsAverage(time_average_table);
% fprintf('Done!!!\n');
% 
disp('Plotting participant joints over time...');
plotParticipantJointsOverTime(difference_table);
fprintf('Done!!!\n');

% disp('Plotting participant average over time...');
% plotParticipantAverageOverTime(difference_table);
% fprintf('Done!!!\n');

% disp('Plotting studies average...');
% plotStudiesJointsAverage(study_average_table);
% fprintf('Done!!!\n');